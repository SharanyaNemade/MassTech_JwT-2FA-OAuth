using JwTDBLogin.Models;
using OtpNet;
using QRCoder;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

namespace JwTDBLogin.Controllers
{
    public class AccountController : Controller
    {
        private IConfiguration _config;

        public AccountController(IConfiguration config)
        {
            _config = config;
        }

        // LOGOUT
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Login", "Account");
        }

        // LOGIN PAGE
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // DASHBOARD
        public IActionResult Dashboard()
        {
            return View();
        }



public IActionResult Setup2FA()
    {
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        var base32Key = Base32Encoding.ToString(secretKey);

        string email = User.Identity.Name;

        string authenticatorUri =
            $"otpauth://totp/JWTApp:{email}?secret={base32Key}&issuer=JWTApp";

        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(authenticatorUri, QRCodeGenerator.ECCLevel.Q);

        Base64QRCode qrCode = new Base64QRCode(qrCodeData);

        var model = new TwoFactorSetupModel
        {
            QRCodeImage = qrCode.GetGraphic(20),
            ManualKey = base32Key
        };

        TempData["SecretKey"] = base32Key;

        return View(model);
    }




        [HttpPost]
        public IActionResult Verify2FA(string code)
        {
            string secret = TempData["SecretKey"].ToString();

            var totp = new Totp(Base32Encoding.ToBytes(secret));

            bool isValid = totp.VerifyTotp(code, out long timeStepMatched);

            if (isValid)
            {
                // Save secret to database
                // Enable 2FA

                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid OTP";
            return View("Setup2FA");
        }

        // DATABASE LOGIN
        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            string cs = _config.GetConnectionString("dbconn")!;

            using (SqlConnection conn = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("sp_UserLogin", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Username", SqlDbType.VarChar).Value = model.Username;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = model.Password;

                conn.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string token = GenerateJwtToken(model.Username);

                    Response.Cookies.Append("jwt", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false
                    });

                    return RedirectToAction("Dashboard", "Account");
                }
            }

            ViewBag.Error = "Invalid Username or Password";
            return View();
        }

        // GOOGLE LOGIN START
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // GOOGLE RESPONSE HANDLER
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync();

            if (!result.Succeeded)
                return RedirectToAction("Login");

            var claims = result.Principal.Identities
                        .FirstOrDefault()
                        ?.Claims;

            string email = claims
                ?.FirstOrDefault(c => c.Type == ClaimTypes.Email)
                ?.Value;

            string token = GenerateJwtToken(email);

            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false
            });

            return RedirectToAction("Dashboard");
        }

        // JWT TOKEN GENERATION
        private string GenerateJwtToken(string username)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username)
            };

            var token = new JwtSecurityToken(
                issuer: "JwtMvcDemo",
                audience: "JwtMvcDemoUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}






















//using JwTDBLogin.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.IdentityModel.Tokens.Jwt;
//using Microsoft.IdentityModel.Tokens;
//using System.Security.Claims;
//using System.Text;
//using System.Data;
//using Microsoft.Data.SqlClient;

//namespace JwTDBLogin.Controllers
//{
//    public class AccountController : Controller
//    {
//        private IConfiguration _config;

//        public AccountController(IConfiguration config)
//        {
//            _config = config;
//        }

//        public IActionResult Logout()
//        {
//            Response.Cookies.Delete("jwt");
//            return RedirectToAction("Login","Account");
//        }

//        [HttpGet]
//        public IActionResult Login()
//        {
//            return View();
//        }

//        public IActionResult Dashboard()
//        {
//            return View();
//        }

//        [HttpPost]
//        public IActionResult Login(LoginModel model)
//        {
//            string cs = _config.GetConnectionString("dbconn")!;

//            using (SqlConnection conn = new SqlConnection(cs))
//            {
//                SqlCommand cmd = new SqlCommand("sp_UserLogin", conn);
//                cmd.CommandType = CommandType.StoredProcedure;

//                cmd.Parameters.Add("@Username", SqlDbType.VarChar).Value = model.Username;
//                cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = model.Password;

//                conn.Open();

//                SqlDataReader dr = cmd.ExecuteReader();

//                if (dr.Read())
//                {
//                    string token = GenerateJwtToken(model.Username);

//                    Response.Cookies.Append("jwt", token, new CookieOptions
//                    {
//                        HttpOnly = true,
//                        Secure = false
//                    });

//                    return RedirectToAction("Dashboard", "Home");
//                }
//            }

//            ViewBag.Error = "Invalid Username or Password";
//            return View();
//        }

//        private string GenerateJwtToken(string username)
//        {
//            var key = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

//            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//            var claims = new[]
//            {
//                new Claim(ClaimTypes.Name, username)
//            };

//            var token = new JwtSecurityToken(
//                issuer: "JwtMvcDemo",
//                audience: "JwtMvcDemoUsers",
//                claims: claims,
//                expires: DateTime.Now.AddMinutes(30),
//                signingCredentials: creds);

//            return new JwtSecurityTokenHandler().WriteToken(token);
//        }


//    }
//}
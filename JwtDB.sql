CREATE DATABASE JwtDB;
GO

USE JwtDB;
GO

CREATE TABLE Users
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE,
    Password NVARCHAR(200),
    Role NVARCHAR(50),
    TwoFactorSecret NVARCHAR(200),
    Is2FAEnabled BIT DEFAULT 0
);



INSERT INTO Users (Username, Password, Role)
VALUES ('admin', '1234', 'Admin'),
       ('user1', '1234', 'User');



select * from Users;



ALTER TABLE Users
ADD AuthenticatorSecret NVARCHAR(100);



CREATE PROCEDURE sp_UserLogin
(
    @Username NVARCHAR(50),
    @Password NVARCHAR(50)
)
AS
BEGIN
    SELECT Id, Username, Role, TwoFactorSecret, Is2FAEnabled
    FROM Users
    WHERE Username = @Username
    AND Password = @Password
END






CREATE PROCEDURE sp_Save2FASecret
(
    @Username NVARCHAR(50),
    @Secret NVARCHAR(200)
)
AS
BEGIN
    UPDATE Users
    SET TwoFactorSecret = @Secret,
        Is2FAEnabled = 1
    WHERE Username = @Username
END






CREATE PROCEDURE sp_Get2FASecret
(
    @Username NVARCHAR(50)
)
AS
BEGIN
    SELECT TwoFactorSecret
    FROM Users
    WHERE Username = @Username
END









-- create database JwtDB;



--CREATE TABLE Users
--(
--   Id INT IDENTITY,
--   Username NVARCHAR(50),
--    Password NVARCHAR(50)
--)



--INSERT INTO Users (Username, Password)
--VALUES ('admin', '1234');

--select * from users;



--CREATE PROCEDURE sp_UserLogin
--(
--   @Username NVARCHAR(50),
--    @Password NVARCHAR(50)
--)
--AS
--BEGIN
--    SELECT Id, Username
--    FROM Users
--    WHERE Username = @Username
--    AND Password = @Password
--END

--EXEC sp_UserLogin 'admin','admin'
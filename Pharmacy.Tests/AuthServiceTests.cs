using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Pharmacy.AuthService.Data;
using Pharmacy.AuthService.DTOs;
using Pharmacy.AuthService.Entities;
using Pharmacy.AuthService.Interfaces;
using JwtService = Pharmacy.AuthService.Services.JwtService;
using AuthServiceImplementation =
    Pharmacy.AuthService.Services.AuthService;

namespace Pharmacy.Tests;

[TestFixture]
public class AuthServiceTests
{
    [Test]
    public void GenerateToken_ShouldContainUserRoleAndIdentityClaims()
    {
        var jwtService = CreateJwtService();

        var user = CreateUser();

        var token = jwtService.GenerateToken(user);
        var parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.That(
            parsedToken.Claims.First(c => c.Type == ClaimTypes.Role).Value,
            Is.EqualTo("Doctor"));

        Assert.That(
            parsedToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value,
            Is.EqualTo("9"));

        Assert.That(parsedToken.Issuer, Is.EqualTo("PharmacyAPI"));
        Assert.That(parsedToken.Audiences, Does.Contain("PharmacyUsers"));
        Assert.That(parsedToken.ValidTo, Is.GreaterThan(DateTime.UtcNow));
    }

    [Test]
    public async Task ForgotPassword_ShouldStoreOtpAndSendEmail()
    {
        await using var context = CreateContext();
        var emailService = new FakeEmailService();

        context.Users.Add(CreateUser());
        await context.SaveChangesAsync();

        var service = CreateAuthService(context, emailService);

        var result = await service.ForgotPasswordAsync(
            new ForgotPasswordDto
            {
                Email = "testdoctor@example.com"
            });

        var user = await context.Users.SingleAsync();

        Assert.That(result, Is.EqualTo("OTP sent successfully"));
        Assert.That(user.ResetOtp, Is.Not.Null.And.Not.Empty);
        Assert.That(user.OtpExpiry, Is.GreaterThan(DateTime.UtcNow));
        Assert.That(user.IsOtpVerified, Is.False);
        Assert.That(emailService.LastBody, Does.Contain("Your OTP is:"));
        Assert.That(emailService.LastRecipient, Is.EqualTo("testdoctor@example.com"));
    }

    [Test]
    public async Task ForgotPassword_ShouldReturnUserNotFoundForUnknownEmail()
    {
        await using var context = CreateContext();
        var service = CreateAuthService(context, new FakeEmailService());

        var result = await service.ForgotPasswordAsync(
            new ForgotPasswordDto
            {
                Email = "missing@example.com"
            });

        Assert.That(result, Is.EqualTo("User not found"));
    }

    [Test]
    public async Task VerifyOtp_ShouldRejectInvalidOtp()
    {
        await using var context = CreateContext();

        context.Users.Add(CreateUser(
            resetOtp: "123456",
            otpExpiry: DateTime.UtcNow.AddMinutes(5)));

        await context.SaveChangesAsync();

        var service = CreateAuthService(context, new FakeEmailService());

        var result = await service.VerifyOtpAsync(
            new VerifyOtpDto
            {
                Email = "testdoctor@example.com",
                Otp = "000000"
            });

        Assert.That(result, Is.EqualTo("Invalid OTP"));
    }

    [Test]
    public async Task VerifyOtp_ShouldRejectExpiredOtp()
    {
        await using var context = CreateContext();

        context.Users.Add(CreateUser(
            resetOtp: "123456",
            otpExpiry: DateTime.UtcNow.AddMinutes(-1)));

        await context.SaveChangesAsync();

        var service = CreateAuthService(context, new FakeEmailService());

        var result = await service.VerifyOtpAsync(
            new VerifyOtpDto
            {
                Email = "testdoctor@example.com",
                Otp = "123456"
            });

        Assert.That(result, Is.EqualTo("OTP Expired"));
    }

    [Test]
    public async Task VerifyOtp_ShouldMarkOtpAsVerified()
    {
        await using var context = CreateContext();

        context.Users.Add(CreateUser(
            resetOtp: "123456",
            otpExpiry: DateTime.UtcNow.AddMinutes(5)));

        await context.SaveChangesAsync();

        var service = CreateAuthService(context, new FakeEmailService());

        var result = await service.VerifyOtpAsync(
            new VerifyOtpDto
            {
                Email = "testdoctor@example.com",
                Otp = "123456"
            });

        var user = await context.Users.SingleAsync();

        Assert.That(result, Is.EqualTo("OTP Verified Successfully"));
        Assert.That(user.IsOtpVerified, Is.True);
    }

    [Test]
    public async Task ResetPassword_ShouldRequireOtpVerification()
    {
        await using var context = CreateContext();

        context.Users.Add(CreateUser(
            resetOtp: "123456",
            otpExpiry: DateTime.UtcNow.AddMinutes(5)));

        await context.SaveChangesAsync();

        var service = CreateAuthService(context, new FakeEmailService());

        var result = await service.ResetPasswordAsync(
            new ResetPasswordDto
            {
                Email = "testdoctor@example.com",
                Otp = "123456",
                NewPassword = "NewPassword@123"
            });

        Assert.That(result, Is.EqualTo("OTP verification required"));
    }

    [Test]
    public async Task ResetPassword_ShouldResetPasswordAfterOtpVerification()
    {
        await using var context = CreateContext();

        context.Users.Add(CreateUser(
            resetOtp: "123456",
            otpExpiry: DateTime.UtcNow.AddMinutes(5),
            isOtpVerified: true));

        await context.SaveChangesAsync();

        var service = CreateAuthService(context, new FakeEmailService());

        var result = await service.ResetPasswordAsync(
            new ResetPasswordDto
            {
                Email = "testdoctor@example.com",
                Otp = "123456",
                NewPassword = "NewPassword@123"
            });

        var user = await context.Users.SingleAsync();

        Assert.That(result, Is.EqualTo("Password Reset Successful"));
        Assert.That(user.PasswordHash, Is.Not.EqualTo("existing-password-hash"));
        Assert.That(user.ResetOtp, Is.Null);
        Assert.That(user.OtpExpiry, Is.Null);
        Assert.That(user.IsOtpVerified, Is.False);
    }

    private static AuthServiceImplementation CreateAuthService(
    UsersDbContext context,
    IEmailService emailService)
{
    return new AuthServiceImplementation(
        context,
        CreateJwtService(),
        emailService);
}

    private static JwtService CreateJwtService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-key-that-is-long-enough-for-hmac-sha256-123456",
                ["Jwt:Issuer"] = "PharmacyAPI",
                ["Jwt:Audience"] = "PharmacyUsers"
            })
            .Build();

        return new JwtService(configuration);
    }

    private static UsersDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase($"AuthTests-{Guid.NewGuid()}")
            .Options;

        return new UsersDbContext(options);
    }

    private static User CreateUser(
        string? resetOtp = null,
        DateTime? otpExpiry = null,
        bool isOtpVerified = false)
    {
        return new User
        {
            Id = 9,
            Name = "Test Doctor",
            Email = "testdoctor@example.com",
            PasswordHash = "existing-password-hash",
            RoleId = 2,
            Role = new Role
            {
                Id = 2,
                Name = "Doctor"
            },
            CreatedAt = DateTime.UtcNow,
            ResetOtp = resetOtp,
            OtpExpiry = otpExpiry,
            IsOtpVerified = isOtpVerified
        };
    }

    private sealed class FakeEmailService : IEmailService
    {
        public string? LastRecipient { get; private set; }

        public string? LastSubject { get; private set; }

        public string? LastBody { get; private set; }

        public Task SendEmailAsync(
            string toEmail,
            string subject,
            string body)
        {
            LastRecipient = toEmail;
            LastSubject = subject;
            LastBody = body;

            return Task.CompletedTask;
        }
    }
}
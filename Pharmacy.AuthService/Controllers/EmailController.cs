using Microsoft.AspNetCore.Mvc;
using Pharmacy.AuthService.Interfaces;

namespace Pharmacy.AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTestEmail(string email)
    {
        await _emailService.SendEmailAsync(
            email,
            "SMTP Test",
            "SMTP is working successfully.");

        return Ok("Email Sent Successfully");
    }
}
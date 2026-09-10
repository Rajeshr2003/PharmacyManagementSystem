namespace Pharmacy.AuthService.Helpers;

public static class OtpGenerator
{
    public static string GenerateOtp()
    {
        Random random = new();

        return random.Next(100000, 999999).ToString();
    }
}
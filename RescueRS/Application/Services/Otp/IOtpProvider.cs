namespace ResgateRS.Domain.Application.Services.Otp;

public interface IOtpProvider
{
    Task<bool> SendCode(string cellphone);
    Task<bool> VerifyCode(string cellphone, string code);
}

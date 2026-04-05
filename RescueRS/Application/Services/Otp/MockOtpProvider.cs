namespace ResgateRS.Domain.Application.Services.Otp;

public class MockOtpProvider : IOtpProvider
{
    private readonly Dictionary<string, string> _codes = new();

    public Task<bool> SendCode(string cellphone)
    {
        _codes[cellphone] = "123456";
        return Task.FromResult(true);
    }

    public Task<bool> VerifyCode(string cellphone, string code)
    {
        if (_codes.TryGetValue(cellphone, out var stored) && stored == code)
        {
            _codes.Remove(cellphone);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}

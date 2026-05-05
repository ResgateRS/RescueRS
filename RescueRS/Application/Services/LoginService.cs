using ResgateRS.Domain.Application.Entities;
using ResgateRS.Domain.Application.Services.Interfaces;
using ResgateRS.Infrastructure.Repositories;
using ResgateRS.Presenter.Controllers.App.V1.DTOs;
using ResgateRS.Tools;
using ResgateRS.Auth;
using ResgateRS.DTOs;
using ResgateRS.Middleware;
using ResgateRS.Extensions;
using ResgateRS.Domain.Application.Services.Otp;

namespace ResgateRS.Domain.Application.Services;

public class LoginResult
{
    public bool OtpSent { get; set; }
    public string? Token { get; set; }
    public bool Rescuer { get; set; }
    public bool Admin { get; set; }
}

public class LoginService(IServiceProvider serviceProvider, UserSession userSession) : BaseService(serviceProvider, userSession), IService
{
    public async Task<LoginResult> Handle(LoginRequestDTO dto)
    {
        if (string.IsNullOrEmpty(dto.Cellphone))
            throw new MessageException("Telefone não informado.");

        if (!dto.Cellphone.IsValidCellphone())
            throw new MessageException("Telefone inválido.");

        var otpProvider = _serviceProvider.GetService<IOtpProvider>();
        if (otpProvider != null)
        {
            if (string.IsNullOrEmpty(dto.OtpCode))
            {
                await otpProvider.SendCode(dto.Cellphone);
                return new LoginResult { OtpSent = true };
            }

            if (!await otpProvider.VerifyCode(dto.Cellphone, dto.OtpCode))
                throw new MessageException("Código OTP inválido.");
        }

        UserEntity? user = await _serviceProvider.GetRequiredService<UserRepository>().GetUser(dto);

        if (user?.Banned == true)
            throw new MessageException("Conta suspensa. Entre em contato com o administrador.", 403);

        if (user == null)
        {
            user = new UserEntity
            {
                Cellphone = dto.Cellphone,
                CellphoneHash = ResgateRS.Infrastructure.Database.Encryption.PhoneHashHelper.ComputeHash(dto.Cellphone),
                Rescuer = false,
            };

            user = await _serviceProvider.GetRequiredService<UserRepository>().InsertOrUpdate(user);
        }

        var session = new UserSession
        {
            UserId = user.UserId,
            Rescuer = user.Rescuer,
            Admin = user.Admin,
            Cellphone = user.Cellphone
        };

        var jwt = _serviceProvider.GetRequiredService<JwtTool>();
        jwt.setUserData(session);
        string token = jwt.generateToken();

        return new LoginResult { Token = token, Rescuer = user.Rescuer, Admin = user.Admin };
    }
}

using ResgateRS.Auth;

namespace ResgateRS.Domain.Application.Services
{
    public class BaseService
    {
        protected readonly IServiceProvider _serviceProvider;
        
        protected readonly UserSession _userSession;

        public BaseService(IServiceProvider serviceProvider, UserSession userSession) => 
            (this._serviceProvider, _userSession) = (serviceProvider, userSession);

        public string GetExceptionMessage(Exception ex)
        {
            if (ex.InnerException == null)
                return ex.Message;

            return GetExceptionMessage(ex);
        }
    }
}


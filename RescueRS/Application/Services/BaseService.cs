using ResgateRS.Auth;

namespace ResgateRS.Domain.Application.Services
{
    public class BaseService<Repository>
    {
        protected readonly Repository _mainRepository;
        
        protected readonly UserSession _userSession;

        public BaseService(Repository repository, UserSession userSession) => 
            (this._mainRepository, _userSession) = (repository, userSession);

        public string GetExceptionMessage(Exception ex)
        {
            if (ex.InnerException == null)
                return ex.Message;

            return GetExceptionMessage(ex);
        }
    }
}


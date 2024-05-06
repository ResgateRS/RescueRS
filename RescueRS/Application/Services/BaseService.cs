namespace ResgateRS.Domain.Application.Services
{
    public class BaseService<Repository>
    {
        protected readonly Repository _mainRepository;

        public BaseService(Repository repository) { this._mainRepository = repository; }

        public string GetExceptionMessage(Exception ex)
        {
            if (ex.InnerException == null)
                return ex.Message;

            return GetExceptionMessage(ex);
        }
    }
}


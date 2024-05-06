using System.Reflection;
using ResgateRS.Domain.Application.Services.Interfaces;
using ResgateRS.Infrastructure.Repositories;

namespace ResgateRS.Extensions;

public static class IServiceCollectionExtensions
{
    public static void RegisterRepositoriesAndServices(this IServiceCollection services)
    {
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => typeof(IRepository).IsAssignableFrom(x) && !x.IsAbstract)
            .ToList()
            .ForEach(x => services.AddTransient(x));

        Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => typeof(IService).IsAssignableFrom(x) && !x.IsAbstract)
                .ToList()
                .ForEach(x => services.AddScoped(x));
    }
}
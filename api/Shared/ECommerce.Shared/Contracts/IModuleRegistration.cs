using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Shared.Contracts;

public interface IModuleRegistration
{
    IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration);
}

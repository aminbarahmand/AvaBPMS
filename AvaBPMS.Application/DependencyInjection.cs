


using AvaBPMS.Application;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(WorkFlowDesignService));

        services.AddScoped(typeof(WorkFlowExecutionService));
        return services;
    }
}


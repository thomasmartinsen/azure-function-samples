using Microsoft.Extensions.DependencyInjection;

namespace Agents;

public static class DependencyInjection
{
    public static IServiceCollection AddAgentSetup<T>(this IServiceCollection services) where T : class, IAgentService
    {
        services.AddSingleton<IAgentHandler, AgentHandler>();
        services.AddSingleton<IAgentService, T>();

        return services;
    }
}

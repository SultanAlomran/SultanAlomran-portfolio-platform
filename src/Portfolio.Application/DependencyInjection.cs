namespace Portfolio.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<Assistant.IPortfolioAssistantService, Assistant.PortfolioAssistantService>();
        services.AddScoped<Assistant.IGuideAiService, Assistant.GuideAiService>();
        services.AddScoped<Assistant.IAssistantTools, Assistant.AssistantTools>();
        return services;
    }
}

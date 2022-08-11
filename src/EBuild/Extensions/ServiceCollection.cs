using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace EBuild.Extensions;

internal static class ServiceCollection
{
    public static void AddImplementation<TService,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
        this IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        services.AddSingleton<TImplementation>();
        services.AddSingleton<TService, TImplementation>(s => s.GetService<TImplementation>());
    }
}
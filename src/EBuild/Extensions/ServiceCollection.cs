using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBuild.Extensions;

internal static class ServiceCollection
{

    public static void AddImplementation<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
    {
        services.AddSingleton<TImplementation>();
        services.AddSingleton<TService, TImplementation>(s => s.GetService<TImplementation>());
    }

}

using System.Reflection;
using FluentValidation;
using LocMp.Identity.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LocMp.Identity.Application.Extensions;

public static class ApplicationExtension
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.Lifetime = ServiceLifetime.Scoped;
            configuration.RegisterServicesFromAssembly(typeof(ApplicationExtension).GetTypeInfo().Assembly);
        });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddAutoMapper(typeof(ApplicationExtension).Assembly);
    }
}
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using N1coLoyalty.Application.Common.Behaviours;
using N1coLoyalty.Application.Common.Services;
using N1coLoyalty.Application.Common.Services.Void;
using N1coLoyalty.Application.Profile.Services;
using N1coLoyalty.Application.Users.Services;
using N1coLoyalty.Domain.Entities;

namespace N1coLoyalty.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });

        services.AddTransient<UserService>();
        services.AddTransient<UserWalletService>();
        services.AddTransient<ProfileService>();
        services.AddTransient<VoidService>();

        return services;
    }
}

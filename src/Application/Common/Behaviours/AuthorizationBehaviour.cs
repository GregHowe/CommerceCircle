using System.Reflection;
using N1coLoyalty.Application.Common.Exceptions;
using N1coLoyalty.Application.Common.Interfaces;
using N1coLoyalty.Application.Common.Security;

namespace N1coLoyalty.Application.Common.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse>(IUser currentUserService)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // when there is no user, we don't need to check permissions
        if (currentUserService.Id is null)
        {
            return await next();
        }

        // apply permissions only to email users
        var isEmail = currentUserService.Id.Contains('@');
        if (!isEmail)
        {
            return await next();
        }

        var authorizeAttribute = request.GetType().GetCustomAttribute<AuthorizeAttribute>();
        if (authorizeAttribute is null) return await next();

        var attributePermissions = authorizeAttribute.Permissions;
        if (IsEmpty(attributePermissions)) return await next();

        var authorizationException = new AuthorizationException("Acceso denegado debido a permisos insuficientes.");
        if (IsEmpty(currentUserService.Permissions)) throw authorizationException;

        // check if any of the user's permissions match any of the attribute permissions
        var authorized = currentUserService.Permissions.Any(p => attributePermissions.Contains(p));
        if (!authorized) throw authorizationException;

        return await next();
    }

    private static bool IsEmpty(string[] strings) => strings is null || strings.Length == 0;
}

using N1coLoyalty.Application.Common.Exceptions;

namespace N1coLoyalty.AdminApi.GraphQL.Filters;

public class CustomErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is AuthorizationException authorizationException)
        {
            return ErrorBuilder.New()
                .SetMessage(authorizationException.Message)
                .SetCode("AUTHORIZATION_ERROR")
                .Build();
        }
        
        return error;
    }
}
namespace N1coLoyalty.Application.Common.Exceptions;

public class AuthorizationException : Exception
{
    public AuthorizationException(string message) : base(message) { }
}
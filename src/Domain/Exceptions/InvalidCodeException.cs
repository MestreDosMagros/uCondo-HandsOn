namespace Domain.Exceptions;

public sealed class InvalidCodeException : DomainException
{
    public InvalidCodeException(string? message) : base(message)
    {
    }
}

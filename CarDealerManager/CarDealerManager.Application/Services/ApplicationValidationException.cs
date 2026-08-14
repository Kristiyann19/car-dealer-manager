namespace CarDealerManager.Application.Services;

public sealed class ApplicationValidationException : Exception
{
    public ApplicationValidationException(string message)
        : base(message)
    {
    }
}

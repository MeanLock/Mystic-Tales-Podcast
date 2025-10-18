namespace SystemConfigurationService.BusinessLogic.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = "Forbidden") : base(message) { }
    }
}

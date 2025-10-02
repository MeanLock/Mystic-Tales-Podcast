namespace UserService.Common.AppConfigurations.App.interfaces
{
    public interface IAppConfig
    {
        int APP_PORT { get; set; }
        string? APP_BASE_URL { get; set; }
        string? API_GATEWAY_URL { get; set; }
        string? HEALTH_CHECK_ENDPOINT { get; set; }
        ResetPasswordConfig? RESET_PASSWORD { get; set; }
        string? FILE_STORAGE_SRC { get; set; }
        string? TIME_ZONE { get; set; }
        string? EMBEDDING_VECTOR_API_URL { get; set; }
    }
}

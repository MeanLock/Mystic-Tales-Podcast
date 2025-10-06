namespace SagaOrchestratorService.Common.AppConfigurations.App.interfaces
{
    public interface IAppConfig
    {
        int APP_PORT { get; set; }
        string APP_BASE_URL { get; set; }
        string HEALTH_CHECK_ENDPOINT { get; set; }
        string IMAGE_SRC { get; set; }
        string TIME_ZONE { get; set; }
    }
}

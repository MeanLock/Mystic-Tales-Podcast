namespace SagaOrchestratorService.Common.AppConfigurations.Bcrypt.interfaces
{
    public interface IBcryptConfig
    {
        int SALT_ROUNDS { get; set; }

    }
}

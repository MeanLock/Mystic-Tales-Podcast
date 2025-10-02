using SagaOrchestratorService.DataAccess.Repositories;
using SagaOrchestratorService.DataAccess.Repositories.interfaces;

namespace SagaOrchestratorService.DataAccess.UOW;
public interface IUnitOfWork
{
    int Complete();
}

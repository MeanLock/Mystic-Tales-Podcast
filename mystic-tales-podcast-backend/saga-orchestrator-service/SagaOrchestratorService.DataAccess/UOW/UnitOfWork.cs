using Google;
using SagaOrchestratorService.DataAccess.Data;
using SagaOrchestratorService.DataAccess.Repositories.interfaces;

namespace SagaOrchestratorService.DataAccess.UOW;
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(
        AppDbContext dbContext
        )
    {
        _dbContext = dbContext;
    }

    public int Complete()
    {
        return _dbContext.SaveChanges();
    }
}

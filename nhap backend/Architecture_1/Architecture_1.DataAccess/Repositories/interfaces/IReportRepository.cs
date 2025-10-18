using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.DataAccess.Repositories.interfaces
{
    public interface IReportRepository
    {
        Task<IEnumerable<Report>> FindByMajorId(int majorId);
        Task<Report> FindByIdAsync(int id);
        Task<IEnumerable<Report>> FindAllAsync();
        Task<bool> Resolve(int id, bool resolve);

    }
}

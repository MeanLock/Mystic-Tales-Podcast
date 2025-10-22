using Architecture_1.DataAccess.Data;
using Architecture_1.DataAccess.Entities;

namespace Architecture_1.DataAccess.Repositories.interfaces
{
    public interface IFeedbackRepository
    {
        Task<IEnumerable<Feedback>> FindByMajorId(int majorId);
        Task<IEnumerable<Feedback>> FindAllAsync();
        Task<bool> Deactivate(int id, bool deactivate);

    }
}

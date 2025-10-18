using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Entities;

namespace BookingManagementService.DataAccess.Repositories.interfaces
{
    public interface IFilterTagRepository
    {
        Task<IEnumerable<FilterTag>> FindByTagTypeIdAsync(int tagTypeId);
        
    }
}

using UserService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using UserService.DataAccess.Data;
using System.Linq.Expressions;

namespace UserService.BusinessLogic.Models.CrossService
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}
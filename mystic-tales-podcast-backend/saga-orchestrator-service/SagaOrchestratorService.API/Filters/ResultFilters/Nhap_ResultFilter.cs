using Microsoft.AspNetCore.Mvc.Filters;

namespace SagaOrchestratorService.API.Filters.ResultFilters
{
    public class Nhap_ResultFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            Console.WriteLine("[BEGIN RESULT] ==> <OnResultExecuting>");
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            Console.WriteLine("[BEGIN RESULT] ==> <OnResultExecuted>");
        }
    }
}



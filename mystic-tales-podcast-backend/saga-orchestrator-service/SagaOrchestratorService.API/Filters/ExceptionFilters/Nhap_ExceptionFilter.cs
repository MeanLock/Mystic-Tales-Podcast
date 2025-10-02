using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SagaOrchestratorService.API.Filters.ExceptionFilters
{
    public class Nhap_ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            Console.WriteLine("[BEGIN EXCEPTION] ==> <OnException>");
            context.Result = new ObjectResult(new { message = "An error occurred" }) { StatusCode = 500 };
        }
    }
}



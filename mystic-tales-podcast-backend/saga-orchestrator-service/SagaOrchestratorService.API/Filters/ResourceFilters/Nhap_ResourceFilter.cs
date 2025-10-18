using Microsoft.AspNetCore.Mvc.Filters;

namespace SagaOrchestratorService.API.Filters.ResourceFilters
{
    public class Nhap_ResourceFilter : IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            Console.WriteLine("[BEGIN RESOURCE] ==> <OnResourceExecuting>");
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            Console.WriteLine("[END RESOURCE] ==> <OnResourceExecuted>");
        }
    }
}


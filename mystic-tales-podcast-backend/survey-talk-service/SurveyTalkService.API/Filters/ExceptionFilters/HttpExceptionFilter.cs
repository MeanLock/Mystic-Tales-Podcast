using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SurveyTalkService.BusinessLogic.Exceptions;
using System;
using System.Net;


namespace SurveyTalkService.API.Filters.ExceptionFilters
{
    public class HttpExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            Console.WriteLine("Context Exception: "+ context.Exception);

            int status = (int)HttpStatusCode.InternalServerError;
            // string message = "Lỗi hệ thống!";
            string message = "Something went wrong!";

            if (context.Exception is HttpRequestException httpRequestException)
            {
                status = (int)HttpStatusCode.BadRequest; // Hoặc status khác tùy vào lỗi
                message = httpRequestException.Message;
            }
            else if (context.Exception is ForbiddenException)
            {
                // context.Result = new ObjectResult(context.Exception.Message)
                // {
                //     StatusCode = StatusCodes.Status403Forbidden
                // };
                // context.ExceptionHandled = true;
                status = (int)HttpStatusCode.Forbidden;
                message = context.Exception.Message;
            }


            context.Result = new ObjectResult(new
            {
                statusCode = status,
                message = message,
                timestamp = DateTime.UtcNow.ToString("o")
            })
            {
                StatusCode = status
            };

            context.ExceptionHandled = true;
        }
    }
}

using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using FluentEmail.Core;
using UnDotNet.BootstrapEmail;

namespace UserService.Infrastructure.Services.Google.Email
{
    public class FluentEmailService
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFluentEmail _fluentEmail;



        public FluentEmailService(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider,
            IFluentEmail fluentEmail
            )
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _fluentEmail = fluentEmail;
        }

        public async Task SendEmail(string to, object model, string templateFilePath, string subject)
        {

            try
            {
                string body = await RenderViewToStringAsync(templateFilePath, model);

                var compiler = new BootstrapCompiler(body);
                var result = compiler.Multipart();

                await _fluentEmail.To(to)
                    .Subject(subject)
                    .Body(body, isHtml: true)
                    .SendAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw new Exception("Gửi mail thất bại", ex);
            }

        }

        public async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            var actionContext = new ActionContext(
                new DefaultHttpContext { RequestServices = _serviceProvider },
                new RouteData(),
                new ActionDescriptor()
            );

            using var sw = new StringWriter();
            var viewResult = _viewEngine.FindView(actionContext, viewName, false);

            if (!viewResult.Success)
                throw new FileNotFoundException($"View {viewName} not found.");

            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                tempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
        
        public bool IsValid()
        {
            return _fluentEmail != null && _viewEngine != null && _serviceProvider != null && _tempDataProvider != null;
        }
    }
}

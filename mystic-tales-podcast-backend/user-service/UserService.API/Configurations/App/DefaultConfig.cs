using UserService.Common.AppConfigurations.Media;

namespace UserService.API.Configurations.App
{
    public static class DefaultConfig
    {
        public static void AppAppDefaultConfig(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                //app.UseSwagger();
                //app.UseSwaggerUI();
                //app.UseDeveloperExceptionPage();
            }


            // app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = MediaTypeConfig.GetFileExtensionContentTypeProvider()
            });
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
        }

    }
}

namespace ApiGatewayService.Entry.Configurations.App
{
    public static class CorsConfig
    {
        public static void AddAppCorsConfig(this WebApplication app)
        {
            app.UseCors("AllowAllOrigins");
        }

    }
}

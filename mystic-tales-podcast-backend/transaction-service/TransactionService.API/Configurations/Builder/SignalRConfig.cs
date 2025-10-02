

namespace TransactionService.API.Configurations.Builder
{
    public static class SignalRConfig
    {

        public static void AddBuilderSignalRConfig(this WebApplicationBuilder builder)
        {
            builder.Services.AddSignalR(o =>
        {
            o.EnableDetailedErrors = true;
            o.MaximumReceiveMessageSize = 10485760; // bytes
        });


        }


    }
}

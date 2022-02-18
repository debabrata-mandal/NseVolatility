using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace NseVolatility
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging);
            return host;
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            //Add workers
            services.AddHostedService<Worker>();
        }

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging)
        {
            foreach (ServiceDescriptor serviceDescriptor in logging.Services)
            {
                if (serviceDescriptor.ImplementationType == typeof(ConsoleLoggerProvider))
                {
                    // remove ConsoleLoggerProvider service only
                    logging.Services.Remove(serviceDescriptor);
                    break;
                }
            }
        }
    }
}

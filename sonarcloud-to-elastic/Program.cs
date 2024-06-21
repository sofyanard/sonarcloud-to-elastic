namespace sonarcloud_to_elastic
{
    using Microsoft.Extensions.Logging;

    internal class Program
    {
        static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            /*
            // create a logger factory
            var loggerFactory = LoggerFactory.Create(
                    builder => builder
                            // add console as logging target
                            .AddConsole()
                            // add debug output as logging target
                            .AddDebug()
                            // set minimum level to log
                            .SetMinimumLevel(LogLevel.Debug)
            );

            // create a logger
            var logger = loggerFactory.CreateLogger<Program>();

            // logging
            logger.LogTrace("Trace message");
            logger.LogDebug("Debug message");
            logger.LogInformation("Info message");
            logger.LogWarning("Warning message");
            logger.LogError("Error message");
            logger.LogCritical("Critical message");
            */

            // https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

            using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger logger = factory.CreateLogger("Program");
            logger.LogInformation("Hello World! Logging is {Description}.", "fun");

            // logging
            logger.LogTrace("Trace message");
            logger.LogDebug("Debug message");
            logger.LogInformation("Info message");
            logger.LogWarning("Warning message");
            logger.LogError("Error message");
            logger.LogCritical("Critical message");



            // https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient

            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                using HttpResponseMessage response = await client.GetAsync("http://www.contoso.com/");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            Console.WriteLine("Hello, World!");
        }
    }
}

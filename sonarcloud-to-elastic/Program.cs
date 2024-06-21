namespace sonarcloud_to_elastic
{
    using Microsoft.Extensions.Logging;

    internal class Program
    {
        static void Main(string[] args)
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

            using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger logger = factory.CreateLogger("Program");
            logger.LogInformation("Hello World! Logging is {Description}.", "fun");

            Console.WriteLine("Hello, World!");
        }
    }
}

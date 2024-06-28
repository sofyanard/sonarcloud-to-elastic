namespace sonarcloud_to_elastic
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using System.Text;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Serilog;
    using Serilog.Extensions.Logging;
    using Serilog.Sinks.Elasticsearch;

    internal class Program
    {
        static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            string elasticPassword = Environment.GetEnvironmentVariable("ELASTIC_TOKEN");
            string elasticUrl = config["Elastic:Url"];
            string elasticUser = config["Elastic:User"];
            string elasticIndex = config["Elastic:Index"];

            // Set up Serilog
            Serilog.Log.Logger = new LoggerConfiguration()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUrl))
                {
                    IndexFormat = "sonarcloud-to-elastic",
                    ModifyConnectionSettings = conn =>
                        conn.BasicAuthentication(elasticUser, elasticPassword)
                })
                .CreateLogger();

            using ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog();
                builder.AddConsole();
            });
            Microsoft.Extensions.Logging.ILogger logger = factory.CreateLogger("Program");
            logger.LogInformation("Hello World! Logging is {Description}.", "fun");

            // logging
            logger.LogTrace("Trace message");
            logger.LogDebug("Debug message");
            logger.LogInformation("Info message");
            logger.LogWarning("Warning message");
            logger.LogError("Error message");
            logger.LogCritical("Critical message");

            try
            {
                foreach (SoftwareQuality softwareQuality in Enum.GetValues(typeof(SoftwareQuality)))
                {
                    foreach (Severity severity in Enum.GetValues(typeof(Severity)))
                    {
                        string strQ = softwareQuality.ToString();
                        string strS = severity.ToString();

                        int iPage = 1;
                        logger.LogInformation($"Software Quality: {softwareQuality}, Severity: {severity}, Page: {iPage}");
                        string sonarResponse = await SonarCloudProcess.GetIssues(strQ, strS, 500, iPage);

                        // Parsing Sonar Response...
                        JObject jsonObject = JObject.Parse(sonarResponse);
                        JArray arrayOfIssue = (JArray)jsonObject["issues"];
                        int totalIssues = (int)jsonObject["total"];
                        logger.LogInformation($"Total Issues: {totalIssues}");

                        // Here split 2 conditions,
                        // if totalIssues >= 10000, break to specific rule issues,
                        // else, continue to get all issues
                        if (totalIssues >= 10000)
                        {
                            List<Facet> listFacet = await SonarCloudProcess.GetFacets(strQ, strS);
                            foreach (Facet facet in listFacet)
                            {
                                iPage = 1;
                                logger.LogInformation($"Software Quality: {softwareQuality}, Severity: {severity}, Rule: {facet.Val}, Page: {iPage}");
                                sonarResponse = await SonarCloudProcess.GetSpecificRuleIssues(strQ, strS, facet.Val, 500, iPage);

                                // Parsing Sonar Response...
                                jsonObject = JObject.Parse(sonarResponse);
                                arrayOfIssue = (JArray)jsonObject["issues"];
                                totalIssues = (int)jsonObject["total"];
                                logger.LogInformation($"Total Issues: {totalIssues}");

                                // Loop for each issue
                                if (arrayOfIssue.Count > 0)
                                {
                                    foreach (var issue in arrayOfIssue)
                                    {
                                        string elasticResponse = await ElasticProcess.PostIssue(issue.ToString());
                                    }
                                }

                                int remainingIssues = totalIssues;
                                while (remainingIssues > 500)
                                {
                                    remainingIssues -= 500;
                                    iPage++;
                                    logger.LogInformation($"Software Quality: {softwareQuality}, Severity: {severity}, Rule: {facet.Val}, Page: {iPage}");
                                    sonarResponse = await SonarCloudProcess.GetSpecificRuleIssues(strQ, strS, facet.Val, 500, iPage);

                                    // Parsing Sonar Response...
                                    jsonObject = JObject.Parse(sonarResponse);
                                    arrayOfIssue = (JArray)jsonObject["issues"];
                                    totalIssues = (int)jsonObject["total"];
                                    logger.LogInformation($"Total Issues: {totalIssues}");

                                    // Loop for each issue
                                    if (arrayOfIssue.Count > 0)
                                    {
                                        foreach (var issue in arrayOfIssue)
                                        {
                                            string elasticResponse = await ElasticProcess.PostIssue(issue.ToString());
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Loop for each issue
                            if (arrayOfIssue.Count > 0)
                            {
                                foreach (var issue in arrayOfIssue)
                                {
                                    string elasticResponse = await ElasticProcess.PostIssue(issue.ToString());
                                }
                            }

                            int remainingIssues = totalIssues;
                            while (remainingIssues > 500)
                            {
                                remainingIssues -= 500;
                                iPage++;
                                logger.LogInformation($"Software Quality: {softwareQuality}, Severity: {severity}, Page: {iPage}");
                                sonarResponse = await SonarCloudProcess.GetIssues(strQ, strS, 500, iPage);

                                // Parsing Sonar Response...
                                jsonObject = JObject.Parse(sonarResponse);
                                arrayOfIssue = (JArray)jsonObject["issues"];
                                totalIssues = (int)jsonObject["total"];
                                logger.LogInformation($"Total Issues: {totalIssues}");

                                // Loop for each issue
                                if (arrayOfIssue.Count > 0)
                                {
                                    foreach (var issue in arrayOfIssue)
                                    {
                                        string elasticResponse = await ElasticProcess.PostIssue(issue.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError("Exception Caught!");
                logger.LogError($"Message: {e.Message} ", e);
            }

            Serilog.Log.CloseAndFlush();
            Console.WriteLine("Hello, World!");
        }

        enum SoftwareQuality
        {
            SECURITY,
            RELIABILITY,
            MAINTAINABILITY
        }

        enum Severity
        {
            HIGH,
            MEDIUM,
            LOW
        }
    }
}

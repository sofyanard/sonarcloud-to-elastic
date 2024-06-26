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
    

    internal class Program
    {
        static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
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
            catch (HttpRequestException e)
            {
                logger.LogError("Exception Caught!");
                logger.LogError($"Message :{e.Message} ", e.Message);
            }

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

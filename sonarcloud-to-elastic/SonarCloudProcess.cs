using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace sonarcloud_to_elastic
{
    static class SonarCloudProcess
    {
        static readonly HttpClient client = new HttpClient();

        static ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        static ILogger logger = factory.CreateLogger("SonarCloudProcess");

        static IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

        static string sonarCloudToken = Environment.GetEnvironmentVariable("SONARCLOUD_TOKEN");
        static string sonarUrl = config["SonarCloud:Url"];
        static string sonarComponentKeys = config["SonarCloud:ComponentKeys"];

        public static async Task<string> GetIssues(string softwareQualities, string severities, int pageSize=500, int page=1)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sonarCloudToken);
            Uri requestUri = new Uri(sonarUrl + "/issues/search?" + "componentKeys=" + sonarComponentKeys + "&impactSoftwareQualities=" + softwareQualities + "&impactSeverities=" + severities + "&ps=" + pageSize + "&p=" + page);
            try
            {
                logger.LogInformation($"Requesting to SonarCloud API: {requestUri}...");
                using HttpResponseMessage response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Request is succeed, getting the response...");
                string responseBody = await response.Content.ReadAsStringAsync();
                string pattern = $@"^(.{{0,{100}}}).*";
                string truncated = Regex.Replace(responseBody, pattern, "$1");
                logger.LogInformation($"Response: {truncated}...");

                return responseBody;
            }
            catch (Exception e)
            {
                logger.LogError("Request Error: {0}", e.Message);
                throw;
            }
        }

        public static async Task<List<Facet>> GetFacets(string softwareQualities, string severities)
        {
            logger.LogInformation($"Getting Facets ({softwareQualities} - {severities})");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sonarCloudToken);
            Uri requestUri = new Uri(sonarUrl + "/issues/search?" + "componentKeys=" + sonarComponentKeys + "&impactSoftwareQualities=" + softwareQualities + "&impactSeverities=" + severities + "&ps=1&p=1&facets=rules");
            try
            {
                logger.LogInformation($"Requesting to SonarCloud API: {requestUri}...");
                using HttpResponseMessage response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Request is succeed, getting the response...");
                string responseBody = await response.Content.ReadAsStringAsync();
                string pattern = $@"^(.{{0,{100}}}).*";
                string truncated = Regex.Replace(responseBody, pattern, "$1");
                logger.LogInformation($"Response: {truncated}...");

                JObject jResponse = JObject.Parse(responseBody);
                JArray jArrayFacet = (JArray)jResponse["facets"];
                JObject jFirstFacet = (JObject)jArrayFacet[0];
                JArray jArrayValue = (JArray)jFirstFacet["values"];
                List<Facet> listFacet = jArrayValue.ToObject<List<Facet>>();
                foreach (Facet facet in listFacet)
                {
                    logger.LogInformation($"Facet: {facet.Val} - {facet.Count}");
                }

                return listFacet;
            }
            catch (Exception e)
            {
                logger.LogError($"Error: {e.Message}", e);
                throw;
            }
        }

        public static async Task<string> GetSpecificRuleIssues(string softwareQualities, string severities, string rule, int pageSize=500, int page=1)
        {
            logger.LogInformation($"Getting Issues on Rule ({rule})");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sonarCloudToken);
            Uri requestUri = new Uri(sonarUrl + "/issues/search?" + "componentKeys=" + sonarComponentKeys + "&impactSoftwareQualities=" + softwareQualities + "&impactSeverities=" + severities + "&ps=" + pageSize + "&p=" + page + "&rules=" + rule);
            try
            {
                logger.LogInformation($"Requesting to SonarCloud API: {requestUri}...");
                using HttpResponseMessage response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Request is succeed, getting the response...");
                string responseBody = await response.Content.ReadAsStringAsync();
                string pattern = $@"^(.{{0,{100}}}).*";
                string truncated = Regex.Replace(responseBody, pattern, "$1");
                logger.LogInformation($"Response: {truncated}...");

                return responseBody;
            }
            catch (Exception e)
            {
                logger.LogError("Request Error: {0}", e.Message);
                throw;
            }
        }
    }
}

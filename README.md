### sonarcloud-to-elastic
<p>we grab <a href=https://sonarcloud.io">sonarcloud.io</a> issues via <a href="https://sonarcloud.io/web_api">web-api</a> and insert those to elastic</p>

we will inquiry per ```impact.softwareQualities```:```(SECURITY/RELIABILITY/MAINTAINABILITY)```
and per ```impact.severity```:```(HIGH/MEDIUM/LOW)```

sonarcloud.io web-api limit max page size = 500 issues, and max total issue = 10.000

### sonarcloud.io configuration:
> ```
> "SonarCloud": {
>  "Url": "https://sonarcloud.io/api",
>  "ComponentKeys": "rh-sakti_sakti-fuse-main"
> }
> ```
we put sonarcloud web api base url and project name in the ```appsettings.json``` file
> ```
> string sonarCloudToken = Environment.GetEnvironmentVariable("SONARCLOUD_TOKEN");
> ```
and also auth token in the environment variables
> ```
> {sonarUrl} + "/issues/search?" + "componentKeys=" + {sonarComponentKey} + "&impactSoftwareQualities=" + {softwareQuality} + "&impactSeverities=" + {severity} + "&ps=" + {pageSize} + "&p=" + {page}
> ```
we use ```/issues/search``` endpoint with ```componentKeys```, ```impactSoftwareQualities```, ```impactSeverities```, ```ps```, and ```p``` parameters

### elastic configuration:
> ```
> "Elastic": {
>  "Url": "http://localhost:9200",
>  "User": "elastic",
>  "Index": "issues"
> }
> ```
we put elastic base url, elastic username, and index name in the ```appsettings.json``` file
> ```
> string elasticPassword = Environment.GetEnvironmentVariable("ELASTIC_TOKEN");
> ```
and also password in the environment variables

> ```
> foreach (SoftwareQuality softwareQuality in Enum.GetValues(typeof(SoftwareQuality)))
> {
>     foreach (Severity severity in Enum.GetValues(typeof(Severity)))
>     {
> ```
then we make loop per SoftwareQuality and per Severity,<br>
we make request to sonarcloud.io (with max 500 issues per page, and then we repeat the request for the next max 500 issues if remains)<br>
with a maximum result of 500 issues, we break down to each single issue, and insert that issue to elastic<br>
(we will repeat next max 500 issues in the next page request if issues are remains)<br>

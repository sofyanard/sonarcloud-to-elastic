### sonarcloud-to-elastic
<p>we grab <a href=https://sonarcloud.io">sonarcloud.io</a> issues via <a href="https://sonarcloud.io/web_api">web-api</a> and insert those to elastic</p>

we will inquiry per ```impact.softwareQualities```:```(SECURITY/RELIABILITY/MAINTAINABILITY)```
and per ```impact.severity```:```(HIGH/MEDIUM/LOW)```

sonarcloud.io web-api limit max page size = 500 issues, and max total issue = 10.000

___
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

___
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

___
### program overview:
> ```
> foreach (SoftwareQuality softwareQuality in Enum.GetValues(typeof(SoftwareQuality)))
> {
>     foreach (Severity severity in Enum.GetValues(typeof(Severity)))
>     {
> ```
we make loop per SoftwareQuality and per Severity,<br>
we make request to sonarcloud.io (with max 500 issues per page, and then we repeat the request for the next page if issues remains)<br>

> ```
> {sonarUrl} + "/issues/search?" + "componentKeys=" + {sonarComponentKey} + "&impactSoftwareQualities=" + {softwareQuality} + "&impactSeverities=" + {severity} + "&ps=" + {pageSize} + "&p=" + {page}
> ```
we use ```/issues/search``` endpoint with ```componentKeys```, ```impactSoftwareQualities```, ```impactSeverities```, ```ps```, and ```p``` parameters

> ```
> {
>	"total": 13,
>	"p": 1,
>	"ps": 100,
>	"paging": {
>		"pageIndex": 1,
>		"pageSize": 100,
>		"total": 13
>	},
>	"issues": [
>		{
>			"key": "AY_y2Cupg5EIr5cf4Ktf",
>			"rule": "secrets:S6702",
>			"severity": "BLOCKER",
>			"component": "rh-sakti_sakti-fuse-main:pom.xml",
>			"project": "rh-sakti_sakti-fuse-main",
>			"hash": "4c0b2a2d3c69d88eb5721ce584303c3d",
>			"impacts": [
>				{
>					"softwareQuality": "SECURITY",
>					"severity": "HIGH"
>				}
>			]
>		},
>		{
>			"key": "AY_y2CH0g5EIr5cf4Idb",
>			.....
> ```
we will get json like this
with a maximum result of 500 issues, we break down to each single issue, and insert that issue to elastic<br>
(we will repeat next max 500 issues in the next page request if issues are remains)<br>

___
### handling over 10000 issues:
if ```total``` shows the number 10000, means it reached the total issue limit within the inquiry condition
> ```
> if (totalIssues >= 10000)
> {
>     // adding loop for each rule
> }
> else
> {
>     // leave as previously described
> }
> ```
we break down each SoftwareQuality and Severity condition with additional filter: ```rule```<br>
so, we make additional loop for each ```rule``` within the SoftwareQuality and Severity loop

> ```
> {sonarUrl} + "/issues/search?" + "componentKeys=" + {sonarComponentKey} + "&impactSoftwareQualities=" + {softwareQuality} + "&impactSeverities=" + {severity} + "&ps=1&p=1&facets=rules"
> ```
we can get the list of ```rules``` applied in the previous SoftwareQuality and Severity inquiry with add ```facets=rules``` to the parameters

> ```
> "facets": [
>         {
>             "property": "rules",
>             "values": [
>                 {
>                     "val": "external_spotbugs:EI_EXPOSE_REP2",
>                     "count": 4479
>                 },
>                 {
>                     "val": "java:S3740",
>                     "count": 4409
>                 },
>                 {
>                     "val": "external_spotbugs:EI_EXPOSE_REP",
>                     "count": 4397
>                 }, .....
> ```
and within the json we get facets object like this

> ```
> {sonarUrl} + "/issues/search?" + "componentKeys=" + {sonarComponentKey} + "&impactSoftwareQualities=" + {softwareQuality} + "&impactSeverities=" + {severity} + "&ps=" + {pageSize} + "&p=" + {page} + "&rules=" + rule
> ```
then we create additional ```rules``` loop whithin the previous SoftwareQuality and Severity loop<br>
and also insert each single issue into elastic the way like previously described

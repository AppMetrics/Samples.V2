# App Metrics 2.0 Samples
Sample applications demonstrating [App.Metrics](http://app-metrics.io) features.

## ASP.NET Core 2.0

### Solutions

#### `AspNetCore2.Api.sln`

`AspNetCore2.Api.QuickStart.csproj`: An ASP.NET Core 2.0 Api with App Metrics 2.0 basics configured. This project works with the web monitoring dashboards which can be [imported from Grafana Labs](https://grafana.com/dashboards?search=app%20metrics). Enable the following compiler directives to highlight example configuration options: 

- `HOSTING_OPTIONS`: Custom hosting configuration which sets a custom port and endpoint for each of the endpoints added by App Metrics. 
- `REPORTING`: Schedules metric reporting via console  


### Documentation

Also see the [ASP.NET Core 2.0 documentation](http://app-metrics.io/web-monitoring/aspnet-core/) for more details.



Microsoft Azure WebJobs SDK Table Sample
-----------------------------------

This sample demonstrates miscellaneous operations that can be performed with the Microsoft WebJobs SDK.
Includes:
- Cancellation tokens
- Graceful shutdown using the shutdown monitor
- Configurable queue names (custom name resolver)

In order to run the sample, you need to set the Microsoft Azure Storage connection strings in the
App.config file or environment variables.
Example (app.config):
  <add name="AzureWebJobsDashboard" connectionString="DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY" />
  <add name="AzureWebJobsStorage" connectionString="DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY" />
Example (environment variables):
  SET AzureWebJobsDashboard=DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY
  SET AzureWebJobsStorage=DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY

For more information about the WebJobs feature of Microsoft Azure Web Sites, 
see http://go.microsoft.com/fwlink/?LinkId=390226
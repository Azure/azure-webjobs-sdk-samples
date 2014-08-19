Microsoft Azure WebJobs SDK Table Sample
-----------------------------------

This sample demonstrates blob bindings and operations with Microsoft Azure WebJobs SDK. The functions
are triggered when a new blob is created in the "input" container of the Azure Storage Account or
when a new queue message is added to the "persons" queue.

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
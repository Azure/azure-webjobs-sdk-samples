Microsoft Azure WebJobs SDK ServiceBus Sample
-----------------------------------

This sample demonstrates how Microsoft Azure WebJobs SDK can be used to listen to ServiceBus queues/subscriptions and 
how to push messages into queues/topic

In order to run the sample, you need to set the Microsoft Azure Storage connection strings in the
App.config file or environment variables.
Example (app.config):
  <add name="AzureJobsStorage" connectionString="DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY" />
  <add name="AzureJobsDashboard" connectionString="DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY" />
  <add name="ServiceBus" connectionString="<ServiceBusConnectionString>" />
Example (environment variables):
  SET AzureJobsStorage=DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY
  SET AzureJobsDashboard=DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY
  The ServiceBus connection string cannot be set through an environment variable

For more information about the WebJobs feature of Microsoft Azure Web Sites, 
see http://go.microsoft.com/fwlink/?LinkId=390226
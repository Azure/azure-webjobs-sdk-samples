Windows Azure WebJobs SDK Samples
-----------------------------------

These samples demonstrate the most basic operations you can do with the SDK. Following are some of the examples
- How to bind to Windows Azure Tables
- How to use binding and triggers on Windows Azure Blobs
- How to use binding and triggers on Windows Azure Queues

-----------------------------------
In order to run the sample, you need to set the Windows Azure Storage connection strings in the
App.config file or environment variables.
Example (app.config):
  <add name="AzureJobsRuntime" connectionString="DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY" />
  <add name="AzureJobsData" connectionString="DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY" />
Example (environment variables):
  SET AzureJobsRuntime=DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY
  SET AzureJobsData=DefaultEndpointsProtocol=https;AccountName=NAME;AccountKey=KEY

For more information about the WebJobs feature of Windows Azure Web Sites, 
see http://go.microsoft.com/fwlink/?LinkId=390226


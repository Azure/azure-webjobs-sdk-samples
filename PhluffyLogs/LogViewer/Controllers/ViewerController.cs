using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Common;
using LogViewer.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace LogViewer.Controllers
{
    public class ViewerController : Controller
    {
        private readonly CloudStorageAccount _storageAccount = StorageAccount.Create();

        [HttpGet]
        public ActionResult Index()
        {
            var tableClient = _storageAccount.CreateCloudTableClient();
            var indicesTable = tableClient.GetTableReference(StorageNames.BenchmarksTable);

            IEnumerable<TableEntity> benchmarks;

            if (indicesTable.Exists())
            {
                var query = new TableQuery<TableEntity>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "index"));

                benchmarks = indicesTable.ExecuteQuery(query);
            }
            else
            {
                benchmarks = new TableEntity[0];
            }


            ViewerModel model = new ViewerModel()
            {
                Benchmarks = benchmarks.Select(r => r.RowKey)
            };

            return View(model);
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;

namespace Company.Function
{

    public static class PageCounter01
    {
        const string tableName = "viewcountertable";

        //public static string GetConnectionString(string name)
        //{
        //    string conStr = System.Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", 
        //                                                            EnvironmentVariableTarget.Process);
        //    if (string.IsNullOrEmpty(conStr)) 
        //        conStr = System.Environment.GetEnvironmentVariable($"SQLCONNSTR_{name}", EnvironmentVariableTarget.Process);
        //    return conStr;
        //}

        [FunctionName("PageCounter01")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //var storageAccountConnectionString = GetConnectionString("StorageConnectionString");
            var storageAccountConnectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            var storageAccount = CloudStorageAccount.Parse($"{storageAccountConnectionString}");
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);

            await table.CreateIfNotExistsAsync(); // we can let our code create the table if needed

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string pageViewURL = data?.Key;
            //pageViewURL = "LANDER2";

            string name = req.Query["Key"];
            if (pageViewURL == null && String.IsNullOrEmpty(name))
            {
                return (ActionResult)new StatusCodeResult(503);
            }
            pageViewURL = name;

            var retrievedResult = table.Execute(TableOperation.Retrieve<ViewCount>(pageViewURL, "visits"));
            var pageView = (ViewCount)retrievedResult.Result;

            pageView = pageView ?? new ViewCount(pageViewURL);

            pageView.Count++;

            table.Execute(TableOperation.InsertOrReplace(pageView));
            return (ActionResult)new OkObjectResult(pageView.Count.ToString());
        }
    }
}

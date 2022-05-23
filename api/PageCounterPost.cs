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
using Company.Function;

namespace api
{
    public static class PageCounterPost
    {
        const string tableName = "viewcountertable";

        [FunctionName("PageCounterPost")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "PageCounter")] HttpRequest request,
            ILogger log)
        {
            var storageAccountConnectionString = Environment.GetEnvironmentVariable("StorageConnectionString");

            var storageAccount = CloudStorageAccount.Parse($"{storageAccountConnectionString}");
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName);

            var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string pageViewURL = data?.Key;

            var retrievedResult = table.Execute(TableOperation.Retrieve<ViewCount>(pageViewURL, "visits"));
            var pageView = (ViewCount)retrievedResult.Result;

            pageView = pageView ?? new ViewCount(pageViewURL);

            pageView.Count++;

            table.Execute(TableOperation.InsertOrReplace(pageView));

            return new OkObjectResult(pageView);
        }
    }
}

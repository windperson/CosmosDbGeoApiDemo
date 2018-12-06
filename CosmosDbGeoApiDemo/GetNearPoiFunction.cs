using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using CosmosDbGeoApiDemo.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents.Spatial;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CosmosDbGeoApiDemo
{
    public static class GetNearPoiFunction
    {
        private const string CosmosDBname = @"base-points";
        private const string CosmosDBcollectionName = @"base-poi";
        
        [FunctionName("GetNearPOI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName : CosmosDBname,
                collectionName : CosmosDBcollectionName,
                ConnectionStringSetting = "CosmosDBconn")] DocumentClient client,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var input = JsonConvert.DeserializeObject<InputDto>(requestBody);

                Point inputPoint = new Point(new Position(input.Location.Longitude, input.Location.Latitude, input.Location.Altitude));
                double withInDistance = input.Distance;

                log.LogInformation($"location = {JsonConvert.SerializeObject(inputPoint, Formatting.Indented)}, withInDistance={withInDistance}");

                var collectionUri = UriFactory.CreateDocumentCollectionUri(CosmosDBname, CosmosDBcollectionName);

                var query = client.CreateDocumentQuery<PoiDto>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                    .Where(p => p.Location.Distance(inputPoint) <= withInDistance)
                    .Select(o => new PoiDto
                    {
                        Id = o.Id,
                        PoiId = o.PoiId,
                        Location = o.Location,
                        Distance = o.Location.Distance(inputPoint),
                        Name = o.Name
                    }).AsDocumentQuery();

                var ret = new List<PoiDto>();

                while (query.HasMoreResults)
                {
                    foreach (var poi in await query.ExecuteNextAsync<PoiDto>())
                    {
                        ret.Add(poi);
                    }
                    log.LogInformation("Doing another fetch");
                }
                log.LogInformation($"get {ret.Count} result(s).");
                return new OkObjectResult(ret.OrderBy(r=>r.Distance).ToArray());
            }
            catch (Exception ex)
            {
                log.LogError(ex,"runtime error");
                throw;
            }
        }
    }
}

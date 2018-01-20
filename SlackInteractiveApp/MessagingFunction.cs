using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SlackInteractiveApp
{
    public static class MessagingFunction
    {
        [FunctionName("MessagingFunction")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "")]HttpRequestMessage req, TraceWriter log)
        {
            string jsonContent = await req.Content.ReadAsStringAsync();
            var payload = PayloadHelper.GetPayload(WebUtility.UrlDecode(jsonContent));
            if (string.IsNullOrWhiteSpace(payload))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, new { error = "Invalid payload format" });
            }

            var payloadObject = JObject.Parse(payload);
            var selectedValue = payloadObject["actions"][0].Value<bool>("value");
            var userName = payloadObject.SelectToken("user.name").Value<string>();

            RedisCacheProvider cacheProvider = new RedisCacheProvider();
            var currentLunch = cacheProvider.Get<IList<LunchMember>>("lunch", () => { return new List<LunchMember>(); });
            currentLunch.Add(new LunchMember { Name = userName, IsAccepted = selectedValue });
            cacheProvider.Set<IList<LunchMember>>("lunch", currentLunch);

            return req.CreateResponse(HttpStatusCode.OK, new
            {
                text = selectedValue ? ":yum: *Aðzýnýn tadýný biliyorsun!*" : ":weary: *E ne yapalým, bir sonraki sefere artýk!*"
            });
        }
    }
}

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MailCheck.Spf.Contracts.SharedDomain.Serialization
{
    public class TermConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer){}

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            switch (jo["termType"].Value<string>().ToLower())
            {
                case "a":
                    return JsonConvert.DeserializeObject<A>(jo.ToString());
                case "all":
                    return JsonConvert.DeserializeObject<All>(jo.ToString());
                case "exists":
                    return JsonConvert.DeserializeObject<Exists>(jo.ToString());
                case "explanation":
                    return JsonConvert.DeserializeObject<Explanation>(jo.ToString());
                case "include":
                    return JsonConvert.DeserializeObject<Include>(jo.ToString());
                case "ip4":
                    return JsonConvert.DeserializeObject<Ip4>(jo.ToString());
                case "ip6":
                    return JsonConvert.DeserializeObject<Ip6>(jo.ToString());
                case "mx":
                    return JsonConvert.DeserializeObject<Mx>(jo.ToString());
                case "ptr":
                    return JsonConvert.DeserializeObject<Ptr>(jo.ToString());
                case "redirect":
                    return JsonConvert.DeserializeObject<Redirect>(jo.ToString());
                case "unknown":
                    return JsonConvert.DeserializeObject<UnknownTerm>(jo.ToString());
                default:
                    throw new InvalidOperationException($"Failed to convert type of {jo["TermType"].Value<string>()}.");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Term);
        }
    }
}

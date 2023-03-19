using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DiffDemo.JSON
{
    public static class JsonUtils
    {
        public static IEnumerable<JToken> GetTokens(string json)
        {
            if(string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json));

            return JsonConvert.DeserializeObject<JObject>(json)
                .GetEntitiesDescendants(j => j is JValue || !j.HasValues);
        }

        public static IEnumerable<JToken> GetEntitiesDescendants(this JToken? self, Predicate<JToken> isEntity)
        {
            if (self != null)
            {
                if (isEntity(self))
                    yield return self;

                foreach (JToken o in self.Children())
                {
                    if (isEntity(o))
                        yield return o;

                    if (o is JContainer c)
                    {
                        foreach (JToken d in c.Descendants())
                        {
                            if (isEntity(d))
                                yield return d;
                        }
                    }
                }
            }
        }
    }




}

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath
{
    internal class FieldMultipleFilter : PathFilter
    {
        internal List<string> Names;

        public FieldMultipleFilter(List<string> names)
        {
            Names = names;
        }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
        {
            foreach (JToken t in current)
            {
                if (t is JObject o)
                {
                    foreach (string name in Names)
                    {
                        JToken? v = o[name];

                        if (v != null)
                        {
                            yield return v;
                        }

                        if (settings?.ErrorWhenNoMatch ?? false)
                        {
                            throw new JsonException("Property '{0}' does not exist on JObject.".FormatWith(CultureInfo.InvariantCulture, name));
                        }
                    }
                }
                else
                {
                    if (settings?.ErrorWhenNoMatch ?? false)
                    {
                        throw new JsonException("Properties {0} not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, string.Join(", ", Names.Select(n => "'" + n + "'")), t.GetType().Name));
                    }
                }
            }
        }
    }
}
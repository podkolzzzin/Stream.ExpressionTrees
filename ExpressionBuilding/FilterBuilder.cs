using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ExpressionBuilding;

public class FilterBuilder
{
    public Filter BuildFilter(JToken obj)
    {
        if (obj.Type == JTokenType.Property && obj is JProperty prop)
        {
            var (op, val) = BuildOperatorAndValue(prop.Value);
            return new SimpleFilter() { Property = prop.Name, Operator = op, Value = val };
        }

        if (obj.Type != JTokenType.Object)
            throw new NotImplementedException();
        
        var result = new List<Filter>();
        foreach (var property in ((JObject)obj).Properties())
        {
            if (property.Value.Type == JTokenType.String)
                result.Add(new SimpleFilter()
                    { Operator = "$eq", Property = property.Name, Value = property.Value.ToString() });
            else
                result.Add(BuildFilter(property));
        }

        return new ComplexFilter() { Filters = result };
    }

    private (string, string) BuildOperatorAndValue(JToken propValue)
    {
        if (propValue.Type != JTokenType.Object)
            throw new NotImplementedException();
        var obj = (JObject)propValue;
        var prop = obj.Properties().Single();
        return (prop.Name, prop.Value.ToString());
    }
}
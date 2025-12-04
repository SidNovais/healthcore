using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HC.Core.Infrastructure.Serialization;

public class AllPropertiesContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        ArgumentNullException.ThrowIfNull(type, "Type cannot be null");
        var properties = type.GetProperties(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance)
            .Select(p => CreateProperty(p, memberSerialization))
            .ToList();
        properties.ForEach(p =>
        {
            p.Writable = true;
            p.Readable = true;
        });
        return properties;
    }
}

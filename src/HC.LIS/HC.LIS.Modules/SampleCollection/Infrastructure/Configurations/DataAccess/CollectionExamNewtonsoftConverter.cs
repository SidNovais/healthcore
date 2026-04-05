using System;
using HC.LIS.Modules.SampleCollection.Domain.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.DataAccess;

internal sealed class CollectionExamNewtonsoftConverter : JsonConverter<CollectionExam>
{
    public override CollectionExam ReadJson(
        JsonReader reader,
        Type objectType,
        CollectionExam? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        var examId = obj["ExamId"]?.ToObject<Guid>() ?? default;
        var tubeType = obj["TubeType"]?.Value<string>() ?? string.Empty;
        var examMnemonic = obj["ExamMnemonic"]?.Value<string>() ?? string.Empty;
        return CollectionExam.Of(examId, tubeType, examMnemonic);
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, CollectionExam? value, JsonSerializer serializer)
        => throw new NotSupportedException();
}

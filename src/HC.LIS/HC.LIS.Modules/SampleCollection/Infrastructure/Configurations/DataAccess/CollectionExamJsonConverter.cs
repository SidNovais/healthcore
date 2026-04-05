using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using HC.LIS.Modules.SampleCollection.Domain.Collections;

namespace HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.DataAccess;

internal sealed class CollectionExamJsonConverter : JsonConverter<CollectionExam>
{
    public override CollectionExam Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Guid examId = default;
        string tubeType = string.Empty;
        string examMnemonic = string.Empty;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName) continue;
            string propertyName = reader.GetString()!;
            reader.Read();

            if (propertyName.Equals("ExamId", StringComparison.OrdinalIgnoreCase))
                examId = reader.GetGuid();
            else if (propertyName.Equals("TubeType", StringComparison.OrdinalIgnoreCase))
                tubeType = reader.GetString()!;
            else if (propertyName.Equals("ExamMnemonic", StringComparison.OrdinalIgnoreCase))
                examMnemonic = reader.GetString()!;
            else
                reader.Skip();
        }

        return CollectionExam.Of(examId, tubeType, examMnemonic);
    }

    public override void Write(Utf8JsonWriter writer, CollectionExam value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("ExamId", value.ExamId);
        writer.WriteString("TubeType", value.TubeType);
        writer.WriteString("ExamMnemonic", value.ExamMnemonic);
        writer.WriteEndObject();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;

namespace WarManager.Sharing
{

    public class ScheduleDescriptionModelJSONConverter : JsonConverter<ScheduleDescription>
    {
        public override ScheduleDescription Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            ScheduleDescription desc = new ScheduleDescription();

            desc.Name = reader.GetString();
            desc.Value = reader.GetString();

            return desc;
        }

        public override void Write(Utf8JsonWriter writer, ScheduleDescription value, JsonSerializerOptions options)
        {
            writer.WriteString("name", value.Name);
            writer.WriteString("value", value.Value);
        }
    }
}

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using BetterOtherRoles.EnoFw.Utils;
using UnityEngine;

namespace BetterOtherRoles.EnoFw.Libs;

public class BoolConverter : JsonConverter<bool>
{
    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
        writer.WriteBooleanValue(value);
    
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String => bool.TryParse(reader.GetString(), out var b) ? b : throw new JsonException(),
            JsonTokenType.Number => reader.TryGetInt64(out var l) ? Convert.ToBoolean(l) : reader.TryGetDouble(out var d) && Convert.ToBoolean(d),
            _ => throw new JsonException(),
        };
}

public class UnityColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            return value == string.Empty ? Color.clear : Colors.FromHex(value);
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return Color.clear;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.a < 1f ? Colors.ToHex(value) : Colors.ToHex(value, false));
    }
}
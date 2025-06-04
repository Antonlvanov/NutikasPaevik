using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace NutikasPaevik.Services
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string dateString = reader.GetString();
                Console.WriteLine($"Попытка десериализации строки даты: '{dateString}'");

                if (DateTime.TryParseExact(dateString, DateTimeFormat, null, System.Globalization.DateTimeStyles.None, out DateTime result))
                {
                    Console.WriteLine($"Успешная десериализация даты: {result}");
                    return result;
                }

                throw new JsonException($"Cannot convert '{dateString}' to DateTime. Expected format: {DateTimeFormat}");
            }

            throw new JsonException($"Unexpected token type: {reader.TokenType}. Expected a string.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DateTimeFormat));
        }
    }

    public class CustomNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                string dateString = reader.GetString();
                if (string.IsNullOrEmpty(dateString))
                {
                    return null;
                }

                if (DateTime.TryParseExact(dateString, DateTimeFormat, null, System.Globalization.DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }

                throw new JsonException($"Cannot convert '{dateString}' to DateTime. Expected format: {DateTimeFormat}");
            }

            throw new JsonException($"Unexpected token type: {reader.TokenType}. Expected a string or null.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString(DateTimeFormat));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}

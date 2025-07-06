using System;
using Unity.Mathematics.FixedPoint;
using Newtonsoft.Json;

namespace SepM.Utils
{
    public class JsonConverters
    {
        public class Fp2Converter : JsonConverter<fp2>
        {
            public override void WriteJson(JsonWriter writer, fp2 value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override fp2 ReadJson(JsonReader reader, Type objectType, fp2 existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return existingValue;
            }
        }

        public class Fp3Converter : JsonConverter<fp3>
        {
            public override void WriteJson(JsonWriter writer, fp3 value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override fp3 ReadJson(JsonReader reader, Type objectType, fp3 existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return existingValue;
            }
        }

        public class Fp4Converter : JsonConverter<fp4>
        {
            public override void WriteJson(JsonWriter writer, fp4 value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override fp4 ReadJson(JsonReader reader, Type objectType, fp4 existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return existingValue;
            }
        }
    }
}
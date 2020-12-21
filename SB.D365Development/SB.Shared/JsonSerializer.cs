using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SB.Shared
{
    public static class JsonSerializer
    {
        private static DataContractJsonSerializer GetSerializer<T>() => new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings { DateTimeFormat = new DateTimeFormat("o") });

        public static string Serialize<T>(T value)
        {
            using (var memoryStream = new MemoryStream())
            {
                var serializer = GetSerializer<T>();
                serializer.WriteObject(memoryStream, value);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        public static T Deserialize<T>(string json)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                memoryStream.Position = 0;
                var serializer = GetSerializer<T>();
                return (T)serializer.ReadObject(memoryStream);
            }
        }
    }
}

using System.Text;

namespace ERC.Chat.Engine
{
    /// <summary>
    /// The binary formatter uses json to convert an object to string and then encodes with utf 8 to a byte array and reverse.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class BinaryFormatter<T>
    {
        /// <summary>
        /// Use json to encode an object to string and then to a byte array using utf8
        /// </summary>
        /// <param name="toBinary"></param>
        /// <returns></returns>
        public static byte[] ToBinary(T toBinary)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(toBinary);
            var result = Encoding.UTF8.GetBytes(json);
            return result;
        }

        /// <summary>
        /// Converts an byte array to a string using utf 8 and then to the corresponding object with json
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T FromBinary(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            return result;
        }
    }
}

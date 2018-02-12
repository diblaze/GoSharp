using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GoSharp.Data
{
    /// <summary>
    ///     Functions for performing common binary Serialization operations.
    ///     <para>All properties and variables will be serialized.</para>
    ///     <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
    ///     <para>
    ///         To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be
    ///         applied to properties.
    ///     </para>
    /// </summary>
    public static class BinarySerialization
    {
        //TODO: JSON/XML 
        /// <summary>
        ///     Writes the given object instance to a binary file.
        /// </summary>
        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite)
        {
            using (Stream stream = File.Open(filePath, FileMode.Create))
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }

        /// <summary>
        ///     Reads an object instance from a binary file.
        /// </summary>
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new BinaryFormatter();
                return (T) binaryFormatter.Deserialize(stream);
            }
        }
    }
}
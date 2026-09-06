namespace FileSystem.Interfaces
{

    /// <summary>
    /// Interface for a class that is serializable.
    /// </summary>
    public interface ISerializable
    {

        #region Methods

        /// <summary>
        /// Serializes the data of the class into a byte stream. 
        /// </summary>
        /// <param name="stream"> The byte stream the data is serialized into. </param>
        public void Serialize(Stream stream);

        /// <summary>
        /// Deserializes the data of the class from a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream the data is deserialized from. </param>
        public void Deserialize(Stream stream);

        #endregion

    }
}

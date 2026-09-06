namespace CriticalData.Interfaces
{

    /// <summary>
    /// Interface for a critical data class.
    /// </summary>
    public interface ICriticalData
    {

        /// <summary>
        /// The critical data manager.
        /// </summary>
        public CriticalDataManager? CriticalDataManager { get; set; }

        /// <summary>
        /// Serializes critical data for the class.
        /// </summary>
        /// <param name="stream"> Byte stream that the data is serialized into. </param>
        public void Serialize(Stream stream);

        /// <summary>
        /// Deserializes critical data for the class.
        /// </summary>
        /// <param name="stream"> Byte stream that the data is serialized from. </param>
        public void Deserialize(Stream stream);

        /// <summary>
        /// Sends a call to the critical data manager to save critical data.
        /// </summary>
        public void SaveCriticalData();

    }

}

using FileSystem.Interfaces;
using CriticalData.Interfaces;

namespace CriticalData
{

    /// <summary>
    /// Manages saving/loading critical data.
    /// </summary>
    public class CriticalDataManager
    {

        #region Fields

        /// <summary>
        /// The file path that critical data is saved to/loaded from.
        /// </summary>
        protected readonly string filePath;

        /// <summary>
        /// The FileSystem controller.
        /// </summary>
        protected readonly IFileSystemController fileSystemController;

        /// <summary>
        /// List of critical data objects this manager is in charge of.
        /// </summary>
        protected readonly List<ICriticalData> criticalDataObjects;

        #endregion

        #region Methods

        /// <summary>
        /// Constructor.
        /// </summary>
        public CriticalDataManager(string filePath, IFileSystemController fileSystemController, List<ICriticalData> criticalDataObjects)
        {
            this.fileSystemController = fileSystemController;
            this.filePath = filePath;
            this.criticalDataObjects = criticalDataObjects;
            foreach (ICriticalData data in criticalDataObjects)
                data.CriticalDataManager = this;
            LoadCriticalData();
        }

        /// <summary>
        /// Saves critical data to a file.
        /// </summary>
        public void SaveCriticalData()
        {
            using Stream stream = new MemoryStream();
            foreach (ICriticalData data in criticalDataObjects)
            {
                data.Serialize(stream);
            }
            stream.Position = 0;
            fileSystemController.WriteFile(filePath, stream);
        }

        /// <summary>
        /// Loads critical data from a file.
        /// </summary>
        public void LoadCriticalData()
        {
            if (!fileSystemController.FileExists(filePath))
                return;
            using Stream stream = new MemoryStream();
            fileSystemController.ReadFileStream(filePath, stream);
            stream.Position = 0;
            foreach (ICriticalData data in criticalDataObjects)
            {
                data.Deserialize(stream);
            }
        }

        /// <summary>
        /// Removes the critical data file.
        /// </summary>
        public void RemoveCriticalData()
        {
            fileSystemController.DeleteFile(filePath);
        }

        #endregion

    }

}

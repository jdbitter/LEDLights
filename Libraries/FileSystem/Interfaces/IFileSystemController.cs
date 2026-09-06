namespace FileSystem.Interfaces
{

    /// <summary>
    /// Base interface for a file system.
    /// </summary>
    public interface IFileSystemController
    {

        #region Methods

        /// <summary>
        /// Checks if the file exists.
        /// </summary>
        /// <param name="path"> The file path being checked. </param>
        /// <returns> Flag of if the file exists. </returns>
        public bool FileExists(string path);

        /// <summary>
        /// Deletes the file at the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool DeleteFile(string path);

        #region Read Methods

        /// <summary>
        /// Asynchronous operation that reads from a file in string form.
        /// </summary>
        /// <param name="path"> Path to the file. </param>
        /// <returns> A task that represents the asynchronous operation that contains the contents of the file in string form. </returns>
        public Task<string?> ReadFileTextAsync(string path);

        /// <summary>
        /// Reads from a file in string form.
        /// </summary>
        /// <param name="path"> Path to the file. </param>
        /// <returns> The contents of the file in string form. </returns>
        public string? ReadFileText(string path);

        /// <summary>
        /// Asynchronous operation that reads from a file in byte[] form.
        /// </summary>
        /// <returns> A task that represents the asynchronous operation that contains the contents of the file in byte[] form. </returns>
        /// <returns></returns>
        public Task<byte[]?> ReadFileBytesAsync(string path);

        /// <summary>
        /// Reads from a file in byte[] form.
        /// </summary>
        /// <param name="path"> Path to the file. </param>
        /// <returns> The contents of the file in byte[] form. </returns>
        public byte[]? ReadFileBytes(string path);

        /// <summary>
        /// asynchronous operation that reads from a file into a byte stream.
        /// </summary>
        /// <param name="path"> Path to the file. </param>
        /// <param name="stream"> The byte stream being read into. </param>
        /// <returns> A task that represents the asynchronous operation that contains a bool for the file being successfully read. </returns>
        public Task<bool> ReadFileStreamAsync(string path, Stream stream);

        /// <summary>
        /// Reads from a file into a byte stream.
        /// </summary>
        /// <param name="path"> Path to the file. </param>
        /// <param name="stream"> The byte stream being read into. </param>
        /// <returns> A bool for the file being successfully read. </returns>

        public bool ReadFileStream(string path, Stream stream);

        #endregion

        #region Write Methods

        /// <summary>
        /// Asynchronous operation that writes to a file in string form.
        /// </summary>
        /// <param name="path"> Path to the file. </param>
        /// <param name="contents"> The content to be written to the file in string form. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public Task WriteFileAsync(string path, string contents);

        /// <summary>
        /// Writes to a file in string form.
        /// </summary>
        /// <param name="path"> Path to the file. </param>
        /// <param name="contents"> The content to be written to the file in string form. </param>
        public void WriteFile(string path, string contents);

        /// <summary>
        /// Asynchronous operation that writes to a file in byte[] form.
        /// </summary>
        /// <param name="path"> Path to the file. </param>
        /// <param name="contents"> The content to be written to the file in byte[] form. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public Task WriteFileAsync(string path, byte[] contents);

        /// <summary>
        /// Writes to a file in byte[] form.
        /// </summary>
        /// <param name="path"> Path to the file. </param>
        /// <param name="contents"> The content to be written to the file in byte[] form. </param>
        public void WriteFile(string path, byte[] contents);

        /// <summary>
        /// Asynchronous operation that write bytes to a file via a stream.
        /// </summary>
        /// <param name="path"> The file path. </param>
        /// <param name="stream"> The stream containing the bytes to be written. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public Task WriteFileAsync(string path, Stream stream);

        /// <summary>
        /// Writes bytes to a file via a stream.
        /// </summary>
        /// <param name="path"> Path to the file. </param>
        /// <param name="stream"> The stream containing the bytes to be written. </param>
        public void WriteFile(string path, Stream stream);

        #endregion

        #endregion

    }
}

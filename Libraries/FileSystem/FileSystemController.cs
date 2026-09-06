using FileSystem.Interfaces;

namespace FileSystem
{

    /// <summary>
    /// General implementation of a file system.
    /// </summary>
    public class FileSystemController : IFileSystemController
    {

        #region Methods

        #region BaseFileSystem Implementation

        ///<inheritdoc/>
        public virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        ///<inheritdoc/>
        public virtual bool DeleteFile(string path)
        {
            if (!FileExists(path))
                return true;

            File.Delete(path);
            return true;
        }

        #region Read Methods

        ///<inheritdoc/>
        public virtual async Task<string?> ReadFileTextAsync(string path)
        {
            if (!FileExists(path))
                return null;

            return await File.ReadAllTextAsync(path);
        }

        ///<inheritdoc/>
        public virtual string? ReadFileText(string path)
        {
            if (!FileExists(path))
                return null;

            return File.ReadAllText(path);
        }

        ///<inheritdoc/>
        public virtual async Task<byte[]?> ReadFileBytesAsync(string path)
        {
            if (!FileExists(path))
                return null;

            return await File.ReadAllBytesAsync(path);
        }

        ///<inheritdoc/>
        public virtual byte[]? ReadFileBytes(string path)
        {
            if (!FileExists(path))
                return null;

            return File.ReadAllBytes(path);
        }

        ///<inheritdoc/>
        public virtual async Task<bool> ReadFileStreamAsync(string path, Stream stream)
        {
            if (!FileExists(path))
                return false;

            using FileStream fileStream = File.OpenRead(path);
            await fileStream.CopyToAsync(stream);

            return true;
        }

        ///<inheritdoc/>
        public virtual bool ReadFileStream(string path, Stream stream)
        {
            if (!FileExists(path))
                return false;

            using FileStream fileStream = File.OpenRead(path);
            fileStream.CopyTo(stream);

            return true;
        }

        #endregion

        #region Write Methods

        ///<inheritdoc/>
        public virtual Task WriteFileAsync(string path, string contents)
        {
            return File.WriteAllTextAsync(path, contents);
        }

        ///<inheritdoc/>
        public virtual void WriteFile(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        ///<inheritdoc/>
        public virtual Task WriteFileAsync(string path, byte[] contents)
        {
            return File.WriteAllBytesAsync(path, contents);
        }

        ///<inheritdoc/>
        public virtual void WriteFile(string path, byte[] contents)
        {
            File.WriteAllBytes(path, contents);
        }

        ///<inheritdoc/>
        public virtual Task WriteFileAsync(string path, Stream stream)
        {
            stream.Position = 0;
            using FileStream fileStream = File.Create(path);
            return stream.CopyToAsync(fileStream);
        }

        ///<inheritdoc/>
        public virtual void WriteFile(string path, Stream stream)
        {
            stream.Position = 0;
            using FileStream fileStream = File.Create(path);
            stream.CopyTo(fileStream);
        }

        #endregion

        #endregion

        #endregion

    }

}

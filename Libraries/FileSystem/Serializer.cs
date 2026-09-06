using FileSystem.Interfaces;
using System.Buffers.Binary;
using System.Text;

namespace FileSystem
{
    /// <summary>
    /// Handles the serialization/deserialization of various data types.
    /// </summary>
    public static class Serializer
    {

        #region Public Methods

        /// <summary>
        /// Writes a value of type T to a byte stream.
        /// </summary>
        /// <typeparam name="T"> Generic, serializable type. </typeparam>
        /// <param name="stream"> The byte stream being written to. </param>
        /// <param name="value"> The value getting written to the stream. </param>
        /// <exception cref="NotSupportedException"> Passed in type T is not supported/serializable. </exception>
        public static void Write<T>(Stream stream, T value)
        {
            switch (value)
            {
                case bool val:
                    Write(stream, val);
                    break;

                case int val:
                    Write(stream, val);
                    break;

                case long val:
                    Write(stream, val);
                    break;

                case ulong val:
                    Write(stream, val);
                    break;

                case float val:
                    Write(stream, val);
                    break;

                case double val:
                    Write(stream, val);
                    break;

                case char val:
                    Write(stream, val);
                    break;

                case string val:
                    Write(stream, val);
                    break;

                case ISerializable val:
                    Write(stream, val);
                    break;

                default:
                    throw new NotSupportedException($"Type '{typeof(T)}' is not supported.");
            }
        }

        /// <summary>
        /// Reads a value of type T from a byte stream.
        /// </summary>
        /// <typeparam name="T"> Generic, serializable type. </typeparam>
        /// <param name="stream"> The byte stream being read from. </param>
        /// <returns> value of type T. </returns>
        /// <exception cref="NotSupportedException"> Passed in type T is not supported/serializable. </exception>
        public static T Read<T>(Stream stream)
        {
            Type type = typeof(T);

            if (type == typeof(bool))
                return (T)(object)ReadBool(stream);

            if (type == typeof(int))
                return (T)(object)ReadInt(stream);

            if (type == typeof(long))
                return (T)(object)ReadLong(stream);

            if (type == typeof(ulong))
                return (T)(object)ReadULong(stream);

            if (type == typeof(float))
                return (T)(object)ReadFloat(stream);

            if (type == typeof(double))
                return (T)(object)ReadDouble(stream);

            if (type == typeof(char))
                return (T)(object)ReadChar(stream);

            if (type == typeof(string))
                return (T)(object)ReadString(stream);

            if (type.IsAssignableTo(typeof(ISerializable)))
                return ReadSerializable<T>(stream);

            throw new NotSupportedException($"Type {typeof(T)} is not supported.");
        }

        #region Arrays

        /// <summary>
        /// Writes an array of type T to a byte stream.
        /// </summary>
        /// <typeparam name="T"> Generic, serializable type. </typeparam>
        /// <param name="stream"> The byte stream being written to. </param>
        /// <param name="array"> The array getting written to the stream. </param>
        public static void Write<T>(Stream stream, T[] array)
        {
            Write(stream, array.Length);

            foreach (T value in array)
                Write(stream, value);
        }

        /// <summary>
        /// Reads an array of type T from a byte stream.
        /// </summary>
        /// <typeparam name="T"> Generic, serializable type. </typeparam>
        /// <param name="stream"> The byte stream being read from. </param>
        /// <returns> An array of type T. </returns>
        public static T[] ReadArray<T>(Stream stream)
        {
            int length = ReadInt(stream);
            T[] array = new T[length];

            for(int i = 0; i < length; i++)
                array[i] = Read<T>(stream);

            return array;
        }

        #endregion

        #region Lists

        /// <summary>
        /// Writes a list of type T to a byte stream.
        /// </summary>
        /// <typeparam name="T"> Generic, serializable type. </typeparam>
        /// <param name="stream"> The byte stream being written to. </param>
        /// <param name="list"> The list being written to the stream. </param>
        public static void Write<T>(Stream stream, List<T> list)
        {
            Write(stream, list.ToArray());
        }

        /// <summary>
        /// Reads a list of type T from a byte stream.
        /// </summary>
        /// <typeparam name="T"> Generic, serializable type. </typeparam>
        /// <param name="stream"> The byte stream being read from. </param>
        /// <returns> A list of type T.</returns>
        public static List<T> ReadList<T>(Stream stream)
        {
            return ReadArray<T>(stream).ToList();
        }

        #endregion

        #region Dictionaries

        /// <summary>
        /// Writes a dictionary of type <TKey, TValue> to a byte stream.
        /// </summary>
        /// <typeparam name="TKey"> Generic, serializable type key. </typeparam>
        /// <typeparam name="TValue"> Generic, serializable type value. </typeparam>
        /// <param name="stream"> The byte stream being written to. </param>
        /// <param name="dictionary"> The dictionary being written to the stream. </param>
        public static void Write<TKey, TValue>(Stream stream, Dictionary<TKey, TValue> dictionary) where TKey : notnull
        {
            Write(stream, dictionary.Count);
            foreach(KeyValuePair<TKey, TValue> pair in dictionary)
            {
                Write(stream, pair.Key);
                Write(stream, pair.Value);
            }
        }

        /// <summary>
        /// Reads a dictionary of type <TKey, TValue> from a byte stream.
        /// </summary>
        /// <typeparam name="TKey"> Generic, serializable type key. </typeparam>
        /// <typeparam name="TValue"> Generic, serializable type value. </typeparam>
        /// <param name="stream"> The byte stream being read from. </param>
        /// <returns> A dictionary of type <TKey, TValue>. </returns>
        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(Stream stream) where TKey : notnull
        {
            int count = ReadInt(stream);
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
            for (int i = 0; i < count; i++)
            {
                TKey key = Read<TKey>(stream);
                TValue value = Read<TValue>(stream);
                dictionary[key] = value;
            }
            return dictionary;
        }

        #endregion

        #endregion

        #region private Methods

        #region Bools

        /// <summary>
        /// Writes a bool to a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream being written to. </param>
        /// <param name="value"> The bool. </param>
        private static void Write(Stream stream, bool value)
        {
            stream.WriteByte(value ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Reads a bool from a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream being read from. </param>
        /// <returns> A bool. </returns>
        private static bool ReadBool(Stream stream)
        {
            int value = stream.ReadByte();

            if (value == -1)
                throw new EndOfStreamException();
            return value == 1;
        }

        #endregion

        #region Ints

        /// <summary>
        /// Writes an int to a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream being written to. </param>
        /// <param name="value"> The int. </param>
        private static void Write(Stream stream, int value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];

            BinaryPrimitives.WriteInt32LittleEndian(buffer, value);

            stream.Write(buffer);
        }

        /// <summary>
        /// Reads an int from a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream being read from. </param>
        /// <returns> An int. </returns>
        private static int ReadInt(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            stream.ReadExactly(buffer);
            return BinaryPrimitives.ReadInt32LittleEndian(buffer);
        }

        #endregion

        #region Longs

        /// <summary>
        /// Writes a ;pmg to a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream being written to. </param>
        /// <param name="value"> The long. </param>
        private static void Write(Stream stream, long value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes an unisgned long to a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream being written to. </param>
        /// <param name="value"> The unsigned long. </param>
        private static void Write(Stream stream, ulong value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Reads a long from a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream being read from. </param>
        /// <returns> A long. </returns>
        private static long ReadLong(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            stream.ReadExactly(buffer);
            return BinaryPrimitives.ReadInt64LittleEndian(buffer);
        }

        /// <summary>
        /// Reads an unsigned long from a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream being read from. </param>
        /// <returns> An unsigned long. </returns>
        private static ulong ReadULong(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            stream.ReadExactly(buffer);
            return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
        }

        #endregion

        #region Floats

        /// <summary>
        /// Writes a float to a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream being written to. </param>
        /// <param name="value"> The float. </param>
        private static void Write(Stream stream, float value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(float)];
            BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Reads a float from a byte stream. 
        /// </summary>
        /// <param name="stream"> The byte stream being read from. </param>
        /// <returns> A float. </returns>
        private static float ReadFloat(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[sizeof(float)];
            stream.ReadExactly(buffer);
            return BinaryPrimitives.ReadSingleLittleEndian(buffer);
        }

        #endregion

        #region Doubles

        /// <summary>
        /// Writes a double to a byte stream. 
        /// </summary>
        /// <param name="stream"> The byte stream being written to. </param>
        /// <param name="value"> The double. </param>
        private static void Write(Stream stream, double value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Reads a double from a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream being read from. </param>
        /// <returns> A double. </returns>
        private static double ReadDouble(Stream stream)
        {
            Span<byte> buffer = stackalloc Byte[sizeof(double)];
            stream.ReadExactly(buffer);
            return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
        }

        #endregion

        #region Chars

        /// <summary>
        /// Writes a char to a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream being written to. </param>
        /// <param name="value"> The char. </param>
        private static void Write(Stream stream, char value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(char)];
            BinaryPrimitives.WriteInt16LittleEndian(buffer, (short)value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Reads a char from a byte stream. 
        /// </summary>
        /// <param name="stream"> The byte stream being read from. </param>
        /// <returns> A char. </returns>
        private static char ReadChar(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[sizeof(char)];
            stream.ReadExactly(buffer);
            return (char)BinaryPrimitives.ReadInt16LittleEndian(buffer);
        }

        #endregion

        #region Strings

        /// <summary>
        /// Writes a string to a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream being written to. </param>
        /// <param name="value"> The string. </param>
        private static void Write(Stream stream, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);

            Write(stream, bytes.Length);
            stream.Write(bytes);
        }

        /// <summary>
        /// Reads a string from a byte stream. 
        /// </summary>
        /// <param name="stream"> The byte stream being read from. </param>
        /// <returns> A string. </returns>
        private static string ReadString(Stream stream)
        {
            Span<byte> bytes = stackalloc byte[ReadInt(stream)];
            stream.ReadExactly(bytes);
            return Encoding.UTF8.GetString(bytes);
        }

        #endregion

        #region Serializables

        /// <summary>
        /// Writes an ISerializable object to a byte stream.
        /// </summary>
        /// <param name="stream"> The byte stream being written to. </param>
        /// <param name="value"> The ISerializable object. </param>
        private static void Write(Stream stream, ISerializable value)
        {
            value.Serialize(stream);
        }

        /// <summary>
        /// Reads an ISerializable object from a byte stream.
        /// </summary>
        /// <typeparam name="T"> Type T that implements ISerializable. </typeparam>
        /// <param name="stream"> The byte stream being read from. </param>
        /// <returns> An ISerializable object. </returns>
        private static T ReadSerializable<T>(Stream stream)
        {
            ISerializable value = (ISerializable)Activator.CreateInstance(typeof(T))!;
            value.Deserialize(stream);
            return (T)value;
        }

        #endregion

        #endregion

        }

    }
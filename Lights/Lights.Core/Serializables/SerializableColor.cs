using FileSystem;
using FileSystem.Interfaces;
using System.Drawing;


namespace Lights.Core.Serializables
{

    /// <summary>
    /// Represents a Color for serialization/deserialization.
    /// </summary>
    internal class SerializableColor : ISerializable
    {

        #region Properties

        /// <summary>
        /// Flag for the color being invalid.
        /// </summary>
        public bool Invalid { get; protected set; }

        /// <summary>
        /// The R value of the color.
        /// </summary>
        public int R { get; protected set; }

        /// <summary>
        /// The G value of the color.
        /// </summary>
        public int G { get; protected set; }

        /// <summary>
        /// The B value of the color.
        /// </summary>
        public int B { get; protected set; }

        #endregion

        #region Methods 

        /// <summary>
        /// Parameterless constructor, required for ISerializable
        /// </summary>
        public SerializableColor() { }

        /// <summary>
        /// Constructs a Keepsmile color from a Color variable.
        /// </summary>
        /// <param name="color"></param>
        public SerializableColor(Color? color)
        {
            if (color == null)
            {
                Invalid = true;
                R = -1;
                G = -1;
                B = -1;
                return;
            }

            Color validatedColor = (Color)color;
            Invalid = false;
            R = validatedColor.R;
            G = validatedColor.G;
            B = validatedColor.B;
        }

        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <returns> The Color? this object is representing. </returns>
        public Color? GetColor()
        {
            if (Invalid)
                return null;

            return Color.FromArgb(R, G, B);
        }

        #endregion

        #region Serializable Implementation

        /// <inheritdoc/>
        public virtual void Serialize(Stream stream)
        {
            Serializer.Write(stream, Invalid);
            if (Invalid)
                return;
            Serializer.Write(stream, R);
            Serializer.Write(stream, G);
            Serializer.Write(stream, B);
        }

        /// <inheritdoc/>
        public virtual void Deserialize(Stream stream) 
        {
            Invalid = Serializer.Read<bool>(stream);
            if (Invalid)
            {
                R = -1;
                G = -1;
                B = -1;
                return;
            }

            R = Serializer.Read<int>(stream);
            G = Serializer.Read<int>(stream);
            B = Serializer.Read<int>(stream);
        }

        #endregion

    }
}

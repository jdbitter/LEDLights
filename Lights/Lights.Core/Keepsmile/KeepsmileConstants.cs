namespace Lights.Core.Keepsmile
{

    #region Enums

    /// <summary>
    /// Constants for Keepsmile LED controller.
    /// </summary>
    public class KeepsmileConstants
    {

        /// <summary>
        /// Enum for the different light sequences on the keepsmile bluetooth device.
        /// </summary>
        public enum LightsSequence : byte
        {
            Invalid = 0x00,
            SevenColorJumpingChange = 0x80,
            RGBJumpingChange = 0x81,
            SevenColorsPulsating = 0x82,
            RGBPulsating = 0x83,
            RedPulsating = 0x84,
            GreenPulsating = 0x85,
            BluePulsating = 0x86,
            YellowPulsating = 0x87,
            TealPulsating = 0x88,
            PurplePulsating = 0x89,
            WhitePulsating = 0x8A,
            RedGreenPulsating = 0x8B,
            RedBluePulsating = 0x8C,
            GreenBluePulsating = 0x8D,
            SevenColorStrobe = 0x8E,
            RGBStrobe = 0x8F,
            RedStrobe = 0x90,
            GreenStrobe = 0x91,
            BlueStrobe = 0x92,
            YellowStrobe = 0x93,
            TealStrobe = 0x94,
            PurpleStrobe = 0x95,
            WhiteStrobe = 0x96,
        }

    #endregion

    }

}

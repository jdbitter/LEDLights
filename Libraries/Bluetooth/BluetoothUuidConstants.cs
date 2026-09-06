namespace Bluetooth
{

    /// <summary>
    /// General bluteooth uuids.
    /// </summary>
    public static class BluetoothUuidConstants
    {

        /// <summary>
        /// The uuid of a generic access service.
        /// </summary>
        public static readonly Guid GenericAccessService = Guid.Parse("00001800-0000-1000-8000-00805F9B34FB");

        /// <summary>
        /// The uuid of a general device name characteristic.
        /// </summary>
        public static readonly Guid DeviceNameCharacteristic = Guid.Parse("00002A00-0000-1000-8000-00805F9B34FB");
    }

}

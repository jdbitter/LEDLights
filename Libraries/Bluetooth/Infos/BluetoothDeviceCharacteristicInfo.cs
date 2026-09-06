using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Bluetooth.Infos
{

    /// <summary>
    /// Class that contains the info for a characteristic of a bluetooth device.
    /// </summary>
    public class BluetoothDeviceCharacteristicInfo
    {

        #region Properties

        /// <summary>
        /// The uuid of the characteristic.
        /// </summary>
        public Guid Uuid { get; }

        /// <summary>
        /// The actual Gatt characteristic object.
        /// </summary>
        public GattCharacteristic Characteristic { get; private set; }

        /// <summary>
        /// The Gatt charcteristics properties.
        /// </summary>
        public GattCharacteristicProperties Properties { get; private set; }

        /// <summary>
        /// Flag for this characteristic being writable.
        /// </summary>
        public bool CanWrite => Properties.HasFlag(GattCharacteristicProperties.Write) || Properties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse);

        /// <summary>
        /// Flag for this characteristic being writable with a response.
        /// </summary>
        public bool CanWriteWithResponse => Properties.HasFlag(GattCharacteristicProperties.Write);

        /// <summary>
        /// Flag for this characteristic being readable.
        /// </summary>
        public bool CanRead => Properties.HasFlag(GattCharacteristicProperties.Read);

        #endregion

        #region Methods

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="characteristic"> The actual Gatt characteristic. </param>
        public BluetoothDeviceCharacteristicInfo(GattCharacteristic characteristic)
        {
            Uuid = characteristic.Uuid;
            Characteristic = characteristic;
            Properties = characteristic.CharacteristicProperties;
            Console.WriteLine($"Uuid: {Uuid}, Properties: {Properties}");
        }

        /// <summary>
        /// Refreshes the characteristic in situations where the previous went stale/was disposed.
        /// </summary>
        /// <param name="characteristic"> The new characteristic. </param>
        /// <exception cref="ArgumentException"> Thrown if the uuids don't match between the new characteristic and the original characteristic. </exception>
        public void RefreshCharacteristic(GattCharacteristic characteristic)
        {
            if (Uuid != characteristic.Uuid)
                throw new ArgumentException("Can't refresh characteristic, uuids don't match. ");

            Characteristic = characteristic;
            Properties = characteristic.CharacteristicProperties;
        }

        #endregion

    }

}

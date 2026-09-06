using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace Bluetooth.Infos
{

    /// <summary>
    /// Class that contains the info for a service of a bluetooth device.
    /// </summary>
    public class BluetoothDeviceServiceInfo
    {

        #region Properties

        /// <summary>
        /// The uuid of the service.
        /// </summary>
        public Guid Uuid { get; }

        /// <summary>
        /// The actual Gatt device service object.
        /// </summary>
        public GattDeviceService Service { get; private set; }

        /// <summary>
        /// List of the characteristic for this service.
        /// </summary>
        public Dictionary<Guid, BluetoothDeviceCharacteristicInfo> Characteristics { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"> The Gatt device service. </param>
        /// <param name="characteristics"> The Gatt Characteristics result of this service. </param>
        public BluetoothDeviceServiceInfo(GattDeviceService service, GattCharacteristicsResult characteristics)
        {
            Uuid = service.Uuid;
            Service = service;

            Characteristics = new Dictionary<Guid, BluetoothDeviceCharacteristicInfo>();
            foreach(GattCharacteristic characteristic in characteristics.Characteristics)
            { 
                Characteristics.Add(characteristic.Uuid, new BluetoothDeviceCharacteristicInfo(characteristic)); 
            }
        }

        /// <summary>
        /// Refreshes the service in situations where the previous went stale/was disposed.
        /// </summary>
        /// <param name="service"> The new service. </param>
        /// <exception cref="ArgumentException"> Thrown if the uuids don't match between the new service and the original service. </exception>
        public void RefreshService(GattDeviceService service)
        {
            if (Uuid != service.Uuid)
                throw new ArgumentException("Can't refresh service, uuids don't match. ");

            Service = service;
        }

        #endregion

    }

}

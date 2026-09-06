using System.Text;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

namespace Bluetooth.Infos
{

    /// <summary>
    /// Class that contains the info of a bluetooth device.
    /// </summary>
    public class BluetoothDeviceInfo
    {

        #region Fields

        #region Constants

        /// <summary>
        /// The default name to assign to a device if it doesn't define a name for itself.
        /// </summary>
        public const string DEFAULT_NAME = "<Unknown>";

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// The name of the device.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The bluetooth address of the device.
        /// </summary>
        public ulong Address { get; set; }

        /// <summary>
        /// The device's signal strength
        /// </summary>
        public int SignalStrength { get; set; }

        /// <summary>
        /// Flag for this device being connectable.
        /// </summary>
        public bool Connectable { get; set; }

        /// <summary>
        /// Flag for this device being scannable.
        /// </summary>
        public bool Scannable { get; set; }

        /// <summary>
        /// Enum of the bluetooth address type for this device.
        /// </summary>
        public BluetoothAddressType AddressType { get; set; }

        /// <summary>
        /// Enum of the advertisement type for this device.
        /// </summary>
        public BluetoothLEAdvertisementType AdvertisementType { get; set; }

        /// <summary>
        /// DataTimeOffset of the last time this device was detected.
        /// </summary>
        public DateTimeOffset LastSeen { get; set; }

        /// <summary>
        /// The uuids of services this device has.
        /// </summary>
        public List<Guid> ServiceUuids { get; } = new();

        /// <summary>
        /// The manufacturer ids that this device has.
        /// </summary>
        public List<ushort> ManufacturerIds { get; } = new();

        /// <summary>
        /// Translator for the signal quality of this device.
        /// </summary>
        public string SignalQuality
        {
            get
            {
                return SignalStrength switch
                {
                    >= -50 => "Excellent",
                    >= -60 => "Good",
                    >= -70 => "Fair",
                    >= -80 => "Weak",
                    _ => "Poor"
                };
            }
        }

        /// <summary>
        /// Translator for the formatted address of this device.
        /// </summary>
        public string FormattedAddress
        {
            get
            {
                return string.Join(":",
                    BitConverter.GetBytes(Address)
                        .Reverse()
                        .Select(b => b.ToString("X2")));
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="signalStrength"></param>
        /// <param name="connectable"></param>
        /// <param name="scannable"></param>
        /// <param name="addressType"></param>
        /// <param name="advertisementType"></param>
        /// <param name="lastSeen"></param>
        /// <param name="serviceUuids"></param>
        /// <param name="manufacturerIds"></param>
        public BluetoothDeviceInfo(string name, ulong address, int signalStrength, bool connectable, bool scannable, BluetoothAddressType addressType, BluetoothLEAdvertisementType advertisementType, DateTimeOffset lastSeen, List<Guid> serviceUuids, List<ushort> manufacturerIds)
        {
            Name = name;
            Address = address;
            SignalStrength = signalStrength;
            Connectable = connectable;
            Scannable = scannable;
            AddressType = addressType;
            AdvertisementType = advertisementType;
            LastSeen = lastSeen;
            ServiceUuids = serviceUuids;
            ManufacturerIds = manufacturerIds;
        }

        /// <summary>
        /// Implementation of ToString method.
        /// </summary>
        /// <returns> BluetoothDeviceInfo converted into a readable string. </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("--------------------");
            sb.AppendLine($"Name: {Name}");
            sb.AppendLine($"Address: {FormattedAddress}");
            sb.AppendLine($"Signal Strength: {SignalStrength} dBm ({SignalQuality})");
            sb.AppendLine($"Connectable: {Connectable}");
            sb.AppendLine($"Scannable: {Scannable}");
            sb.AppendLine($"Address Type: {AddressType}");
            sb.AppendLine($"Advertisement Type: {AdvertisementType}");
            sb.AppendLine($"Last Seen: {LastSeen:HH:mm:ss.fff}");

            if (ServiceUuids.Count > 0)
            {
                sb.AppendLine("Services:");

                foreach (Guid service in ServiceUuids)
                    sb.AppendLine($"  {service}");
            }

            if (ManufacturerIds.Count > 0)
            {
                sb.AppendLine("Manufacturer IDs:");

                foreach (ushort id in ManufacturerIds)
                    sb.AppendLine($"  {id}");
            }

            return sb.ToString();
        }

        #endregion

    }
}

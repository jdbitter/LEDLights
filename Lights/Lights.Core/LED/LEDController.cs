using Bluetooth;
using Bluetooth.Infos;

namespace Lights.Core.LED
{

    /// <summary>
    /// Abstract implementation for a bluetooth LED controller.
    /// </summary>
    public abstract class LEDController
    {

        #region Fields 

        /// <summary>
        /// The logger that handles logs made by this class.
        /// </summary>
        protected readonly Action<string>? logger;

        #endregion

        #region Properties

        protected BluetoothController BluetoothController { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger"> Logger that this class will log to.  Won't log anywhere if null. </param>
        public LEDController(Action<string>? logger = null)
        {
            this.logger = logger;
            BluetoothController = new BluetoothController(logger);
        }

        /// <summary>
        /// Connects to a bluetooth device with the given device info.
        /// </summary>
        /// <param name="DeviceInfo"> The device info aquired from <see cref="FindDevicesAsync"/>. </param>
        /// <returns> Task that represents the synchronous operation that contains a bool for successfully connecting to a device with the given device info. </returns>
        public virtual Task<bool> ConnectToDeviceAsync(BluetoothDeviceInfo DeviceInfo)
        {
            return BluetoothController.ConnectToDeviceAsync(DeviceInfo);
        }

        /// <summary>
        /// Connects to a bluetooth device with the specified address
        /// </summary>
        /// <param name="DeviceAddress"> The address of the bluetooth device. </param>
        /// <returns> Task that represents the asynchronous operation that contains a bool for successfully connecting to a device with the given address. </returns>
        public virtual async Task<bool> ConnectToDeviceAsync(ulong DeviceAddress)
        {
            if (!await BluetoothController.ConnectToDeviceAsync(DeviceAddress))
                return false;
            if (!await VerifyDeviceAsync())
            {
                await DisconnectFromDeviceAsync();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Disconnects from a bluetooth device.
        /// </summary>
        /// <returns> Task that represents the asynchronous operation. </returns>
        public virtual Task DisconnectFromDeviceAsync()
        {
            return BluetoothController.DisconnectAsync();
        }

        /// <summary>
        /// Finds nearby bluetooth devices.
        /// </summary>
        /// <param name="searchTime"> The length of the search time in seconds. </param>
        /// <returns> Task that represents the asynchronous operation that contains a list of all detected bluetooth devices. </returns>
        public virtual Task<List<BluetoothDeviceInfo>> FindDevicesAsync(float searchTime = 1f)
        {
            return BluetoothController.DetectDevicesAsync(searchTime);
        }

        #region Protected/Private Methods

        /// <summary>
        /// Does any necessary verification on the device.
        /// </summary>
        /// <returns> Task that represents the asynchronous operation that contains a bool for passing verification. </returns>
        protected abstract Task<bool> VerifyDeviceAsync();

        #endregion

        #endregion

    }

}

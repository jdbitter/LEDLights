using Bluetooth.Infos;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace Bluetooth
{

    /// <summary>
    /// Controller that handles interacting with a bluetooth device.
    /// </summary>
    public class BluetoothController
    {

        #region Fields

        /// <summary>
        /// The connected bluetooth device.
        /// </summary>
        protected BluetoothLEDevice? connectedDevice;

        /// <summary>
        /// The bluetooth advertisement watcher, which is used to find bluetooth devices.
        /// </summary>
        protected BluetoothLEAdvertisementWatcher watcher;

        /// <summary>
        /// The Gatt session of the connected bluetooth device.
        /// </summary>
        protected GattSession? session;

        /// <summary>
        /// List of the services the connected bluetooth device has.
        /// </summary>
        protected Dictionary<Guid, BluetoothDeviceServiceInfo> connectedDeviceServices;

        /// <summary>
        /// List of all detected bluetooth devices.
        /// </summary>
        protected List<BluetoothDeviceInfo> detectedBluetoothDevices;

        /// <summary>
        /// The logger that handles logs made by this class.
        /// </summary>
        protected readonly Action<string>? logger;

        #endregion

        #region Methods

         /// <summary>
         /// Constructor.
         /// </summary>
         /// <param name="logger"> Logger that this class will log to.  Won't log anywhere if null. </param>
        public BluetoothController(Action<string>? logger = null)
        {
            watcher = new BluetoothLEAdvertisementWatcher();
            detectedBluetoothDevices = new List<BluetoothDeviceInfo>();
            connectedDeviceServices = new Dictionary<Guid, BluetoothDeviceServiceInfo>();
            this.logger = logger;
        }

        /// <summary>
        /// Clears out previously detected devices and detects bluetooth devices and adds their info into <see cref="detectedBluetoothDevices"/>.
        /// </summary>
        /// <param name="searchTime"> The time length that devices should be searched for. </param> 
        /// <param name="resolveNames"> Flag for if the names of devices should attempt to be resolved. </param>
        /// <returns> A task that represents the asynchronous operation which contains a list of infos of all detected bluetooth devices. </returns>
        public virtual async Task<List<BluetoothDeviceInfo>> DetectDevicesAsync(float searchTime = 1f, bool resolveNames = false)
        {
            detectedBluetoothDevices.Clear();
            watcher.Received += DetectDevice;
            watcher.Start();

            await Task.Delay(TimeSpan.FromSeconds(searchTime));

            watcher.Received -= DetectDevice;
            watcher.Stop();

            if (resolveNames)
            { 
                foreach (BluetoothDeviceInfo device in detectedBluetoothDevices)
                    await ResolveDeviceNameAsync(device);
            }

            return detectedBluetoothDevices;
        }

        /// <summary>
        /// Connects to a bluetooth device.
        /// </summary>
        /// <param name="deviceInfo"> The info of the bluetooth device. </param>
        /// <param name="maintainConnection"> Flag for if the controller should attempt to reconnect to the device if the connection is lost. </param>
        /// <returns> A task that represents the asynchronous operation which contains the boolean success of connecting to the device . </returns>
        public virtual Task<bool> ConnectToDeviceAsync(BluetoothDeviceInfo deviceInfo, bool maintainConnection = false)
        {
            return ConnectToDeviceAsync(deviceInfo.Address, maintainConnection);
        }

        /// <summary>
        /// Connects to a bluetooth device.
        /// </summary>
        /// <param name="address"> The address of the bluetooth device. </param>
        /// <param name="maintainConnection"> Flag for if the controller should attempt to reconnect to the device if the connection is lost. </param>
        /// <returns> A task that represents the asynchronous operation which contains the boolean success of connecting to the device . </returns>
        public virtual async Task<bool> ConnectToDeviceAsync(ulong address, bool maintainConnection = false)
        {
            await DisconnectAsync();

            connectedDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(address);

            if (connectedDevice == null)
                return false;

            session = await GattSession.FromDeviceIdAsync(connectedDevice.BluetoothDeviceId);

            if (session == null)
                return false;

            session.SessionStatusChanged += Session_SessionStatusChanged;
            session.MaintainConnection = maintainConnection;

            GattDeviceServicesResult serviceResult = await connectedDevice.GetGattServicesAsync();
            if (serviceResult.Status != GattCommunicationStatus.Success)
            {
                await DisconnectAsync();
                return false;
            }

            foreach (GattDeviceService service in serviceResult.Services)
            {
                GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsAsync();

                if (characteristicResult.Status != GattCommunicationStatus.Success)
                    continue;

                connectedDeviceServices.Add(service.Uuid, new BluetoothDeviceServiceInfo(service, characteristicResult));
            }

            return true;
        } 

        /// <summary>
        /// Writes to a bluetooth device.
        /// </summary>
        /// <param name="serviceUuid"> The uuid of the device's service. </param>
        /// <param name="characteristicUuid"> The uuid of the device's write characteristic. </param>
        /// <param name="data"> The data to be written, represented as an array of bytes. </param>
        /// <param name="refreshConnection"> Flag for attempting to refresh the services/characteristics if they've been disposed. </param>
        /// <returns> A task that represents the asynchronous operation which contains the boolean success of writing to the device . </returns>
        public virtual async Task<bool> WriteAsync(Guid serviceUuid, Guid characteristicUuid, byte[] data, bool refreshConnection = false)
        {
            if (connectedDevice == null)
                return false;
  
            if (!connectedDeviceServices.TryGetValue(serviceUuid, out BluetoothDeviceServiceInfo? service))
                return false; 
  
            if (!service.Characteristics.TryGetValue(characteristicUuid, out BluetoothDeviceCharacteristicInfo? characteristic))
                return false;
        
            if (!characteristic.CanWrite)
                return false;
       
            GattWriteOption option = characteristic.CanWriteWithResponse ? GattWriteOption.WriteWithResponse : GattWriteOption.WriteWithoutResponse;

            IBuffer buffer = data.AsBuffer();
            GattCommunicationStatus writeStatus;
            try
            {
                 writeStatus = await characteristic.Characteristic.WriteValueAsync(buffer, option);
            }
            catch (ObjectDisposedException)
            {
                if (!refreshConnection)
                    return false; 

                await RefreshCharacteristic(service, characteristic);

                writeStatus = await characteristic.Characteristic.WriteValueAsync(buffer, option);
            }

            if (writeStatus != GattCommunicationStatus.Success)
            {
                await RefreshCharacteristic(service, characteristic);

                writeStatus = await characteristic.Characteristic.WriteValueAsync(buffer, option);
            }

            return writeStatus == GattCommunicationStatus.Success;
        }

        /// <summary>
        /// Reads from a bluetooth device. 
        /// </summary>
        /// <param name="serviceUuid"> The uuid of the device's service. </param>
        /// <param name="characteristicUuid"> The uuid of the device's write characteristic. </param>
        /// <returns> A task that represents the asynchronous operation which contains an array of bytes of the read data. </returns>
        public virtual async Task<byte[]?> ReadAsync(Guid serviceUuid, Guid characteristicUuid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Verifies that the connected device has the requested service.
        /// </summary>
        /// <param name="serviceUuid"> The uuid of the requested service. </param>
        /// <returns> Boolean result of the connected device having the requested service. </returns>
        public virtual bool VerifyService(Guid serviceUuid)
        {
            if (connectedDevice == null)
                return false;

            return connectedDeviceServices.ContainsKey(serviceUuid);
        }

        /// <summary>
        /// Verifies that the connected device has the requested characteristic.
        /// </summary>
        /// <param name="serviceUuid"> The uuid of the service that the requested characteristic belongs to. </param>
        /// <param name="characteristicUuid"> The uuid of the requested characteristic. </param>
        /// <returns> Boolean result of the connected device having the requested characteristic. </returns>
        public virtual bool VerifyCharacteristic(Guid serviceUuid, Guid characteristicUuid)
        {
            if (connectedDevice == null)
                return false;

            if (!connectedDeviceServices.TryGetValue(serviceUuid, out BluetoothDeviceServiceInfo? service))
                return false;

            return service.Characteristics.ContainsKey(characteristicUuid);
        }

        /// <summary>
        /// Disconnects from a bluetooth device.
        /// </summary>
        /// <returns> A task that represents the asynchronous operation. </returns>
        public virtual async Task DisconnectAsync()
        {
            if (session != null)
            {
                session.SessionStatusChanged -= Session_SessionStatusChanged;
                session.MaintainConnection = false;
                session.Dispose();
                session = null;
            }

            connectedDevice?.Dispose();
            connectedDevice = null;

            foreach (BluetoothDeviceServiceInfo service in connectedDeviceServices.Values)
            {
                service.Service.Dispose();
            }

            connectedDeviceServices.Clear();
        }

        #region Protected Methods

        /// <summary>
        /// Handles a SessionStatusChanged notification.
        /// </summary>
        /// <param name="sender"> The Gatt session that sent the notification. </param>
        /// <param name="args"> The arguments of the notification. </param>
        protected virtual void Session_SessionStatusChanged( GattSession sender, GattSessionStatusChangedEventArgs args)
        {
            logger?.Invoke($"GATT Session Status Changed: {args.Status}, " + $"Device Connection: {connectedDevice?.ConnectionStatus}");
        }

        /// <summary>
        /// Subscribes to BluetoothLEAdvertisementWatcher Recieved event and handles Advertisement notifications
        /// </summary>
        /// <param name="sender"> The sender of the notification. </param>
        /// <param name="args"> The notification args. </param>
        protected virtual void DetectDevice(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            string localName = string.IsNullOrWhiteSpace(args.Advertisement.LocalName) ? BluetoothDeviceInfo.DEFAULT_NAME : args.Advertisement.LocalName;
            ulong address = args.BluetoothAddress;
            int signalStrength = args.RawSignalStrengthInDBm;
            bool isConnectable = args.IsConnectable;
            bool isScannable = args.IsScannable;
            BluetoothAddressType addressType = args.BluetoothAddressType;
            BluetoothLEAdvertisementType advertisementType = args.AdvertisementType;
            DateTimeOffset lastSeen = args.Timestamp;
            List<Guid> serviceUuids = args.Advertisement.ServiceUuids.ToList();
            List<ushort> manufacturerIds = args.Advertisement.ManufacturerData.Select(data => data.CompanyId).ToList();

            BluetoothDeviceInfo? existingDevice = detectedBluetoothDevices.Find(device => device.Address == address);
            if (existingDevice == null)
            {
                detectedBluetoothDevices.Add(new BluetoothDeviceInfo(
                    localName,
                    address,
                    signalStrength,
                    isConnectable,
                    isScannable,
                    addressType,
                    advertisementType,
                    lastSeen,
                    serviceUuids,
                    manufacturerIds));
            }
            else
            {
                if (localName != BluetoothDeviceInfo.DEFAULT_NAME &&
                    existingDevice.Name == BluetoothDeviceInfo.DEFAULT_NAME)
                {
                    existingDevice.Name = localName;
                }

                existingDevice.SignalStrength = signalStrength;
                existingDevice.Connectable = isConnectable;
                existingDevice.Scannable = isScannable;
                existingDevice.AddressType = addressType;
                existingDevice.AdvertisementType = advertisementType;
                existingDevice.LastSeen = lastSeen;

                foreach (Guid service in serviceUuids)
                {
                    if (!existingDevice.ServiceUuids.Contains(service))
                        existingDevice.ServiceUuids.Add(service);
                }

                foreach (ushort manufacturerId in manufacturerIds)
                {
                    if (!existingDevice.ManufacturerIds.Contains(manufacturerId))
                        existingDevice.ManufacturerIds.Add(manufacturerId);
                }
            }
        }

        /// <summary>
        /// Attempts to resolve the name of a bluetooth device given its device info.
        /// </summary>
        /// <param name="deviceInfo"> The <see cref="BluetoothDeviceInfo"/> of the bluetooth device. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        protected async Task ResolveDeviceNameAsync(BluetoothDeviceInfo deviceInfo)
        {
            if (deviceInfo.Name != BluetoothDeviceInfo.DEFAULT_NAME)
                return;

            BluetoothLEDevice? bluetoothDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(deviceInfo.Address);
            if (bluetoothDevice == null)
                return;

            GattDeviceServicesResult serviceResult = await bluetoothDevice.GetGattServicesAsync();
            if (serviceResult.Status != GattCommunicationStatus.Success)
            {
                bluetoothDevice?.Dispose();
                return; 
            }

            GattDeviceService? deviceNameService = serviceResult.Services.FirstOrDefault(service => service.Uuid == BluetoothUuidConstants.GenericAccessService);
            if (deviceNameService == null)
            {
                bluetoothDevice?.Dispose();
                return;
            }

            GattCharacteristicsResult? characteristicsResult = await deviceNameService.GetCharacteristicsAsync();
            if (characteristicsResult == null)
            {
                bluetoothDevice?.Dispose();
                return;
            }

            GattCharacteristic? deviceNameCharacteristic = characteristicsResult.Characteristics.FirstOrDefault
                (
                    characteristic => characteristic.Uuid == BluetoothUuidConstants.DeviceNameCharacteristic
                    && characteristic.CharacteristicProperties == GattCharacteristicProperties.Read
                );
            if (deviceNameCharacteristic == null)
            {
                bluetoothDevice?.Dispose();
                return;
            }

            GattReadResult result = await deviceNameCharacteristic.ReadValueAsync();
            byte[] bytes = result.Value.ToArray();
            string name = Encoding.UTF8.GetString(bytes);
            deviceInfo.Name = name;
            bluetoothDevice?.Dispose();
        }

        /// <summary>
        /// Refreshes a characteristic that has been disposed.
        /// </summary>
        /// <param name="serviceInfo"> The <see cref="BluetoothDeviceServiceInfo"/> of the service. </param>
        /// <param name="characteristicInfo"> The <see cref="BluetoothDeviceCharacteristicInfo"/> of the characteristic. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        protected virtual async Task RefreshCharacteristic(BluetoothDeviceServiceInfo serviceInfo, BluetoothDeviceCharacteristicInfo characteristicInfo)
        {
            if (connectedDevice == null)
                return;

            GattCharacteristicsResult characteristicResult;
            try
            {
                characteristicResult = await serviceInfo.Service.GetCharacteristicsAsync();
            }
            catch (ObjectDisposedException)
            {
                await RefreshService(serviceInfo);
                characteristicResult = await serviceInfo.Service.GetCharacteristicsAsync();
            }

            foreach (GattCharacteristic characteristic in characteristicResult.Characteristics)
            { 
                if (characteristic.Uuid == characteristicInfo.Uuid)
                {
                    characteristicInfo.RefreshCharacteristic(characteristic);
                    return;
                }
            }
        }

        /// <summary>
        /// Refreshes a service that has been disposed.
        /// </summary>
        /// <param name="serviceInfo"> The <see cref="BluetoothDeviceServiceInfo"/> of the service. </param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        protected async Task RefreshService(BluetoothDeviceServiceInfo serviceInfo)
        {
            if (connectedDevice == null)
                return;

            GattDeviceServicesResult serviceResult = await connectedDevice.GetGattServicesAsync();
            if (serviceResult.Status != GattCommunicationStatus.Success)
            {
                await DisconnectAsync();
                return;
            }

            foreach (GattDeviceService service in serviceResult.Services)
            {
                GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsAsync();

                if (characteristicResult.Status != GattCommunicationStatus.Success)
                    continue;

                if (service.Uuid == serviceInfo.Uuid)
                {
                    serviceInfo.RefreshService(service);
                    return;
                }    
            }
        }

        #endregion

        #endregion

    }

}

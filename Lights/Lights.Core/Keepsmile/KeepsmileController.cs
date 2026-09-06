using Bluetooth.Infos;
using CriticalData;
using CriticalData.Interfaces;
using FileSystem;
using Lights.Core.LED;
using Lights.Core.Serializables;
using System.Drawing;

namespace Lights.Core.Keepsmile
{

    /// <summary>
    /// Keepsmile-specific light controller for a KS03 bluetooth device.
    /// </summary>
    public class KeepsmileController : LEDController, ICriticalData
    {

        #region Fields

        #region Constants

        /// <summary>
        /// The uuid of the service that the keepsmile KS03  bluetooth device uses.
        /// </summary>
        protected static readonly Guid ServiceUuid = Guid.Parse("0000afd0-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// The uuid of the write characteristic.
        /// </summary>
        protected static readonly Guid WriteCharacteristicUuid = Guid.Parse("0000afd1-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// The uuid of the notify charactersitic.
        /// </summary>
        protected static readonly Guid NotifyCharacteristicUuid = Guid.Parse("0000afd2-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// The uuid of the read characteristic.
        /// </summary>
        protected static readonly Guid ReadCharacteristicUuid = Guid.Parse("0000afd3-0000-1000-8000-00805f9b34fb");

        /// <summary>
        /// The packet that is sent when connecting to the KS03 bluetooth device to maintain the connection.
        /// </summary>
        protected static readonly Byte[] InitializationPacket = { 0x5F, 0x01, 0x00, 0xF5 };

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// The address of the connected device.
        /// </summary>
        protected ulong? DeviceAddress { get; set; }

        /// <summary>
        /// Backing field for <see cref="RememberDevice"/>
        /// </summary>
        protected bool rememberDevice;

        /// <summary>
        /// Flag for if the connected device should be remembered or not.
        /// </summary>
        public bool RememberDevice
        {
            get { return rememberDevice; }
            set
            {
                if (!DeviceConnected)
                    return;

                if (rememberDevice == value)
                    return;

                rememberDevice = value;
                if (rememberDevice)
                    SaveCriticalData();
                else 
                    CriticalDataManager?.RemoveCriticalData();
            }
        }

        /// <summary>
        /// Flag for if there is a connected device.
        /// </summary>
        public bool DeviceConnected { get; protected set; }


        /// <summary>
        /// The current lights color.
        /// </summary>
        public Color? CurrentColor { get; protected set; }

        /// <summary>
        /// The current lights sequence.
        /// </summary>
        public KeepsmileConstants.LightsSequence CurrentLightsSequence { get; protected set; }

        /// <summary>
        /// The current brightness of the lights.
        /// </summary>
        public int CurrentBrightness { get; protected set;  }

        /// <summary>
        /// The current speed of the lights.
        /// </summary>
        public int CurrentSpeed { get; protected set; }

        /// <summary>
        /// The status of the lights being on.
        /// </summary>
        public bool LightsOn { get; protected set; }

        #endregion

        #region Methods

        #region LEDController Implementation

        /// <inheritdoc/>
        public override async Task<bool> ConnectToDeviceAsync(BluetoothDeviceInfo DeviceInfo)
        {
            if (DeviceConnected)
            {
                logger?.Invoke("already connected to a device, can't connect to multiple devices");
                return false;
            }

            if (await base.ConnectToDeviceAsync(DeviceInfo))
            {  
                bool result =  await SendPacketAsync(InitializationPacket); 
                DeviceConnected = result;
                if (result)
                {
                    DeviceAddress = DeviceInfo.Address;
                    await TurnLightsOnAsync(true);
                    await SetColorAsync(Color.Red, 100, true);
                }
                return result;
            }

            await DisconnectFromDeviceAsync();
            return false;
        }

        /// <inheritdoc/>
        protected override Task<bool> VerifyDeviceAsync()
        {
            if (!BluetoothController.VerifyService(ServiceUuid))
            {
                logger?.Invoke("device does not have correct service");
                return Task.FromResult(false);
            }

            if (!BluetoothController.VerifyCharacteristic(ServiceUuid, WriteCharacteristicUuid))
            {
                logger?.Invoke("device does not have correct write characteristic");
                return Task.FromResult(false);
            }

            if (!BluetoothController.VerifyCharacteristic(ServiceUuid, NotifyCharacteristicUuid))
            {
                logger?.Invoke("device does not have correct notify characteristic");
                return Task.FromResult(false);
            }

            if (!BluetoothController.VerifyCharacteristic(ServiceUuid, ReadCharacteristicUuid))
            {
                logger?.Invoke("device does not have correct read characteristic");
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public override async Task DisconnectFromDeviceAsync()
        {
            await base.DisconnectFromDeviceAsync();
            DeviceAddress = null;
            rememberDevice = false;
            CurrentLightsSequence = KeepsmileConstants.LightsSequence.Invalid;
            CurrentColor = null;
            CurrentBrightness = -1;
            CurrentSpeed = -1;
            LightsOn = false;
            DeviceConnected = false;
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger"> Logger that this class will log to.  Won't log anywhere if null. </param>
        public KeepsmileController(Action<string>? logger = null) : base(logger)
        {
            DeviceAddress = null;
            rememberDevice = false;
            CurrentLightsSequence = KeepsmileConstants.LightsSequence.Invalid;
            CurrentColor = null;
            CurrentBrightness = -1;
            CurrentSpeed= -1;
            LightsOn = false;
            DeviceConnected = false;
        }

        /// <summary>
        /// Connects the controller to the last saved device.
        /// </summary>
        /// <returns> A task that represents the asynchronous operation that contains a flag for successfully connecting. </returns>
        public async Task<bool> ConnectToSavedDeviceAsync()
        {
            if (DeviceConnected)
            {
                logger?.Invoke("already connected to a device, can't connect to multiple devices");
                return false;
            }

            if (DeviceAddress == null)
            {
                logger?.Invoke("No saved device available, either doesn't exist, or hasn't been loaded in.");
                return false;
            }
            if (await ConnectToDeviceAsync((ulong)DeviceAddress))
            {
                bool result = await SendPacketAsync(InitializationPacket);
                DeviceConnected = result;
                if (!result)
                    return false;

                RememberDevice = true;

                if (CurrentColor != null)
                    result = await SendPacketAsync(CreateLightPacket((Color)CurrentColor, CurrentBrightness));
                else
                    result = await SendPacketAsync(CreateLightSequencePacket(CurrentLightsSequence, CurrentSpeed, CurrentBrightness));
                return result;
            }

            await DisconnectFromDeviceAsync();
            return false;
        }

        /// <summary>
        /// Sets a new color on the lights.
        /// </summary>
        /// <param name="color"> The new color for the lights. </param>
        /// <param name="brightness"> Optional parameter to set a new brightness for the lights. </param>
        /// <param name="force"> Optional flag to force a new color update. </param>
        /// <returns> A task that represents the asynchronous operation that contains a flag for successfully updating the color. </returns>
        public async Task<bool> SetColorAsync(Color color, int brightness = -1, bool force = false)
        {
            brightness = ProcessBrightness(brightness);
            if (!force && CurrentColor == color && CurrentBrightness == brightness)
                return true;

            byte[] packet = CreateLightPacket(color, brightness);

            if (await SendPacketAsync(packet))
            {
                CurrentLightsSequence = KeepsmileConstants.LightsSequence.Invalid;
                CurrentColor = color;
                CurrentBrightness = brightness;
                SaveCriticalData();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets a new brightness on the lights.
        /// </summary>
        /// <param name="brightness"> The new brightness for the lights. </param>
        /// <param name="force"> Optional flag to force a new brightness update.</param>
        /// <returns> A task that represents the asynchronous operation that contains a flag for successfully updating the brightness. </returns>
        public async Task<bool> SetBrightnessAsync(int brightness, bool force = false)
        {
            brightness = ProcessBrightness(brightness);

            if (!force && brightness == CurrentBrightness)
                return true;

            byte[] packet;
            if (CurrentColor != null)
                packet = CreateLightPacket((Color)CurrentColor, brightness);
            else if (CurrentLightsSequence != KeepsmileConstants.LightsSequence.Invalid)
                packet = CreateLightSequencePacket(CurrentLightsSequence, CurrentSpeed, brightness);
            else
                return false;

            if (await SendPacketAsync(packet))
            {
                CurrentBrightness = brightness;
                SaveCriticalData();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets a new speed on the lights.
        /// </summary>
        /// <param name="speed"> The new speed for the lights. </param>
        /// <param name="force"> Optional flag to force a new speed update. </param>
        /// <returns> A task that represents the asynchronous operation that contains a flag for successfully updating the speed. </returns>
        public async Task<bool> SetSpeedAsync(int speed, bool force = false)
        {
            speed = ProcessSpeed(speed);

            if (!force && speed == CurrentSpeed)
                return true;

            if (CurrentLightsSequence == KeepsmileConstants.LightsSequence.Invalid)
                return false;

            byte[] packet = CreateLightSequencePacket(CurrentLightsSequence, speed, CurrentBrightness);
            if (await SendPacketAsync(packet))
            {
                CurrentSpeed = speed;
                SaveCriticalData();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets a new brightness and speed on the lights.
        /// </summary>
        /// <param name="brightness"> The new brightness for the lights. </param>
        /// <param name="speed"> The new speed for the lights. </param>
        /// <param name="force"> Optional flag to force a new brightness and speed update. </param>
        /// <returns> A task that represents the asynchronous operation that contains a flag for successfully updating the brightness and speed. </returns>
        public async Task<bool> SetBrightnessAndSpeedAsync(int brightness, int speed, bool force = false)
        {
            brightness = ProcessBrightness(brightness);
            speed = ProcessSpeed(speed);

            if (!force && brightness == CurrentBrightness && speed == CurrentSpeed)
                return true;

            if (CurrentLightsSequence == KeepsmileConstants.LightsSequence.Invalid)
                return false;

            byte[] packet = CreateLightSequencePacket(CurrentLightsSequence, speed, brightness);
            if (await SendPacketAsync(packet))
            {
                CurrentBrightness = brightness;
                CurrentSpeed = speed;
                SaveCriticalData();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Turns the lights on.
        /// </summary>
        /// <param name="force"> Optional flag to force a lights on message. </param>
        /// <returns> A task that represents the asynchronous operation that contains a flag for successfully turning the lights on. </returns>
        public async Task<bool> TurnLightsOnAsync(bool force = false)
        {
            if (!force && LightsOn)
                return true;

            byte[] packet = CreateOnOffPacket(true);
            if (await SendPacketAsync(packet))
            {
                LightsOn = true;
                SaveCriticalData();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Turns the lights off.
        /// </summary>
        /// <param name="force"> Optional flag to force a lights off message. </param>
        /// <returns> A task that represents the asynchronous operation that contains a flag for successfully turning the lights off. </returns>
        public async Task<bool> TurnLightsOffAsync(bool force = false)
        {
            if (!force && !LightsOn)
                return true;

            byte[] packet = CreateOnOffPacket(false);
            if (await SendPacketAsync(packet))
            {
                LightsOn = false;
                SaveCriticalData();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Toggles the lights on/off.
        /// </summary>
        /// <returns> A task that represents the asynchronous operation that contains a flag for successfully toggling the lights on/off. </returns>
        public async Task<bool> ToggleLightsOnOffAsync()
        {
            byte[] packet = CreateOnOffPacket(!LightsOn);
            if (await SendPacketAsync(packet))
            { 
                LightsOn = !LightsOn;
                SaveCriticalData();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets a new light sequence on the lights.
        /// </summary>
        /// <param name="sequence"> The new lights sequence. </param>
        /// <param name="speed"> Optional parameter for the lights speed. </param>
        /// <param name="brightness"> Optional parameter for the lights brightness. </param>
        /// <param name="force"> Optional flag to force a new lights sequence update. </param>
        /// <returns> A task that represents the asynchronous operation that contains a flag for successfully setting the lights sequence. </returns>
        public async Task<bool> SetLightsSequenceAsync(KeepsmileConstants.LightsSequence sequence, int speed = -1, int brightness = -1, bool force = false)
        {
            if (!force && CurrentLightsSequence == sequence && speed == CurrentSpeed && brightness == CurrentBrightness)
                return true; ;

            speed = ProcessSpeed(speed);
            brightness = ProcessBrightness(brightness);
            byte[] packet = CreateLightSequencePacket(sequence, speed, brightness);
            if (await SendPacketAsync(packet))
            {
                CurrentColor = null;
                CurrentLightsSequence = sequence;
                CurrentSpeed = speed;
                CurrentBrightness = brightness;
                SaveCriticalData();
                return true;
            }
            return false;
        }

        #region Protected/Private Methods

        /// <summary>
        /// Creates an on/off byte packet. 
        /// </summary>
        /// <param name="on"> Flag for creating the packet as on (true) or off (false). </param>
        /// <returns> The on/off byte packet. </returns>
        protected byte[] CreateOnOffPacket(bool on)
        {
            byte onOffByte = on ? (byte)0xF0 : (byte)0x0F;
            return new byte[]
                {
                    0x5B,
                    onOffByte,
                    0x00,
                    0xB5
                };
        }

        /// <summary>
        /// Creates a light byte packet.
        /// </summary>
        /// <param name="color"> The color of the light packet. </param>
        /// <param name="brightness"> The brightness of the light packet. </param>
        /// <returns> The light byte packet. </returns>
        protected byte[] CreateLightPacket(Color color, int brightness)
        {
            return new byte[]
                {
                    0x5A,
                    0x00,
                    0x01,
                    color.R,
                    color.G,
                    color.B,
                    0x00,
                    (byte)brightness,
                    0x00,
                    0xA5
                };
        }

        /// <summary>
        /// Creates a light sequence  byte packet.
        /// </summary>
        /// <param name="color"> The speed of the light sequence  packet. </param>
        /// <param name="brightness"> The brightness of the light sequence  packet. </param>
        /// <returns> The light sequence  byte packet. </returns>
        protected byte[] CreateLightSequencePacket(KeepsmileConstants.LightsSequence lightSequence, int speed, int brightness)
        {
            return new byte[]
            {
                0x5C,
                0x00,
                (byte)lightSequence,
                (byte) speed,
                (byte)brightness,
                0x00,
                0xC5
            };
        }

        /// <summary>
        /// Sends a byte packet to the keepsmile bluetooth device.
        /// </summary>
        /// <param name="packet"> The byte packet. </param>
        /// <returns> A task that represents the asynchronous operation that contains a flag a flag for successfully sending the byte packet. </returns>
        protected Task<bool> SendPacketAsync(byte[] packet)
        {
            return BluetoothController.WriteAsync(ServiceUuid, WriteCharacteristicUuid, packet);
        }

        /// <summary>
        /// Processes brightness value into a value between 0 and 100.
        /// </summary>
        /// <param name="brightness"> Passed in brightness value. </param>
        /// <returns> Processed brightness int between 0 and 100. </returns>
        protected int ProcessBrightness(int brightness) 
        {
            if (brightness == -1)
                brightness = CurrentBrightness;

            if (brightness < 0)
                brightness = 0;
            else if (brightness > 100)
                brightness = 100;

            return brightness;
        }

        /// <summary>
        /// Processes speed value into a value between 0 and 100.
        /// </summary>
        /// <param name="speed"> Passed in speed value. </param>
        /// <returns> Processed speed int between 0 and 100. </returns>
        protected int ProcessSpeed(int speed)
        {
            if (speed == -1)
                speed = CurrentBrightness;

            if (speed < 0)
                speed = 0;
            else if (speed > 100)
                speed = 100;
            return speed;
        }

        #region ICriticalData Implementation

        /// <inheritdoc/>
        public CriticalDataManager? CriticalDataManager { get; set; }

        /// <inheritdoc/>
        public virtual void Serialize(Stream stream)
        {
            Serializer.Write(stream, DeviceAddress);
            Serializer.Write(stream, (int)CurrentLightsSequence);
            Serializer.Write(stream, new SerializableColor(CurrentColor));
            Serializer.Write(stream, CurrentBrightness);
            Serializer.Write(stream, CurrentSpeed);
            Serializer.Write(stream, LightsOn);
        }

        /// <inheritdoc/>
        public virtual void Deserialize(Stream stream) 
        {
            DeviceAddress = Serializer.Read<ulong>(stream);
            CurrentLightsSequence = (KeepsmileConstants.LightsSequence)Serializer.Read<int>(stream);
            CurrentColor = Serializer.Read<SerializableColor>(stream).GetColor();
            CurrentBrightness = Serializer.Read<int>(stream);
            CurrentSpeed = Serializer.Read<int>(stream);
            LightsOn = Serializer.Read<bool>(stream);
        }

        /// <inheritdoc/>
        public virtual void SaveCriticalData()
        {
            if (CriticalDataManager == null)
                return;
            CriticalDataManager.SaveCriticalData();
        }

        #endregion

        #endregion

        #endregion

    }

}

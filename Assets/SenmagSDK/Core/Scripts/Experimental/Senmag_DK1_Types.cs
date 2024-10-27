using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;
using Senmag_DK1_USBComms;
using System.Net;
using System.Net.Sockets;
using System.Drawing;
using SenmagHaptic;
using UnityEngine;


namespace Senmag_DK1_Types
{
	public class DK1_Client
	{
		public Image applicationIcon;
		public IPEndPoint endpoint;
		public byte clientID;
		public string clientName = "Unknown client";
		public float skdVersion;

		public float[] forceTarget = new float[3];
		/*public string helloMessage;
		public int clienIpAddress;
		
		public DK1_ServerPacketType packetType = new DK1_ServerPacketType();
		public string appName;*/
	}

	public enum DK1_ServerPacketType
	{
		hello = 0,                  // device responding to search
		logo = 1,
		deviceStatus = 2,
		forceTarget = 3,
	}

	public enum DK1_UsbPacketType
	{
		error = 0,					// operation completed successfully
		ack = 1,					// an error occoured (contains error type)
		searchDevices = 2,          // packet is a search for devices
		deviceConfig = 3,			// packet contains the config of a device (MantisDeviceConfig)
		saveConfig = 4,				// packet asks the devices to save its config
		setCalibrationState = 5,	// packet sets the device's state (e.g. calibration)
		deviceStatus = 6,			// packet sets the devices mode
		deviceTargets = 7,			// packet instructs device to reboot into DFU mode for firmware upload
		resetDFU = 8,
		factoryReset = 9,			// factory reset
		firmwareData = 10,			// data for new firmware
	}

	public enum DK1_ErrorCodes
	{
		usbcommsCRC = 0,						//crc error with usb
		sensor_commsCRC = 1,					//crc error with sensor
		sensor_commsLost = 2,					//lost comms with sensor
		sensor_firmwareIncompatible = 3,		//sensor firmware mismatch
		sensor_endEffectorCommsError = 4,		//comms error with the stylus
		sensor_endEffectorUnplugged = 5,		//no response from end effector
		encoder_unstable = 6,					//errors detected with an encoder
		encoder_connectionLost = 7,				//encoder connection lost
		controller_overheat = 8,				//overheat detected
		controller_notCalibrated = 9,			//calibration needed
	}

	public struct DK1_PIDConfig
	{
		public float	rateLimit;
		public float	KGain;
		public float	IGain;
		public float	ILimit;
		public float	DGain;
		
	}

	public enum DK1_DeviceModes
	{
		estop = 0,                  // device is fully disengaged
		bootloader = 1,             // device is in bootloader mode
		errorFatal = 2,             // device is in an unrecoverable error state
		errorLight = 3,             // device is flagging an error, but is ok to continue
		calibrationMode = 4,        // device is in calibration mode
		idle = 5,                   // device is in idle mode
		forcedPause = 6,            // device has forced a pause (e.g. stylus disconnected)
		active = 7,                 // device is active
	};

	public enum DK1_EndEffectors
	{
		none = 0,
		stylus = 1,
		thimble = 2,
	}

	public enum DK1_CalibrationStates
	{
		angleAlignment = 0,
		stylusThreshold = 1,
		encoderAlignment = 2,
        stylusGryo = 3,
    }

	/*public class DK1_ApplicationTargets //(server -> Unity)
	{
		public byte deviceID = new byte();                               //the ID of the device in the server

		public DK1_DeviceModes currentMode = new DK1_DeviceModes();		// current mode the device is in
		public float[] currentPosition = new float[3];					// the current position of a device after position and rotation	offsets - mm.		(cartesian [x, y, z])							(read only)
		public float[] currentRotation = new float[4];					// the current rotation of a device	after position and rotation	offsets - rad.		(quartonian [w, i, j, k])						(read only)

		public UInt16 framerate;										// the device's current framerate (Hz)
		public UInt16 framerateVarience;								// the varience of the device's framerate (% * 100)

		public byte stylusState;										// the state of a stylus (button clicks)																							(read only)
	}*/

	public class DK1_ApplicationTargets // to send data from server to Unity
	{
		public byte deviceID = new byte();                               //the ID of the device in the server
		public char[] deviceName = new char[20];
		public DK1_DeviceModes currentMode = new DK1_DeviceModes();        // current mode the device is in
		public float[] currentPosition = new float[3];                    // the current position of a device after position and rotation offsets - mm. (cartesian [x, y, z])                                (read only)
		public float[] currentRotation = new float[4];                    // the current rotation of a device    after position and rotation offsets - rad. (quartonian [w, i, j, k])                        (read only)

		public float[] currentForce = new float[3];                     //[x, y,z] force data coming from unity - to be sent to robot

		public UInt16 framerate;                                        // the device's current framerate (Hz)
		public UInt16 framerateVarience;                                // the varience of the device's framerate (% * 100)

		public byte stylusState;                                        // the state of a stylus (button clicks)                                                                                             (read only)

		public bool innerBounderiesOK;
		public bool outerBounderiesOK;

		public DK1_EndEffectors endEffector = new DK1_EndEffectors();
	}


	public class DK1_USBDeviceTargets //(Unity -> Server)
	{
		public byte deviceID = new byte();								//the ID of the device in the server
		public float[] targetForce = new float[3];						// the current force targets for a device - gramms									(cartesian [x, y, z, roll, pitch, yaw])			(read-write)
	}

	public class DK1_DeviceState
	{
		public DK1_DeviceModes		currentMode = new DK1_DeviceModes();    // current mode the device is in
		public float[]				currentPosition = new float[3];			// the current position of a device after position and rotation	offsets - mm.		(cartesian [x, y, z])							(read only)
		public float[]				currentRotation = new float[4];			// the current rotation of a device	after position and rotation	offsets - rad.		(quartonian [w, i, j, k])						(read only)
		public float[]				targetForce = new float[3];             // the current force targets for a device - gramms									(cartesian [x, y, z, roll, pitch, yaw])			(read-write)

		public UInt16				framerate;								// the device's current framerate (Hz)

		public byte					stylusState;                            // the state of a stylus (button clicks)																							(read only)

		public UInt16				statusFlags;


		public bool					calibrationOk;
		public bool					innerBounderiesOk;
		public bool					outerBounderiesOK;
		public bool					encoder0OK;
		public bool					encoder1OK;
		public bool					encoder2OK;
		public bool					sensorBoardOK;
		public bool					endEffectorAttached;
		public bool					endEffectorOK;
	}

	public class DK1_KinematicConfig
	{
		public float lengthToElbow;										// the length between the base joint and elbow joint				(mm)
		public float lengthToTip;											// the length between the elbow joint and tip of the device			(mm)
		public float mass1;												// the mass of the linkage between the base and elbow				(gramms)
		public float mass2;												// the mass of the linkage between the elbow and tip				(gramms)
		public float gravityCompensation;                                   // tha gain of the gravity compensation
		public float inertiaCompensation;                                   // the gain setting for inertia compensation
		public float accelerationFilter;									// the frequency of the filter applied to acceleration detection
		
		public float[] frictionCompensation = new float[3];					// the gain settings for friction compensation (Base, Shoulder, Elbow)
		//public float positionFilter;										// the frequency of the filter applied to position measurements
	}

	
	

	public class DK1_DeviceConfig
	{
		public const int DK1_DEVICENAME_CHARACTERCOUNT = 20;
		public const int DK1_DEVICEFAMILY_CHARACTERCOUNT = 20;
		public const int DK1_DEVICESERIALNUM_CHARACTERCOUNT = 15;


		public char[] deviceName = new char[DK1_DEVICENAME_CHARACTERCOUNT];			// a user-set name for the device		
		

		public UInt16				maxForce;								// maximum force target												(MiliNewtons)						
		public byte					powerLimit;								// maximum motor power to use										(%)									
		public UInt16				targetFramerate;                        // the device's target framerate (Hz)
		public UInt16				sleepTimer;								// the time before the device will automatically enter sleep mode
		public DK1_EndEffectors		endEffectorType = new DK1_EndEffectors();   // end effector type
		public float				positionFilter;							//the strength of the position filtering
		public float				tempThrottling;							// the strength of temperature throttling
		public float				firmwareVersion;						// the firmware version
		public byte					usbDebugConfiguration;                  //debug options

		public DK1_PIDConfig[] pidConfig = new DK1_PIDConfig[3];               // the PID configuration of the device

		public DK1_KinematicConfig	kinematicConfig = new DK1_KinematicConfig();	// the kinematics configuration items for a device
		

																							
		
		public DK1_DeviceRoConnfig	readOnlyConfig = new DK1_DeviceRoConnfig();     // read-only configuration items
		public bool					deviceCalibrated;

		
	}

	public class DK1_DeviceRoConnfig
	{
		public byte		isCalibrated;						// does the device need to be calibrated?
		public UInt32	controllerFirmwareVersion;			// firmware version of the main controller
		public UInt32	sensorFirmwareVersion;				// firmware version of the sensor board
		public char[]	deviceFamily = new char[32];		// the family of device (Mantis Desktop / Mantis footpedal etc)
		public char[]	serialNumber = new char[32];		// the serial number of a device
	}


	public class DK1_Device
	{
		public DK1_DeviceState state = new DK1_DeviceState();
		public DK1_DeviceConfig config = new DK1_DeviceConfig();
		public DK1_DeviceConfig defaultConfig = new DK1_DeviceConfig();
		public DK1_USBComms usbComms = new DK1_USBComms();  //library to process incoming packets

		public DK1_USBDeviceTargets totalForceTarget = new DK1_USBDeviceTargets();
		public List<DK1_USBDeviceTargets> forceTargets = new List<DK1_USBDeviceTargets>();

		public bool awaitingConfigUpdate = false;
		public bool unsavedConfig = true;

		public DK1_DeviceConfig getDefaultConfig()
		{
			DK1_DeviceConfig defaults = new DK1_DeviceConfig();
			defaults.maxForce = 10000;
			defaults.powerLimit = 75;
			defaults.targetFramerate = 1000;
			defaults.endEffectorType = DK1_EndEffectors.stylus;
			defaults.kinematicConfig.frictionCompensation[0] = 5.0f;
			defaults.kinematicConfig.frictionCompensation[1] = 5.0f;
			defaults.kinematicConfig.frictionCompensation[2] = 5.0f;
			defaults.kinematicConfig.gravityCompensation = 0.06f;
            defaults.kinematicConfig.lengthToElbow = 220;
			defaults.kinematicConfig.lengthToTip = 230;
			defaults.kinematicConfig.mass1 = 300;
			defaults.kinematicConfig.mass2 = 300;
			defaults.kinematicConfig.accelerationFilter = 0.03f;
			defaults.kinematicConfig.inertiaCompensation = 0.1f;
			defaults.positionFilter = 1.0f;

			defaults.endEffectorType = DK1_EndEffectors.none;
			defaults.sleepTimer = 120;
			defaults.deviceName = "9D Stylus DK1".ToCharArray();
			defaults.deviceCalibrated = false;
			defaults.usbDebugConfiguration = 0;

			defaults.pidConfig[0].rateLimit = 2.0f;
			defaults.pidConfig[0].KGain = 4.0f;
			defaults.pidConfig[0].IGain = 0f;
			defaults.pidConfig[0].ILimit = 0f;
			defaults.pidConfig[0].DGain = 5.0f;

			defaults.pidConfig[1].rateLimit = 5.0f;
			defaults.pidConfig[1].KGain = 4.0f;
			defaults.pidConfig[1].IGain = 0.0f;
			defaults.pidConfig[1].ILimit = 0.0f;
			defaults.pidConfig[1].DGain = 40f;

			defaults.pidConfig[2].rateLimit = 3.0f;
			defaults.pidConfig[2].KGain = 2.0f;
			defaults.pidConfig[2].IGain = 0.0f;
			defaults.pidConfig[2].ILimit = 0.0f;
			defaults.pidConfig[2].DGain = 2.0f;
			return defaults;
		}
	}
    public enum Senmag_DeviceTypes
    {
        DK1 = 0,
        //add more devices here...
    }

    public class Senmag_USBDevice
    {
        public DK1DeviceState state = new DK1DeviceState();
        public DK1_DeviceTargets targets = new DK1_DeviceTargets();
        public GameObject cursor;// = new Senmag_HapticCursor();

		public bool newTargets;
        public bool newDevice;
        public byte deviceID;
        public Image deviceIcon;                            //icon to use for the device
        public Senmag_DeviceTypes deviceType;               //type of this device
        public DK1_Device deviceData = new DK1_Device();    //
        public event EventHandler newDeviceStatusHandler;

        public void initialiseDevice(Senmag_DeviceTypes deviceType)
        {
            deviceData.defaultConfig = deviceData.getDefaultConfig();
            deviceData.config = deviceData.getDefaultConfig();

            deviceData.state.sensorBoardOK = false;
            deviceData.state.endEffectorOK = false;
            deviceData.state.encoder0OK = false;
            deviceData.state.encoder1OK = false;
            deviceData.state.encoder2OK = false;
            deviceData.state.calibrationOk = false;

            if (deviceType == Senmag_DeviceTypes.DK1)
            {
                //deviceIcon = Properties.Resources.DK1_Icon;
            }
            while (deviceData.usbComms.serialPort.BytesToRead > 0)
            {
                byte[] tmpData = new byte[1];
                deviceData.usbComms.serialPort.Read(tmpData, 0, 1);
            }

            deviceData.usbComms.serialPort.DataReceived += new SerialDataReceivedEventHandler(usbRxHandler);
        }

        public void setdeviceCalibrationState(int calibrationState)
        {
            deviceData.usbComms.send_setCalibrationState(calibrationState);
        }

        public void usbRxHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                while (deviceData.usbComms.serialPort.BytesToRead > 0)
                {

                    byte[] tmpData = new byte[1];
                    deviceData.usbComms.serialPort.Read(tmpData, 0, 1);

                    int result = deviceData.usbComms.processByte(tmpData[0], deviceData.usbComms.usbData);

                    if (result == 1)
                    {
                        deviceData.usbComms.serialPort.DiscardInBuffer();
                        if (deviceData.usbComms.usbData.opacket.type == (byte)DK1_UsbPacketType.error)
                        {
                            string message = "";
                            for (int x = 0; x < deviceData.config.deviceName.Length; x++)
                            {
                                if (deviceData.config.deviceName[x] == '\0') break;
                                message += deviceData.config.deviceName[x];
                            }
                            message += ": ";
                            for (int x = 0; x < deviceData.usbComms.usbData.opacket.dataLength; x++) message += (char)deviceData.usbComms.usbData.opacket.data[1 + x];


                            Console.WriteLine(String.Format("Debug msg (class: {0}) from {1}", deviceData.usbComms.usbData.opacket.data[0], message));
                        }

                        else if (deviceData.usbComms.usbData.opacket.type == (byte)DK1_UsbPacketType.ack)
                        {
                            deviceData.usbComms.processAck();
                            Console.WriteLine(String.Format("got ack from {0}", new string(deviceData.config.deviceName)));
                        }

                        else if (deviceData.usbComms.usbData.opacket.type == (byte)DK1_UsbPacketType.deviceConfig)
                        {
                            Console.WriteLine(String.Format("got config from {0}", new string(deviceData.config.deviceName)));
                        }

                        else if (deviceData.usbComms.usbData.opacket.type == (byte)DK1_UsbPacketType.deviceStatus)
                        {
                            deviceData.state = deviceData.usbComms.processDeviceState(deviceData.usbComms.usbData);

                            newDeviceStatusHandler.Invoke(this, null);      //call event handler to forward the new status to applciations
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }
    }

}




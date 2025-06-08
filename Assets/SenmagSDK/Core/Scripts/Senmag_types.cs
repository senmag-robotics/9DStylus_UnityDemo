/*		Senmag_types.cs, Version 1.0
 *		G.Barnaby, Senmag Robotics, 27/05/2023
 *		
 *		Contains definitions for datatstructures used to store and pass information for 9D stylus systems.
 * 
 * */
using System;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.UI;
using System.Drawing;
using System.IO;
using UnityEditor;
using UnityEngine.Rendering;
using System.Security.Cryptography;

namespace SenmagHaptic
{
    public class LPFilter
	{
		public float a = 0;
		private float yprev = 0;

		public void init(float aval) {
			a = aval;
			yprev = 0;
		}

		public float update(float value)
		{
			yprev = (1 - a) * yprev + value * a;// (1 − a);// * yprev + input ∗ a;
			return yprev;
		}
	}

    public enum Senmag_DeviceTooltip
    {
		senmagTooltip_auto = 0,
        senmagTooltip_none = 1,
        senmagTooltip_v1Stylus = 2,
        senmagTooltip_v2Stylus = 3,
    }

    public enum SenmagDeviceType
	{
        senmagDeviceType_unknown = 0,
        senmagDeviceType_DK1 = 1,
		senmagDeviceType_LibreOne = 2,
	}

	public class SenmagDeviceStatusFlags
	{
		public int combined = new byte();

        public byte toolConnected = new byte();			//flags if the tooltip is disconnected
        public byte innerBoundaries = new byte();       //flags if the device is touching its own inner bounderies
        public byte outerBounderies = new byte();       //flags if the device is touching its own outer bounderies
        public byte toolError = new byte();         //flags if the tooltip is disconnected
        public byte sensorBoardError = new byte();			//flags if the tooltip is disconnected


        public byte calibrationOk = new byte();			//flags an issue with the device's calibration
        public byte encoder0OK = new byte();			//flags an issue with the device's encoders
        public byte encoder1OK = new byte();			//flags an issue with the device's encoders
        public byte encoder2OK = new byte();			//flags an issue with the device's encoders




    }

	public class SenmagDeviceSoftSettings
	{
        public float[] forceGains = new float[3] { 1, 1, 1 };
        public float[] positionGains = new float[3]{1, 1, 1};
	}
	public class SenmagDeviceStatus
	{


        
        public byte						stylusButtons = new byte();						//the state of the stylus buttons
        public float[]					currentPosition = new float[3];					//the current position of a device after position and rotation offsets - mm. (cartesian [x, y, z])                                (read only)
        public float[]					currentOrientation = new float[4];				//the current rotation of a device    after position and rotation offsets - rad. (quartonian [w, i, j, k])                     (read only)
		public SenmagDeviceStatusFlags	statusFlags = new SenmagDeviceStatusFlags();    //status flags raised by the device
		public Senmag_DeviceTooltip		currentTooltip = new Senmag_DeviceTooltip();	//the current tool atatched to the device

    }

    public class Senmag_DeviceTargets // send data from Unity to server
    {
        public float[]					targetForce = new float[3];                        // the current force targets for a device - gramms                                (cartesian [x, y, z, roll, pitch, yaw])         (read-write)
		public byte						targetTypes = new byte();
    }

    public class SenmagDevice
	{
        
        SenmagDeviceType				deviceType = new SenmagDeviceType();		//the type of device
        public char[]					deviceName = new char[20];					//the user-configurable device name
		public volatile bool			newStatus = new bool();						//flag set when a new status update has been rewceived from the device
        public bool						newDevice = new bool();						//flag set when the device is first discovered (used to generate cursors)
		public SenmagDeviceStatus		deviceStatus = new SenmagDeviceStatus();    //the current status of the device
        public Senmag_DeviceTargets		deviceTargets = new Senmag_DeviceTargets(); //the targets to be sent to the device
        public int						zeroCounter = new int();					//a counter used to ensure 

        

        public Senmag_USBComms			usbComms = new Senmag_USBComms();           //the communications interface associated with this device

		//public LibreOneComms libreOneComms = new LibreOneComms();
        public SenmagDeviceSoftSettings softSettings = new SenmagDeviceSoftSettings();

        public GameObject				cursor;										//the cursor gameobject associated with this device

		public SenmagDevice()
		{
			usbComms.newStatus = readStatusFromUSB;
        }

		public void close()
		{
			if (cursor.gameObject != null) UnityEngine.Object.Destroy(cursor.gameObject);
			if (usbComms.serialPort.IsOpen) usbComms.serialPort.Close();
        }

		public void readStatusFromUSB()
		{
			deviceType = usbComms.deviceType;
			deviceStatus = usbComms.deviceStatus;
            for (int x = 0; x < 3; x++) deviceStatus.currentPosition[x] = (usbComms.deviceStatus.currentPosition[x] * softSettings.positionGains[x] / 1000f);
            for (int x = 0; x < 4; x++) deviceStatus.currentOrientation[x] = usbComms.deviceStatus.currentOrientation[x];
            //direct write to cursor here?
        }
        public int setTargets()
        {
			for (int x = 0; x < 3; x++) deviceTargets.targetForce[x] *= softSettings.forceGains[x] / softSettings.positionGains[x];

            if (zeroCounter > 0) {
				float threshold = 1;
				if (Math.Abs(deviceTargets.targetForce[0]) < threshold && Math.Abs(deviceTargets.targetForce[1]) < threshold && Math.Abs(deviceTargets.targetForce[2]) < threshold){
					zeroCounter--;
				}
				deviceTargets.targetForce[0] = 0;
				deviceTargets.targetForce[1] = 0;
				deviceTargets.targetForce[2] = 0;
			}
			return 0;
            //return usbComms.sendTargets(deviceTargets);
        }
    }

















    // Used to define the device mode
    /*public enum DK1_DeviceModes {
		estop = 0,                  // device is fully disengaged
		bootloader = 1,             // device is in bootloader mode
		errorFatal = 2,             // device is in an unrecoverable error state
		errorLight = 3,             // device is flagging an error, but is ok to continue
		calibrationMode = 4,        // device is in calibration mode
		idle = 5,                   // device is in idle mode
		forcedPause = 6,            // device has forced a pause (e.g. stylus disconnected)
		active = 7,                 // device is active

	};*/
    // Used to define the type of a serial packet
	public enum DK1_ServerPacketType
	{
		serverPacketType_hello = 0,                  // device responding to search
		serverPacketType_icon = 1,
		serverPacketType_deviceStatus = 2,
		serverPacketType_deviceTarget = 3,
	}

	public struct SenmagDeviceAddress{
		public string ip;
		public int port;
		LPFilter filter;
	}
	/*
	public class SenmagDevice
	{
		public bool newTargets = false;
		public bool newDevice = true;
		public DK1DeviceState state = new DK1DeviceState();
		public DK1_DeviceTargets targets = new DK1_DeviceTargets();
		public GameObject cursor;// = new Senmag_HapticCursor();
	}*/
	
	/*public class DK1DeviceState
	{
		public bool dataLock = false;
		public byte deviceID = new byte();                               //the ID of the device in the server
		public char[] deviceName = new char[20];
		public DK1_DeviceModes currentMode = new DK1_DeviceModes();        // current mode the device is in
		public float[] currentPosition = new float[3];                    // the current position of a device after position and rotation offsets - mm. (cartesian [x, y, z])                                (read only)
		public float[] currentOrientation = new float[4];                    // the current rotation of a device    after position and rotation offsets - rad. (quartonian [w, i, j, k])                        (read only)

		public float[] targetForce = new float[3];                     //[x, y,z] force data coming from unity - to be sent to robot

		public UInt16 framerate;                                        // the device's current framerate (Hz)
		public UInt16 framerateVarience;                                // the varience of the device's framerate (% * 100)

		public byte stylusState;                                        // the state of a stylus (button clicks)                                                                                             (read only)

		public bool innerBounderiesOK;
		public bool outerBounderiesOK;

		public Senmag_DeviceEndEffectors endEffector = new Senmag_DeviceEndEffectors();
	}*/



	

	
}
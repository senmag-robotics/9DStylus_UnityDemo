/*		Senmag_types.cs, Version 1.0
 *		G.Barnaby, Senmag Robotics, 27/05/2023
 *		
 *		Contains definitions for datatstructures used to store and pass information for 9D stylus systems.
 * 
 * */
using System;
using System.Net;
using UnityEngine;

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

	// Used to define the device mode
	public enum DK1_DeviceModes {
		/*estop =					0,		// device is fully disengaged
		idle =					1,		// device is in idle mode
		forceControlReady =		2,		// device is ready in force control mode
		positionControlReady =	3,		// device is ready in position control mode
		forceControlRunning =	4,		// device is running force control mode
		positionControlRunning =5,		// device is running position control mode
		calibrateForce =		6,      // calibrate the force sensors
		calibratePosition1 =	7,		// calibrate the position sensors, alignment
		calibratePosition2 =	8,		// calibrate the position sensors, direction
		calibrateTactile =		9,		// calibrate the tactile system*/

		estop = 0,                  // device is fully disengaged
		bootloader = 1,             // device is in bootloader mode
		errorFatal = 2,             // device is in an unrecoverable error state
		errorLight = 3,             // device is flagging an error, but is ok to continue
		calibrationMode = 4,        // device is in calibration mode
		idle = 5,                   // device is in idle mode
		forcedPause = 6,            // device has forced a pause (e.g. stylus disconnected)
		active = 7,                 // device is active

	};
	// Used to define the type of a serial packet
	public enum DK1_ServerPacketType
	{
		hello = 0,                  // device responding to search
		logo = 1,
		deviceStatus = 2,
		forceTarget = 3,
	}

	public struct MantisDeviceAddress{
		public string ip;
		public int port;
		LPFilter filter;
	}

	public class SenmagDevice
	{
		public bool newTargets = false;
		public bool newDevice = true;
		public DK1DeviceState state = new DK1DeviceState();
		public DK1_DeviceTargets targets = new DK1_DeviceTargets();
		public GameObject cursor;// = new Senmag_HapticCursor();
	}
	
	public class DK1DeviceState
	{
		public bool dataLock = false;
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



	public enum DK1_EndEffectors
	{
		V1Stylus = 0,
		
	}

	public class DK1_DeviceTargets // send data from Unity to server
	{
		public float[] targetForce = new float[3];                        // the current force targets for a device - gramms                                (cartesian [x, y, z, roll, pitch, yaw])         (read-write)
	}
}
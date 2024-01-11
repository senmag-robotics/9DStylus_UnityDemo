/* Senmag Workspace.cs, Version 1.0
 * G.Barnaby, Senmag Robotics, 27/05/2023
 * 
 * Attach this script to an empty gameObject. it will:
 *		- Automatically connect to the Senmag Server,
 *		- Detect 9D stylus devices,
 *		- Generate and update haptic cursors in your project
 *		*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
//using SenmagTypes;
using System.Net;
using UnityEngine.UI;
using System.Drawing;
using System.IO;

namespace SenmagHaptic
{

	public static class SenmagSDK_Metadata
	{
		public const float sdk_version = 1.001f;
	}

	public class cursor
	{
		public GameObject cursorParent;
	};

    public class Senmag_Workspace : MonoBehaviour
    {
       public  Senmag_Server		osenmagServer = new Senmag_Server();
		Thread				osocketThread;// = new Thread;

		[Header("Application Settings")]
		public string		applicationName = "Unity Application";
		//public Image		applicationLogo = new Image("");
		
		//public Bitmap		applicationIcon = new Bitmap(@"C:\Users\spyro\Documents\StartUp\CodePhotos\UnityLogo.jpg");
		public Texture2D	applicationIcon;
		//public Bitmap		applicationIcon = new Bitmap(System.IO.Directory.GetFiles(Application.dataPath, "UnitySDKIcon.png.cs")[0]);
		//new Bitmap(@"C:\Users\spyro\Documents\StartUp\CodePhotos\UnityLogo.jpg");


		[Header("Haptics Settings")]
		public float		positionFilterStrength = 0.2f;		//lower value is a stronger filter
		public float		maximumForce = 1000;
		public float		physicsFramerate = 200;
		public int			physicsIterations = 15;
		public float		hapticStiffness=1;
		public float		spatialMultiplier=1;

		[Header("Cursor Settings")]
		
		public GameObject	defaultCursorModel;
		public GameObject	defaultRightClickMenu;

		public float		teleportThreshold = 0.5f;			//if the distance to target exceeds this value, the cursor will teleport to the target
		public float		cursorScale = 0.1f;
		public float		cursorFrictionStatic = 0.5f;
		public float		cursorFrictionDynamic = 0.5f;
		public bool			allowCustomCursors;

		public float		antigrav = 1f;		//a constant upwards force applied to counter gravity

		bool				oappRunning;
		bool				oupdatedForces;
		bool				ostreamingEnabled;


		long timeLast;

		// Start is called before the first frame update
		void Start()
        {
			if(applicationIcon == null)			//if a custom icon is not specified...
			{
				applicationIcon = Resources.Load("Graphics/UnitySDKIcon_50px") as Texture2D;
				if(applicationIcon == null) UnityEngine.Debug.Log("Failed to load default app icon from Senmag SDK...");
				UnityEngine.Debug.Log("Loaded default application icon from Senmag SDK...");
			}
		
			//this.GetComponent<MeshRenderer>().material.mainTexture = applicationIcon;
			//this.GetComponent<MeshRenderer>().enabled = true;

			if(defaultRightClickMenu == null)
			{
				defaultRightClickMenu = Resources.Load("MenuPrefabs/Senmag_RadialMenu_RC_Default") as GameObject;
				if (defaultCursorModel == null) UnityEngine.Debug.Log("Failed to load default right click menu from Senmag SDK...");
				else UnityEngine.Debug.Log("Loaded default right click menu from Senmag SDK...");
			}

			if (defaultCursorModel == null){	//if a custom cursor is not specified...
				defaultCursorModel = Resources.Load("EndEffectors/DK1_Stylus_V1/DK1_Stylus_prefab") as GameObject;
				if(defaultCursorModel == null) UnityEngine.Debug.Log("Failed to load default cursor from Senmag SDK...");
				else UnityEngine.Debug.Log("Loaded default cursor model from Senmag SDK...");
			}
			Time.fixedDeltaTime = (float)1.0 / physicsFramerate;				//set physics framerate
			Physics.defaultSolverIterations = physicsIterations;            //set physics iterations

			//Application.targetFrameRate = (int)targetFramerate;
			//Time.DeltaTime = (float)1.0 / targetFramerate;

			oappRunning = true;												//flag app running
			osenmagServer.spatialMultiplier = spatialMultiplier;
			osenmagServer.openSocket(applicationName, applicationIcon);		//open the server port

			//osocketThread = new Thread(new ThreadStart(socketRecieve));		//start UDP client rx handler
			//osocketThread.Start();

			ostreamingEnabled = true;
			ostreamingEnabled = true;

			//omantisServer.sendBroadcast();									//search for devices
		}



		// Update is called once per frame
		void Update()
        {
			osenmagServer.checkTimeouts();
			if (osenmagServer.serverConnected == 0)
			{
				if(osenmagServer.oserverConnectRetryCounter == 0)
				{
					osenmagServer.sendHelloPacket();
				}
				osenmagServer.oserverConnectRetryCounter += 1;
				if(osenmagServer.oserverConnectRetryCounter > 5.0f / Time.deltaTime) osenmagServer.oserverConnectRetryCounter = 0;
			}

			

			//if (Input.GetKeyDown("space"))                                  //search for devices on spacebar press
			if (Input.GetKeyDown(KeyCode.F5))                                  //search for devices on spacebar press
			{
			    //Debug.Log("sending broadcast...");
				//omantisServer.mantisDevices.Clear();						//erase current device list
				//ocursors.Clear();											//erase ocursors

				//int serverPort = osenmagServer.getServerPort();
				//string serverPortString = serverPort.ToString();
				//UnityEngine.Debug.Log("Port: " + serverPortString);

				/*
				try
				{
					osenmagServer.deviceList.Add(new DK1_Device());
					osenmagServer.deviceList[0].targets.deviceID = 0x00;
					osenmagServer.deviceList[0].targets.targetForce[0] = 1.0f;
					osenmagServer.deviceList[0].targets.targetForce[1] = 2.5f;
					osenmagServer.deviceList[0].targets.targetForce[2] = 4.3f;
					UnityEngine.Debug.Log("set force");
				}
				catch (Exception e)
				{
					UnityEngine.Debug.Log(e.ToString());

				}*/

				//osenmagServer.readDataAsync();

				//osenmagServer.sendHelloPacket();
				//UnityEngine.Debug.Log("Before logo");
				//osenmagServer.sendAppLogo();

				//omantisServer.sendHelloPacket();
				//UnityEngine.Debug.Log("malaka");
				

				//osenmagServer.sendForceData(0);

				//omantisServer.readData();

				//UnityEngine.Debug.Log("sent force");

				//omantisServer.readData();
				//omantisServer.sendBroadcast();								//search for devices
			}
			/*if (Input.GetKeyDown("enter")|| Input.GetKeyDown("return"))                                  //search for devices on spacebar press
			{
				if (ostreamingEnabled == false)
				{
					ostreamingEnabled = true;
					UnityEngine.Debug.Log("Haptic streaming enabled...");
				}
				else
				{
					ostreamingEnabled = false;
					UnityEngine.Debug.Log("Haptic streaming disabled...");
				}
				
			}*/

		}
		

		void FixedUpdate()
		{
			if (oupdatedForces == false)		//if no collisions have triggered a force update, send an update with zeros
			{
				updateCursorForces(0);
			}
			updateCursorPositions();
			oupdatedForces = false;
		}

		void setSpatialMultiplier(float multiplier)			//sets the position multiplier for attached devices
		{
			osenmagServer.spatialMultiplier = multiplier;
		}

		void updateCursorPositions()
		{

			for(int x = 0; x < osenmagServer.deviceList.Count; x++)
			{
				if (osenmagServer.deviceList[x].newDevice == true)
				{

					//UnityEngine.Debug.Log()
					//osenmagServer.deviceList[x].cursor = new Senmag_HapticCursor();
					osenmagServer.deviceList[x].newDevice = false;
					osenmagServer.deviceList[x].cursor = new GameObject();
					//osenmagServer.deviceList[x].cursor.name = "Test1";
					osenmagServer.deviceList[x].cursor.AddComponent<Senmag_HapticCursor>();

					osenmagServer.deviceList[x].cursor.GetComponent<Senmag_HapticCursor>().generateCursor(this.gameObject, defaultCursorModel, new string(osenmagServer.deviceList[x].state.deviceName), osenmagServer.deviceList[x].state, cursorScale, cursorFrictionStatic, cursorFrictionDynamic);
					osenmagServer.deviceList[x].cursor.GetComponent<Senmag_HapticCursor>().setPositionFilterStrength(positionFilterStrength);
					osenmagServer.deviceList[x].cursor.GetComponent<Senmag_HapticCursor>().cursorTeleportThreshold = teleportThreshold;
				}

				if(osenmagServer.deviceList[x].newTargets == true)
				{
					//while(osenmagServer.deviceList[x].state.dataLock == true);
					//osenmagServer.deviceList[x].state.dataLock = true;
					osenmagServer.deviceList[x].newTargets = false;
					//osenmagServer.deviceList[x].state.currentPosition[0] *= spatialMultiplier / 1000.0f;
					//osenmagServer.deviceList[x].state.currentPosition[1] *= spatialMultiplier / 1000.0f;
					//osenmagServer.deviceList[x].state.currentPosition[2] *= spatialMultiplier / 1000.0f;
					osenmagServer.deviceList[x].cursor.GetComponent<Senmag_HapticCursor>().setState(osenmagServer.deviceList[x].state);
					//osenmagServer.deviceList[x].state.dataLock = false;
				}			
			}	
		}

		public void setGlobalStiffness(float stiffness)
		{

			//hapticStiffness = stiffness;
			//for (int x = 0; x < osenmagServer.deviceList.Count; x++) ocursors[x].cursorParent.GetComponent<Senmag_HapticCursor>().setSafeStart();
		}

		public void setGlobalSpatialMultiplier(float spatialMultiplier)
		{

			//this.spatialMultiplier = spatialMultiplier;
			//for (int x = 0; x < osenmagServer.deviceList.Count; x++) ocursors[x].cursorParent.GetComponent<Senmag_HapticCursor>().setSafeStart();
		}
		int startDelay = 100;
		public void updateCursorForces(int sendZeros)
		{
			if(oupdatedForces == true) return;
			for (int x = 0; x < osenmagServer.deviceList.Count; x++)
			{
				if (osenmagServer.deviceList[x].newDevice == false){		//make sure the cursor has been generated first...
					Vector3 displacement = osenmagServer.deviceList[x].cursor.GetComponent<Senmag_HapticCursor>().getCurrentForce();
					displacement *= 100.0f * hapticStiffness / spatialMultiplier;

					Vector2 armExtension = new Vector2(osenmagServer.deviceList[x].state.currentPosition[0], osenmagServer.deviceList[x].state.currentPosition[2]);

					osenmagServer.deviceList[x].targets.targetForce[0] = displacement.x;
					osenmagServer.deviceList[x].targets.targetForce[1] = displacement.y;// + antigrav * armExtension.magnitude / 50f;
					osenmagServer.deviceList[x].targets.targetForce[2] = -displacement.z * 2;
					//if(Input.GetKey(KeyCode.M)){
					if(startDelay == 0){
						//UnityEngine.Debug.Log("sending targets {0}, {1}, {2}" + osenmagServer.deviceList[x].targets.targetForce[0] + osenmagServer.deviceList[x].targets.targetForce[1] + osenmagServer.deviceList[x].targets.targetForce[2]);
						osenmagServer.sendForceTargets(osenmagServer.deviceList[x].state.deviceID, osenmagServer.deviceList[x].targets);
					}
					else startDelay -= 1;
					//}
				}
			}
			oupdatedForces = true;
		}

		private void OnApplicationQuit()
		{	
			oappRunning = false;
			osenmagServer.closeSocket();
		}
	}
}





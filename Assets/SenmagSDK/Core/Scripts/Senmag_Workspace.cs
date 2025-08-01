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

using UnityEngine.Rendering;
using System.Security.Cryptography;


#if UNITY_EDITOR
using UnityEditor;
#endif

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




    #if UNITY_EDITOR

    [CustomEditor(typeof(Senmag_Workspace))]
    public class Senmag_WorkspaceEditor : Editor
    {

        bool showServerSettings = false;
        bool showAppSettings = false;
        bool showHapticsSettings = false;
        bool showCursorSettings = false;
        //bool showSettings = false;

        override public void OnInspectorGUI()
        //void OnInspectorGUI()
        {

            var myScript = target as Senmag_Workspace;

            //[Header("Server Settings")]
            showServerSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showServerSettings, "Server Settings");
            if (showServerSettings)
            {
                myScript.useDirectConnect = EditorGUILayout.Toggle("Use Direct Connect", myScript.useDirectConnect);
                myScript.senmagServerIsRemoteHost = EditorGUILayout.Toggle("Use Remote Host", myScript.senmagServerIsRemoteHost);


                using (new EditorGUI.DisabledScope(!myScript.senmagServerIsRemoteHost))
                {
                    myScript.ServerHostIP = EditorGUILayout.TextField("Host IP", myScript.ServerHostIP);
                    myScript.ServerHostPort = EditorGUILayout.IntField("Host Port", myScript.ServerHostPort);
                    myScript.ApplicationPort = EditorGUILayout.IntField("My Port", myScript.ApplicationPort);
                }
            }
            EditorGUI.EndFoldoutHeaderGroup();

            showAppSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAppSettings, "Application Settings");
            if (showAppSettings)
            {
                myScript.applicationName = EditorGUILayout.TextField("Application Name", myScript.applicationName);
                myScript.applicationIcon = (Texture2D)EditorGUILayout.ObjectField("Application Icon", myScript.applicationIcon, typeof(Texture2D), true);

            }
            EditorGUI.EndFoldoutHeaderGroup();

            showHapticsSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showHapticsSettings, "Haptics Settings");
            if (showHapticsSettings)
            {
                myScript.physicsFramerate = EditorGUILayout.FloatField("Physics FrameRate", myScript.physicsFramerate);
                myScript.physicsIterations = EditorGUILayout.IntField("Physics Iterations", myScript.physicsIterations);
                myScript.hapticStiffness = EditorGUILayout.FloatField("Haptic Stiffness", myScript.hapticStiffness);
                myScript.cursorMass = EditorGUILayout.FloatField("Cursor Mass", myScript.cursorMass);
                myScript.spatialMultiplier = EditorGUILayout.FloatField("Position Multiplier", myScript.spatialMultiplier);
                myScript.maximumForce = EditorGUILayout.FloatField("Max Force", myScript.maximumForce);
                myScript.positionFilterStrength = EditorGUILayout.FloatField("Position Filter", myScript.positionFilterStrength);

            }
            EditorGUI.EndFoldoutHeaderGroup();

            showCursorSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showCursorSettings, "Cursor settings Settings");
            if (showCursorSettings)
            {
                myScript.defaultCursorModel = (GameObject)EditorGUILayout.ObjectField("Default Cursor Model", myScript.defaultCursorModel, typeof(GameObject), true);
                myScript.defaultRightClickMenu = (GameObject)EditorGUILayout.ObjectField("Default Menu Prefab", myScript.defaultRightClickMenu, typeof(GameObject), true);

                
                myScript.teleportThreshold = EditorGUILayout.FloatField("Cursor Teleport Threshold", myScript.teleportThreshold);
                myScript.cursorScale = EditorGUILayout.FloatField("Cursor Scale", myScript.cursorScale);
                myScript.cursorFrictionStatic = EditorGUILayout.FloatField("Cursor Static Friction", myScript.cursorFrictionStatic);
                myScript.cursorFrictionDynamic = EditorGUILayout.FloatField("Cursor Dynamic Friction", myScript.cursorFrictionDynamic);
                myScript.allowCustomCursors = EditorGUILayout.Toggle("Allow Custom Cursors", myScript.allowCustomCursors);

                myScript.antigrav = EditorGUILayout.FloatField("Cursor Gravity Compensation", myScript.antigrav);

            }
            EditorGUI.EndFoldoutHeaderGroup();
        }
    }
    #endif


    public class Senmag_Workspace : MonoBehaviour
    {
//        public Senmag_Server osenmagServer = new Senmag_Server();
        public Senmag_DeviceManager deviceManager = new Senmag_DeviceManager();

        Thread osocketThread;// = new Thread;

        [Header("Application Settings")]
        public bool useDirectConnect = true;
        public string applicationName = "Unity Application";
        public Texture2D applicationIcon;
        public bool senmagServerIsRemoteHost = false;
        public string ServerHostIP = "000.000.000.000";
        public int ServerHostPort = 0;
        public int ApplicationPort = 0;

        [Header("Haptics Settings")]
        public float positionFilterStrength = 1.0f;     //lower value is a stronger filter
        public float maximumForce = 1000;
        public float physicsFramerate = 400;
        public int physicsIterations = 15;
        public float hapticStiffness = 1;
        public float spatialMultiplier = 10;

        [Header("Cursor Settings")]

        public GameObject defaultCursorModel;
        public GameObject defaultRightClickMenu;

        public float cursorMass = 0.001f;
        public float teleportThreshold = 0.5f;          //if the distance to target exceeds this value, the cursor will teleport to the target
        public float cursorScale = 0.1f;
        public float cursorFrictionStatic = 0.5f;
        public float cursorFrictionDynamic = 0.5f;
        public bool allowCustomCursors;

        public float antigrav = 0f;     //a constant upwards force applied to counter gravity

        bool oappRunning;
        bool oupdatedForces;
        bool ostreamingEnabled;


        long timeLast;

        // Start is called before the first frame update
        void Start()
        {
            if (applicationIcon == null)            //set up the icon to use if connecting to the Senmag Server - if a custom icon is not specified, load the SDK default
            {
                applicationIcon = Resources.Load("Graphics/UnitySDKIcon_50px") as Texture2D;
                if (applicationIcon == null) UnityEngine.Debug.Log("Failed to load default app icon from Senmag SDK...");
                UnityEngine.Debug.Log("Loaded default application icon from Senmag SDK...");
            }

            if (defaultCursorModel == null)         //set up the cursor model to use, if not specified, use the SDK's default
            {
                defaultCursorModel = Resources.Load("EndEffectors/DK1_Stylus_V1/DK1_Stylus_prefab") as GameObject;
                if (defaultCursorModel == null) UnityEngine.Debug.Log("Failed to load default cursor from Senmag SDK...");
                else UnityEngine.Debug.Log("Loaded default cursor model from Senmag SDK...");
            }
            Time.fixedDeltaTime = (float)1.0 / physicsFramerate;            //set physics framerate
            Physics.defaultSolverIterations = physicsIterations;            //set physics iterations

            oappRunning = true;                                             //flag app running

            //serach for any compatible devices
            if (useDirectConnect)                                           
            {
                deviceManager.scanForUSBDevices();
            }
            else
            {
                /*if (senmagServerIsRemoteHost)
                {
                    osenmagServer.serverIsRemote = true;
                    osenmagServer.remoteServerIP = ServerHostIP;
                    osenmagServer.remoteServerPort = ServerHostPort;
                    osenmagServer.applicationPort = ApplicationPort;
                }
                else
                {
                    osenmagServer.serverIsRemote = false;
                    osenmagServer.remoteServerIP = "127.0.0.1";
                    osenmagServer.remoteServerPort = 0;
                    osenmagServer.applicationPort = 0;
                }*/
            }

            /*osenmagServer.spatialMultiplier = spatialMultiplier;
            if (useDirectConnect)
            {
                osenmagServer.startRecieveThread();
            }
            else osenmagServer.openSocket(applicationName, applicationIcon);     //open the server port*/

            //osocketThread = new Thread(new ThreadStart(socketRecieve));		//start UDP client rx handler
            //osocketThread.Start();

        }


        float lastAutoSearchTime = 0;
        void Update()
        {

            if (Input.GetKey(KeyCode.F5))
            {
                deviceManager.scanForUSBDevices();
            }

            deviceManager.newDeviceMonitorTask();

            
            if (Time.realtimeSinceStartup - lastAutoSearchTime > 1)
            {
                lastAutoSearchTime = Time.realtimeSinceStartup;
                if (deviceManager.comDevices.Count == 0 && deviceManager.d2xxDevices.Count == 0) deviceManager.scanForUSBDevices();
            }

            /*osenmagServer.checkTimeouts();
            if (useDirectConnect == false && osenmagServer.serverConnected == 0)
            {
                if (osenmagServer.oserverConnectRetryCounter == 0)
                {
                    osenmagServer.sendHelloPacket();
                }
                osenmagServer.oserverConnectRetryCounter += 1;
                if (osenmagServer.oserverConnectRetryCounter > 5.0f / Time.deltaTime) osenmagServer.oserverConnectRetryCounter = 0;
            }
            else if(useDirectConnect == true && osenmagServer.usbdeviceList.Count == 0)
            {
                if (osenmagServer.oserverConnectRetryCounter == 0)
                {
                    osenmagServer.scanForUSBDevices();
                }
                osenmagServer.oserverConnectRetryCounter += 1;
                if (osenmagServer.oserverConnectRetryCounter > 5.0f / Time.deltaTime) osenmagServer.oserverConnectRetryCounter = 0;
            }*/
        }


        void FixedUpdate()
        {
            
            if (oupdatedForces == false)        //if no collisions have triggered a force update, send an update with zeros
            {
                updateCursorForces(0);
            }

            deviceManager.recieveTask();
            oupdatedForces = false;
            updateCursorPositions();
        }

        void setSpatialMultiplier(float multiplier)         //sets the position multiplier for all attached devices
        {
            //osenmagServer.spatialMultiplier = multiplier;
        }
        int posX = 0;
        int counterState;

        void configureNewDevice(SenmagDevice dev)
        {
            dev.cursor = new GameObject();
            dev.softSettings.positionGains = new float[3] { 1f, 1f, 1f };
            dev.softSettings.forceGains = new float[3] { 1f, 1f, 1f };

            dev.cursor.AddComponent<Senmag_HapticCursor>();
            dev.cursor.GetComponent<Senmag_HapticCursor>().generateCursor(this.gameObject, defaultCursorModel, new string(dev.usbComms.deviceName), dev.deviceStatus, cursorScale, cursorFrictionStatic, cursorFrictionDynamic);
            dev.cursor.GetComponent<Senmag_HapticCursor>().setPositionFilterStrength(positionFilterStrength);
            dev.cursor.GetComponent<Senmag_HapticCursor>().cursorTeleportThreshold = teleportThreshold;
        }

        void updateCursorPositions()
        {
            if (useDirectConnect)
            {
                for(int x = 0; x < deviceManager.d2xxDevices.Count; x++)
                {
                    var dev = deviceManager.d2xxDevices[x];
                    if(dev.newDevice == true)
                    {
                        dev.newDevice = false;
                        configureNewDevice(dev);
                    }
                    
                    if (dev.newStatus == true)
                    {
                        dev.newStatus = false;
                        dev.cursor.GetComponent<Senmag_HapticCursor>().setState(dev.deviceStatus, spatialMultiplier);
                    }

                }

                for(int x = 0; x < deviceManager.comDevices.Count; x++)
                {
                    var dev = deviceManager.comDevices[x];
                    if(dev.newDevice == true)
                    {
                        dev.newDevice = false;
                        configureNewDevice(dev);
                    }
                    
                    if (dev.newStatus == true)
                    { 
                        dev.newStatus = false;
                        dev.cursor.GetComponent<Senmag_HapticCursor>().setState(dev.deviceStatus, spatialMultiplier);
                    }

                }
            }

        }

        public void setGlobalStiffness(float stiffness)
        {
            /*for (int x = 0; x < deviceManager.usbDevices.Count; x++)
            {
                //var dev = deviceManager.usbDevices[x];
                //dev.softSettings.forceGains = new float[3] { hapticStiffness, hapticStiffness, hapticStiffness };
                //hapticStiffness = stiffness;
                //for (int x = 0; x < osenmagServer.deviceList.Count; x++) ocursors[x].cursorParent.GetComponent<Senmag_HapticCursor>().setSafeStart();
            }*/
        }

        public void setGlobalSpatialMultiplier(float spatialMultiplier)
        {
            /*for (int x = 0; x < deviceManager.usbDevices.Count; x++)
            {
                //var dev = deviceManager.usbDevices[x];
                //dev.softSettings.positionGains = new float[3] { spatialMultiplier, spatialMultiplier, spatialMultiplier };
                //hapticStiffness = stiffness;
                //for (int x = 0; x < osenmagServer.deviceList.Count; x++) ocursors[x].cursorParent.GetComponent<Senmag_HapticCursor>().setSafeStart();
            }*/
            //this.spatialMultiplier = spatialMultiplier;
            //for (int x = 0; x < osenmagServer.deviceList.Count; x++) ocursors[x].cursorParent.GetComponent<Senmag_HapticCursor>().setSafeStart();
        }
        int startDelay = 100;
        public void updateCursorForces(int sendZeros)
        {
            foreach(SenmagDevice dev in deviceManager.d2xxDevices)
            {
                if (dev.newDevice == false)
                {       
                    //make sure the cursor has been generated first...
                    Vector3 displacement = dev.cursor.GetComponent<Senmag_HapticCursor>().getCurrentForce();
                    displacement *= 100.0f * hapticStiffness / spatialMultiplier;

                    dev.deviceTargets.targetForce[0] = displacement.x;
                    dev.deviceTargets.targetForce[1] = displacement.y;
                    dev.deviceTargets.targetForce[2] = -displacement.z;
                    dev.setTargets();
                }
            }

            for (int x = 0; x < deviceManager.comDevices.Count; x++)
            {
                var dev = deviceManager.comDevices[x];
                if (dev.newDevice == false)
                {       
                    //make sure the cursor has been generated first...
                    Vector3 displacement = dev.cursor.GetComponent<Senmag_HapticCursor>().getCurrentForce();
                    displacement *= 100.0f * hapticStiffness / spatialMultiplier;

                    dev.deviceTargets.targetForce[0] = displacement.x;
                    dev.deviceTargets.targetForce[1] = displacement.y;
                    dev.deviceTargets.targetForce[2] = -displacement.z;
                    dev.setTargets();
                }
            }



            deviceManager.sendTargets();
        }

        private void OnApplicationQuit()
        {
            oappRunning = false;
            deviceManager.closeAllDevices();
            //osenmagServer.closeSocket();
        }
    }
}





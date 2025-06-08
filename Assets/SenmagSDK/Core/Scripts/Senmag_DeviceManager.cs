
using System.Collections.Generic;
using System.IO.Ports;
using System.Security.Cryptography;
using System.Threading;
using UnityEditor.Hardware;


using System;
using SenmagHaptic;
using System.IO;
using System.Diagnostics;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

namespace SenmagHaptic
{
    public class Senmag_DeviceManager
    {
        public List<SenmagDevice>   comDevices = new List<SenmagDevice>();
        public List<SenmagDevice>   d2xxDevices = new List<SenmagDevice>();
        private Thread rxThread;

        int lastNumComDevs = 0;
        int lastNumD2xxDevs = 0;
        bool d2xxDelayedSearchQueued = false;
        bool comDelayedSearchQueued = false;
        float d2xxSearchQueueTime = 0;
        float comSearchQueueTime = 0;

        public void closeAllDevices()
        {
            foreach(SenmagDevice dev in comDevices)
            {
                dev.usbComms.serialPort.Close();
            }
            foreach (SenmagDevice dev in d2xxDevices)
            {
                LibreOneComms.CloseFTDIPort(dev.usbComms.D2XXIndex);
            }
        }
        public void newDeviceMonitorTask()
        {
            
            
            if (d2xxDelayedSearchQueued)
            {
                if(Time.realtimeSinceStartup - d2xxSearchQueueTime > 2)
                {
                    d2xxSearchQueueTime = Time.realtimeSinceStartup;
                    if (scanD2xxDevices() != -1)
                    {
                        d2xxDelayedSearchQueued = false;
                    }
                }
            }
            else checkNewD2xxPorts();
            if (comDelayedSearchQueued)
            {
                if (Time.realtimeSinceStartup - comSearchQueueTime > 4)
                {
                    comDelayedSearchQueued = false;
                    scanComDevices();
                }
            }
            else checkNewComPorts();
        }
        public int checkNewComPorts()
        {
            int numPorts = SerialPort.GetPortNames().Length;
            if(numPorts > lastNumComDevs)
            {
                comSearchQueueTime = Time.realtimeSinceStartup;
                comDelayedSearchQueued = true;
                lastNumComDevs = numPorts;
                return 1;
            }
            lastNumComDevs = numPorts;
            return 0;
        }

        public int checkNewD2xxPorts()
        {
            try
            {
                int numPorts = LibreOneComms.GetNumberOfDevices();
                if (numPorts > lastNumD2xxDevs)
                {
                    d2xxSearchQueueTime = Time.realtimeSinceStartup;
                    d2xxDelayedSearchQueued = true;
                    lastNumD2xxDevs = numPorts;
                    return 1;
                }
                lastNumD2xxDevs = numPorts;
            }
            catch
            {
                //probably d2xx drivers are not installed, 
            }
            return 0;
        }

        public void scanComDevices()
        {
            bool complete = false;
            while (!complete)
            {
                SenmagDevice newDevice = new SenmagDevice();
                newDevice.usbComms.connectionType = ConnectionType.COM;
                if (newDevice.usbComms.scanComPorts() == 1)
                {
                    newDevice.newDevice = true;
                    newDevice.newStatus = true;
                    UnityEngine.Debug.Log("Senmag Workspace: Discovered Senmag DK1 '" + newDevice.usbComms.deviceName);
                    comDevices.Add(newDevice);
                }
                else complete = true;
            }
            lastNumComDevs = SerialPort.GetPortNames().Length;
        }


        public int scanD2xxDevices()
        {
            try
            {
                for (int x = 0; x < LibreOneComms.GetNumberOfDevices(); x++)
                {
                    bool portActive = false;
                    for (int y = 0; y < d2xxDevices.Count; y++)
                    {
                        if (x == d2xxDevices[y].usbComms.D2XXIndex) portActive = true;
                    }
                    if (!portActive)
                    {
                        SenmagDevice newDevice = new SenmagDevice();
                        newDevice.usbComms.connectionType = ConnectionType.D2XX;
                        newDevice.usbComms.D2XXIndex = x;
                        if (LibreOneComms.OpenFTDIPort(x, 1500000) == -1)
                        {
                            UnityEngine.Debug.Log("Senmag Worskspace: failed to open D2XX device...");
                            return -1;
                        }
                        else
                        {
                            if (newDevice.usbComms.sendDiscoveryPacket(SenmagDeviceType.senmagDeviceType_LibreOne) == -1)
                            {
                                LibreOneComms.CloseFTDIPort(x);
                                return -1;
                            }
                            System.Threading.Thread.Sleep(200);
                            while (LibreOneComms.GetBytesAvailable(x) > 0)
                            {
                                newDevice.usbComms.serialRXMonitor();
                            }

                            if (newDevice.usbComms.deviceType != SenmagDeviceType.senmagDeviceType_unknown)
                            {
                                UnityEngine.Debug.Log("Senmag Workspace: Discovered Senmag LibreOne '" + newDevice.usbComms.deviceName);
                                newDevice.newDevice = true;
                                d2xxDevices.Add(newDevice);

                            }
                            else LibreOneComms.CloseFTDIPort(x);
                        }
                    }
                }
                lastNumD2xxDevs = LibreOneComms.GetNumberOfDevices();
            }
            catch
            {
                //probably d2xx drivers are not installed...
            }
            return 0;
        }

        public void scanForUSBDevices()
        {
            UnityEngine.Debug.Log("Starting USB scan for Haptic devices...");
            scanComDevices();
            scanD2xxDevices();
        }

        public void sendTargets()
        {
            for(int x = 0; x < comDevices.Count; x++)
            {
                if(comDevices[x].usbComms.sendTargets(comDevices[x].deviceTargets) == -1)
                {
                    comDevices[x].cursor.GetComponent<Senmag_HapticCursor>().destroyCursor();
                    comDevices.RemoveAt(x);
                }
            }

            for (int x = 0; x < d2xxDevices.Count; x++)
            {
                if (d2xxDevices[x].usbComms.sendTargets(d2xxDevices[x].deviceTargets) == -1)
                {
                    d2xxDevices[x].cursor.GetComponent<Senmag_HapticCursor>().destroyCursor();
                    d2xxDevices.RemoveAt(x);
                }
            }
        }
        public void recieveTask()
        {
            foreach (SenmagDevice dev in d2xxDevices)
            {
                while (LibreOneComms.GetBytesAvailable(dev.usbComms.D2XXIndex) > 0){
                    RxTaskResult newData = dev.usbComms.serialRXMonitor();

                    if (newData == RxTaskResult.deviceDisconnected)
                    {
                        //handle removed devices here
                    }
                    else if (newData == RxTaskResult.newSettings)
                    {
                        dev.newDevice = true;
                    }
                    else if (newData == RxTaskResult.newStatus)
                    {
                        dev.readStatusFromUSB();
                        dev.newStatus = true;
                    }
                }
            }

            try
            {
                for (int x = 0; x < comDevices.Count; x++)
                {
                    var dev = comDevices[x];
                    while (dev.usbComms.serialPort.BytesToRead > 0)
                    {
                        RxTaskResult newData = dev.usbComms.serialRXMonitor();
                        if (newData == RxTaskResult.deviceDisconnected)
                        {
                            UnityEngine.Debug.Log("Device '" + dev.usbComms.deviceName + "' disconnected...");
                            dev.close();
                            comDevices.RemoveAt(x);
                        }
                        else if (newData == RxTaskResult.newSettings)
                        {
                            dev.newDevice = true;
                        }
                        else if (newData == RxTaskResult.newStatus)
                        {
                            dev.usbComms.serialPort.DiscardInBuffer();
                            dev.readStatusFromUSB();
                            dev.newStatus = true;
                        }

                    }
                }
            }
            catch
            {

            }
        }


        public void updateDeviceTargets()
        {

        }




    }

}
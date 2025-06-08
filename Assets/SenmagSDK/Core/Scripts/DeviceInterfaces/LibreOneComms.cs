using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;



namespace SenmagHaptic
{
    enum LibreOne_CommsPacketType
    {
        packetType_ack = 0,
        packetType_ping = 1,
        packetType_settings = 2,
        packetType_status = 3,
        packetType_targets = 4,
        packetType_command = 5,
        packetType_message = 6,
        packetType_calibration = 7,
    };
    public class LibreOneComms : MonoBehaviour
    {
        /*[DllImport("LibreOne_USBInterface", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool OpenSerial(string portName, int baudRate);

        [DllImport("LibreOne_USBInterface", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CloseSerial();

        [DllImport("LibreOne_USBInterface", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SendPacket(int packetType, byte[] data, int dataLength);

        [DllImport("LibreOne_USBInterface", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetStatus(byte[] data, int maxLength);

        [DllImport("LibreOne_USBInterface", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetSettings(byte[] data, int maxLength);

        [DllImport("LibreOne_USBInterface", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool NewSettings();

        [DllImport("LibreOne_USBInterface", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool NewStatus();*/

 
        [DllImport("D2XXWrapper", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumberOfDevices();

        [DllImport("D2XXWrapper", CallingConvention = CallingConvention.Cdecl)]
        public static extern int OpenFTDIPort(int deviceID, int baudrate);

        [DllImport("D2XXWrapper", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CloseFTDIPort(int deviceID);

        [DllImport("D2XXWrapper", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WriteFTDI(int deviceID, byte[] buffer, int length);

        [DllImport("D2XXWrapper", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ReadFTDI(int deviceID, byte[] buffer, int maxLength);

        [DllImport("D2XXWrapper", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetBytesAvailable(int deviceID);
        

    }
}

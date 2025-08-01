using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using SenmagHaptic;
//using UnityEditor.Experimental.GraphView;
using System.IO;
using System.Diagnostics;
using static SenmagHaptic.LibreOneComms;



namespace SenmagHaptic
{
    public class USBPacket
    {
        public byte[] header = new byte[5];
        public byte type = new byte();
        public byte dataLength = new byte();
        public byte crc = new byte();
        public byte[] data = new byte[0xFF];

    }


    public enum DK1_UsbPacketType
    {
        error = 0,                  // operation completed successfully
        ack = 1,                    // an error occoured (contains error type)
        searchDevices = 2,          // packet is a search for devices
        deviceConfig = 3,           // packet contains the config of a device (MantisDeviceConfig)
        saveConfig = 4,             // packet asks the devices to save its config
        setCalibrationState = 5,    // packet sets the device's state (e.g. calibration)
        deviceStatus = 6,           // packet sets the devices mode
        deviceTargets = 7,          // packet instructs device to reboot into DFU mode for firmware upload
        resetDFU = 8,
        factoryReset = 9,           // factory reset
        firmwareData = 10,          // data for new firmware
    }


    public class Senmag_USBParser
    {
        public int headerProgress;
        public int packetProgress;
        public bool headerRecieved;
        public byte[] rxBuffer = new byte[200];

        public int packetLength;
        public int packetType;
        public int packetCrc;
        public int dataCounter;
    }

    public enum RxTaskResult
    {
        deviceDisconnected = -1,
        noData = 0,
        badCrc = 1,
        newSettings = 2,
        newStatus = 3,

    }

    public enum ConnectionType
    {
        COM = 0,
        D2XX = 1,
    }



    public class Senmag_USBComms
    {

        public const byte DK1_USBCRC_SEED = 0x55;
        public const string DK1_USBCRC_HEADER = "SGHAP";

        public ConnectionType       connectionType = ConnectionType.COM;
        public Action               newStatus;
        public SenmagDeviceType     deviceType = new SenmagDeviceType();
        public SerialPort           serialPort;
        public SenmagDeviceStatus   deviceStatus = new SenmagDeviceStatus();

        public string               deviceName = new string("Unknown device");

        private Senmag_USBParser    dk1Parser = new Senmag_USBParser();
        private Senmag_USBParser    libreOneParser = new Senmag_USBParser();
        private byte[]              txBuf = new byte[200];


        public int D2XXIndex;       

        //LibreOneComms libreOneComms;

        public Senmag_USBComms()
        {
            deviceType = SenmagDeviceType.senmagDeviceType_unknown;
        //    logFile = new StreamWriter("serialPortLog.txt", append: false);
        //    logFile.AutoFlush = true;

        }

        ~Senmag_USBComms()
        {
            if (serialPort.IsOpen) serialPort.Close();
        }

       

        public int scanComPorts()
        {
            foreach (string s in SerialPort.GetPortNames())     //for each serial port
            {
                deviceType = SenmagDeviceType.senmagDeviceType_unknown;
                serialPort = new SerialPort(s, 1500000, Parity.None, 8, StopBits.One);
                try
                {
                    serialPort.Open();
   
                    if (sendDiscoveryPacket(SenmagDeviceType.senmagDeviceType_DK1) == 1)
                    {
                        System.Threading.Thread.Sleep(200);
                        while (serialPort.BytesToRead > 0) serialRXMonitor();
                        if (deviceType != SenmagDeviceType.senmagDeviceType_unknown)
                        {
                            return 1;
                        }
                    }
                    //add scans for further device types here...   
                    serialPort.Close();     //if we didn;t detect any device, close the port and move onto the next one
                }
                catch (Exception e)
                {
                   
                }   
            }
            return 0;
        }

        byte[] rxBuffer = new byte[1000];


        public RxTaskResult serialRXMonitor()
        {

            //  IAsyncResult result = Port.BaseStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar)
            if (connectionType == ConnectionType.COM)
            {
                if (!serialPort.IsOpen) return RxTaskResult.deviceDisconnected;
                while (serialPort.BytesToRead > 0)
                {
                    byte[] newData = new byte[1];
                    serialPort.BaseStream.Read(newData, 0, 1);
                    return processRxByte(newData[0]);
                }
            }
            else
            {
                while(LibreOneComms.GetBytesAvailable(D2XXIndex) > 0)
                {
                    byte[] newData = new byte[1];
                    LibreOneComms.ReadFTDI(D2XXIndex, newData, 1);
                    return processRxByte(newData[0]);
                }
            }

            return RxTaskResult.noData;
        }

        public RxTaskResult processRxByte(byte newData) {
            RxTaskResult returnVal = RxTaskResult.noData;

            if (deviceType == SenmagDeviceType.senmagDeviceType_unknown)
            {
                //pass to all reciever types to determine device type
                RxTaskResult result = processRX_DK1(newData);
                if (result == RxTaskResult.newSettings)
                {
                    deviceType = SenmagDeviceType.senmagDeviceType_DK1;
                    return RxTaskResult.newSettings;
                }



                result = processRX_LibreOne(newData);
                if (result == RxTaskResult.newSettings)
                {
                    deviceType = SenmagDeviceType.senmagDeviceType_LibreOne;
                    return RxTaskResult.newSettings;
                }
            }
                    
            else if (deviceType == SenmagDeviceType.senmagDeviceType_DK1)
            {
                return processRX_DK1(newData);
            }
            else if (deviceType == SenmagDeviceType.senmagDeviceType_LibreOne)
            {

                return processRX_LibreOne(newData);
            }
            return returnVal;
        }



        public RxTaskResult processRX_DK1(byte data)
        {
            if (dk1Parser.headerProgress > DK1_USBCRC_HEADER.Length - 1) dk1Parser.headerProgress = (byte)(DK1_USBCRC_HEADER.Length - 1);
            if (data == DK1_USBCRC_HEADER[dk1Parser.headerProgress])
            {
                //Console.WriteLine("Reading header: {0}, ({1})", newData, (char)newData);
                dk1Parser.headerProgress++;
                if (dk1Parser.headerProgress > DK1_USBCRC_HEADER.Length - 1)
                {
                    dk1Parser.headerProgress = 0;
                    dk1Parser.packetProgress = 0;
                    dk1Parser.dataCounter = 0;
                }
            }

            if (dk1Parser.packetProgress == 1)
            {
                dk1Parser.packetType = data;
                //Console.WriteLine("Reading type: {0}, ({1})", newData, (char)newData);
            }
            else if (dk1Parser.packetProgress == 2)
            {
                dk1Parser.packetLength = data;
                //Console.WriteLine("Reading length: {0}, ({1})", newData, (char)newData);
            }
            else if (dk1Parser.packetProgress == 3)
            {
                dk1Parser.packetCrc = data;
                //Console.WriteLine("Reading crc: {0}, ({1})", newData, (char)newData);
            }
            if (dk1Parser.packetProgress > 3 || (dk1Parser.packetProgress == 3 && dk1Parser.packetLength == 0))
            {
                //Console.WriteLine("processing data byte({0}): {1}, ({2})", usbData.odataCounter, newData, (char)newData);
                dk1Parser.rxBuffer[dk1Parser.dataCounter] = data;

                if (dk1Parser.dataCounter >= dk1Parser.packetLength - 1)
                {
                    dk1Parser.headerProgress = 0;
                    dk1Parser.packetProgress = 0;
                    dk1Parser.dataCounter = 0;

                    byte crc = DK1_USBCrc(dk1Parser.rxBuffer, 0, dk1Parser.packetLength);
                    //Console.WriteLine("Got Packet with crc: {0}, expected: {1}", usbData.opacket.crc, crc);
                    if (crc == dk1Parser.packetCrc)
                    {
                        if(dk1Parser.packetType == (int)DK1_UsbPacketType.deviceConfig)
                        {
                            char[] name = new char[20];
                            for (int x = 0; x < 20; x++) name[x] = (char)dk1Parser.rxBuffer[x];
                            deviceName = new string(name).Split('\0')[0];
                            return RxTaskResult.newSettings;
                        }
                        if (dk1Parser.packetType == (int)DK1_UsbPacketType.deviceStatus)
                        {
                            int count = 0;

                           // deviceStatus.currentMode = (DK1_DeviceModes)dk1Parser.rxBuffer[count];
                            count += 4;
                            deviceStatus.currentPosition[0] = BitConverter.ToSingle(dk1Parser.rxBuffer, count);
                            count += 4;
                            deviceStatus.currentPosition[1] = BitConverter.ToSingle(dk1Parser.rxBuffer, count);
                            count += 4;
                            deviceStatus.currentPosition[2] = BitConverter.ToSingle(dk1Parser.rxBuffer, count);
                            count += 4;
                            deviceStatus.currentOrientation[0] = -BitConverter.ToSingle(dk1Parser.rxBuffer, count);
                            count += 4;
                            deviceStatus.currentOrientation[1] = BitConverter.ToSingle(dk1Parser.rxBuffer, count);
                            count += 4;
                            deviceStatus.currentOrientation[3] = -BitConverter.ToSingle(dk1Parser.rxBuffer, count);
                            count += 4;
                            deviceStatus.currentOrientation[2] = BitConverter.ToSingle(dk1Parser.rxBuffer, count);
                            count += 4;
                            //Console.WriteLine("{0}, {1}, {2}, {3}", currentState.currentRotation[0], currentState.currentRotation[1], currentState.currentRotation[2], currentState.currentRotation[3]);
                            //currentState.framerate = usbData.opacket.data[count];
                            //deviceStatus.framerate = BitConverter.ToUInt16(dk1Parser.rxBuffer, count);
                            count += 2;
                            //Console.WriteLine(currentState.framerate);

                            deviceStatus.stylusButtons = dk1Parser.rxBuffer[count];
                            count += 2;

                            deviceStatus.statusFlags.combined = (int)BitConverter.ToUInt16(dk1Parser.rxBuffer, count);
                            count += 4;

                            float dataFooter = BitConverter.ToSingle(dk1Parser.rxBuffer, count);
                            count += 4;
                            if (dataFooter != (float)1.2345)
                            {
                                /*statusErrorCounter++;
                                if (statusErrorCounter > 1000)
                                {
                                    statusErrorCounter = 0;
                                    Console.WriteLine("Warning, detected error in status from device: '{0}', check firmware/server version compatibility");
                                }
                                Console.WriteLine("Warning, detected error in status from device: '{0}', check firmware/server version compatibility");*/

                                //currentState.currentMode = (DK1_DeviceModes)dk1Parser.rxBuffer[count];
                                deviceStatus.currentPosition[0] = 0;
                                deviceStatus.currentPosition[1] = 0;
                                deviceStatus.currentPosition[2] = 0;
                                deviceStatus.currentOrientation[0] = 0;
                                deviceStatus.currentOrientation[1] = 0;
                                deviceStatus.currentOrientation[2] = 0;
                                deviceStatus.currentOrientation[3] = 0;
                                deviceStatus.stylusButtons = 0;
                                deviceStatus.statusFlags.combined = 0;
                            }

                            if ((deviceStatus.statusFlags.combined & 0x01) != 0) deviceStatus.statusFlags.calibrationOk = 1;
                            else deviceStatus.statusFlags.calibrationOk = 0;
                            if ((deviceStatus.statusFlags.combined & 0x02) != 0) deviceStatus.statusFlags.innerBoundaries = 1;
                            else deviceStatus.statusFlags.innerBoundaries = 0;
                            if ((deviceStatus.statusFlags.combined & 0x04) != 0) deviceStatus.statusFlags.outerBounderies = 1;
                            else deviceStatus.statusFlags.outerBounderies = 0;
                            if ((deviceStatus.statusFlags.combined & 0x08) != 0) deviceStatus.statusFlags.encoder0OK = 1;
                            else deviceStatus.statusFlags.encoder0OK = 0;
                            if ((deviceStatus.statusFlags.combined & 0x10) != 0) deviceStatus.statusFlags.encoder1OK = 1;
                            else deviceStatus.statusFlags.encoder1OK = 0;
                            if ((deviceStatus.statusFlags.combined & 0x20) != 0) deviceStatus.statusFlags.encoder2OK = 1;
                            else deviceStatus.statusFlags.encoder2OK = 0;
                            if ((deviceStatus.statusFlags.combined & 0x40) != 0) deviceStatus.statusFlags.sensorBoardError = 1;
                            else deviceStatus.statusFlags.sensorBoardError = 0;
                            if ((deviceStatus.statusFlags.combined & 0x80) != 0) deviceStatus.statusFlags.toolConnected = 1;
                            else deviceStatus.statusFlags.toolConnected = 0;
                            if ((deviceStatus.statusFlags.combined & 0x100) != 0) deviceStatus.statusFlags.toolError = 1;
                            else deviceStatus.statusFlags.toolError = 0;

                            return RxTaskResult.newStatus;
                        }
                        
                    }
                    else return RxTaskResult.badCrc;
                    //check data
                }
                else
                {

                    dk1Parser.dataCounter++;
                    if (dk1Parser.dataCounter >= dk1Parser.rxBuffer.Length) dk1Parser.dataCounter = 0;
                }
            }

            dk1Parser.packetProgress++;

            return RxTaskResult.noData; 
        }

        public int errorCount= 0;
        public int okCount= 0;
        public RxTaskResult processRX_LibreOne(byte data)
        {
            if (data == 0xFF)
            {
                libreOneParser.headerProgress++;
                if (libreOneParser.headerProgress >= 4)
                {
                    libreOneParser.packetProgress = -1;
                    libreOneParser.headerRecieved = true;
                }
            }
            else libreOneParser.headerProgress = 0;

            if(libreOneParser.headerRecieved == true)
            {
                if (libreOneParser.packetProgress == -1) libreOneParser.packetProgress = 0;
                else
                {
                    libreOneParser.rxBuffer[libreOneParser.packetProgress] = data;
                    libreOneParser.packetProgress++;
                    if (libreOneParser.packetProgress >= libreOneParser.rxBuffer.Length) libreOneParser.packetProgress = 4;

                    if (libreOneParser.packetProgress == 4)
                    {
                        libreOneParser.packetType = libreOneParser.rxBuffer[0];
                        libreOneParser.packetLength = libreOneParser.rxBuffer[1] | libreOneParser.rxBuffer[2] << 8;
                        libreOneParser.packetCrc = libreOneParser.rxBuffer[3];
                    }
                    if (libreOneParser.packetProgress >= 4)
                    {
                        if (libreOneParser.packetProgress >= 4 + libreOneParser.packetLength)
                        {
                            
                            byte expectedCrc = LibreOne_USBCrc(libreOneParser.rxBuffer, 4, libreOneParser.packetLength);
                            if (libreOneParser.packetCrc == expectedCrc)
                            {
                                okCount++;
                                libreOneParser.packetProgress = 0;
                                libreOneParser.headerRecieved = false;

                                if (libreOneParser.packetType == 2)           //setting packet
                                {
                                    char[] name = new char[20];
                                    for (int x = 0; x < 20; x++) name[x] = (char)libreOneParser.rxBuffer[x + 44];
                                    deviceName = new string(name).Split('\0')[0];
                                    return RxTaskResult.newSettings;
                                }
                                else if (libreOneParser.packetType == 3)      //status packet
                                {
                                    int count = 4;
                                    deviceStatus.currentPosition[0] = BitConverter.ToSingle(libreOneParser.rxBuffer, count);
                                    count += 4;
                                    deviceStatus.currentPosition[1] = BitConverter.ToSingle(libreOneParser.rxBuffer, count);
                                    count += 4;
                                    deviceStatus.currentPosition[2] = BitConverter.ToSingle(libreOneParser.rxBuffer, count);
                                    count += 4;
                                    deviceStatus.currentOrientation[2] = BitConverter.ToSingle(libreOneParser.rxBuffer, count);//z
                                    count += 4;
                                    deviceStatus.currentOrientation[0] = BitConverter.ToSingle(libreOneParser.rxBuffer, count);//y
                                    count += 4;
                                    deviceStatus.currentOrientation[1] = BitConverter.ToSingle(libreOneParser.rxBuffer, count);//x
                                    count += 4;
                                    deviceStatus.currentOrientation[3] = BitConverter.ToSingle(libreOneParser.rxBuffer, count);//w
                                    count += 4;
                                    deviceStatus.stylusButtons = (byte)(libreOneParser.rxBuffer[count] ^ 0xFF);
                                    count += 1;
                                    if (deviceStatus.stylusButtons != 0)
                                    {
                                        int x = 0;
                                        for (int y = 0; y < 10; y++) x++;
                                    }

                                    count += 4;
                                    deviceStatus.currentTooltip = (Senmag_DeviceTooltip)libreOneParser.rxBuffer[count];
                                    count += 1;
                                    deviceStatus.statusFlags.combined = libreOneParser.rxBuffer[count];

                                    if ((deviceStatus.statusFlags.combined & 0x1) != 0) deviceStatus.statusFlags.toolConnected = 1;
                                    else deviceStatus.statusFlags.toolConnected = 0;
                                    if ((deviceStatus.statusFlags.combined & 0x2) != 0) deviceStatus.statusFlags.innerBoundaries = 1;
                                    else deviceStatus.statusFlags.innerBoundaries = 0;
                                    if ((deviceStatus.statusFlags.combined & 0x4) != 0) deviceStatus.statusFlags.outerBounderies = 1;
                                    else deviceStatus.statusFlags.outerBounderies = 0;

                                    return RxTaskResult.newStatus;
                                }
                            }
                            else
                            {
                                errorCount++;
                                libreOneParser.packetProgress = 0;
                                libreOneParser.headerRecieved = false;
                                return RxTaskResult.badCrc;
                            }
                            //got a complete packet
                        }
                    }
                }
            }
            return RxTaskResult.noData;
        }

        public void getStatus()
        {

        }

        public int sendTargets(Senmag_DeviceTargets targets)
        {
            if (deviceType == SenmagDeviceType.senmagDeviceType_DK1)
            {
                txBuf[0] = (byte)'S';     //header
                txBuf[1] = (byte)'G';
                txBuf[2] = (byte)'H';
                txBuf[3] = (byte)'A';
                txBuf[4] = (byte)'P';

                txBuf[5] = (byte)DK1_UsbPacketType.deviceTargets;
                txBuf[6] = (byte)0;//length
                txBuf[7] = (byte)0;//crc


                float configCheck = (float)1.2345;

                byte[] packetData = new byte[20];


                txBuf[8] = 0;
                txBuf[9] = 0;
                txBuf[10] = 0;
                txBuf[11] = 0;
                int count = 12;
                Array.Copy(BitConverter.GetBytes(targets.targetForce[0]), 0, txBuf, count, 4);
                count += 4;
                Array.Copy(BitConverter.GetBytes(targets.targetForce[1]), 0, txBuf, count, 4);
                count += 4;
                Array.Copy(BitConverter.GetBytes(targets.targetForce[2]), 0, txBuf, count, 4);
                count += 4;
                Array.Copy(BitConverter.GetBytes(configCheck), 0, txBuf, count, 4);
                count += 4;


                txBuf[6] = (byte)(count-8);
                txBuf[7] = DK1_USBCrc(txBuf, 8, (count-8));

                return sendPacket(txBuf, count);


            }
            else if (deviceType == SenmagDeviceType.senmagDeviceType_LibreOne)
            {
                txBuf[0] = 0xFF;     //header
                txBuf[1] = 0xFF;
                txBuf[2] = 0xFF;
                txBuf[3] = 0xFF;
                txBuf[4] = 4;       //packetType
                txBuf[5] = 0;       //packetLength
                txBuf[6] = 0;       //packetLength
                //metaData end
                int count = 8;


                Array.Copy(BitConverter.GetBytes(targets.targetForce[0]), 0, txBuf, count, 4);
                count += 4;
                Array.Copy(BitConverter.GetBytes(targets.targetForce[1]), 0, txBuf, count, 4);
                count += 4;
                Array.Copy(BitConverter.GetBytes(targets.targetForce[2]), 0, txBuf, count, 4);
                count += 4;
                txBuf[count] = targets.targetTypes;
                count++;


                Array.Copy(BitConverter.GetBytes(count-8), 0, txBuf, 5, 2);

                txBuf[7] = LibreOne_USBCrc(txBuf, 8, count-8);

                return sendPacket(txBuf, count + 8);
            }

                return 0;
        }

        public int sendDiscoveryPacket(SenmagDeviceType deviceType)
        {
            if (deviceType == SenmagDeviceType.senmagDeviceType_DK1)
            {
                //build and send the DK1 discovery packet...
                int byteCount = 0;
                txBuf[0] = (byte)'S';     //header
                txBuf[1] = (byte)'G';
                txBuf[2] = (byte)'H';
                txBuf[3] = (byte)'A';
                txBuf[4] = (byte)'P';
                txBuf[5] = 2;       //packetType
                txBuf[6] = 0;       //data length
                txBuf[7] = DK1_USBCrc(txBuf, 8, 0);
                return sendPacket(txBuf, 8);
            }
            else if (deviceType == SenmagDeviceType.senmagDeviceType_LibreOne)
            {
                int byteCount = 0;
                txBuf[0] = 0xFF;     //header
                txBuf[1] = 0xFF;
                txBuf[2] = 0xFF;
                txBuf[3] = 0xFF;
                txBuf[4] = 1;       //packetType
                txBuf[5] = 0;       //packetLength
                txBuf[6] = 0;       //packetLength
                txBuf[7] = LibreOne_USBCrc(txBuf, 7, 0);
                int result = sendPacket(txBuf, 8);
                if (result != 1)
                {
                    return -1;
                }
                else return 1;
            }

            else return -1;
        }

        public int sendPacket(byte[] data, int length)
        {

            if (connectionType == ConnectionType.COM)
            {
                if (!serialPort.IsOpen) return -1;
                try
                {
                    serialPort.Write(data, 0, length);
                }
                catch
                {
                    return -1;
                }
                return 1;
            }
            else if(connectionType == ConnectionType.D2XX) {
                try
                {
                    int result = 0;
                    result = LibreOneComms.WriteFTDI(D2XXIndex, data, length);
                    if (result != length) return -1;
                    else return 1;
                }
                catch
                {
                    return -1;
                }
            }
            return 0;
        }




        public byte DK1_USBCrc(byte[] data, int startIndex, int dataLength)
        {
            byte crc = 0x55;
            for (int i = startIndex; i < startIndex + dataLength; i++)
            {
                crc ^= data[i];

                //Console.WriteLine("crc process: {0}, ({1})", crc, data[i]);
            }
            return crc;
        }

        public byte LibreOne_USBCrc(byte[] data, int startIndex, int length)
        {
            int crc = 0x5555;
            int newData;
            int y = 0;
            for (int x = startIndex; x < startIndex + length; x++)
            {
                newData = (data[x] & 0xFF) << y;
                crc ^= newData;
                y++;
                if (y >= 8) y = 0;
            }
            return (byte)((crc & 0xFF) | (crc >> 8));
        }
    }


    
}
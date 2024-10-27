using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Senmag_DK1_Types;
using System.IO.Ports;


namespace Senmag_DK1_USBComms
{
    public class DK1_USBPacket
    {
        public byte[] header = new byte[5];
        public byte type = new byte();
        public byte dataLength = new byte();
        public byte crc = new byte();
        public byte[] data = new byte[0xFF];

    }

    public class DK1_USBRxData
    {
        public byte opacketProgress = new byte();
        public byte oheaderProgress = new byte();
        public byte odataCounter = new byte();

        public DK1_USBPacket opacket = new DK1_USBPacket();
    }

    public class DK1_USBComms
    {
        public const byte DK1_USBCRC_SEED = 0x55;
        public const string DK1_USBCRC_HEADER = "SGHAP";
        public bool awaitingAck = false;

        const int ACK_Max_retrys = 5;
        int ACK_Current_retrys = 0;

        public SerialPort serialPort;
        public DK1_USBRxData usbData = new DK1_USBRxData(); //struct used to track incoming packets
        private DK1_USBPacket txPacket = new DK1_USBPacket();

        byte[] TxBuf = new byte[255];
        byte[] dataBuf = new byte[255];

        //Timer timer_requireAck = new Timer();
        byte[] requireAck_dataBuf = new byte[1];
        DK1_UsbPacketType requireAck_typeBuf;
        int txDataLength;

        public Thread usbTxThread;

        public DK1_USBComms()
        {
            usbTxThread = new Thread(sendThread);
        }
        public byte DK1_USBCrc(byte[] data, byte dataLength)
        {
            byte crc = 0x55;
            for (int i = 0; i < dataLength; i++)
            {
                crc ^= data[i];

                //Console.WriteLine("crc process: {0}, ({1})", crc, data[i]);
            }
            return crc;
        }

        public void processAck()
        {
            ACK_Current_retrys = 0;
            //timer_requireAck.Stop();
            awaitingAck = false;

        }

        public int processByte(byte newData, DK1_USBRxData usbData)
        {
            //Console.WriteLine("Got byte: {0}, ({1}), ({2} of {3})", newData, (char)newData, usbData.opacketProgress, usbData.opacket.dataLength);


            if (usbData.oheaderProgress > DK1_USBCRC_HEADER.Length - 1) usbData.oheaderProgress = (byte)(DK1_USBCRC_HEADER.Length - 1);
            if (newData == DK1_USBCRC_HEADER[usbData.oheaderProgress])
            {
                //Console.WriteLine("Reading header: {0}, ({1})", newData, (char)newData);
                usbData.oheaderProgress++;
                if (usbData.oheaderProgress > DK1_USBCRC_HEADER.Length - 1)
                {
                    usbData.oheaderProgress = 0;
                    usbData.opacketProgress = 0;
                    usbData.odataCounter = 0;
                }
            }

            if (usbData.opacketProgress == 1)
            {
                usbData.opacket.type = newData;
                //Console.WriteLine("Reading type: {0}, ({1})", newData, (char)newData);
            }
            else if (usbData.opacketProgress == 2)
            {
                usbData.opacket.dataLength = newData;
                //Console.WriteLine("Reading length: {0}, ({1})", newData, (char)newData);
            }
            else if (usbData.opacketProgress == 3)
            {
                usbData.opacket.crc = newData;
                //Console.WriteLine("Reading crc: {0}, ({1})", newData, (char)newData);
            }
            if (usbData.opacketProgress > 3 || (usbData.opacketProgress == 3 && usbData.opacket.dataLength == 0))
            {
                //Console.WriteLine("processing data byte({0}): {1}, ({2})", usbData.odataCounter, newData, (char)newData);
                usbData.opacket.data[usbData.odataCounter] = newData;

                if (usbData.odataCounter >= usbData.opacket.dataLength - 1)
                {
                    usbData.oheaderProgress = 0;
                    usbData.opacketProgress = 0;
                    usbData.odataCounter = 0;

                    byte crc = DK1_USBCrc(usbData.opacket.data, usbData.opacket.dataLength);
                    //Console.WriteLine("Got Packet with crc: {0}, expected: {1}", usbData.opacket.crc, crc);
                    if (crc == usbData.opacket.crc)
                    {
                        //byte[] tmp = new byte[1];
                        //	tmp[0] = 0;
                        //serialPort.Write(tmp, 0, 1);

                        return 1;       //got valid packet
                    }
                    else return -1;
                    //check data
                }
                else
                {

                    usbData.odataCounter++;
                }
            }

            usbData.opacketProgress++;

            return 0;
        }


        public void send_setCalibrationState(int status)
        {
            dataBuf[0] = (byte)status;
            processOutgoingPacket(DK1_UsbPacketType.setCalibrationState, 1, dataBuf, true, true);
        }

        public void send_sendSaveConfig()
        {
            processOutgoingPacket(DK1_UsbPacketType.saveConfig, 0, dataBuf, true, true);
        }

        public void send_factoryReset()
        {
            processOutgoingPacket(DK1_UsbPacketType.factoryReset, 0, dataBuf, true, true);
        }

        public void send_dfuReset()
        {
            processOutgoingPacket(DK1_UsbPacketType.resetDFU, 0, dataBuf, true, true);
        }

        public void repeat_setState(object sender, EventArgs e)
        {
            if (ACK_Current_retrys >= ACK_Max_retrys)
            {
                ACK_Current_retrys = 0;
                //timer_requireAck.Stop();
                Console.WriteLine("Error, device did not ack command");
                return;
            }
            //Console.WriteLine("Error, device did not ack command");
            sendPacket();
            ACK_Current_retrys++;
        }

        public void processOutgoingPacket(DK1_UsbPacketType type, byte dataLength, byte[] data, bool overrideAckLock = false, bool requireAck = false)
        {
            if (ACK_Current_retrys != 0)
            {
                if (overrideAckLock == false)
                {
                    Console.WriteLine("Warning, tried to send packet while waiting for ACK, new packet abandoned...");
                    return;
                }
                Console.WriteLine("Warning, tried to send packet while waiting for ACK, old packet abandoned...");
                ACK_Current_retrys = 0;
            }

            if (requireAck)
            {
                awaitingAck = true;
                ACK_Current_retrys = 1;
                //timer_requireAck.Interval = 100;
                //timer_requireAck.Tick += new EventHandler(repeat_setState);
                //timer_requireAck.Start();
            }
            int byteCount = 0;
            for (int x = 0; x < DK1_USBCRC_HEADER.Length; x++)
            {
                TxBuf[x] = (byte)DK1_USBCRC_HEADER[x];
                byteCount++;
            }
            TxBuf[byteCount] = (byte)type;
            byteCount++;
            TxBuf[byteCount] = (byte)dataLength;
            byteCount++;
            TxBuf[byteCount] = DK1_USBCrc(data, dataLength);
            byteCount++;
            for (int x = 0; x < dataLength; x++)
            {
                TxBuf[byteCount + x] = data[x];
            }
            byteCount += dataLength;

            txDataLength = DK1_USBCRC_HEADER.Length + 3 + dataLength;
            sendPacket();
        }
        public void sendPacket()
        {
            if (usbTxThread.IsAlive == true)
            {
                //usbTxThread.Interrupt();
                //return;
            }
            //int tmp = serialPort.BytesToWrite;

            if (!serialPort.IsOpen) return;
            try
            {
                serialPort.Write(TxBuf, 0, txDataLength);
            }
            catch (Exception e)
            {
                return;
            }
        }

        public void sendThread()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(Timeout.Infinite);
                }
                catch (ThreadInterruptedException)
                {
                    //Console.WriteLine("Thread '{0}' awoken.", Thread.CurrentThread.Name);
                    //serialPort.Write(TxBuf, 0, txDataLength);
                    try
                    {
                        serialPort.Write(TxBuf, 0, txDataLength);
                    }
                    catch (Exception e)
                    {
                        //return;
                    }
                }
            }
        }


        public bool testSerialPort(string portName, DK1_USBRxData usbData)
        {
            usbData.oheaderProgress = 0;
            //SerialPort tmpSerialPort = new SerialPort();


            if (portName != null)
            {
                serialPort = new SerialPort(portName, 1152000, Parity.None, 8, StopBits.One);
                try
                {
                    serialPort.Open();
                }
                catch
                {
                    // recover from exception
                    return false;
                }
                if (serialPort.IsOpen)
                {
                    processOutgoingPacket(DK1_UsbPacketType.searchDevices, 0, null);

                    System.Threading.Thread.Sleep(500);

                    while (serialPort.BytesToRead > 0)
                    {
                        byte[] tmpData = new byte[1];
                        serialPort.Read(tmpData, 0, 1);

                        int result = processByte(tmpData[0], usbData);

                        if (result == 1)
                        {
                            //Console.WriteLine("Got valid Packet from port: {0}, type {1}", portName, usbData.opacket.type);
                            //for (int x = 0; x < usbData.opacket.dataLength; x++) Console.Write("{0}", (char)usbData.opacket.data[x]);
                            //Console.WriteLine("");
                            //Console.WriteLine("Got valid packet from port: {0}", portName);
                            if (usbData.opacket.type == (byte)DK1_UsbPacketType.deviceConfig)
                            {
                                serialPort.Close();
                                return true;
                            }

                        }
                        else if (result == -1)
                        {
                            //Console.WriteLine("Got invalid packet from port: {0}", portName);
                            //for (int x = 0; x < usbData.opacket.dataLength; x++) Console.Write("{0}", (char)usbData.opacket.data[x]);
                            //Console.WriteLine("");
                        }
                    }
                    serialPort.Close();
                }
                else
                {
                    Console.WriteLine("Failed to open port: {0}", portName);
                }
            }

            return false;
        }

        public DK1_DeviceConfig processUsbConfig(DK1_USBRxData usbData)
        {
            DK1_DeviceConfig config = new DK1_DeviceConfig();

            int count = 0;

            for (int x = 0; x < DK1_DeviceConfig.DK1_DEVICENAME_CHARACTERCOUNT; x++)
            {
                config.deviceName[x] = (char)usbData.opacket.data[x];
                count++;
            }
            for (int x = 0; x < DK1_DeviceConfig.DK1_DEVICEFAMILY_CHARACTERCOUNT; x++)
            {
                config.readOnlyConfig.deviceFamily[x] = (char)usbData.opacket.data[x + count];

            }
            count += DK1_DeviceConfig.DK1_DEVICEFAMILY_CHARACTERCOUNT;
            for (int x = 0; x < DK1_DeviceConfig.DK1_DEVICESERIALNUM_CHARACTERCOUNT; x++)
            {
                config.readOnlyConfig.serialNumber[x] = (char)usbData.opacket.data[x + count];
            }
            count += DK1_DeviceConfig.DK1_DEVICESERIALNUM_CHARACTERCOUNT;

            count++;

            config.maxForce = BitConverter.ToUInt16(usbData.opacket.data, count);
            count += 2;

            config.powerLimit = usbData.opacket.data[count];
            count += 2;
            config.targetFramerate = BitConverter.ToUInt16(usbData.opacket.data, count);
            count += 2;
            config.sleepTimer = BitConverter.ToUInt16(usbData.opacket.data, count);
            count += 2;
            config.endEffectorType = (DK1_EndEffectors)usbData.opacket.data[count];
            count += 4;
            config.positionFilter = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            config.tempThrottling = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            config.firmwareVersion = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            config.usbDebugConfiguration = usbData.opacket.data[count];
            count += 4;

            for (int x = 0; x < config.pidConfig.Length; x++)
            {
                config.pidConfig[x].rateLimit = BitConverter.ToSingle(usbData.opacket.data, count);
                count += 4;
                config.pidConfig[x].KGain = BitConverter.ToSingle(usbData.opacket.data, count);
                count += 4;
                config.pidConfig[x].IGain = BitConverter.ToSingle(usbData.opacket.data, count);
                count += 4;
                config.pidConfig[x].ILimit = BitConverter.ToSingle(usbData.opacket.data, count);
                count += 4;
                config.pidConfig[x].DGain = BitConverter.ToSingle(usbData.opacket.data, count);
                count += 4;
            }

            config.kinematicConfig.lengthToElbow = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            config.kinematicConfig.lengthToTip = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            config.kinematicConfig.mass1 = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            config.kinematicConfig.mass2 = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            config.kinematicConfig.gravityCompensation = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            config.kinematicConfig.inertiaCompensation = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            config.kinematicConfig.accelerationFilter = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;

            for (int x = 0; x < config.kinematicConfig.frictionCompensation.Length; x++)
            {
                config.kinematicConfig.frictionCompensation[x] = BitConverter.ToSingle(usbData.opacket.data, count);
                count += 4;
            }


            float configCheck = BitConverter.ToSingle(usbData.opacket.data, count);
            if (configCheck != (float)1.2345)
            {
                Console.WriteLine("Warning, detected error in config from device: '{0}', check firmware/server version compatibility", new string(config.deviceName));
            }

            return config;
        }

        public void sendDeviceConfig(DK1_DeviceConfig config)
        {
            byte count = 0;
            byte[] packetData = new byte[0xFF];

            for (int x = 0; x < DK1_DeviceConfig.DK1_DEVICENAME_CHARACTERCOUNT; x++)
            {
                packetData[x] = (byte)config.deviceName[x];
                count++;
            }
            for (int x = 0; x < DK1_DeviceConfig.DK1_DEVICEFAMILY_CHARACTERCOUNT; x++)
            {
                packetData[x + count] = (byte)config.readOnlyConfig.deviceFamily[x];
            }
            count += DK1_DeviceConfig.DK1_DEVICEFAMILY_CHARACTERCOUNT;
            for (int x = 0; x < DK1_DeviceConfig.DK1_DEVICESERIALNUM_CHARACTERCOUNT; x++)
            {
                packetData[x + count] = (byte)config.readOnlyConfig.serialNumber[x];
            }
            count += DK1_DeviceConfig.DK1_DEVICESERIALNUM_CHARACTERCOUNT;

            count++;


            //packetData[count] = BitConverter.GetBytes(config.maxForce);
            Array.Copy(BitConverter.GetBytes(config.maxForce), 0, packetData, count, 2);
            count += 2;

            packetData[count] = config.powerLimit;
            count += 2;
            Array.Copy(BitConverter.GetBytes(config.targetFramerate), 0, packetData, count, 2);
            count += 2;
            Array.Copy(BitConverter.GetBytes(config.sleepTimer), 0, packetData, count, 2);
            count += 2;
            //config.endEffectorType = (DK1_EndEffectors)packetData[count];
            packetData[count] = (byte)config.endEffectorType;
            count += 4;
            //config.positionFilter = 0.1f;
            Array.Copy(BitConverter.GetBytes(config.positionFilter), 0, packetData, count, 4);
            count += 4;
            Array.Copy(BitConverter.GetBytes(config.tempThrottling), 0, packetData, count, 4);
            count += 4;
            Array.Copy(BitConverter.GetBytes(config.firmwareVersion), 0, packetData, count, 4);
            count += 4;
            //config.usbDebugConfiguration = packetData[count];
            packetData[count] = config.usbDebugConfiguration;
            count += 4;

            for (int x = 0; x < config.pidConfig.Length; x++)
            {
                Array.Copy(BitConverter.GetBytes(config.pidConfig[x].rateLimit), 0, packetData, count, 4);
                count += 4;
                Array.Copy(BitConverter.GetBytes(config.pidConfig[x].KGain), 0, packetData, count, 4);
                count += 4;
                Array.Copy(BitConverter.GetBytes(config.pidConfig[x].IGain), 0, packetData, count, 4);
                count += 4;
                Array.Copy(BitConverter.GetBytes(config.pidConfig[x].ILimit), 0, packetData, count, 4);
                count += 4;
                Array.Copy(BitConverter.GetBytes(config.pidConfig[x].DGain), 0, packetData, count, 4);
                count += 4;
            }

            Array.Copy(BitConverter.GetBytes(config.kinematicConfig.lengthToElbow), 0, packetData, count, 4);
            count += 4;
            Array.Copy(BitConverter.GetBytes(config.kinematicConfig.lengthToTip), 0, packetData, count, 4);
            count += 4;
            Array.Copy(BitConverter.GetBytes(config.kinematicConfig.mass1), 0, packetData, count, 4);
            count += 4;
            Array.Copy(BitConverter.GetBytes(config.kinematicConfig.mass2), 0, packetData, count, 4);
            count += 4;
            Array.Copy(BitConverter.GetBytes(config.kinematicConfig.gravityCompensation), 0, packetData, count, 4);
            count += 4;
            Array.Copy(BitConverter.GetBytes(config.kinematicConfig.inertiaCompensation), 0, packetData, count, 4);
            count += 4;
            Array.Copy(BitConverter.GetBytes(config.kinematicConfig.accelerationFilter), 0, packetData, count, 4);
            count += 4;

            for (int x = 0; x < config.kinematicConfig.frictionCompensation.Length; x++)
            {
                Array.Copy(BitConverter.GetBytes(config.kinematicConfig.frictionCompensation[x]), 0, packetData, count, 4);
                count += 4;
            }


            float configCheck = (float)1.2345;
            Array.Copy(BitConverter.GetBytes(configCheck), 0, packetData, count, 4);
            count += 4;

            processOutgoingPacket(DK1_UsbPacketType.deviceConfig, count, packetData, true, true);
        }

        public void sendDeviceTargets(DK1_USBDeviceTargets targets)
        {
            byte[] packetData = new byte[20];
            float configCheck = (float)1.2345;
            packetData[0] = 0;
            packetData[1] = 0;
            packetData[2] = 0;
            packetData[3] = 0;
            Array.Copy(BitConverter.GetBytes(targets.targetForce[0]), 0, packetData, 4, 4);
            Array.Copy(BitConverter.GetBytes(targets.targetForce[1]), 0, packetData, 8, 4);
            Array.Copy(BitConverter.GetBytes(targets.targetForce[2]), 0, packetData, 12, 4);
            Array.Copy(BitConverter.GetBytes(configCheck), 0, packetData, 16, 4);
            processOutgoingPacket(DK1_UsbPacketType.deviceTargets, 20, packetData, true, false);
        }

        int statusErrorCounter;
        //float[] posLast = new float[3];

        public DK1_DeviceState processDeviceState(DK1_USBRxData usbData)
        {
            int count = 0;
            DK1_DeviceState currentState = new DK1_DeviceState();

            currentState.currentMode = (DK1_DeviceModes)usbData.opacket.data[count];
            count += 4;
            currentState.currentPosition[0] = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            currentState.currentPosition[1] = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            currentState.currentPosition[2] = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            currentState.currentRotation[0] = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            currentState.currentRotation[1] = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            currentState.currentRotation[2] = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            currentState.currentRotation[3] = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            //Console.WriteLine("{0}, {1}, {2}, {3}", currentState.currentRotation[0], currentState.currentRotation[1], currentState.currentRotation[2], currentState.currentRotation[3]);
            //currentState.framerate = usbData.opacket.data[count];
            currentState.framerate = BitConverter.ToUInt16(usbData.opacket.data, count);
            count += 2;
            //Console.WriteLine(currentState.framerate);

            currentState.stylusState = usbData.opacket.data[count];
            count += 2;

            currentState.statusFlags = BitConverter.ToUInt16(usbData.opacket.data, count);
            count += 4;

            float dataFooter = BitConverter.ToSingle(usbData.opacket.data, count);
            count += 4;
            if (dataFooter != (float)1.2345)
            {
                statusErrorCounter++;
                if (statusErrorCounter > 1000)
                {
                    statusErrorCounter = 0;
                    Console.WriteLine("Warning, detected error in status from device: '{0}', check firmware/server version compatibility");
                }
                Console.WriteLine("Warning, detected error in status from device: '{0}', check firmware/server version compatibility");

                currentState.currentMode = (DK1_DeviceModes)usbData.opacket.data[count];
                currentState.currentPosition[0] = 0;
                currentState.currentPosition[1] = 0;
                currentState.currentPosition[2] = 0;
                currentState.currentRotation[0] = 0;
                currentState.currentRotation[1] = 0;
                currentState.currentRotation[2] = 0;
                currentState.currentRotation[3] = 0;
                currentState.stylusState = 0;
                currentState.statusFlags = 0;
            }

            /*for(int x = 0; x < 3; x++)
			{
				if(Math.Abs(currentState.currentPosition[x] - posLast[x]) > 1f)
				{
					Console.WriteLine("Detected position error from device...");
				}
				posLast[x] = currentState.currentPosition[x];
			}*/




            currentState.calibrationOk = (currentState.statusFlags & 0x01) != 0;
            currentState.innerBounderiesOk = (currentState.statusFlags & 0x02) != 0;
            currentState.outerBounderiesOK = (currentState.statusFlags & 0x04) != 0;
            currentState.encoder0OK = (currentState.statusFlags & 0x08) != 0;
            currentState.encoder1OK = (currentState.statusFlags & 0x10) != 0;
            currentState.encoder2OK = (currentState.statusFlags & 0x20) != 0;
            currentState.sensorBoardOK = (currentState.statusFlags & 0x40) != 0;
            currentState.endEffectorAttached = (currentState.statusFlags & 0x80) != 0;
            currentState.endEffectorOK = (currentState.statusFlags & 0x100) != 0;

            return currentState;
        }
    }


}

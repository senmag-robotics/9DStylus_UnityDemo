﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

//using SenmagTypes;
using System.Net.NetworkInformation;

using UnityEngine;
using System.Threading;

using System.IO;
using System.Drawing;

namespace SenmagHaptic
{

	public class Senmag_Server
	{

		public List<SenmagDevice> deviceList = new List<SenmagDevice>();

		UdpClient udpClient = new UdpClient();
		


		public float spatialMultiplier;

		IPAddress ipAddr;
		IPEndPoint serverEndPoint;
		IPEndPoint myEndPoint;
		string oapplicationName = "Unity Application";
		Texture2D oapplciationIcon;


		public Socket socket;
		public EndPoint senderRemote;

		int serverConnectAttemptsCounter = 0;
		public int oserverConnectRetryCounter = 0;
		public int serverConnected = 0;

		long last;
		int count;
		int myPort = 0;

		UdpState udpState = new UdpState();
		
		Thread recieveThread;

		public Senmag_Server()
		{
		}

		public struct UdpState
		{
			public Socket s;
			public IPEndPoint socket_endPoint;
			public Byte[] message;
		}

		public int openSocket(string applicationName, Texture2D applicationIcon)
		{
			oapplicationName = applicationName;
			oapplciationIcon = applicationIcon;

			udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));		//bind to a any port
			Console.WriteLine(udpClient.Client.LocalEndPoint.ToString());
			myEndPoint = ((IPEndPoint)udpClient.Client.LocalEndPoint);
			myPort = myEndPoint.Port;

			//udpClient = new UdpClient();

			IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
			ipAddr = hostEntry.AddressList[0];
			recieveThread = new Thread(recieveThreadTask);
			recieveThread.Start();

			UnityEngine.Debug.Log("Opening connection to Senmag server using port: " + myPort);
			return 0;
		}

		public int closeSocket()
		{
			udpClient.Close();
			recieveThread.Abort();
			return 0;
		}

		public int getServerPort()
		{
			string path = @"C:\Users\Public\SenmagServerLocation.txt";
			if (File.Exists(path))
			{
				string port = File.ReadAllText(path);
				return Int32.Parse(port);
			}
			else
			{
				Console.WriteLine("Couldn't find Senmag Server location, is the server running?");
				return 1;
			}
		}

		public void checkTimeouts()
		{
			if(serverConnected > 0) serverConnected -= 1;
			if(serverConnected < 0) serverConnected = 0;
		}

		public void sendPacket(int type, int priority, byte[] data, IPEndPoint destinationAddress)
		{
			byte[] message = new byte[4 + data.Length];
			message[0] = 0x53;              //header x3
			message[1] = 0x65;
			message[2] = 0x6E;
			message[3] = (byte)type;        //packet type
			
			Array.Copy(data, 0, message, 4, data.Length);

			try
			{
				udpClient.Send(message, message.Length, destinationAddress);
			}
			catch (Exception e)
			{
				UnityEngine.Debug.Log(e.ToString());
			}
		}

		public static Image resizeImage(Image imgToResize, Size size)
		{
			return (Image)(new Bitmap(imgToResize, size));
		}

		public int sendHelloPacket()
		{
			serverConnectAttemptsCounter += 1;
			if(serverConnectAttemptsCounter > 3)
			{
				serverConnectAttemptsCounter = 0;
				UnityEngine.Debug.Log("No reply from Senmag server, are you sure it is running?");
			}

			serverEndPoint =new IPEndPoint(IPAddress.Parse("127.0.0.1"), getServerPort());

			//serverEndPoint = new IPEndPoint(ipAddr, getServerPort());
			UnityEngine.Debug.Log("Searching for Senmag server on port: " + serverEndPoint.Port + ", Using port: " + myPort);

			byte[] appName = Encoding.ASCII.GetBytes(oapplicationName);
			UInt16 nameLength = (UInt16)appName.Length;
			if(nameLength > 0xFF) nameLength = 0xFF;

			byte[] imageBytes1 = oapplciationIcon.EncodeToPNG();

			Image tmpIcon;
			MemoryStream ms = new MemoryStream(imageBytes1);
			tmpIcon = Image.FromStream(ms);
			Image tmpIcon2 = (Image)(new Bitmap(tmpIcon, new Size(50,50)));

			MemoryStream ms2 = new MemoryStream();
			tmpIcon2.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);
			byte[] imageBytes = ms2.ToArray();
			tmpIcon.Dispose();
			ms2.Close();

			int imageLength = imageBytes.Length;

			//UnityEngine.Debug.Log("Name length: " + nameLength);
			//UnityEngine.Debug.Log("Image length: " + imageLength);

			byte[] txBuf = new byte[nameLength + imageLength + 12];
			Array.Copy(BitConverter.GetBytes(myPort), 0, txBuf, 0, 4);                          //copy own port number
			Array.Copy(BitConverter.GetBytes(SenmagSDK_Metadata.sdk_version), 0, txBuf, 4, 4);	//copy sdk version
			
			txBuf[8] = (byte)nameLength;														//copy name length
			Array.Copy(appName, 0, txBuf, 9, nameLength);										//copy name
			Array.Copy(BitConverter.GetBytes(imageLength), 0, txBuf, 9+ nameLength, 2);			//copy image length
			Array.Copy(imageBytes, 0, txBuf, 12 + nameLength, imageLength);						//copy image
			
			sendPacket((int)DK1_ServerPacketType.hello, 0x00, txBuf, serverEndPoint);			//send
			
			return 0;
		}

		public int readDataAsync()
		{
			//socket.BeginReceiveFrom(udpState.message, 0, udpState.message.Length, 0, ref senderRemote, new AsyncCallback(ReceiveFrom_Callback), udpState);
			return 0;
		}

		public void recieveThreadTask()
		{
			IPEndPoint deviceIPEP = new IPEndPoint(IPAddress.Any, 0);
			UdpState result = new UdpState();

			while (true)
			{
				byte[] message = new byte[3];
				try
				{
					result.message = udpClient.Receive(ref deviceIPEP);                //blocking, handles error when udpClient is closed
					result.socket_endPoint = deviceIPEP;
					processIncomingPacket(result);
				}
				catch (Exception e)
				{
					Console.WriteLine("IOException source: {0}", e.Source);
				}
			}

		}

		//public void ReceiveFrom_Callback(IAsyncResult ar)
		//public float[] posLast = new float[3];
		public void processIncomingPacket(UdpState state)
		{
			Byte[] message = state.message;

			if (message[0] == 0x53 && message[1] == 0x65 && message[2] == 0x6E)
			{
				//serverConnected = (int)(2 / Time.deltaTime);
				serverConnected = 100;
				//UnityEngine.Debug.Log("Correct header " + message[3]);
				
				if (message[3] == (Byte)DK1_ServerPacketType.deviceStatus)
				{
					//UnityEngine.Debug.Log("In dk1Data");
					try
					{
						SenmagDevice currentDevice = new SenmagDevice();
						byte deviceID = (byte)message[4];
						int deviceIndex = -1;

						for(int x = 0; x < deviceList.Count; x++)
						{
							if(deviceList[x].state.deviceID == deviceID){		//if we already know about this device, use its existing entry in the list
								deviceIndex = x;
								currentDevice = deviceList[x];
							}
						}
					
						//while(currentDevice.state.dataLock == true);
						//currentDevice.state.dataLock = true;
						currentDevice.state.currentMode = (DK1_DeviceModes)message[5];
						currentDevice.state.currentPosition[0] = BitConverter.ToSingle(message, 6) * spatialMultiplier / 1000.0f;
						currentDevice.state.currentPosition[1] = BitConverter.ToSingle(message, 10) * spatialMultiplier / 1000.0f;
						currentDevice.state.currentPosition[2] = BitConverter.ToSingle(message, 14) * spatialMultiplier / 1000.0f;
					
						currentDevice.state.currentRotation[0] = BitConverter.ToSingle(message, 18);
						currentDevice.state.currentRotation[1] = BitConverter.ToSingle(message, 22);
						currentDevice.state.currentRotation[2] = BitConverter.ToSingle(message, 26);
						currentDevice.state.currentRotation[3] = BitConverter.ToSingle(message, 30);
					
						currentDevice.state.framerate = BitConverter.ToUInt16(message, 34);
						currentDevice.state.stylusState = message[36];
						currentDevice.state.innerBounderiesOK = Convert.ToBoolean(message[37]);
						currentDevice.state.outerBounderiesOK = Convert.ToBoolean(message[38]);
						currentDevice.state.endEffector = (DK1_EndEffectors)message[39];
						//currentDevice.state.dataLock = false;


						Array.Copy(message, 40, currentDevice.state.deviceName, 0, currentDevice.state.deviceName.Length);
						
						if (deviceIndex == -1){
							currentDevice.state.deviceID = deviceID;
							currentDevice.newDevice = true;		//flag as a new device, the main thread will generate the cursor later
							deviceList.Add(currentDevice);
						}

						currentDevice.newTargets = true;
					}
					catch (Exception e)
					{
						UnityEngine.Debug.Log(e);

					}

					//ToDo: Test this works

				}

			}
		}


		public int sendForceTargets(int deviceID, DK1_DeviceTargets targets)					// sends force tagets from 'mantisDevices' to the relevant device
		{
			byte[] message = new byte[13];
			message[0] = (byte) deviceID;
			Array.Copy(BitConverter.GetBytes(targets.targetForce[0]), 0, message, 1, 4);
			Array.Copy(BitConverter.GetBytes(targets.targetForce[1]), 0, message, 5, 4);
			Array.Copy(BitConverter.GetBytes(targets.targetForce[2]), 0, message, 9, 4);
			sendPacket((int)DK1_ServerPacketType.forceTarget, 0x00, message, serverEndPoint);
			return 0;
		}
	}
}
	
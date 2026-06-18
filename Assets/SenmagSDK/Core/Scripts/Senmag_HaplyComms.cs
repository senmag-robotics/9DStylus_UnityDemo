using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
//using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using SenmagHaptic;

public class Senmag_HaplyComms
{
	public HaplyWebSocketClient socket = new HaplyWebSocketClient();
	public bool newPositions = false;
	public async void openSocket()
	{
		//socket = new HaplyWebSocketClient();
		await socket.ConnectAsync("ws://localhost:10001");
	}

	public int numConnectedDevices()
	{
		return socket.Devices.Count;
	}

	public async void closePort()
	{
		await socket.CloseAsync();
	}


}




public class HaplyWebSocketClient : IDisposable
{
	private float Haply_fixedPosGain = 1.2f;
	private float Haply_fixedZoffset = -0.15f;


	private ClientWebSocket _ws;
	private CancellationTokenSource _cts;

	private const string InverseKey = "inverse3";
	private const string GripKey = "wireless_verse_grip";
	private const string DeviceIdKey = "device_id";

	public bool newPositions = false;
	public class HaplyDevice
	{
		public bool newDevice;
		public int Index;          // stable index
		public string DeviceId;    // internal use only
		public Vector3 Position;
		public SenmagDevice senmagDevice = new SenmagDevice();
	}
	public List<HaplyDevice> Devices { get; } = new();
	public List<HaplyDevice> Grips { get; } = new();

	private readonly Dictionary<string, int> _idToIndex = new();



	public Vector3 Position { get; private set; }
	public string DeviceId { get; private set; }

	public async Task ConnectAsync(string uri)
	{
		if (_ws != null) return;
		_ws = new ClientWebSocket();
		_cts = new CancellationTokenSource();

		await _ws.ConnectAsync(new Uri(uri), CancellationToken.None);
		_ = Task.Run(ReceiveLoop);
	}

	private async Task ReceiveLoop()
	{
		var buffer = new byte[8192];

		while (_ws.State == WebSocketState.Open && !_cts.IsCancellationRequested)
		{
			var result = await _ws.ReceiveAsync(buffer, _cts.Token);
			if (result.MessageType == WebSocketMessageType.Close)
				break;

			var jsonText = Encoding.UTF8.GetString(buffer, 0, result.Count);
			HandleMessage(jsonText);
		}

		int x = 0;
		x += 100;
	}

	private void HandleMessage(string msg)
	{
		var data = JObject.Parse(msg);
		var devices = data[InverseKey] as JArray;
		var grips = data[GripKey] as JArray;

		if (devices == null || devices.Count == 0)
		{
			var updateRequest = new JObject
			{
				["session"] = new JObject
				{
					["force_render_full_state"] = new JObject()
				}
			};

			SendJson(updateRequest);
			return;
		}

		foreach (JObject device in devices)
		{
			newPositions = true;
			string id = device[DeviceIdKey].Value<string>();

			if (!_idToIndex.TryGetValue(id, out int index))
			{
				index = Devices.Count;
				_idToIndex[id] = index;

				Devices.Add(new HaplyDevice
				{
					newDevice = true,
					Index = index,
					DeviceId = id,
					
				});

				Debug.Log($"Haply device connected: {id} → index {index}");
			}


			Devices[index].senmagDevice.newStatus = true;
			var pos = device["state"]["cursor_position"];
			Devices[index].senmagDevice.deviceStatus.currentPosition[0] = pos["x"].Value<float>() * Haply_fixedPosGain;
			Devices[index].senmagDevice.deviceStatus.currentPosition[1] = pos["z"].Value<float>() * Haply_fixedPosGain + Haply_fixedZoffset;
			Devices[index].senmagDevice.deviceStatus.currentPosition[2] = -pos["y"].Value<float>() * Haply_fixedPosGain;



			/*var state = device["state"];
			Devices[index].senmagDevice.deviceStatus.currentPosition[0] = state["cursor_position"]["x"].Value<float>() * Haply_fixedPosGain;
			Devices[index].senmagDevice.deviceStatus.currentPosition[1] = state["cursor_position"]["z"].Value<float>() * Haply_fixedPosGain + Haply_fixedZoffset;
			Devices[index].senmagDevice.deviceStatus.currentPosition[2] = -state["cursor_position"]["y"].Value<float>() * Haply_fixedPosGain;*/

			//Devices[index].senmagDevice.deviceStatus.currentOrientation[0] = state["orientation"]["x"].Value<float>();
			/*Devices[index].senmagDevice.deviceStatus.currentOrientation[1] = state["orientation"]["y"].Value<float>();
			Devices[index].senmagDevice.deviceStatus.currentOrientation[2] = state["orientation"]["z"].Value<float>();
			Devices[index].senmagDevice.deviceStatus.currentOrientation[3] = state["orientation"]["w"].Value<float>();*/
		}


		foreach (JObject device in grips)
		{
			newPositions = true;
			string id = device[DeviceIdKey].Value<string>();

			if (!_idToIndex.TryGetValue(id, out int index))
			{
				index = Grips.Count;
				_idToIndex[id] = index;

				Grips.Add(new HaplyDevice
				{
					newDevice = true,
					Index = index,
					DeviceId = id,

				});

				Debug.Log($"Haply device connected: {id} → index {index}");
			}


			/*Devices[index].senmagDevice.newStatus = true;
			var pos = device["state"]["cursor_position"];
			Devices[index].senmagDevice.deviceStatus.currentPosition[0] = pos["x"].Value<float>() * Haply_fixedPosGain;
			Devices[index].senmagDevice.deviceStatus.currentPosition[1] = pos["z"].Value<float>() * Haply_fixedPosGain + Haply_fixedZoffset;
			Devices[index].senmagDevice.deviceStatus.currentPosition[2] = -pos["y"].Value<float>() * Haply_fixedPosGain;*/

			if (Devices.Count == 0) break;

			var state = device["state"];

			Quaternion orientation = new Quaternion(state["orientation"]["x"].Value<float>(), state["orientation"]["y"].Value<float>(), state["orientation"]["z"].Value<float>(), state["orientation"]["w"].Value<float>());

			orientation = orientation * Quaternion.Euler(0, 0, 90);

			/*Devices[0].senmagDevice.deviceStatus.currentOrientation[0] = state["orientation"]["x"].Value<float>();
			Devices[0].senmagDevice.deviceStatus.currentOrientation[2] = state["orientation"]["y"].Value<float>();
			Devices[0].senmagDevice.deviceStatus.currentOrientation[1] = state["orientation"]["z"].Value<float>();
			Devices[0].senmagDevice.deviceStatus.currentOrientation[3] = -state["orientation"]["w"].Value<float>();*/

			if (index < Devices.Count)
			{
				//if there is an equal number of haptic devices & handles, auto-map them in order
				Devices[index].senmagDevice.deviceStatus.currentOrientation[0] = orientation.x;
				Devices[index].senmagDevice.deviceStatus.currentOrientation[2] = orientation.y;
				Devices[index].senmagDevice.deviceStatus.currentOrientation[1] = orientation.z;
				Devices[index].senmagDevice.deviceStatus.currentOrientation[3] = -orientation.w;

				Devices[index].senmagDevice.deviceStatus.stylusButtons = 0x00;
				if (state["buttons"]["a"].Value<bool>() == true) Devices[index].senmagDevice.deviceStatus.stylusButtons |= 0x1;
				if (state["buttons"]["b"].Value<bool>() == true) Devices[index].senmagDevice.deviceStatus.stylusButtons |= 0x2;
			}

			else if (Devices.Count > 0)
			{
				//if there is only one haptic device,
				Devices[0].senmagDevice.deviceStatus.currentOrientation[0] = orientation.x;
				Devices[0].senmagDevice.deviceStatus.currentOrientation[2] = orientation.y;
				Devices[0].senmagDevice.deviceStatus.currentOrientation[1] = orientation.z;
				Devices[0].senmagDevice.deviceStatus.currentOrientation[3] = -orientation.w;

				Devices[0].senmagDevice.deviceStatus.stylusButtons = 0x00;
				if (state["buttons"]["a"].Value<bool>() == true) Devices[0].senmagDevice.deviceStatus.stylusButtons |= 0x1;
				if (state["buttons"]["b"].Value<bool>() == true) Devices[0].senmagDevice.deviceStatus.stylusButtons |= 0x2;
			}
		}
	}

	public void SendForce(int deviceIndex, Vector3 force)
	{
		if (deviceIndex < 0 || deviceIndex >= Devices.Count)
			return;

		string id = Devices[deviceIndex].DeviceId;

		var request = new JObject
		{
			[InverseKey] = new JArray
		{
			new JObject
			{
				[DeviceIdKey] = id,
				["commands"] = new JObject
				{
					["set_cursor_force"] = new JObject
					{
						["values"] = new JObject
						{
							["x"] = force.x,
							["y"] = force.y,
							["z"] = force.z
						}
					}
				}
			}
		}
		};

		SendJson(request);
	}

	private async void SendJson(JObject json)
	{
		var bytes = Encoding.UTF8.GetBytes(json.ToString());
		await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
	}


	public async Task CloseAsync()
	{
		try
		{
			// Stop the receive loop
			_cts?.Cancel();

			if (_ws != null && _ws.State == WebSocketState.Open)
			{
				// Gracefully close the WebSocket
				await _ws.CloseAsync(
					WebSocketCloseStatus.NormalClosure,
					"Closing",
					CancellationToken.None
				);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"Exception while closing WebSocket: {ex}");
		}
		finally
		{
			_ws?.Dispose();
			_ws = null;

			_cts?.Dispose();
			_cts = null;
		}

		Debug.Log("HaplyWebSocketClient closed.");
	}



public void Dispose()
	{
		_cts?.Cancel();
		_ws?.Dispose();
	}
}
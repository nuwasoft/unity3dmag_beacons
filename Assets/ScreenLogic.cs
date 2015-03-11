using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class ScreenLogic : MonoBehaviour {

	private List<Beacon> mybeacons = new List<Beacon>();
	private bool scanning = true;
	private int sensorVelocity = 0;
	private Vector2 scrolldistance;
	public GameObject sensor;
	public GameObject found;
	public RawImage image;
	public WebCamTexture mCamera = null;
	// Use this for initialization
	void Start () {
		iBeaconReceiver.BeaconRangeChangedEvent += OnBeaconRangeChanged;
		iBeaconReceiver.BluetoothStateChangedEvent += OnBluetoothStateChanged;
		iBeaconReceiver.CheckBluetoothLEStatus();
		Debug.Log ("Listening for beacons");
		mCamera = new WebCamTexture ();
		image.texture = mCamera;
		mCamera.Play ();
	}
	
	void OnDestroy() {
		iBeaconReceiver.BeaconRangeChangedEvent -= OnBeaconRangeChanged;
		iBeaconReceiver.BluetoothStateChangedEvent -= OnBluetoothStateChanged;
	}
	
	// Update is called once per frame
	void Update () {
		sensorVelocity = 0;
		foreach (Beacon b in mybeacons) {
			if (b.UUID.ToUpper() == "8D46E6CA-93C4-495D-81D5-EFA64A005A8D")
			{
				if (b.major == 123)
				{
					if (b.minor == 1)
					{
						switch (b.range.ToString())
						{
						case "IMMEDIATE":
							if (b.accuracy < 1)
							{
								found.animation.Play("found");
							}
							sensorVelocity = 200;
							break;
						case "NEAR":
							sensorVelocity = 100;
							break;
						case "FAR":
							sensorVelocity = 50;
							break;
						case "UNKNOWN":
							sensorVelocity = 0;
							break;
						}
						sensor.transform.Rotate(Vector3.forward * Time.deltaTime*sensorVelocity, Space.World);
					}
				}
			}
		}
	}

	private void OnBluetoothStateChanged(BluetoothLowEnergyState newstate) {
		switch (newstate) {
		case BluetoothLowEnergyState.POWERED_ON:
			iBeaconReceiver.Init();
			Debug.Log ("It is on, go searching");
			break;
		case BluetoothLowEnergyState.POWERED_OFF:
			iBeaconReceiver.EnableBluetooth();
			Debug.Log ("It is off, switch it on");
			break;
		case BluetoothLowEnergyState.UNAUTHORIZED:
			Debug.Log("User doesn't want this app to use Bluetooth, too bad");
			break;
		case BluetoothLowEnergyState.UNSUPPORTED:
			Debug.Log ("This device doesn't support Bluetooth Low Energy, we should inform the user");
			break;
		case BluetoothLowEnergyState.UNKNOWN:
		case BluetoothLowEnergyState.RESETTING:
		default:
			Debug.Log ("Nothing to do at the moment");
			break;
		}
	}


	private void OnBeaconRangeChanged(List<Beacon> beacons) { // 
		foreach (Beacon b in beacons) {
			if (mybeacons.Contains(b)) {
				mybeacons[mybeacons.IndexOf(b)] = b;
			} else {
				// this beacon was not in the list before
				// this would be the place where the BeaconArrivedEvent would have been spawned in the the earlier versions
				mybeacons.Add(b);
			}
		}
		foreach (Beacon b in mybeacons) {
			if (b.lastSeen.AddSeconds(10) < DateTime.Now) {
				// we delete the beacon if it was last seen more than 10 seconds ago
				// this would be the place where the BeaconOutOfRangeEvent would have been spawned in the earlier versions
				mybeacons.Remove(b);
			}
		}
	}


	void OnGUI() {
		GUIStyle labelStyle = GUI.skin.GetStyle("Label");
		#if UNITY_ANDROID
		labelStyle.fontSize = 40;
		#elif UNITY_IOS
		labelStyle.fontSize = 25;
		#endif
		float currenty = 10;
		float labelHeight = labelStyle.CalcHeight(new GUIContent("IBeacons"), Screen.width-20);
		GUI.Label(new Rect(currenty,10,Screen.width-20,labelHeight),"IBeacons");
		
		currenty += labelHeight;
		scrolldistance = GUI.BeginScrollView(new Rect(10,currenty,Screen.width -20, Screen.height - currenty - 10),scrolldistance,new Rect(0,0,Screen.width - 20,mybeacons.Count*100));
		GUILayout.BeginVertical("box",GUILayout.Width(Screen.width-20),GUILayout.Height(50));
		foreach (Beacon b in mybeacons) {
			GUILayout.Label("UUID: "+b.UUID);
			GUILayout.Label("Major: "+b.major);
			GUILayout.Label("Minor: "+b.minor);
			GUILayout.Label("Range: "+b.range.ToString());
			GUILayout.Label("Accuracy: "+b.accuracy.ToString());
		}
		GUILayout.EndVertical();
		GUI.EndScrollView();
	}

}

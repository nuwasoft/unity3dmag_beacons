using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public enum BluetoothLowEnergyState {
	UNKNOWN,
	RESETTING,
	UNSUPPORTED,
	UNAUTHORIZED,
	POWERED_OFF,
	POWERED_ON
}

[ExecuteInEditMode]
public class iBeaconReceiver : MonoBehaviour {
	public delegate void BeaconRangeChanged(List<Beacon> beacons);
	public static event BeaconRangeChanged BeaconRangeChangedEvent;
	public delegate void BluetoothStateChanged(BluetoothLowEnergyState state);
	public static event BluetoothStateChanged BluetoothStateChangedEvent;
	public iBeaconRegion[] regions;
	public string NSLocationUsageDescription;
	public bool updateDescription;
	public static bool btenabled = false;
	
#if UNITY_ANDROID
	private static AndroidJavaObject plugin;
#endif
	
	private static iBeaconReceiver m_instance;
	
	// assign variables to statics
	void Awake()
	{
		m_instance = this;
	}
	
	#if UNITY_IOS	
	[DllImport ("__Internal")]
	private static extern void InitReceiver(string regions, bool shouldLog);

	[DllImport ("__Internal")]
	private static extern void StopIOSScan();

	[DllImport ("__Internal")]
	private static extern void EnableIOSBluetooth();

	[DllImport ("__Internal")]
	private static extern int GetIOSBluetoothState();
#endif
	
	void Start () {
	}
	
	public static void Init() {
		#if !UNITY_EDITOR
		#if UNITY_IOS
		InitReceiver(iBeaconRegion.regionsToString(m_instance.regions),true);
		#elif UNITY_ANDROID
		GetPlugin().Call("Init",true);
		#endif
		#endif
	}
	
	public static void Stop() {
#if !UNITY_EDITOR
#if UNITY_IOS
		StopIOSScan();
#elif UNITY_ANDROID
		GetPlugin().Call("Stop");
#endif
#endif
	}
	
	public static void Scan() {
#if !UNITY_EDITOR
#if UNITY_IOS
		InitReceiver(iBeaconRegion.regionsToString(m_instance.regions),true);
#elif UNITY_ANDROID
		GetPlugin().Call("Scan");
#endif
#endif		
	}

	public static void CheckBluetoothLEStatus() {
#if !UNITY_EDITOR
#if UNITY_ANDROID
		if (!GetPlugin().Call<bool>("IsBLEFeatured")) {
			if (BluetoothStateChangedEvent != null)
				BluetoothStateChangedEvent(BluetoothLowEnergyState.UNSUPPORTED);
		} else {
			if (!GetPlugin().Call<bool>("IsBluetoothAvailable")) {
				if (BluetoothStateChangedEvent != null)
					BluetoothStateChangedEvent(BluetoothLowEnergyState.UNKNOWN);
			} else {
				if (!GetPlugin().Call<bool>("IsBluetoothTurnedOn")) {
					if (BluetoothStateChangedEvent != null)
						BluetoothStateChangedEvent(BluetoothLowEnergyState.POWERED_OFF);
					btenabled = false;
				} else {
					if (BluetoothStateChangedEvent != null)
						BluetoothStateChangedEvent(BluetoothLowEnergyState.POWERED_ON);
					btenabled = true;
				}
			}
		}
#elif UNITY_IOS
		int bletest = GetIOSBluetoothState();
		if (BluetoothStateChangedEvent != null)
			BluetoothStateChangedEvent((BluetoothLowEnergyState)bletest);
		btenabled = (bletest == 5);
#endif
#endif
	}

	public void ReportBluetoothStateChange(string newstate) {
		if (BluetoothStateChangedEvent != null)
			BluetoothStateChangedEvent((BluetoothLowEnergyState)int.Parse(newstate));
		btenabled = (int.Parse(newstate) == 5);
	}

    public static void EnableBluetooth() {
#if !UNITY_EDITOR
#if UNITY_ANDROID
		GetPlugin().Call("EnableBluetooth");
#elif UNITY_IOS
		EnableIOSBluetooth();
#endif
#endif
	}
#if UNITY_ANDROID
	public static AndroidJavaObject GetPlugin() {
		if (plugin == null) {
			plugin = new AndroidJavaObject("com.kaasa.ibeacon.BeaconService");
		}
		return plugin;
	}
#endif
	
	public void RangeBeacons(string beacons) {
		if (!string.IsNullOrEmpty(beacons)) {
			string beaconsClean = beacons.Remove(beacons.Length-1); // Get rid of last ;
			string[] beaconsArr = beaconsClean.Split(';');
			List<Beacon> tempbeacons = new List<Beacon>();
			foreach (string beacon in beaconsArr) {
				string[] beaconArr = beacon.Split(',');
				string uuid = beaconArr[0];
				int major = int.Parse(beaconArr[1]);
				int minor = int.Parse(beaconArr[2]);
				int range = int.Parse(beaconArr[3]);
				int strenght = int.Parse(beaconArr[4]);
				double accuracy = double.Parse(beaconArr[5]);
				Beacon bTmp = new Beacon(uuid,major,minor,range,strenght,accuracy);
				tempbeacons.Add(bTmp);
			}
			if (BeaconRangeChangedEvent != null)
				BeaconRangeChangedEvent(tempbeacons);
		}
	}

	void Update() {
		if (updateDescription) {
			PlayerPrefs.SetString("NSLocationUsageDescription",NSLocationUsageDescription);
			updateDescription = false;
		}
	}
}

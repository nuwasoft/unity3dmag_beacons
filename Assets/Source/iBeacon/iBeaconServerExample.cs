using UnityEngine;
using System.Collections;

public class iBeaconServerExample : MonoBehaviour {

	// Use this for initialization
	void Start () {
		iBeaconServer.Init();
		iBeaconServer.Transmit();
	}
	
	// Update is called once per frame
	void Update () {

	}
}

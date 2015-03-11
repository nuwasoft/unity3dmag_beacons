using UnityEngine;
using System.Collections;

[System.Serializable]
public class iBeaconRegion {

	public string regionName;
	public string UUID;

	public override string ToString() {
		return UUID+":"+regionName;
	}

	public static string regionsToString(iBeaconRegion[] regions) {
		string returnString = "";
		for (int i = 0; i < regions.Length; i++) {
			returnString += regions[i].ToString();
			if (i < regions.Length - 1)
				returnString += ";";
		}
		return returnString;
	}
}

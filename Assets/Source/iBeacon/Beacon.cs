using System;
using System.Collections;
using System.Collections.Generic;

public enum BeaconRange {
	UNKNOWN,
	FAR,
	NEAR,
	IMMEDIATE
}

public class Beacon : EqualityComparer<Beacon>, IComparable, IEquatable<Beacon>, IComparable<Beacon>  {
	
	public string UUID;
	public int major;
	public int minor;
	public BeaconRange range;
	public int strength;
	public double accuracy;
	public DateTime lastSeen;
	
	public Beacon (string _uuid, int _major, int _minor, int _range, int _strength, double _accuracy)
	{
		UUID = _uuid;
		major = _major;
		minor = _minor;
		range = (BeaconRange)_range;
		strength = _strength;
		accuracy = _accuracy;
		lastSeen = DateTime.Now;
	}
	
	public override string ToString() {
		return "UUID: "+this.UUID+"\nMajor: "+this.major+"\nMinor: "+this.minor+"\nRange: "+this.range.ToString();
	}

	public override int GetHashCode(Beacon a) {
		return a.major ^ a.minor;
	}

	public override int GetHashCode() {
		return major ^ minor;
	}

	public override bool Equals(Beacon a, Beacon b) {
		return (a.UUID.Equals(b.UUID)) && (a.major.Equals(b.major)) && (a.minor.Equals(b.minor)); 
	}

	public bool Equals(Beacon a) {
		if (a == null)
			return false;
		return (a.UUID.Equals(UUID)) && (a.major.Equals(major)) && (a.minor.Equals(minor));
	}

	public override bool Equals(Object obj) {
		Beacon beacon = obj as Beacon;
		if ((object)beacon == null)
			return false;
		return (beacon.UUID.Equals(UUID)) && (beacon.major.Equals(major)) && (beacon.minor.Equals(minor));
	}

	public static bool operator ==(Beacon a, Beacon b) {
		if (System.Object.ReferenceEquals(a,b))
			return true;

		if (((object)a == null) || ((object)b == null))
			return false;

		return (a.UUID.Equals(b.UUID)) && (a.major.Equals(b.major)) && (a.minor.Equals(b.minor)); 
	}

	public static bool operator !=(Beacon a, Beacon b) {
		return !(a==b);
	}

	public int CompareTo(Beacon b) {
		if (b == null) return 1;

		return this.accuracy.CompareTo(b.accuracy);
	}

	public int CompareTo(object obj) {
		if (obj == null) return 1;

		Beacon otherBeacon = obj as Beacon;
		if (otherBeacon != null)
			return this.accuracy.CompareTo(otherBeacon.accuracy);
		else
			throw new ArgumentException("Object is not a Beacon");
	}

}
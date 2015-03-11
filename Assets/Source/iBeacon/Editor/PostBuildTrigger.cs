﻿using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;

public static class PostBuildTrigger
{
	// Frameworks Ids  -  These ids have been generated by creating a project using Xcode then
	// extracting the values from the generated project.pbxproj.  The format of this
	// file is not documented by Apple so the correct algorithm for generating these
	// ids is unknown

	const string COREBLUETOOTH_ID = "3807A4591715705700A5474C" ;
	const string COREBLUETOOTH_FILEREFID = "3807A4581715705700A5474C" ;

	// List of all the frameworks to be added to the Projekt
	public struct framework
	{
		public string sName ;
		public string sId ;
		public string sFileId ;

		public framework (string name, string myId, string fileid)
		{
			sName = name;
			sId = myId;
			sFileId = fileid;
		}
	}
	
	/// Processbuild Function
	[PostProcessBuild]
	// <- this is where the magic happens
    public static void OnPostProcessBuild (BuildTarget target, string path)
	{
		// 1: Check this is an iOS build before running
#if UNITY_IPHONE
        {
            // 2: We init our tab and process our project
            framework[] myFrameworks = { new framework("CoreBluetooth.framework", COREBLUETOOTH_ID, COREBLUETOOTH_FILEREFID) } ;
            string xcodeprojPath = Application.dataPath ;
            xcodeprojPath = path+"/Unity-iPhone.xcodeproj" ;
//          Debug.Log("We found xcodeprojPath to be : "+xcodeprojPath) ;
            updateXcodeProject(xcodeprojPath, myFrameworks) ;
			Debug.Log("OnPostProcessBuild - Successfully integrated") ;
			string plistPath = Application.dataPath;
			plistPath = path;
			updateInfoPlist(plistPath,"NSLocationAlwaysUsageDescription",PlayerPrefs.GetString("NSLocationUsageDescription",""));
        }
#endif
	}

	public static void updateInfoPlist (string path, string key, string value) {
		string plistfile = path + "/Info.plist";
		string[] lines = System.IO.File.ReadAllLines(plistfile);
		// the last values of the plist file are </dict> and </plist> so we need to insert our line before that
		string newContent = "";
		for (int i = 0; i < lines.Length; i++) {
			if (i < lines.Length -2) {
				newContent += lines[i];
				newContent += "\n";
			}
		}
		newContent += "<key>";
		newContent += key;
		newContent += "</key><string>";
		newContent += value;
		newContent += "</string>\n";
		newContent += lines[lines.Length - 2];
		newContent += "\n";
		newContent += lines[lines.Length - 1];
		FileStream filestr = new FileStream(plistfile,FileMode.Create);
		filestr.Close();
		StreamWriter plistWriter = new StreamWriter(plistfile);
		plistWriter.Write(newContent);
		plistWriter.Close();
	}

	// MAIN FUNCTION
	// xcodeproj_filename - filename of the Xcode project to change
	// frameworks - list of Apple standard frameworks to add to the project
	public static void updateXcodeProject (string xcodeprojPath, framework[] listeFrameworks)
	{
		// STEP 1 : We open up the file generated by Unity and read into memory as
		// a list of lines for processing
		string project = xcodeprojPath + "/project.pbxproj";
		Debug.Log (project);
		string[] lines = System.IO.File.ReadAllLines (project);

		// STEP 2 : We check if file has already been processed and only proceed if it hasn't,
		// we'll do this by looping through the build files and see if CoreBluetooth.framework
		// is there
		int i = 0;
		bool bFound = false;
		bool bEnd = false;
		List<string> usedids = new List<string>();
		while (!bFound && !bEnd) {
			if (lines [i].Length > 5 && (String.Compare (lines [i].Substring (3, 3), "End") == 0))
				bEnd = true;
			string id = lines[i];
			if (id.StartsWith("\t\t")) {
				id = id.Replace("\t\t","");
				id = id.Remove(24);
				usedids.Add(id);
			}
			bFound = lines [i].Contains ("CoreBluetooth.framework");
			++i;
		}

		if (bFound)
			Debug.Log ("OnPostProcessBuild - ERROR: Frameworks have already been added to XCode project");
		else {
			// STEP 3 : We'll open/replace project.pbxproj for writing and iterate over the old
			// file in memory, copying the original file and inserting every extra we need
			FileStream filestr = new FileStream (project, FileMode.Create); //Create new file and open it for read and write, if the file exists overwrite it.
			filestr.Close ();
			StreamWriter fCurrentXcodeProjFile = new StreamWriter (project); // will be used for writing

			// As we iterate through the list we'll record which section of the
			// project.pbxproj we are currently in
			string section = "";

			// We use this boolean to decide whether we have already added the list of
			// build files to the link line.  This is needed because there could be multiple
			// build targets and they are not named in the project.pbxproj
			bool bFrameworks_build_added = false;
			//string newid = generate_id(usedids.ToArray());
			//listeFrameworks[0] = new framework("CoreBluetooth.framework",newid,newid);
			                        
			int iNbBuildConfigSet = 0; // can't be > 2
			i = 0;
			foreach (string line in lines) {
				if (line.StartsWith ("\t\t\t\tGCC_ENABLE_CPP_EXCEPTIONS") ||
                    line.StartsWith ("\t\t\t\tGCC_ENABLE_CPP_RTTI") ||
                    line.StartsWith ("\t\t\t\tGCC_ENABLE_OBJC_EXCEPTIONS")) {
					// apparently, we don't copy those lines in our new project
				} else {
					fCurrentXcodeProjFile.WriteLine (line);
					if (lines [i].Length > 7 && String.Compare (lines [i].Substring (3, 5), "Begin") == 0) {
						section = line.Split (' ') [2];
						//Debug.Log("NEW_SECTION: "+section) ;
						if (section == "PBXBuildFile") {
							foreach (framework fr in listeFrameworks)
								add_build_file (fCurrentXcodeProjFile, fr.sId, fr.sName, fr.sFileId);
						}

						if (section == "PBXFileReference") {
							foreach (framework fr in listeFrameworks)
								add_framework_file_reference (fCurrentXcodeProjFile, fr.sFileId, fr.sName);
						}
						if (line.Length > 5 && String.Compare (line.Substring (3, 3), "End") == 0)
							section = "";
					}

					// The PBXResourcesBuildPhase section is what appears in XCode as 'Link
					// Binary With Libraries'.  As with the frameworks we make the assumption the
					// first target is always 'Unity-iPhone' as the name of the target itself is
					// not listed in project.pbxproj

					if (section == "PBXFrameworksBuildPhase" &&
                        line.Trim ().Length > 4 &&
                        String.Compare (line.Trim ().Substring (0, 5), "files") == 0 &&
                        !bFrameworks_build_added) {
						foreach (framework fr in listeFrameworks)
							add_frameworks_build_phase (fCurrentXcodeProjFile, fr.sId, fr.sName);
						bFrameworks_build_added = true;
					}

					// The PBXGroup is the section that appears in XCode as 'Copy Bundle Resources'.
					if (section == "PBXGroup" &&
                        line.Trim ().Length > 7 &&
                        String.Compare (line.Trim ().Substring (0, 8), "children") == 0 &&
                        lines [i - 2].Trim ().Split (' ').Length > 0 &&
                        String.Compare (lines [i - 2].Trim ().Split (' ') [2], "Frameworks") == 0) {
						foreach (framework fr in listeFrameworks)
							add_group (fCurrentXcodeProjFile, fr.sFileId, fr.sName);
					}

					if (section == "XCBuildConfiguration" &&
                        line.StartsWith ("\t\t\t\tOTHER_LDFLAGS") &&
                        iNbBuildConfigSet < 2) {
						//fCurrentXcodeProjFile.Write("\t\t\t\t\t\"-all_load\",\n") ;
						fCurrentXcodeProjFile.Write ("\t\t\t\t\t\"-ObjC\",\n");
						//Debug.Log("OnPostProcessBuild - Adding \"-ObjC\" flag to build options") ; // \"-all_load\" and
						++iNbBuildConfigSet;
					}
				}
				++i;
			}
			fCurrentXcodeProjFile.Close ();
		}
	}

	// Adds a line into the PBXBuildFile section
	private static void add_build_file (StreamWriter file, string id, string name, string fileref)
	{
		//Debug.Log("OnPostProcessBuild - Adding build file " + name) ;
		string subsection = "Frameworks";
		if (name == "CoreBluetooth.framework")  // CoreBluetooth.framework should be weak-linked
			file.Write ("\t\t" + id + " /* " + name + " in " + subsection + " */ = {isa = PBXBuildFile; fileRef = " + fileref + " /* " + name + " */; settings = {ATTRIBUTES = (Weak, ); }; };");
		else // Others framework are normal
			file.Write ("\t\t" + id + " /* " + name + " in " + subsection + " */ = {isa = PBXBuildFile; fileRef = " + fileref + " /* " + name + " */; };\n");
	}

	// Adds a line into the PBXBuildFile section
	private static void add_framework_file_reference (StreamWriter file, string id, string name)
	{
		//Debug.Log("OnPostProcessBuild - Adding framework file reference " + name) 
		string path = "System/Library/Frameworks"; // all the frameworks come from here
		if (name == "libsqlite3.0.dylib")           // except for lidsqlite
			path = "usr/lib";
		file.Write ("\t\t" + id + " /* " + name + " */ = {isa = PBXFileReference; lastKnownFileType = wrapper.framework; name = " + name + "; path = " + path + "/" + name + "; sourceTree = SDKROOT; };\n");
	}
	
	// Adds a line into the PBXFrameworksBuildPhase section
	private static void add_frameworks_build_phase (StreamWriter file, string id, string name)
	{
		//Debug.Log("OnPostProcessBuild - Adding build phase " + name) ;
		file.Write ("\t\t\t\t" + id + " /* " + name + " in Frameworks */,\n");
	}

	// Adds a line into the PBXGroup section
	private static void add_group (StreamWriter file, string id, string name)
	{
		//Debug.Log("OnPostProcessBuild - Add group " + name)
		file.Write ("\t\t\t\t" + id + " /* " + name + " */,\n");
	}
	
	// Will generate a unique id according to the following format: 3807A4591715705700A5474C (A-F, 0-8, 24 digits) for the framework.
	// this will also ensure that the id is not yet used in the project.
	private static string generate_id(string[] usedids) {
		string[] subset = new string[] {"A","B","C","D","E","F","0","1","2","3","4","5","6","7","8","9"};
		string gen_id = "";
		for (int i = 0; i < 24; i++) {
			gen_id += subset[UnityEngine.Random.Range(0,subset.Length-1)];		
		}
		bool isused = false;
		foreach (string usedid in usedids) {
			if (usedid.Equals(gen_id))
				isused = true;
		}
		if (isused)
			return generate_id(usedids);
		Debug.Log("Generated id: "+gen_id);
		return gen_id;
	}
}
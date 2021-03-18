using UnityEngine;
using UnityEditor;

public class EventImporterPreferences  {
	private static bool prefsLoaded = false;
	private static EventImporter.ImportMode importMode = EventImporter.ImportMode.ImportOff;
	private static string blenderPath = "";

	public const string importModePref = "com.restemeier.eventImporter.importMode";
	public const string blenderPathPref = "com.restemeier.eventImporter.blenderPath";

	[PreferenceItem("Event Import")]
	public static void PreferencesGUI () 
	{
		// Load the preferences
		if (!prefsLoaded) {
			importMode = (EventImporter.ImportMode)EditorPrefs.GetInt  (importModePref, (int)importMode);
			blenderPath = EditorPrefs.GetString(blenderPathPref, blenderPath);
			prefsLoaded = true;
		}
		
		// Preferences GUI
		importMode = (EventImporter.ImportMode)EditorGUILayout.EnumPopup("Event Import Mode", importMode);
		GUI.enabled = importMode == EventImporter.ImportMode.ImportAutomatic;
		EditorGUILayout.BeginHorizontal();
		blenderPath = EditorGUILayout.TextField("Blender Path", blenderPath);
		if (GUILayout.Button("...", GUILayout.ExpandWidth(false))) {
			string ext;
			if (Application.platform == RuntimePlatform.OSXEditor) {
				ext = "app";
			} else {
				ext = "exe";
			}
			string path = EditorUtility.OpenFilePanel(
				"Find Blender executable",
				blenderPath,
				ext);
			if (path.Length > 0) {
				blenderPath = path;
				GUI.changed = true;
			}
		}
		EditorGUILayout.EndHorizontal();
		GUI.enabled = true;

		// Save the preferences
		if (GUI.changed) {
			EditorPrefs.SetInt(importModePref, (int)importMode);
			EditorPrefs.SetString(blenderPathPref, blenderPath);
		}
	}
}

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;
using System.Runtime.Serialization;
using Catacumba.Entity;

public class EventImporter : AssetPostprocessor {

	public enum ImportMode {
		ImportOff,
		ImportXml,
		ImportAsset,
		ImportAutomatic
	}

	// Menu item to add an empty Event Data asset to the project.
	// This is only active if we import our events from an asset file.
	[MenuItem("Window/Add Event Data", true)]
	public static bool AddEventDataValidate()
	{
		return false;
		ImportMode importMode = (ImportMode)EditorPrefs.GetInt  (EventImporterPreferences.importModePref, (int)ImportMode.ImportOff);
		return (importMode == ImportMode.ImportAsset);
	}

	[MenuItem("Window/Add Event Data")]
	public static void AddEventData()
	{
		return;
		Transform[] selection = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Assets);
		foreach (Transform transform in selection) {
			string assetPath = AssetDatabase.GetAssetPath(transform);

			// maybe it would be better to create a separate events file for each animation found in the asset... but this is only a test anyway.
			string eventPath = assetPath.Substring(0, assetPath.LastIndexOf(".")) + ".events.asset";
			EventData eventData = EventData.CreateInstance<EventData>();
			AssetDatabase.CreateAsset(eventData, eventPath);
		}
	}

	public void OnPostprocessModel (GameObject g)
	{
		return;
		ImportMode importMode = (ImportMode)EditorPrefs.GetInt (EventImporterPreferences.importModePref, (int)ImportMode.ImportOff);
		if (importMode != ImportMode.ImportOff) {
			// ModelImporter modelImporter = assetImporter as ModelImporter;
			string modelBasePath = assetPath.Substring(0, assetPath.LastIndexOf("."));
			string modelExt = assetPath.Substring (assetPath.LastIndexOf(".")).ToLower();

			EventData eventData = null;
			XMLEvents.Scene xmlScene = null;

			if (importMode == ImportMode.ImportXml) {
				// load XML data
				string xmlPath = Path.GetFullPath(Path.Combine (Path.Combine (Application.dataPath, ".."), modelBasePath + ".events.xml"));
				if (File.Exists (xmlPath)) {
					xmlScene = XMLEvents.Scene.Load (xmlPath);
				}
			}

			if (importMode == ImportMode.ImportAutomatic) {
				if (modelExt == ".blend") {
					string blenderPath =  EditorPrefs.GetString(EventImporterPreferences.blenderPathPref);
					if (blenderPath.Length > 0) {
						// call Blender with the export script
						string xmlPath = Path.GetFullPath(Path.Combine (Path.Combine (Application.dataPath, ".."), FileUtil.GetUniqueTempPathInProject () + ".events.xml"));
						string scriptPath = Path.GetFullPath (Path.Combine (Application.dataPath, "Blender\\io_anim_events\\export_events.py"));
						string blendPath = Path.GetFullPath (Path.Combine (Path.Combine (Application.dataPath, ".."), assetPath));
						
						Process p = new Process();
						p.StartInfo.UseShellExecute = false;
						p.StartInfo.RedirectStandardOutput = true;
						p.StartInfo.FileName = blenderPath;
						p.StartInfo.Arguments = string.Format ("-b \"{0}\" -P \"{2}\" -- \"{1}\"", blendPath, xmlPath, scriptPath);
						
						// UnityEngine.Debug.Log (p.StartInfo.Arguments);
						p.Start();
						// Read the output stream first and then wait.
						//string output = p.StandardOutput.ReadToEnd();
						// UnityEngine.Debug.Log (output);
						p.WaitForExit();

						xmlScene = XMLEvents.Scene.Load (xmlPath);
						File.Delete(xmlPath);
					} else {
						UnityEngine.Debug.LogWarning("Please set the path to the Blender executable in the preferences!");
					}
				} else {
					UnityEngine.Debug.LogWarning (String.Format ("Automatic import is only supported for blender assets, {0} isn't a blender file", assetPath));
				}
			}

			if (xmlScene != null) {
				if (xmlScene.timeline.markers.Count > 0) {
					UnityEngine.Debug.LogWarning("Timeline markers are not used, yet.");
				}

				// convert XML data into event data
				eventData = EventData.CreateInstance<EventData>();
				foreach(XMLEvents.Action xmlAction in xmlScene.actions) {
					ActionInfo actionInfo = new ActionInfo();
					actionInfo.name = xmlAction.name;
					foreach (XMLEvents.Marker xmlMarker in xmlAction.markers) {
						EventInfo eventInfo = new EventInfo();
						eventInfo.time = (float)xmlMarker.frame / (float)xmlScene.fps;
						eventInfo.value = xmlMarker.name;
						actionInfo.eventList.Add (eventInfo);
					}
					eventData.actionlist.Add (actionInfo);
				}
			}

			if (importMode == ImportMode.ImportAsset) {
				// load scriptable object data from asset
				string eventPath = modelBasePath + ".events.asset";
				eventData = AssetDatabase.LoadAssetAtPath(eventPath, typeof(EventData)) as EventData;
			}

			if (eventData != null) {
				// add a component that receives the animation events!
				g.AddComponent<CharacterAnimator>();
				System.Type eventReceiverType = typeof(CharacterAnimator);

				// a simple linear search would probably work as well, but let's do this properly in case
				// someone has a silly amount of actions:
				Dictionary<string, ActionInfo> actionInfoLookup = new Dictionary<string, ActionInfo>();
				foreach(ActionInfo actionInfo in eventData.actionlist) {
					actionInfoLookup[actionInfo.name] = actionInfo;
				}

				// get animations attached to the object:
				List<AnimationClip> animationClipList = new List<AnimationClip>(AnimationUtility.GetAnimationClips(g));

				// GetAnimationClips doesn't work with Mecanim, so use this hack:
				if (animationClipList.Count == 0) {
					// This is a bit ugly because it may pick up AnimationClip objects that are defined in the scene, but I 
					// haven't found another way to get the clips imported with mecanim:
					AnimationClip[] objectList = UnityEngine.Object.FindObjectsOfType (typeof(AnimationClip)) as AnimationClip[];
					animationClipList.AddRange(objectList);
				}

                // process all animations
				foreach(AnimationClip animationClip in animationClipList) {
					// check if we have animations for this clip
					if (actionInfoLookup.ContainsKey(animationClip.name)) { 
						// get the events for this clip
						ActionInfo actionInfo = actionInfoLookup[animationClip.name];

                        // get existing animation events if they're defined through mecanim
                        List<AnimationEvent> animationEventList = new List<AnimationEvent>(AnimationUtility.GetAnimationEvents(animationClip));

						// add events
						foreach(EventInfo eventInfo in actionInfo.eventList) {
							AnimationEvent animationEvent = new AnimationEvent();
							animationEvent.time = eventInfo.time;

							// parse the string value into a function call. This can be a simple 
							// tag to function mapping or any system you want.
							// This code tries to parse the marker into a function call.

							int openBracket = eventInfo.value.IndexOf("(");
							int closeBracket = eventInfo.value.IndexOf (")");

							string functionName;
							string parameter;
							if (openBracket < 0) {
								functionName = eventInfo.value;
								parameter = "";
							} else {
								functionName = eventInfo.value.Substring(0, openBracket).Trim();
								if (closeBracket < 0) {
									parameter = "";
								} else {
									parameter = eventInfo.value.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim ();
								}
							}

							// check if the event is defined and which type the parameter has. If there is more than one component to receive
							// events this can become more complicated:
							MethodInfo methodInfo = eventReceiverType.GetMethod(functionName);
							animationEvent.functionName = functionName;
							if (methodInfo != null) {
								// use reflection data to parse the parameter
								ParameterInfo[] parameterInfo = methodInfo.GetParameters();
								if (parameterInfo.Length > 1) {
									UnityEngine.Debug.LogWarning(String.Format("Method {0} has {1} parameters. One parameter is expected.", functionName, parameterInfo.Length));
								} else {
									if (parameterInfo.Length > 0) {
										if (parameterInfo[0].ParameterType == typeof(float)) {
											animationEvent.floatParameter = float.Parse(parameter, CultureInfo.InvariantCulture);
										} else if (parameterInfo[0].ParameterType == typeof(int)) {
											animationEvent.intParameter = int.Parse(parameter);
										} else if (parameterInfo[0].ParameterType == typeof(string)) {
											int openQuote = parameter.IndexOf("\"");
											int closeQuote = parameter.LastIndexOf("\"");
											animationEvent.stringParameter = parameter.Substring(openQuote + 1, closeQuote - openQuote - 1);
										}
										else if (parameterInfo[0].ParameterType == typeof(EAttackType)) {
											animationEvent.intParameter = int.Parse(parameter);
										} else {
											UnityEngine.Debug.LogWarning (String.Format("Method {0} parameter type {1} not handled.", functionName, parameterInfo.GetType().Name));
										}
									}
								}
							} else {
								// fallback if a function is not defined.
								UnityEngine.Debug.LogWarning (String.Format("Method {0} not found on class {1}. Adding animation event without type checking.", functionName, eventReceiverType.Name)); 
								if (parameter.Length > 0) {
									if (parameter.Contains("\"")) {
										int openQuote = parameter.IndexOf("\"");
										int closeQuote = parameter.LastIndexOf("\"");
										animationEvent.stringParameter = parameter.Substring(openQuote + 1, closeQuote - openQuote - 1);
									} else if (parameter.Contains (".")) {
										animationEvent.floatParameter = float.Parse(parameter, CultureInfo.InvariantCulture);
									} else {
										animationEvent.intParameter = int.Parse(parameter);
									}
								}
							}
							animationEventList.Add (animationEvent);
						}

						// store new events in the clip
						//AnimationUtility.SetAnimationEvents(animationClip, animationEventList.ToArray());
						animationClip.events = animationEventList.ToArray();

						string path = assetPath.Replace(".fbx", $"_{animationClip.name}.anim");
						AssetDatabase.CreateAsset(animationClip, path);
					}
				}
			}
		}
    }
}

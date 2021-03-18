using UnityEngine;
using UnityEditor;
using System.Reflection;
using Catacumba.Entity;
using System.Collections.Generic;
using System.Globalization;

public class CharacterAnimationEventDataImporter : AssetPostprocessor
{
    public bool IsValid
    {
        get { return assetPath.Contains("Characters") &&
                     assetPath.Contains(".fbx") &&
                     assetPath.Contains("Custom") 
                     ; }
    }

    private string GetXMLPath()
    {
        return assetPath.Replace(".fbx", ".xml");
    }

    void OnPreprocessAnimation()
    { 
        if (!IsValid) return;
        Debug.Log($"Processing model {assetImporter.assetPath}");
        RenameClips();
    }

    // Removes "Armature|" from clip names.
    void RenameClips()
    {
        ModelImporter modelImporter = assetImporter as ModelImporter;
        ModelImporterClipAnimation[] clipAnimations = modelImporter.defaultClipAnimations;

        for (int i = 0; i < clipAnimations.Length; i++)
            clipAnimations[i].name = clipAnimations[i].name.Replace("Armature|", string.Empty);

        modelImporter.clipAnimations = clipAnimations;
        modelImporter.SaveAndReimport();
    }

    void OnPostprocessModel(GameObject obj)
    {
        if (!IsValid) return;
        ModelImporter modelImporter = assetImporter as ModelImporter;
        ProcessXML(obj);
    }

    void ProcessXML(GameObject obj)
    {
        string xmlPath = GetXMLPath();

        XMLEvents.Scene xmlScene = XMLEvents.Scene.Load (xmlPath);
        if (xmlScene.timeline.markers.Count == 0)
            return;

        EventData eventData = LoadEventData(xmlScene);

        System.Type eventReceiverType = typeof(CharacterAnimator);

        // a simple linear search would probably work as well, but let's do this properly in case
        // someone has a silly amount of actions:
        Dictionary<string, ActionInfo> actionInfoLookup = new Dictionary<string, ActionInfo>();
        foreach(ActionInfo actionInfo in eventData.actionlist) {
            actionInfoLookup[actionInfo.name] = actionInfo;
        }

        // get animations attached to the object:
        List<AnimationClip> animationClipList = LoadClips(obj);

        ModelImporter modelImporter = assetImporter as ModelImporter;

        // process all animations
        for (int i = 0; i < animationClipList.Count; i++) {
            AnimationClip animationClip = animationClipList[i];

            // check if we have animations for this clip
            if (!actionInfoLookup.ContainsKey(animationClip.name))  
                continue;

            ActionInfo actionInfo = actionInfoLookup[animationClip.name];

            // get existing animation events if they're defined through mecanim
            List<AnimationEvent> animationEventList = new List<AnimationEvent>();

            // add events
            foreach(EventInfo eventInfo in actionInfo.eventList) {
                AnimationEvent animationEvent = ParseEvent(eventInfo);
                animationEventList.Add (animationEvent);
            }

            AddEvent.SetEvents(modelImporter, animationClip.name, animationEventList.ToArray());

            // copy animation
            string path = assetPath.Replace(".fbx", $"_{animationClip.name}.anim");
            AnimationClip clip = UnityEngine.Object.Instantiate(animationClip);
            AssetDatabase.CreateAsset(clip, path);

            AnimationUtility.SetAnimationEvents(clip, animationEventList.ToArray());
        }
    }

    EventData LoadEventData(XMLEvents.Scene xmlScene)
    {
        // convert XML data into event data
        EventData eventData = EventData.CreateInstance<EventData>();
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

        return eventData;
    }

    List<AnimationClip> LoadClips(GameObject obj)
    {
        List<AnimationClip> animationClipList = new List<AnimationClip>(AnimationUtility.GetAnimationClips(obj));

        ModelImporter modelImporter = assetImporter as ModelImporter;

        // GetAnimationClips doesn't work with Mecanim, so use this hack:
        if (animationClipList.Count == 0) {
            // This is a bit ugly because it may pick up AnimationClip objects that are defined in the scene, but I 
            // haven't found another way to get the clips imported with mecanim:
            AnimationClip[] objectList = UnityEngine.Object.FindObjectsOfType (typeof(AnimationClip)) as AnimationClip[];
            animationClipList.AddRange(objectList);
        }

        return animationClipList;
    }

    AnimationEvent ParseEvent(EventInfo eventInfo)
    {
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
        System.Type eventReceiverType = typeof(CharacterAnimator);
        MethodInfo methodInfo = eventReceiverType.GetMethod(functionName);
        animationEvent.functionName = functionName;
        if (methodInfo == null) {
            UnityEngine.Debug.LogWarning (string.Format("Method {0} not found on class {1}. Adding animation event without type checking.", functionName, eventReceiverType.Name)); 
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

            return animationEvent;
        }

        // use reflection data to parse the parameter
        ParameterInfo[] parameterInfo = methodInfo.GetParameters();
        if (parameterInfo.Length > 1) {
            UnityEngine.Debug.LogWarning(string.Format("Method {0} has {1} parameters. One parameter is expected.", functionName, parameterInfo.Length));
            return animationEvent;
        } 

        if (parameterInfo.Length == 0) {
            return animationEvent;
        }

        ParameterInfo param = parameterInfo[0];

        if (param.ParameterType == typeof(float)) 
        {
            animationEvent.floatParameter = float.Parse(parameter, CultureInfo.InvariantCulture);
        } 
        else if (param.ParameterType == typeof(int)) 
        {
            animationEvent.intParameter = int.Parse(parameter);
        } 
        else if (param.ParameterType == typeof(string)) 
        {
            int openQuote = parameter.IndexOf("\"");
            int closeQuote = parameter.LastIndexOf("\"");
            animationEvent.stringParameter = parameter.Substring(openQuote + 1, closeQuote - openQuote - 1);
        }
        else if (param.ParameterType == typeof(EAttackType)) 
        {
            animationEvent.intParameter = int.Parse(parameter);
        } 
        else 
        {
            UnityEngine.Debug.LogWarning($"Method {functionName} parameter type {parameterInfo.GetType().Name} not handled.");
        }
        

        return animationEvent;
    }

}
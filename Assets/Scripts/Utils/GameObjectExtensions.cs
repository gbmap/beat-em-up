using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public static class GameObjectExtensions
{
    public delegate void EventFunction<T>(T handler);

    public static void SendMessage<T>(this GameObject go, EventFunction<T> functor) where T : IEventSystemHandler
    {
        ExecuteEvents.Execute<T>(go, null, (obj, data) => functor(obj));
    }

    public static void BroadcastMessage<T>(this GameObject go, EventFunction<T> functor) where T : IEventSystemHandler
    {
        ExecuteEvents.ExecuteHierarchy<T>(go, null, (obj, data) => functor(obj));
    }

    public static T GetComponentFromInterface<T>(this GameObject obj) where T : class
    {
        MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>(); 
        foreach (MonoBehaviour c in components)
        {
            if (c is T)
                return c as T;
        }
        return null;
    }

    public static T[] GetComponentsFromInterface<T>(this GameObject obj) where T : class
    {
        List<T> interfaces = new List<T>();
        MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in components)
        {
            if (c is T)
                interfaces.Add(c as T);
        }
        return interfaces.ToArray();
    }

    public static T[] GetComponentsInChildrenFromInterface<T>(this GameObject obj) where T : class
    {
        List<T> interfaces = new List<T>();
        MonoBehaviour[] components = obj.GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour c in components)
        {
            if (c is T)
                interfaces.Add(c as T);
        }
        return interfaces.ToArray();
    }

    public static Transform GetFirstChildByNameRecursive (this Transform transform, string childName)
    {       
        Transform foundChild = null;
        for (int i = 0; i < transform.childCount; i ++)
        {
            Transform child = transform.GetChild(i);

            if (child.name == childName) {
                foundChild = child;
            }           
            
            if (foundChild == null) {
                foundChild = GetFirstChildByNameRecursive (child, childName);               
            }
            
        }   
        return foundChild;
    }

}
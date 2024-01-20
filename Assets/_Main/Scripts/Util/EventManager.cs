using UnityEngine;
using System;
using System.Collections.Generic;

public enum EventID {
    None = 0, // default value - should never be invoked
    PrimaryDown = 1,        // Input
    SecondaryDown = 2,
    TertiaryDown = 3,
    
}

public class EventManager : MonoBehaviour {
    // Dict holds events per gameObject instance
    static Dictionary<object, Dictionary<EventID, Delegate>> eventsDict;
    static Dictionary<object, Dictionary<EventID, Delegate>> oneParamEventsDict;

    // public static EventManager Instance => _instance;
    static EventManager _instance;

    void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
        } else {
            _instance = this;
        }

        eventsDict = new Dictionary<object, Dictionary<EventID, Delegate>>();
        oneParamEventsDict = new Dictionary<object, Dictionary<EventID, Delegate>>();
    }

    public static void Subscribe(object ownerObj, EventID eventID, Action listener) {
        Dictionary<EventID, Delegate> ownerObjEvents;
        if (eventsDict.TryGetValue(ownerObj, out ownerObjEvents)) {
            if (ownerObjEvents.ContainsKey(eventID)) {
                // Delegate.Combine adds listeners - similar to event += listener
                ownerObjEvents[eventID] = Delegate.Combine(ownerObjEvents[eventID], listener);
            } else {
                ownerObjEvents[eventID] = listener;
            }
        } else {
            ownerObjEvents = new Dictionary<EventID, Delegate>();
            ownerObjEvents[eventID] = listener;
            eventsDict[ownerObj] = ownerObjEvents;
        }
    } 
    public static void Subscribe<T>(object ownerObj, EventID eventID, Action<T> listener) {
        Dictionary<EventID, Delegate> ownerObjEvents;
        if (oneParamEventsDict.TryGetValue(ownerObj, out ownerObjEvents)) {
            if (ownerObjEvents.ContainsKey(eventID)) {
                ownerObjEvents[eventID] = Delegate.Combine(ownerObjEvents[eventID], listener);
            } else {
                ownerObjEvents[eventID] = listener;
            }
        } else {
            ownerObjEvents = new Dictionary<EventID, Delegate>();
            ownerObjEvents[eventID] = listener;
            oneParamEventsDict[ownerObj] = ownerObjEvents;
        }
    }
    
    public static void Unsubscribe(object ownerObj, EventID eventID, Action listener) {
        Dictionary<EventID, Delegate> ownerObjEvents;
        if (eventsDict.TryGetValue(ownerObj, out ownerObjEvents)) {
            if (ownerObjEvents.ContainsKey(eventID)) {
                // Delegate.Remove removes listener - similar to event -= listener
                ownerObjEvents[eventID] = Delegate.Remove(ownerObjEvents[eventID], listener);
                if (ownerObjEvents[eventID] == null) {
                    eventsDict.Remove(ownerObj);
                }
            }
        }
    }
    public static void Unsubscribe<T>(object ownerObj, EventID eventID, Action<T> listener) {
        Dictionary<EventID, Delegate> ownerObjEvents;
        if (oneParamEventsDict.TryGetValue(ownerObj, out ownerObjEvents)) {
            if (ownerObjEvents.ContainsKey(eventID)) {
                ownerObjEvents[eventID] = Delegate.Remove(ownerObjEvents[eventID], listener);
                if (ownerObjEvents[eventID] == null) {
                    oneParamEventsDict.Remove(ownerObj);
                }
            }
        }
    }

    public static void Invoke(object ownerObj, EventID eventID) {
        Dictionary<EventID, Delegate> ownerObjEvents;
        if (eventsDict.TryGetValue(ownerObj, out ownerObjEvents)) {
            if (ownerObjEvents.TryGetValue(eventID, out Delegate eventAction)) {
                foreach (Delegate d in eventAction.GetInvocationList()) {   // c# v4 only has this to get combined delegates seemingly...
                    if (d is Action) {
                        (d as Action).Invoke();
                    }
                }
            }
        }
    }
    public static void Invoke<T>(object ownerObj, EventID eventID, T eventData = default) {
        Dictionary<EventID, Delegate> ownerObjEvents;
        if (oneParamEventsDict.TryGetValue(ownerObj, out ownerObjEvents)) {
            if (ownerObjEvents.TryGetValue(eventID, out Delegate eventAction)) {
                foreach (Delegate d in eventAction.GetInvocationList()) {
                    if (d is Action<T>) {
                        (d as Action<T>).Invoke(eventData);
                    }
                }
            }
        }
    }
}
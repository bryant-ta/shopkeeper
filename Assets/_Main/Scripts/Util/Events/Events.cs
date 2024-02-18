using System;
using System.Collections.Generic;

namespace EventManager {
public enum EventID {
    None = 0,        // default value - should never be invoked
    PrimaryDown = 1, // Input
    PrimaryHeld = 2,
    PrimaryUp = 3,
    SecondaryDown = 4,
    SecondaryHeld = 5,
    SecondaryUp = 6,
    Point = 7,
    Scroll = 8,
    Move = 10,
    Rotate = 11,
    Drop = 12,
    Cancel = 13,
    Pause = 14,
    
    // ProductDelivered = 100,
    // ProductOrdered = 101,
    // ProductFulfilled = 102,
}

/// <summary>
/// Set Events to load first in script order (Project Settings) to enable events subs in Awake().
/// Otherwise, event subs must occur in Start().
/// </summary>
public class Events : Singleton<Events> {
    // holds events per gameObject instance
    static Dictionary<object, Dictionary<EventID, Delegate>> eventsDict;
    static Dictionary<object, Dictionary<EventID, Delegate>> oneParamEventsDict;

    void Awake() {
        // Required to reset dicts every Play mode start because static
        eventsDict = new Dictionary<object, Dictionary<EventID, Delegate>>();
        oneParamEventsDict = new Dictionary<object, Dictionary<EventID, Delegate>>();
    }

    public static void Sub(object ownerObj, EventID eventID, Action listener) {
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
    public static void Sub<T>(object ownerObj, EventID eventID, Action<T> listener) {
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

    public static void Unsub(object ownerObj, EventID eventID, Action listener) {
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
    public static void Unsub<T>(object ownerObj, EventID eventID, Action<T> listener) {
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

    static Delegate[] invocationArray;
    public static void Invoke(object ownerObj, EventID eventID) {
        Dictionary<EventID, Delegate> ownerObjEvents;
        if (eventsDict.TryGetValue(ownerObj, out ownerObjEvents)) {
            if (ownerObjEvents.TryGetValue(eventID, out Delegate eventAction)) {
                invocationArray = eventAction.GetInvocationList(); // c# v4 only has this to get multicast delegates seemingly...
                for (int i = 0; i < invocationArray.Length; i++) {
                    if (invocationArray[i] is Action action) {
                        action.Invoke();
                    }
                }
            }
        }
    }
    static Delegate[] invocationArrayOneParam;
    public static void Invoke<T>(object ownerObj, EventID eventID, T eventData) {
        Dictionary<EventID, Delegate> ownerObjEvents;
        if (oneParamEventsDict.TryGetValue(ownerObj, out ownerObjEvents)) {
            if (ownerObjEvents.TryGetValue(eventID, out Delegate eventAction)) {
                invocationArrayOneParam = eventAction.GetInvocationList();
                for (int i = 0; i < invocationArrayOneParam.Length; i++) {
                    if (invocationArrayOneParam[i] is Action<T> action) {
                        action.Invoke(eventData);
                    }
                }
            }
        }
    }
}
}
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public delegate void EventDispatcherDelegate(object evtData);
public delegate void EventDispatcherDelegateNonParams();
public interface IEventDispatcher
{
    void addListener(int evtKey, EventDispatcherDelegate callback);
    void dropListener(int evtKey, EventDispatcherDelegate callback);
    void dispatch(int evtKey, object evt);

}
public static class EventDispatcherExtension
{
    public static void AddEvent(this MonoBehaviour mono, int evtKey, EventDispatcherDelegateNonParams callback)
    {
        if (EventDispatcher.Instance)
            EventDispatcher.Instance.addListener(evtKey, callback);
    }
    public static void DropEvent(this MonoBehaviour mono, int evtKey, EventDispatcherDelegateNonParams callback)
    {
        if (EventDispatcher.Instance)
            EventDispatcher.Instance.dropListener(evtKey, callback);
    }
    public static void DispatchEvent(this MonoBehaviour mono, int evtKey)
    {
        if (EventDispatcher.Instance)
            EventDispatcher.Instance.dispatch(evtKey);
    }
    public static void AddEvent(this MonoBehaviour mono, int evtKey, EventDispatcherDelegate callback)
    {
        if (EventDispatcher.Instance)
            EventDispatcher.Instance.addListener(evtKey, callback);
    }
    public static void DropEvent(this MonoBehaviour mono, int evtKey, EventDispatcherDelegate callback)
    {
        if (EventDispatcher.Instance)
            EventDispatcher.Instance.dropListener(evtKey, callback);
    }
    public static void DispatchEvent(this MonoBehaviour mono, int evtKey, object evt)
    {
        if (EventDispatcher.Instance)
            EventDispatcher.Instance.dispatch(evtKey, evt);
    }
}
public class EventDispatcher : Singleton<EventDispatcher>, IEventDispatcher
{
    Dictionary<int, List<EventDispatcherDelegate>> m_listeners = new Dictionary<int, List<EventDispatcherDelegate>>();
    Dictionary<int, List<EventDispatcherDelegateNonParams>> m_listenersNonParams = new Dictionary<int, List<EventDispatcherDelegateNonParams>>();

    public void addListener(int evtKey, EventDispatcherDelegateNonParams callback)
    {
        List<EventDispatcherDelegateNonParams> evtListeners;
        if (m_listenersNonParams.TryGetValue(evtKey, out evtListeners))
        {
            evtListeners.Remove(callback); //make sure we dont add duplicate
            evtListeners.Add(callback);
        }
        else
        {
            evtListeners = new List<EventDispatcherDelegateNonParams>
            {
                callback
            };

            m_listenersNonParams.Add(evtKey, evtListeners);
        }
    }
    public void dropListener(int evtKey, EventDispatcherDelegateNonParams callback)
    {
        List<EventDispatcherDelegateNonParams> evtListeners;
        if (m_listenersNonParams.TryGetValue(evtKey, out evtListeners))
        {
            for (int i = 0; i < evtListeners.Count; i++)
            {
                evtListeners.Remove(callback);
            }
        }
    }
    public void addListener(int evtKey, EventDispatcherDelegate callback)
    {
        List<EventDispatcherDelegate> evtListeners;
        if (m_listeners.TryGetValue(evtKey, out evtListeners))
        {
            evtListeners.Remove(callback); //make sure we dont add duplicate
            evtListeners.Add(callback);
        }
        else
        {
            evtListeners = new List<EventDispatcherDelegate>
            {
                callback
            };

            m_listeners.Add(evtKey, evtListeners);
        }
    }
    public void dropListener(int evtKey, EventDispatcherDelegate callback)
    {
        List<EventDispatcherDelegate> evtListeners;
        if (m_listeners.TryGetValue(evtKey, out evtListeners))
        {
            for (int i = 0; i < evtListeners.Count; i++)
            {
                evtListeners.Remove(callback);
            }
        }
    }
    public void dispatch(int evtKey, object evt)
    {
        //FIXME: might need to COPY the list<dispatchers> here so that an 
        //	event listener that results in adding/removing listeners does 
        //	not invalidate this for loop

        List<EventDispatcherDelegate> evtListeners;
        if (m_listeners.TryGetValue(evtKey, out evtListeners))
        {
            for (int i = 0; i < evtListeners.Count; i++)
            {
                evtListeners[i](evt);
            }
        }
    }
    public void dispatch(int evtKey)
    {
        //FIXME: might need to COPY the list<dispatchers> here so that an 
        //	event listener that results in adding/removing listeners does 
        //	not invalidate this for loop

        List<EventDispatcherDelegateNonParams> evtListeners;
        if (m_listenersNonParams.TryGetValue(evtKey, out evtListeners))
        {
            for (int i = 0; i < evtListeners.Count; i++)
            {
                evtListeners[i]();
            }
        }
    }
}
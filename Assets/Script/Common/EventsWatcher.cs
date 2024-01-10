using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/**
方法	                    功能	    备注
EventsWatcher.on	        注册监听	注册一个事件监听j
EventsWatcher.off	        移除监听	移除一个事件监听
EventsWatcher.Dispatch	    派发事件	广播事件
 */

public delegate void EventCallBack();
public delegate void EventCallBack<T>(T arg1);
public delegate void EventCallBack<T, W>(T arg1, W arg2);
public delegate void EventCallBack<T, W, E>(T arg1, W arg2, E arg3);
public delegate void EventCallBack<T, W, E, R>(T arg1, W arg2, E arg3, R arg4);
public delegate void EventCallBack<T, W, E, R, Y>(T arg1, W arg2, E arg3, R arg4, Y arg5);


public class EventsWatcher
{

    private static Dictionary<string, Delegate> m_EventTable = new Dictionary<string, Delegate>();

    static void TryAddListen(string eventType, Delegate callBack)
    {
        if (!m_EventTable.ContainsKey(eventType))
        {
            m_EventTable.Add(eventType, null);
        }
        Delegate d = m_EventTable[eventType];
        if (d != null && d.GetType() != callBack.GetType())
        {
            throw new Exception(string.Format("尝试为事件{0}添加不同类型的委托，当前事件所对应的委托是{1}，要添加的委托是{2}", eventType, d.GetType(), callBack.GetType()));
        }
    }

    static void TryRemoveListen(string eventType, Delegate callBack)
    {
        if (m_EventTable.ContainsKey(eventType))
        {
            Delegate d = m_EventTable[eventType];
            if (d == null)
            {
                throw new Exception(string.Format("移除监听错误：事件{0}没有对应的委托", eventType));
            }
            else if (d.GetType() != callBack.GetType())
            {
                throw new Exception(string.Format("移除监听错误：尝试为事件{0}移除不同的类型的委托，当前委托类型为{1}，要移除的对象为{2}", eventType, d, callBack));
            }
        }
        else
        {
            throw new Exception(string.Format("移除监听错误：没有事件码{0}", eventType));
        }
    }

    static void OnListenRemove(string eventType)
    {
        if (m_EventTable[eventType] == null)
        {
            m_EventTable.Remove(eventType);
        }
    }

    public static void On(string eventType, EventCallBack callback)
    {
        TryAddListen(eventType, callback);
        m_EventTable[eventType] = (EventCallBack)m_EventTable[eventType] + callback;
    }

    public static void On<T>(string eventType, EventCallBack<T> callback)
    {
        TryAddListen(eventType, callback);
        m_EventTable[eventType] = (EventCallBack<T>)m_EventTable[eventType] + callback;
    }

    public static void On<T, W>(string eventType, EventCallBack<T, W> callback)
    {
        TryAddListen(eventType, callback);
        m_EventTable[eventType] = (EventCallBack<T, W>)m_EventTable[eventType] + callback;
    }

    public static void On<T, W, E>(string eventType, EventCallBack<T, W, E> callback)
    {
        TryAddListen(eventType, callback);
        m_EventTable[eventType] = (EventCallBack<T, W, E>)m_EventTable[eventType] + callback;
    }


    public static void On<T, W, E, R>(string eventType, EventCallBack<T, W, E, R> callback)
    {
        TryAddListen(eventType, callback);
        m_EventTable[eventType] = (EventCallBack<T, W, E, R>)m_EventTable[eventType] + callback;
    }

    public static void On<T, W, E, R, Y>(string eventType, EventCallBack<T, W, E, R, Y> callback)
    {
        TryAddListen(eventType, callback);
        m_EventTable[eventType] = (EventCallBack<T, W, E, R, Y>)m_EventTable[eventType] + callback;
    }


    public static void Off(string eventType, EventCallBack callback = null)
    {
        TryRemoveListen(eventType, callback);
        m_EventTable[eventType] = (EventCallBack)m_EventTable[eventType] - callback;
        OnListenRemove(eventType);
    }
    public static void Off<T>(string eventType, EventCallBack<T> callBack)
    {
        TryRemoveListen(eventType, callBack);
        m_EventTable[eventType] = (EventCallBack<T>)m_EventTable[eventType] - callBack;
        OnListenRemove(eventType);

    }
    public static void Off<T, W>(string eventType, EventCallBack<T, W> callBack)
    {
        TryRemoveListen(eventType, callBack);
        m_EventTable[eventType] = (EventCallBack<T, W>)m_EventTable[eventType] - callBack;
        OnListenRemove(eventType);
    }
    public static void Off<T, W, E>(string eventType, EventCallBack<T, W, E> callBack)
    {
        TryRemoveListen(eventType, callBack);
        m_EventTable[eventType] = (EventCallBack<T, W, E>)m_EventTable[eventType] - callBack;
        OnListenRemove(eventType);
    }
    public static void Off<T, W, E, R>(string eventType, EventCallBack<T, W, E, R> callBack)
    {
        TryRemoveListen(eventType, callBack);
        m_EventTable[eventType] = (EventCallBack<T, W, E, R>)m_EventTable[eventType] - callBack;
        OnListenRemove(eventType);
    }
    public static void Off<T, W, E, R, Y>(string eventType, EventCallBack<T, W, E, R, Y> callBack)
    {
        TryRemoveListen(eventType, callBack);
        m_EventTable[eventType] = (EventCallBack<T, W, E, R, Y>)m_EventTable[eventType] - callBack;
        OnListenRemove(eventType);
    }


    public static void Dispatch(string eventType)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            EventCallBack callBack = d as EventCallBack;
            if (callBack != null)
            {
                callBack();
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
            }
        }
    }

    public static void Dispatch<T>(string eventType, T arg)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            EventCallBack<T> callBack = d as EventCallBack<T>;
            if (callBack != null)
            {
                callBack(arg);
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
            }
        }
    }

    public static void Dispatch<T, W>(string eventType, T arg1, W arg2)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            EventCallBack<T, W> callBack = d as EventCallBack<T, W>;
            if (callBack != null)
            {
                callBack(arg1, arg2);
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
            }
        }
    }

    public static void Dispatch<T, W, E>(string eventType, T arg1, W arg2, E arg3)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            EventCallBack<T, W, E> callBack = d as EventCallBack<T, W, E>;
            if (callBack != null)
            {
                callBack(arg1, arg2, arg3);
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
            }
        }
    }

    public static void Dispatch<T, W, E, R>(string eventType, T arg1, W arg2, E arg3, R arg4)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            EventCallBack<T, W, E, R> callBack = d as EventCallBack<T, W, E, R>;
            if (callBack != null)
            {
                callBack(arg1, arg2, arg3, arg4);
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
            }
        }
    }

    public static void Dispatch<T, W, E, R, Y>(string eventType, T arg1, W arg2, E arg3, R arg4, Y arg5)
    {
        Delegate d;
        if (m_EventTable.TryGetValue(eventType, out d))
        {
            EventCallBack<T, W, E, R, Y> callBack = d as EventCallBack<T, W, E, R, Y>;
            if (callBack != null)
            {
                callBack(arg1, arg2, arg3, arg4, arg5);
            }
            else
            {
                throw new Exception(string.Format("广播事件错误：事件{0}对应委托具有不同的类型", eventType));
            }
        }
    }



}
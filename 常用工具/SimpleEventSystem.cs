using System;
using System.Collections.Generic;
using UnityEngine;

public static class QuickEvent {
    public delegate void EventHandle<in T>(T message) where T : struct;

    private static readonly Dictionary<Type, Delegate> DelegateDict = new();

    public static void SubscribeListener<T>(EventHandle<T> handle) where T : struct {
        if (handle == null) {
            Debug.LogError("QuickEvent>Error>新增监听器为Null");
            return;
        }
        var type = typeof(T);
        if (DelegateDict.TryGetValue(type, out var delegates)) {
            DelegateDict[type] = Delegate.Combine(delegates, handle);
        }
        else {
            DelegateDict[type] = handle;
        }
    }

    public static void UnsubscribeListener<T>(EventHandle<T> handle) where T : struct {
        var type = typeof(T);
        if (!DelegateDict.TryGetValue(type, out var delegates)) {
            Debug.LogError($"QuickEvent>Error>{type}事件未订阅监听器");
            return;
        }

        DelegateDict[type] = Delegate.Remove(delegates, handle);
        if (DelegateDict[type] == null) {
            DelegateDict.Remove(type);
        }
    }

    public static void DispatchMessage<T>(T message) where T : struct {
        if (!DelegateDict.TryGetValue(typeof(T), out var delegates)) {
            return;
        }

        var handle = delegates as EventHandle<T>;
        handle?.Invoke(message);
    }
}

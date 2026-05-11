using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Interfaces;
namespace VGDSystem.DesignPattern
{
    public class ObserverVGD : IObserver
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<T>(Action<T> listener) where T : IEvent
        {
            var keyType = typeof(T);
            if (!_subscribers.ContainsKey(keyType))
            {
                _subscribers[keyType] = new List<Delegate>();
            }
            if (!_subscribers[keyType].Contains(listener))
            {
                _subscribers[keyType].Add(listener);
            }
        }

        public void UnSubscribe<T>(Action<T> listener) where T : IEvent
        {
            var keyType = typeof(T);
            if (_subscribers.ContainsKey(keyType))
            {
                _subscribers[keyType].Remove(listener);
            }
        }

        public void NotifyObserver<T>(T signal) where T : IEvent
        {
            var keyType = typeof(T);
            if (_subscribers.ContainsKey(keyType))
            {
                var listeners = _subscribers[keyType];
                for (int i = listeners.Count - 1; i >= 0; i--)
                {
                    ((Action<T>)listeners[i]).Invoke(signal);
                }
            }
        }
    }
    public static class GameServices
    {

        public static IObserver Observer { get; private set; }

        public static void Initialize()
        {
            if (Observer == null)
            {
                Observer = new ObserverVGD();
                Debug.Log("ObserverVGD đã được khởi tạo.");
            }
        }
    }
}



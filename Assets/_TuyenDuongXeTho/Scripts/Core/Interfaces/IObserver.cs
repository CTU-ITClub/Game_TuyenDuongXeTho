using System;
using UnityEngine;
namespace Core.Interfaces
{
    public interface IObserver
    {
        public void Subscribe<T>(Action<T> action) where T : IEvent;
        public void UnSubscribe<T>(Action<T> action) where T : IEvent;
        public void NotifyObserver<T>(T signal) where T : IEvent;
    }
}


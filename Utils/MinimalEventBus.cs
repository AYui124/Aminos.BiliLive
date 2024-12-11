using Aminos.BiliLive.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Utils
{
    public class MinimalEventBus
    {
        public static class EventName
        {
            public const string ChangeMenu = "ChangeMenu";
            public const string ReloadMenu = "ReloadMenu";
        }

        public static MinimalEventBus Global { get; } = new();

        private MinimalEventBus()
        {
            
        }

        private readonly Dictionary<string, List<Action<MinimalEventArg>>> _eventHandlers = new();

        public void Subscribe(string eventName, Action<MinimalEventArg> handler)
        {
            if (!_eventHandlers.TryGetValue(eventName, out _))
            {
                _eventHandlers[eventName] = new List<Action<MinimalEventArg>>();
            }
            _eventHandlers[eventName].Add(handler);
        }

        public void Unsubscribe(string eventName, Action<MinimalEventArg> handler)
        {
            if (_eventHandlers.TryGetValue(eventName, out _))
            {
                _eventHandlers[eventName].Remove(handler);
            }
        }

        public void Publish(string eventName, MinimalEventArg data)
        {
            if (_eventHandlers.TryGetValue(eventName, out _))
            {
                foreach (var handler in _eventHandlers[eventName])
                {
                    handler(data);
                }
            }
        }
    }
}

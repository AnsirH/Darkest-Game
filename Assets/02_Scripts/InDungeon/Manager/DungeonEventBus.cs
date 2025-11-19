using System.Collections;
using System.Collections.Generic;
using DarkestLike.Map;
using UnityEngine;
using UnityEngine.Events;

namespace DarkestLike.InDungeon.Manager
{
    public enum DungeonEventType
    {
        Loading, ExitRoom, ExitHallway, EnterRoom, EnterHallway, EnterTile, StartBattle
    }
    
    public class DungeonEventBus
    {
        private static readonly IDictionary<DungeonEventType, UnityEvent>
            Events = new Dictionary<DungeonEventType, UnityEvent>();

        public static void Subscribe(DungeonEventType type, UnityAction listener)
        {
            if (Events.TryGetValue(type, out UnityEvent thisEvent))
                thisEvent.AddListener(listener);
            else
            {
                thisEvent = new UnityEvent();
                thisEvent.AddListener(listener);
                Events.Add(type, thisEvent);
            }
        }

        public static void Unsubscribe(DungeonEventType type, UnityAction listener)
        {
            if (Events.TryGetValue(type, out UnityEvent thisEvent))
                thisEvent.RemoveListener(listener);
        }

        public static void Publish(DungeonEventType type)
        {
            if (Events.TryGetValue(type, out UnityEvent thisEvent))
                thisEvent.Invoke();
        }
    }
}
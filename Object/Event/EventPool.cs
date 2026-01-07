using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace CM.Core.EventPool
{
    public class EventPool<T> where T : BaseEventArgs
    {
        public interface IGameEventHandler { }
        public class GameEventHandler : IGameEventHandler, IRefPoolItem
        {
            public int idx { get; set; }

            public UnityAction<T> unityAction;
            //todo
            //LinkedListNode<UnityAction<T>> terminal;
            //public LinkedList<UnityAction<T>> linkedList_action = new();

            public static GameEventHandler Create(UnityAction<T> action)
            {
                GameEventHandler handler = ReferencePool.GetRef<GameEventHandler>();
                handler.unityAction += action;
                return handler;
            }

            public void Dispose()
            {
                unityAction = null;
            }
        }

        public class GameEvent : IRefPoolItem
        {
            public int idx { get; set; }

            private int sender;
            public int Sender { get { return sender; } }
            private T data;
            public T Data { get { return data; } }


            public static GameEvent Create(int sender, T data)
            {
                GameEvent e = ReferencePool.GetRef<GameEvent>();
                e.sender = sender;
                e.data = data;
                return e;
            }

            public void Dispose()
            {
                sender = -1;
                data = default(T);
            }
        }

        private readonly Dictionary<int, IGameEventHandler> dic_ID_gameEventHandler = new();
        private readonly ConcurrentQueue<GameEvent> que_event;

        private readonly Dictionary<int, LinkedListNode<GameEventHandler>> dic_ID_cachedNode;
        private readonly Dictionary<int, LinkedListNode<GameEventHandler>> dic_ID_tempNode;

        public EventPool()
        {
            que_event = new ConcurrentQueue<GameEvent>();

            ObjectPoolCtorData<GameEventHandler> data = new ObjectPoolCtorData<GameEventHandler>("GameEvent", 64, () => { return new GameEventHandler(); }, true, null, (e) => { e.Dispose(); });
            ReferencePool.GetPool<GameEventHandler>(data);
        }

        public void Update()
        {
            while(que_event.TryDequeue(out GameEvent e))
            {
                HandleEvent(e.Sender, e.Data);
                ReferencePool.Return<GameEvent>(e.idx);
            }
        }

        public void Subscribe(int id, UnityAction<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("[EventPool.Subscribe] - action is invalid");

            if (dic_ID_gameEventHandler.ContainsKey(id))
            {
                if (dic_ID_gameEventHandler[id] is GameEventHandler e)
                    e.unityAction += action;
            }
            else
                dic_ID_gameEventHandler.Add(id, GameEventHandler.Create(action));
        }

        public void Unsubscribe(int id, UnityAction<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("[EventPool.Subscribe] - action is invalid");

            if (dic_ID_gameEventHandler.ContainsKey(id))
            {
                if (dic_ID_gameEventHandler[id] is GameEventHandler e)
                    e.unityAction -= action;
            }
        }

        private void HandleEvent(int sender, T data)
        {
            if(dic_ID_gameEventHandler.TryGetValue(sender, out IGameEventHandler handler))
                (handler as GameEventHandler)?.unityAction.Invoke(data);

            //ReferencePool.Return<T>(data.idx);
        }

        public void Fire(int sender, T data)
        {
            GameEvent e = GameEvent.Create(sender, data);
            que_event.Enqueue(e);
        }

        public void ShutDown()
        {
            foreach (var e in dic_ID_gameEventHandler.Values)
                ReferencePool.Return<GameEventHandler>(((GameEventHandler)e).idx);


        }
    }
}

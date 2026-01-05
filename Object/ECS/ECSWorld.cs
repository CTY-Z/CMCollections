using CMFramework.Core;
using CMFramework.Core.Collections;
using System;
using System.Collections.Generic;
using static CMFramework.Core.ECS.ECSWorld;

namespace CMFramework.Core.ECS
{
    public delegate void StartUpSystem(Commands command);
    public delegate void UpdateSystem(Commands command, Queryer queryer, SingletonHandler handler);

    public class ECSWorld
    {
        private Dictionary<int, ComponentInfo> dic_componentID_componentInfo = new();
        private Dictionary<int, Dictionary<int, BaseComponent>> dic_entity_componentContainer = new();

        public class ComponentInfo
        {
            public SparseSet set = new SparseSet(32);

            public void AddEntity(int e) => set.Add(e);
            public void RemoveEntity(int e) => set.Remove(e);
        }


        private Dictionary<int, SingletonComponentInfo> dic_componentID_singletonComponentInfo = new();

        public class SingletonComponentInfo
        {
            public BaseSingletonComponent component;
            public Action<BaseSingletonComponent> Dispose;

            public SingletonComponentInfo(Action<BaseSingletonComponent> Dispose)
            {
                this.Dispose = Dispose;
            }
        }

        public class SingletonHandler
        {
            private ECSWorld m_world;

            public SingletonHandler(ECSWorld world)
            {
                m_world = world;
            }

            public bool Has<SingletonComp>() 
            {
                int idx = IdxGetter<SingletonID>.Get<SingletonComp>();
                return m_world.dic_componentID_singletonComponentInfo.TryGetValue(idx, out SingletonComponentInfo info); 
            }

            public SingletonComp Get<SingletonComp>() where SingletonComp : BaseSingletonComponent
            {
                int idx = IdxGetter<SingletonID>.Get<SingletonComp>();
                SingletonComponentInfo info = null;
                if (m_world.dic_componentID_singletonComponentInfo.TryGetValue(idx, out info))
                    return (SingletonComp)info.component;
                else
                    return null;
            }
        }


        public class Commands
        {
            private ECSWorld m_world;

            public Commands(ECSWorld world)
            {
                m_world = world; 
            }
             
            public Commands Spawn<Component>(Action<Component> ctor = null) where Component : BaseComponent, new()
            {
                SpawnAndReturn(ctor);
                return this;
            }
            public Commands Spawn<Component>(int entity, Action<Component> ctor = null) 
                where Component : BaseComponent, new()
            {
                DoSpawn(entity, ctor);
                return this;
            }
            public int SpawnAndReturn<Component>(Action<Component> ctor = null) where Component : BaseComponent, new()
            {
                int entity = IDGenerator.Get();
                DoSpawn(entity, ctor);
                return entity;
            }
            private void DoSpawn<Component>(int entity, Action<Component> ctor = null) where Component : BaseComponent, new()
            {
                int idx = IdxGetter<ComponentID>.Get<Component>();
                ComponentInfo info = null;
                if (!m_world.dic_componentID_componentInfo.TryGetValue(idx, out info))
                {
                    info = new ComponentInfo();
                    m_world.dic_componentID_componentInfo[idx] = info;
                    ObjectPoolCtorData<Component> ctorData = new ObjectPoolCtorData<Component>("ECS." + typeof(Component).Name, 32, () => { return new Component(); }, true);
                    ReferencePool.GetPool(ctorData);
                }

                info.AddEntity(entity);

                Dictionary<int, BaseComponent> dic_idx_component = null;
                if (!m_world.dic_entity_componentContainer.TryGetValue(entity, out dic_idx_component))
                {
                    dic_idx_component = new();
                    m_world.dic_entity_componentContainer.Add(entity, dic_idx_component);
                }

                Component item = ReferencePool.GetRef<Component>();
                dic_idx_component[idx] = item;
                if (ctor != null)
                    ctor(item);
            }

            public Commands Remove(int entity)
            {
                Dictionary<int, BaseComponent> dic_idx_component;
                if(m_world.dic_entity_componentContainer.TryGetValue(entity, out dic_idx_component))
                {
                    foreach (var item in dic_idx_component)
                    {
                        m_world.dic_componentID_componentInfo[item.Key].RemoveEntity(entity);
                        ReferencePool.Return<BaseComponent>(item.Value.idx);
                    }
                    m_world.dic_entity_componentContainer.Remove(entity);
                }

                return this;
            }



            public Commands SetSingleton<SingletonComp>(Action<SingletonComp> ctor = null) where SingletonComp : BaseSingletonComponent, new()
            {
                int idx = IdxGetter<SingletonID>.Get<SingletonComp>();

                SingletonComponentInfo info = null;
                if (!m_world.dic_componentID_singletonComponentInfo.TryGetValue(idx, out info))
                {
                    info = new SingletonComponentInfo((comp) => { comp = null; });
                    SingletonComp comp = new SingletonComp();
                    info.component = comp;
                    if (ctor != null)
                        ctor(comp);
                    m_world.dic_componentID_singletonComponentInfo[idx] = info;
                }

                return this;
            }

            public Commands RemoveSingleton<SingletonComp>()
            {
                int idx = IdxGetter<SingletonID>.Get<SingletonComp>();

                SingletonComponentInfo info = null;
                if (m_world.dic_componentID_singletonComponentInfo.TryGetValue(idx, out info))
                {
                    info.Dispose(info.component);
                    m_world.dic_componentID_singletonComponentInfo.Remove(idx);
                }

                return this;
            }
        }

        public class Queryer
        {
            private ECSWorld m_world;

            public Queryer(ECSWorld world)
            {
                m_world = world;
            }

            public bool Has<Component>(int entity)
            {
                int idx = IdxGetter<ComponentID>.Get<Component>();
                bool hasEntity = m_world.dic_entity_componentContainer.TryGetValue(entity, out Dictionary<int, BaseComponent> dic_idx_component);

                return hasEntity && dic_idx_component.TryGetValue(idx, out BaseComponent comp);
            }

            public Component Get<Component>(int entity) where Component : BaseComponent
            {
                int idx = IdxGetter<ComponentID>.Get<Component>();
                bool hasEntity = m_world.dic_entity_componentContainer.TryGetValue(entity, out Dictionary<int, BaseComponent> dic_idx_component);

                if (hasEntity)
                {
                    if (dic_idx_component.TryGetValue(idx, out BaseComponent comp))
                        return (Component)comp;
                }

                return null;
            }

            public List<int> Query<Component>([MustSubclassOf(typeof(BaseComponent))] params Type[] remains)
            {
                List<int> list_entity = new();
                DoQuery<Component>(ref list_entity, remains);
                return list_entity;
            }

            private void DoQuery<Component>(ref List<int> list_entity, [MustSubclassOf(typeof(BaseComponent))] params Type[] remains)
            {
                int idx = IdxGetter<ComponentID>.Get<Component>();
                if (m_world.dic_componentID_componentInfo.TryGetValue(idx, out ComponentInfo info))
                {
                    foreach (int e in info.set)
                    {
                        if (remains.Length > 0)
                            DoQueryRemains(e, ref list_entity, remains);
                        else
                            list_entity.Add(e);
                    }
                }
            }

            private bool DoQueryRemains(int entity, ref List<int> list_entity, [MustSubclassOf(typeof(BaseComponent))] params Type[] remains)
            {
                if (remains.Length < 1) return false;
                if (!remains[remains.Length - 1].IsSubclassOf(typeof(BaseComponent))) return false;

                bool flag = false;
                Dictionary<int, BaseComponent> dic_idx_component = null;
                for (int i = 0; i < remains.Length; i++)
                {
                    int idx = IdxGetter<ComponentID>.Get(remains[i]);
                    if (m_world.dic_entity_componentContainer.TryGetValue(entity, out dic_idx_component))
                    {
                        if (dic_idx_component.ContainsKey(idx))
                        {
                            flag = true;
                            list_entity.Add(entity);
                        }
                    }
                }
                return flag;
            }
        }


        Commands command;
        Queryer queryer;
        SingletonHandler singletonHandler;

        StartUpSystem startUpSystem;
        UpdateSystem updateSystem;

        public ECSWorld()
        {
            command = new Commands(this);
            queryer = new Queryer(this);
            singletonHandler = new SingletonHandler(this);
        }

        public ECSWorld AddStartUpSystem(StartUpSystem addStartUpSystem)
        {
            if (addStartUpSystem == null)
                startUpSystem = new StartUpSystem(addStartUpSystem);
            else
                startUpSystem += addStartUpSystem;

            return this;
        }

        public ECSWorld AddSystem(UpdateSystem addUpdateSystem)
        {
            if (addUpdateSystem == null)
                updateSystem = new UpdateSystem(addUpdateSystem);
            else
                updateSystem += addUpdateSystem;

            return this;
        }

        public ECSWorld AddSingleton<T>(Action<T> ctor = null) where T : BaseSingletonComponent, new()
        {
            command.SetSingleton(ctor);
            return this;
        }


        public void Start()
        {
            startUpSystem?.Invoke(command);
        }


        public void Update()
        {
            updateSystem?.Invoke(command, queryer, singletonHandler);
        }

        public void ShutDown()
        {
            startUpSystem = null;
            updateSystem = null;

            dic_componentID_componentInfo.Clear();
            dic_componentID_singletonComponentInfo.Clear();
            dic_entity_componentContainer.Clear();
        }

    } 
}


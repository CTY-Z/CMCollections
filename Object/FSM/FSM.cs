using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace CM.Core
{
    public class FSM<T> : FSMBase where T : class
    {
        private FSMState<T> currentState;
        private bool isDestroy;

        private static FSM<T> m_fsm;

        private Dictionary<Type, FSMState<T>> dic_type_state;

        public FSMState<T> CurrentState { get { return currentState; } }
        public bool IsRunning { get { return currentState != null; } }
        public bool IsDestroy { get { return isDestroy; } }

        public FSM()
        {
            dic_type_state = new();

            Reset();
        }

        public static FSM<T> Create(string name, List<FSMState<T>> list_state)
        {
            if (list_state == null || list_state.Count < 1) 
                throw new InvalidExpressionException($"[FSM:{name}] - FSM states is invalid");

            m_fsm = ReferencePool.GetRefByCreatePool<FSM<T>>(new ObjectPoolCtorData<FSM<T>>("test", 10,
                () => { return new FSM<T>(); },
                false,
                (FSM<T> fsm) => { Debug.Log($"获取了{name}fsm"); },
                (FSM<T> fsm) => { Debug.Log($"回收了{name}fsm"); }, true));
            FSM<T> fsm = m_fsm;
            fsm.fsmName = name;
            fsm.isDestroy = false;

            foreach (var state in list_state)
            {
                if(state == null)
                    throw new InvalidExpressionException($"[FSM:{name}] - state is invalid");

                Type stateType = state.GetType();
                if (fsm.dic_type_state.ContainsKey(stateType))
                    throw new InvalidExpressionException($"[FSM:{name}] - state:{stateType.Name} is is already exist");

                fsm.dic_type_state[stateType] = state;
                state.Init(fsm);
            }

            return fsm;
        }

        public void Start<TState>() where TState : FSMState<T>
        {
            if (IsDestroy)
                throw new InvalidExpressionException($"[FSM:{fsmName}] - fsm is already destroy");

            if (IsRunning)
                throw new InvalidExpressionException($"[FSM:{fsmName}] - fsm is running, can not start again");

            FSMState<T> state = GetState<TState>();
            if (state == null)
                throw new InvalidExpressionException($"[FSM:{fsmName}] can not start state {typeof(TState).Name} which is not exist");
            
            currentState = state;
            state.OnEnter(this);
        }

        public TState GetState<TState>() where TState : FSMState<T>
        {
            if (IsDestroy)
                throw new InvalidExpressionException($"[FSM:{fsmName}] - fsm is already destroy");

            FSMState<T> state = null;
            if (dic_type_state.TryGetValue(typeof(TState), out state))
                return (TState)state;

            return null;
        }

        public void ChangeState<TState>() where TState : FSMState<T>
        {
            if (IsDestroy)
                throw new InvalidExpressionException($"[FSM:{fsmName}] - fsm is already destroy");

            if (currentState == null)
                throw new InvalidExpressionException($"[FSM:{FSMName}] - currentState is null");

            FSMState<T> state = GetState<TState>();
            if (state == null)
                throw new InvalidExpressionException($"[FSM:{FSMName}] FSM can not change state to '{typeof(TState).Name}' which is not exist");

            currentState.OnExit();
            currentState = state;
            currentState.OnEnter(this);
        }

        public void OnUpdate()
        {
            currentState.OnUpdate();
        }

        public virtual void Reset()
        {
            currentState = null;
            isDestroy = true;
            dic_type_state.Clear();
        }

        public virtual void ShutDown()
        {
            currentState.OnExit();
            Reset();
            ReferencePool.Return<FSM<T>>(m_fsm.idx);
        }
    }
}



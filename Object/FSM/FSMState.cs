using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CM.Core
{
    public class FSMState<T> where T : class
    {
        protected internal virtual void Init(FSMBase fsm) { }

        protected internal virtual void OnEnter(FSMBase fsm) { }

        protected internal virtual void OnUpdate() { }

        protected internal virtual void OnExit() { }
    }
}

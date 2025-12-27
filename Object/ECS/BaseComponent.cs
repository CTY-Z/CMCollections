using CMFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMFramework.ECS
{
    public class BaseComponent : IPoolItem
    {
        public int idx { get; set; }
    }

    public class BaseSingletonComponent
    {

    }
}


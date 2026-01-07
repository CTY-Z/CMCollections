using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CM.Core.ECS
{
    public class BaseComponent : IRefPoolItem
    {
        public int idx { get; set; }
    }

    public class BaseSingletonComponent
    {

    }
}


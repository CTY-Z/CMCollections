using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CM.Core
{
    public static class SceneUtil
    {
        private static GameObject DontDestroyOnLoad;

        public static void AddToDontDestroyOnLoad(GameObject obj)
        {
            if (DontDestroyOnLoad == null)
            {
                DontDestroyOnLoad = new GameObject("DontDestroyOnLoad");
                GameObject.DontDestroyOnLoad(DontDestroyOnLoad);
            }
            obj.transform.parent = DontDestroyOnLoad.transform;
        }
    }
}

using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectPool {
    public class GameObjectPool : PoolBase<GameObject>{
        public GameObjectPool(GameObject gameObject, int preloadCount) 
            : base (() => Preload(gameObject), GetAction, ReturnAction, preloadCount)
            { }

        public static GameObject Preload(GameObject gameObject) => Object.Instantiate(gameObject);
        public static void GetAction(GameObject gameObject) => gameObject.SetActive(true);
        public static void ReturnAction(GameObject gameObject) => gameObject.SetActive(false);

    }
}

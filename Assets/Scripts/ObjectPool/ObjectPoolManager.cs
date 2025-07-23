using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ObjectPool {

    public enum PoolType {
        Projectiles,
        Enemies,
        None
    }

    public class ObjectPoolManager : MonoBehaviour {

        public static List<PooledObjectInfo> m_objectPools = new List<PooledObjectInfo>();

        private static GameObject m_projectilesPoolEmpty;
        private static GameObject m_enemiesPoolEmpty;

        private GameObject m_objectPoolEmptyHolder;

        public static PoolType poolType;

        private void Awake() {
            SetupEmpties();
        }

        private void SetupEmpties() {
            m_objectPoolEmptyHolder = new GameObject("Pooled Objects");

            m_enemiesPoolEmpty = new GameObject("Pooled Enemies");
            m_enemiesPoolEmpty.transform.SetParent(m_objectPoolEmptyHolder.transform);
            m_projectilesPoolEmpty = new GameObject("Pooled Projectiles");
            m_projectilesPoolEmpty.transform.SetParent(m_objectPoolEmptyHolder.transform);

        }

        public static GameObject SpawnObject(GameObject objectToSpawn, PoolType poolType = PoolType.None) {
            PooledObjectInfo pool = m_objectPools.Find(p => p.m_lookupString == objectToSpawn.name);

            if (pool == null) {
                pool = new PooledObjectInfo() { m_lookupString = objectToSpawn.name};
                m_objectPools.Add(pool);
            }

            GameObject spawnableObj = pool.m_inactiveObjects.FirstOrDefault();

            if (spawnableObj == null) {
                GameObject parentObject = SetParentObject(poolType);
                spawnableObj = Instantiate(objectToSpawn, parentObject.transform);
            } else {
                pool.m_inactiveObjects.Remove(spawnableObj);
                spawnableObj.SetActive(true);
            }

            return spawnableObj;
        }

        public static void ReturnObjectToPool(GameObject obj) {

            string goName = obj.name.Substring(0, obj.name.Length - 7); // remove (Clone) postfix in go name
            PooledObjectInfo pool = m_objectPools.Find(p => p.m_lookupString == goName);

            if (pool == null) {
                Debug.LogWarning("ReturnObjectToPool: object was not pooled " + obj.name);

            } else {
                obj.SetActive(false);
                pool.m_inactiveObjects.Add(obj);
            }
        }

        private static GameObject SetParentObject(PoolType poolType) {
            switch (poolType) {
                case PoolType.Projectiles:
                    return m_projectilesPoolEmpty;
                case PoolType.Enemies:
                    return m_enemiesPoolEmpty;
                case PoolType.None:
                    return null;
                default:
                    return null;
            }
        }
    }

    public class PooledObjectInfo {
        public string m_lookupString;
        public List<GameObject> m_inactiveObjects = new List<GameObject>();
    }
}

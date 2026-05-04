using System.Collections.Generic;
using UnityEngine;

namespace LawnDefense.Core
{
    public sealed class PoolManager : MonoBehaviour
    {
        private readonly Dictionary<GameObject, Queue<GameObject>> pools =
            new Dictionary<GameObject, Queue<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> prefabByInstance = new Dictionary<GameObject, GameObject>();
        private readonly HashSet<GameObject> pooledInstances = new HashSet<GameObject>();

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                return null;
            }

            Queue<GameObject> pool;
            if (!pools.TryGetValue(prefab, out pool))
            {
                pool = new Queue<GameObject>();
                pools.Add(prefab, pool);
            }

            GameObject instance;
            if (pool.Count > 0)
            {
                instance = pool.Dequeue();
                pooledInstances.Remove(instance);
            }
            else
            {
                instance = Instantiate(prefab);
            }

            prefabByInstance[instance] = prefab;

            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.SetActive(true);
            NotifyPoolables(instance, true);

            return instance;
        }

        public void Despawn(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (pooledInstances.Contains(instance))
            {
                return;
            }

            NotifyPoolables(instance, false);
            instance.SetActive(false);

            GameObject prefab;
            if (prefabByInstance.TryGetValue(instance, out prefab))
            {
                Queue<GameObject> pool;
                if (!pools.TryGetValue(prefab, out pool))
                {
                    pool = new Queue<GameObject>();
                    pools.Add(prefab, pool);
                }

                pooledInstances.Add(instance);
                pool.Enqueue(instance);
                return;
            }

            Destroy(instance);
        }

        private static void NotifyPoolables(GameObject instance, bool spawned)
        {
            MonoBehaviour[] components = instance.GetComponentsInChildren<MonoBehaviour>(true);

            for (int i = 0; i < components.Length; i++)
            {
                IPoolable poolable = components[i] as IPoolable;
                if (poolable != null)
                {
                    if (spawned)
                    {
                        poolable.OnSpawned();
                    }
                    else
                    {
                        poolable.OnDespawned();
                    }
                }
            }
        }
    }
}

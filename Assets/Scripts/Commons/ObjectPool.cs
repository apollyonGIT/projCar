using Foundations;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Commons
{
    public class ObjectPoolFactory : Singleton<ObjectPoolFactory>
    {
        private Dictionary<string, ObjectPool> pools = new();

        public bool AddObjectPool(string name, ObjectPool pool)
        {
            if (pools.ContainsKey(name))
                return false;

            pools.Add(name, pool);
            return true;
        }

        public bool RemoveObjectPool(string name)
        {
            if (pools.ContainsKey(name))
            {
                pools.Remove(name);
                return true;
            }
            return false;
        }

        public void ClearAllObjectPool()
        {
            pools.Clear();
        }

        public ObjectPool<T> GetObjectPool<T>(string name) where T : MonoBehaviour
        {
            pools.TryGetValue(name, out var pool);

            return pool as ObjectPool<T>;
        }

        public MixedObjectPool<T> GetMixedObjectPool<T>(string name) where T : MonoBehaviour
        {
            pools.TryGetValue(name, out var pool);

            return pool as MixedObjectPool<T>;
        }

        #region Name&&Count Config
        public const string ProjectilePool_Name = "ProjectilePool";

        public const int ProjectilePool_Count = 10;
        #endregion
    }

    #region ObjectPool
    public class ObjectPool
    {
    }

    public class ObjectPool<T> : ObjectPool where T : MonoBehaviour
    {
        public int max_size { get; set; }
        public int count
        {
            get { return objectsPool.Count + objectsPoped.Count; }
        }

        private List<T> objectsPool = new();            //实例化但没active的物体
        private List<T> objectsPoped = new();           //实例化且active的物体

        private T prefab;
        private Transform root;

        public ObjectPool(T prefab, int max_size, Transform root)
        {
            this.prefab = prefab;
            this.max_size = max_size;
            this.root = root;
        }


        /// <summary>
        /// 先Get外观,然后在把会返回的T用逻辑init
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            T obj = null;
            if (objectsPool.Count == 0)
            {
                obj = GameObject.Instantiate(prefab, root, false);
                obj.gameObject.SetActive(true);
                objectsPoped.Add(obj);
            }
            else
            {
                int lastIndex = objectsPool.Count - 1;
                obj = objectsPool[lastIndex];
                obj.gameObject.SetActive(true);

                objectsPool.RemoveAt(lastIndex);
                objectsPoped.Add(obj);
            }
            return obj;
        }

        /// <summary>
        /// 在外面清除逻辑后,使用该函数回收外观
        /// </summary>
        /// <param name="obj"></param>
        public void Recycle(T obj)
        {
            if (count > max_size)
            {
                objectsPoped.Remove(obj);

                GameObject.Destroy(obj.gameObject);
            }
            else
            {
                objectsPoped.Remove(obj);
                objectsPool.Add(obj);

                obj.gameObject.SetActive(false);
            }
        }
    }


    public class MixedObjectPool<T> : ObjectPool where T : MonoBehaviour
    {
        public int max_size { get; set; }

        public int count
        {
            get
            {
                int num = 0;
                foreach (var (_, pool) in objectsPool) { num += pool.Count; }
                foreach (var (_, poped) in objectsPoped) { num += poped.Count; }

                return num;
            }
        }

        private Dictionary<string, List<T>> objectsPool = new();
        public Dictionary<string, List<T>> objectsPoped = new();      //处于激活态的Objects
        private Dictionary<string, T> prefabs = new();
        private Transform root;

        public MixedObjectPool(int max_size, Transform root)
        {
            this.max_size = max_size;
            this.root = root;
        }

        public T Get(string key, T prefab)
        {
            T obj = null;

            if (prefabs.ContainsKey(key))                      //是已有的预制体
            {
                if (objectsPool.TryGetValue(key, out var pool) && objectsPoped.TryGetValue(key, out var poped))
                {
                    if (pool.Count == 0)
                    {
                        obj = GameObject.Instantiate(prefab, root, false);
                        obj.gameObject.SetActive(true);
                        poped.Add(obj);
                    }
                    else
                    {
                        int last = pool.Count - 1;
                        obj = pool[last];
                        obj.gameObject.SetActive(true);

                        pool.RemoveAt(last);
                        poped.Add(obj);
                    }
                }
                else
                {
                    Debug.LogError("一个记录在案的prefab,却没有对应的池,这是不可能事件");
                }
            }
            else
            {
                prefabs.Add(key, prefab);
                objectsPool.Add(key, new List<T>());
                objectsPoped.Add(key, new List<T>());
                return Get(key, prefab);
            }

            return obj;
        }

        public void Recycle(string key, T obj)
        {
            if (objectsPool.TryGetValue(key, out var pool) && objectsPoped.TryGetValue(key, out var poped))
            {
                if (count > max_size)
                {
                    poped.Remove(obj);
                    GameObject.Destroy(obj.gameObject);
                }
                else
                {
                    poped.Remove(obj);
                    pool.Add(obj);

                    obj.gameObject.SetActive(false);
                }
            }
        }

    }
    #endregion
}

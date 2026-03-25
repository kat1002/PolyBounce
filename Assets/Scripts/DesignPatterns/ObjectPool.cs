using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Queue<T> pool = new Queue<T>();
    private readonly Transform parent;
    private readonly bool autoExpand;

    public ObjectPool(T prefab, int initialSize, Transform parent = null, bool autoExpand = true)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.autoExpand = autoExpand;

        for (int i = 0; i < initialSize; i++)
        {
            CreateNew();
        }
    }

    private T CreateNew()
    {
        T instance = Object.Instantiate(prefab, parent);
        instance.gameObject.SetActive(false);
        pool.Enqueue(instance);
        return instance;
    }

    public T Get()
    {
        if (pool.Count == 0)
        {
            if (!autoExpand)
            {
                Debug.LogWarning($"Pool of {typeof(T)} is empty.");
                return null;
            }

            CreateNew();
        }

        T item = pool.Dequeue();
        item.gameObject.SetActive(true);

        // Optional reset hook
        if (item is IPoolable poolable)
        {
            poolable.OnSpawn();
        }

        return item;
    }

    public void Release(T item)
    {
        if (!item.gameObject.activeSelf) return;

        if (item is IPoolable poolable)
        {
            poolable.OnDespawn();
        }

        item.gameObject.SetActive(false);
        item.transform.SetParent(parent);
        pool.Enqueue(item);
    }

    public void Clear()
    {
        while (pool.Count > 0)
        {
            var item = pool.Dequeue();
            if (item != null)
                Object.Destroy(item.gameObject);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    // Queue that holds the pooled objects
    private Queue<T> pool;

    // The template for creating new objects in the pool
    private T prefab;

    public ObjectPool(T prefab, int initialSize)
    {
        this.prefab = prefab;

        // Initialize the queue
        this.pool = new Queue<T>();

        // Instantiate the objects and deactivate them, and then add them into the pool
        for (int i = 0; i < initialSize; i++)
        {
            T instance = GameObject.Instantiate(prefab);
            instance.gameObject.SetActive(false);
            pool.Enqueue(instance);
        }
    }

    // Fetches an object from the pool. If the pool is empty, it creates a new object, adds it to the pool, and then returns it.
    public T GetObject()
    {
        if (pool.Count == 0)
        {
            T instance = GameObject.Instantiate(prefab);
            pool.Enqueue(instance);
        }

        T objectToReuse = pool.Dequeue();
        objectToReuse.gameObject.SetActive(true);
        return objectToReuse;
    }

    // Returns an object back into the pool. The object is deactivated before being added back.
    public void ReturnObject(T objectToReturn)
    {
        objectToReturn.gameObject.SetActive(false);
        pool.Enqueue(objectToReturn);
    }
}

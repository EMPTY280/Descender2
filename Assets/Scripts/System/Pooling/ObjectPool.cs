using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectPool<T> : MonoBehaviour
{
    [SerializeField] private GameObject prefab = null;
    [SerializeField] private int initialCount = 10;
    private List<T> comps = new List<T>();
    private int pointer = 0;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        Add(initialCount);
    }

    public void Add(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Transform newInst = Instantiate(prefab, this.transform).transform;
            newInst.SetAsFirstSibling();
            T comp = newInst.GetComponent<T>();
            if (comp == null)
            {
                this.enabled = false;
                return;
            }
            comps.Add(comp);
        }
    }

    public T Get()
    {
        if (pointer > comps.Count - 1)
            Add(1);
        if (!IsAvailable(comps[pointer]))
            Add(comps.Count);
        int result = pointer;
        pointer = (pointer + 1) % comps.Count;
        return comps[result];
    }

    public abstract bool IsAvailable(T comp);
}

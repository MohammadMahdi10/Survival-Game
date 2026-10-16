using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherableResource : MonoBehaviour {
    public ItemType resourceType;
    public int amount = 1;
    private ResourceManager resourceManager;

    private void Start()
    {
        resourceManager = FindFirstObjectByType<ResourceManager>();
    }

    public void Gather()
    {
        resourceManager.AddResource(resourceType, amount);
        Destroy(gameObject);
    }
}


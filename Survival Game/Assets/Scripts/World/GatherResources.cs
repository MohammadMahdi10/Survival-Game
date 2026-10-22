using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatherableResource : MonoBehaviour
{
    public ItemType resourceType;
    public int amount = 1;

    public void Gather()
    {
        Debug.Log("Gathered " + amount + " " + resourceType);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGathering : MonoBehaviour {
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float gatherDistance = 4f;

    private void Update() {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryGather();
        }
    }

    private void TryGather() {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, gatherDistance)) {
            GatherableResource resource = hit.collider.GetComponentInParent<GatherableResource>();

            if (resource != null) {
                resource.Gather();
            }
        }
    }
}


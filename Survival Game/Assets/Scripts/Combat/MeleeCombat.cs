using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeCombat : MonoBehaviour
{
    public Camera playerCamera;
    public float attackRange = 2f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.Locked)
        {
            DetectHit();
        }
    }

    void DetectHit()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackRange))
        {
            Debug.Log("Hit: " + hit.collider.name);

            EnemyHealth enemy = hit.collider
                .GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(10f);
            }
        }
        else
        {
            Debug.Log("Missed!");
        }
    }

}

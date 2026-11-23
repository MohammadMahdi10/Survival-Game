using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class RangedCombat : MonoBehaviour
{
    public Transform Camera;
    public GameObject projectile;
    public float speed = 4;
    public float attackCooldownTime = 1f;
    private bool attackReady = true;

    void Update()
    {
        if (InputManager.instance.rangedAction.WasPressedThisFrame() && attackReady)
        {
            shoot();
        }
    }
    void shoot()
    {
        StartCoroutine(attackCooldown(attackCooldownTime));
        GameObject p = Instantiate(projectile, Camera.position, Camera.rotation);
        Rigidbody rb = p.GetComponent<Rigidbody>();
        rb.velocity = Camera.forward * speed;
    }

    private IEnumerator attackCooldown(float cooldown)
    {
        attackReady = false;
        yield return new WaitForSeconds(cooldown);
        attackReady = true;
    }
}

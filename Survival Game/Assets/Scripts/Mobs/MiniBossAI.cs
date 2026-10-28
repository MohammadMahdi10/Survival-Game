using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.AI;

/// Controls the mini boss's movement, targeting and melee attacks.
/// Uses direct movement instead of NavMesh to chase the player,
/// and scales attack damage with the current world difficulty.
[RequireComponent(typeof(CharacterController))]
public class MiniBossAI : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform playerTarget;

    [Header("Detection Settings")]
    public float detectionRange = 15f;

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float gravity = -20f;
    public float rotationSpeed = 5f;

    [Header("Combat Settings")]
    public float attackRange = 3.2f;
    public float attackCooldown = 1f;
    public float attackDamage = 15f;
    private float nextAttackTime = 0f;

    [Header("Weapon Settings")]
    public Transform swordTransform;
    private bool isAttacking = false;

    [Header("World Difficulty")]
    public WorldDifficultyManager worldDifficultyManager;

    private CharacterController controller;
    private float verticalVelocity;

    /// Initialises the CharacterController and finds the player if no
    /// target has been assigned, allowing the boss to track the player.
    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerTarget = player.transform;
            }
        }
    }

    /// Checks the distance to the player each frame.
    /// Stops chasing outside detection range, approaches the player
    /// within detection range, and attacks when close enough.
    void Update()
    {
        if (playerTarget == null)
        {
            ApplyMovement(Vector3.zero);
            return;
        }

        Vector3 direction = playerTarget.position - transform.position;
        direction.y = 0f;

        float distanceToPlayer = direction.magnitude;

        if (distanceToPlayer > detectionRange)
        {
            ApplyMovement(Vector3.zero);
            return;
        }

        if (distanceToPlayer > attackRange)
        {
            LookAtTarget();

            // Only move as far as the attack range so the boss
            // does not continuously push into the player.
            float movementDistance = Mathf.Min(
                moveSpeed * Time.deltaTime,
                distanceToPlayer - attackRange
            );

            Vector3 movement = direction.normalized * movementDistance;

            ApplyMovement(movement);
        }
        else
        {
            ApplyMovement(Vector3.zero);

            LookAtTarget();

            if (Time.time >= nextAttackTime && !isAttacking)
            {
                StartCoroutine(SwingSwordRoutine());
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    /// Applies horizontal movement and gravity together.
    /// CharacterController handles collisions with terrain and obstacles
    /// without requiring a NavMeshAgent.
    void ApplyMovement(Vector3 horizontalMovement)
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = horizontalMovement;
        movement.y = verticalVelocity * Time.deltaTime;

        CollisionFlags collisions = controller.Move(movement);

        if ((collisions & CollisionFlags.Below) != 0 &&
            verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
    }

    /// Smoothly rotates the mini-boss to face the player.
    /// Ignores vertical direction so the boss stays upright
    /// rather than tilting towards the player's height.
    void LookAtTarget()
    {
        Vector3 direction = playerTarget.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    /// Animates the sword swing and checks whether the player is in range.
    /// Uses a coroutine to spread the attack animation over multiple frames
    /// instead of instantly changing the sword's rotation.
    IEnumerator SwingSwordRoutine()
    {
        isAttacking = true;

        if (swordTransform != null)
        {
            Quaternion originalRotation = swordTransform.localRotation;
            Quaternion attackRotation = originalRotation * Quaternion.Euler(-90f, 0f, 0f);

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                swordTransform.localRotation = Quaternion.Slerp(
                    originalRotation,
                    attackRotation,
                    elapsed / duration
                );

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Check range when the swing reaches its target so the player
            // can avoid damage by moving away before the hit connects.
            if (playerTarget != null &&
                Vector3.Distance(transform.position, playerTarget.position) <= attackRange + 0.5f)
            {
                PlayerStats stats = playerTarget.GetComponent<PlayerStats>();

                if (stats != null)
                {
                    float difficulty = worldDifficultyManager != null
                        ? worldDifficultyManager.worldDifficulty
                        : 1f;

                    // Scale boss damage with world difficulty to increase
                    // the challenge as the player progresses.
                    float finalDamage = attackDamage * difficulty;

                    stats.TakeDamage(finalDamage);
                }
            }

            // Brief pause before returning the sword to its starting position.
            yield return new WaitForSeconds(0.2f);

            elapsed = 0f;

            while (elapsed < duration)
            {
                swordTransform.localRotation = Quaternion.Slerp(
                    attackRotation,
                    originalRotation,
                    elapsed / duration
                );

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Restore the exact starting rotation to avoid animation drift.
            swordTransform.localRotation = originalRotation;
        }

        isAttacking = false;
    }
}


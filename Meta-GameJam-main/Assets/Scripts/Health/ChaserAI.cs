using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ChaserAI : MonoBehaviour
{
    [Header("CHASER SETTINGS")]
    [Space(5)]
    public float startDelay = 5f; // 5 second delay before chasing starts
    public float baseSpeed = 3f;
    public float speedIncreaseRate = 0.5f; // Speed increase per second
    public float maxSpeed = 8f;
    public float damageAmount = 15f;
    public float damageInterval = 1f; // Time between damage applications

    [Header("TARGETING")]
    [Space(5)]
    public Transform[] players; // Array of player transforms
    public float detectionRange = 15f;
    public float attackRange = 2f;

    private NavMeshAgent agent;
    private Transform currentTarget;
    private bool isChasing = false;
    private float currentSpeed;
    private float lastDamageTime;

    // Visual effects
    [Header("VISUAL EFFECTS")]
    [Space(5)]
    public GameObject chaserModel;
    public ParticleSystem chaserEffect;
    public AudioSource chaserAudio;

    [Header("ENHANCED EFFECTS")]
    [Space(5)]
    public ParticleSystem smokeTrail; // continuous trail behind chaser
    public ParticleSystem catchBurst; // big explosion when catching player
    public AudioClip catchSound; // sound when catching player
    public Light chaserLight; // glowing eyes effect

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentSpeed = baseSpeed;
        agent.speed = 0; // Start stationary

        // Start the chasing sequence after delay
        StartCoroutine(StartChasing());

        // Find players if not assigned
        if (players == null || players.Length == 0)
        {
            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
            players = new Transform[playerObjects.Length];
            for (int i = 0; i < playerObjects.Length; i++)
            {
                players[i] = playerObjects[i].transform;
            }
        }
    }

    private IEnumerator StartChasing()
    {
        Debug.Log("Chaser will start hunting in " + startDelay + " seconds...");

        // Visual indicator that chaser is about to start
        if (chaserEffect != null)
        {
            chaserEffect.Play();
        }

        yield return new WaitForSeconds(startDelay);

        isChasing = true;
        agent.speed = currentSpeed;

        if (chaserAudio != null)
        {
            chaserAudio.Play();
        }

        Debug.Log("Sanitarium Specter is now hunting!");

        // Start speed increase coroutine
        StartCoroutine(IncreaseSpeedOverTime());
    }

    private IEnumerator IncreaseSpeedOverTime()
    {
        while (isChasing)
        {
            yield return new WaitForSeconds(1f);

            currentSpeed += speedIncreaseRate;
            currentSpeed = Mathf.Clamp(currentSpeed, baseSpeed, maxSpeed);
            agent.speed = currentSpeed;

            Debug.Log("Chaser speed increased to: " + currentSpeed);
        }
    }

    private void Update()
    {
        if (!isChasing) return;

        // Find the closest player
        FindClosestPlayer();

        // Chase the target
        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);

            // Check if close enough to attack
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToTarget <= attackRange)
            {
                AttackPlayer();
            }
        }
    }

    private void FindClosestPlayer()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (Transform player in players)
        {
            if (player == null) continue;

            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < closestDistance && distance <= detectionRange)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        currentTarget = closestPlayer;
    }

    private void AttackPlayer()
    {
        if (Time.time - lastDamageTime >= damageInterval)
        {
            lastDamageTime = Time.time;

            // Check if player has shield active
            ShieldSystem playerShield = currentTarget.GetComponent<ShieldSystem>();
            if (playerShield != null && playerShield.IsShieldActive())
            {
                // Shield blocks damage and knocks player forward
                playerShield.OnChaserHit(transform.position);
                Debug.Log("Chaser hit was blocked by shield! Player knocked forward.");

                // Visual effect for shield hit
                if (chaserEffect != null)
                {
                    chaserEffect.Emit(15);
                }
                return;
            }

            // No shield - apply damage normally
            HealthSystem playerHealth = currentTarget.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log("Chaser damaged player for " + damageAmount + " damage!");

                // Visual effect for attack
                if (chaserEffect != null)
                {
                    chaserEffect.Emit(10);
                }
            }
        }
    }

    // Method to temporarily stun or slow the chaser (for power-ups)
    public void ApplySlowEffect(float duration, float slowMultiplier = 0.5f)
    {
        StartCoroutine(SlowEffect(duration, slowMultiplier));
    }

    private IEnumerator SlowEffect(float duration, float slowMultiplier)
    {
        float originalSpeed = agent.speed;
        agent.speed *= slowMultiplier;

        // dim smoke trail when slowed
        if (smokeTrail != null)
        {
            var emission = smokeTrail.emission;
            float originalRate = emission.rateOverTime.constant;
            emission.rateOverTime = originalRate * 0.5f; // reduce smoke when slowed
        }

        yield return new WaitForSeconds(duration);

        agent.speed = originalSpeed;

        // restore smoke trail
        if (smokeTrail != null)
        {
            var emission = smokeTrail.emission;
            emission.rateOverTime = emission.rateOverTime.constant * 2f; // back to normal
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

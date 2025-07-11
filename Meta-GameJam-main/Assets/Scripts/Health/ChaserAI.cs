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
    public float damageInterval = 1f; // yimr btwn damage 
    
    [Header("TARGETING")]
    [Space(5)]
    public Transform[] players;
    public float detectionRange = 15f;
    public float attackRange = 2f;
    
    private NavMeshAgent agent;
    private Transform currentTarget;
    private bool isChasing = false;
    private float currentSpeed;
    private float lastDamageTime;
    
  
    [Header("VISUAL EFFECTS")]
    [Space(5)]
    public GameObject chaserModel;
    public ParticleSystem chaserEffect;
    public AudioSource chaserAudio;
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentSpeed = baseSpeed;
        agent.speed = 0;
        
        
        StartCoroutine(StartChasing());
        
       
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
        Debug.Log("enemy thing will start hunting in " + startDelay + " seconds...");
        
       
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
        
        Debug.Log("enemyy chasing u");
        
        //  speed increase 
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
            
            Debug.Log("enemy speed went  to: " + currentSpeed);
        }
    }
    
    private void Update()
    {
        if (!isChasing) return;
        
       
        FindClosestPlayer();
        
       
        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
            
          
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
            
            HealthSystem playerHealth = currentTarget.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log("Chaser damaged player for " + damageAmount + " damage!");
                
               
                if (chaserEffect != null)
                {
                    chaserEffect.Emit(10);
                }
            }
        }
    }
    
    //  to temporarily stun or slow the chaser (for power-ups maybe)
    public void ApplySlowEffect(float duration, float slowMultiplier = 0.5f)
    {
        StartCoroutine(SlowEffect(duration, slowMultiplier));
    }
    
    private IEnumerator SlowEffect(float duration, float slowMultiplier)
    {
        float originalSpeed = agent.speed;
        agent.speed *= slowMultiplier;
        
        yield return new WaitForSeconds(duration);
        
        agent.speed = originalSpeed;
    }
    
    private void OnDrawGizmosSelected()
    {
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
      
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
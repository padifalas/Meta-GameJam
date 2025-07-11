using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ChaserAI : MonoBehaviour
{
    [Header("CHASER SETTINGS")]
    [Space(5)]
    public float startDelay = 5f; 
    public float baseSpeed = 3f;
    public float speedIncreaseRate = 0.5f; // speed increase per second
    public float maxSpeed = 8f;
    public float normalDamage = 15f; //  damage from chaser attacks
    public float catchDamagePercent = 0.5f; //  health lost when caught 
    public float damageInterval = 1f; 
    public float catchupSpeedBonus = 2f; // extra speed when player is slow
    
    [Header("TARGETING")]
    [Space(5)]
    public Transform assignedPlayer; //  player this chaser targets
    public float detectionRange = 15f;
    public float attackRange = 2f;
    
    private NavMeshAgent agent;
    private Transform currentTarget;
    private bool isChasing = false;
    private float currentSpeed;
    private float lastDamageTime;
    private Vector3 lastPlayerPosition;
    private float playerStationaryTime = 0f;
    private bool playerIsSlowOrStopped = false;
    
   
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
        
       
        currentTarget = assignedPlayer;
        
        
        if (assignedPlayer == null)
        {
           
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                assignedPlayer = playerObject.transform;
                currentTarget = assignedPlayer;
            }
        }
        
        if (assignedPlayer != null)
        {
         
            lastPlayerPosition = assignedPlayer.position;
        }
        else
        {
           
        }
    }
    
    private IEnumerator StartChasing()
    {
        
        
        
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
            
           
        }
    }
    
    private void Update()
    {
        if (!isChasing || assignedPlayer == null) return;
        
      
        currentTarget = assignedPlayer;
        
        
        CheckPlayerMovement();
        
     
        AdjustChaserSpeed();
        
        
        agent.SetDestination(currentTarget.position);
        
      
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        if (distanceToTarget <= attackRange)
        {
            CatchPlayer(); 
        }
    }
    
    private void CheckPlayerMovement()
    {
        float distanceMoved = Vector3.Distance(assignedPlayer.position, lastPlayerPosition);
        
        if (distanceMoved < 0.1f) // check is barely moving or stopped
        {
            playerStationaryTime += Time.deltaTime;
            if (playerStationaryTime > 0.5f) // half second of being stationary
            {
                playerIsSlowOrStopped = true;
            }
        }
        else
        {
            playerStationaryTime = 0f;
            playerIsSlowOrStopped = false;
        }
        
        lastPlayerPosition = assignedPlayer.position;
    }
    
    private void AdjustChaserSpeed()
    {
        float targetSpeed = currentSpeed;
        
        if (playerIsSlowOrStopped)
        {
            
            targetSpeed += catchupSpeedBonus;
           
        }
        
        agent.speed = Mathf.Min(targetSpeed, maxSpeed);
    }
    
  
    public void OnPlayerHitTrap()
    {
        
        StartCoroutine(TrapSpeedBoost());
    }
    
    private System.Collections.IEnumerator TrapSpeedBoost()
    {
        float originalSpeed = agent.speed;
        agent.speed += catchupSpeedBonus; 
        
        yield return new WaitForSeconds(3f); 
        
        agent.speed = originalSpeed;
    }
    
    
    
    private void CatchPlayer()
    {
        if (Time.time - lastDamageTime >= damageInterval)
        {
            lastDamageTime = Time.time;
            
          
            ShieldSystem playerShield = currentTarget.GetComponent<ShieldSystem>();
            if (playerShield != null && playerShield.IsShieldActive())
            {
               
                playerShield.OnChaserHit(transform.position);
               
                
               
                if (chaserEffect != null)
                {
                    chaserEffect.Emit(15);
                }
                return;
            }
            
           
            HealthSystem playerHealth = currentTarget.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
               
                float damageAmount = playerHealth.currentHealth * catchDamagePercent;
                playerHealth.TakeDamage(damageAmount);
               
                
               
                if (chaserEffect != null)
                {
                    chaserEffect.Emit(25);
                }
                
            
                TriggerCatchEffect();
            }
        }
    }
    
    private void TriggerCatchEffect()
    {
       
        Debug.Log("catcher caught player");
        
        // we  add camera shake, screen flash, etc. here
    }
    
    
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
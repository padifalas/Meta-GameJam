using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("TRAP SETTINGS")]
    [Space(5)]
    public float trapDamage = 5f;
    public float cooldownTime = 2f; 
    public GameObject trapEffect; 
    public AudioClip trapSound;
    
    [Header("SIDE EFFECT SETTINGS")]
    [Space(5)]
    public float speedReduction = 0.7f; // multiply speed by this (0.7 = 30% slower)
    public float disoriententationDuration = 2f; // how long the effect lasts
    public bool notifyChaserOnHit = true; // should chaser get speed boost when player hits trap
    
    private bool canTrigger = true;
    private AudioSource audioSource;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // ensure this gameobject has the "trap" tag
        gameObject.tag = "Trap";
        
        // ensure it has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning($"trap {gameObject.name} needs a collider component!");
        }
        
        Debug.Log($"trap {gameObject.name} initialized - damage: {trapDamage}");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!canTrigger) return;
        
        if (other.CompareTag("Player"))
        {
            Debug.Log($"player {other.name} triggered trap!");
            
            // check for shield protection first
            ShieldSystem playerShield = other.GetComponent<ShieldSystem>();
            if (playerShield != null && playerShield.IsShieldActive())
            {
                Debug.Log("trap blocked by shield!");
                TriggerTrapEffect(); // visual effect still plays
                return; // no damage or side effects
            }
            
            // no shield - apply full trap effects
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(trapDamage);
                Debug.Log($"trap dealt {trapDamage} damage to {other.name}!");
                
                // check if trap killed player
                if (playerHealth.currentHealth <= 0)
                {
                    Debug.Log($"trap killed {other.name}!");
                    // healthsystem will automatically notify roundmanager of death
                }
            }
            
            // notify chaser for speed boost
            if (notifyChaserOnHit)
            {
                NotifyPlayerChaser(other.transform);
            }
            
            // apply disorientation side effect
            ApplyTrapSideEffect(other);
            
            // trigger visual and audio effects
            TriggerTrapEffect();
        }
    }
    
    private void NotifyPlayerChaser(Transform player)
    {
        // find chaser assigned to this player and give it speed boost
        ChaserAI[] allChasers = FindObjectsOfType<ChaserAI>();
        Debug.Log($"found {allChasers.Length} chasers to check");
        
        foreach (ChaserAI chaser in allChasers)
        {
            // use reflection to safely check for assignedPlayer field
            var assignedPlayerField = chaser.GetType().GetField("assignedPlayer");
            if (assignedPlayerField != null)
            {
                Transform assignedPlayer = assignedPlayerField.GetValue(chaser) as Transform;
                if (assignedPlayer == player)
                {
                    // try to call OnPlayerHitTrap method if it exists
                    var trapMethod = chaser.GetType().GetMethod("OnPlayerHitTrap");
                    if (trapMethod != null)
                    {
                        trapMethod.Invoke(chaser, null);
                        Debug.Log($"notified chaser of {player.name} hitting trap via reflection");
                    }
                    else
                    {
                        Debug.LogWarning("OnPlayerHitTrap method not found on ChaserAI - adding manual speed boost");
                        // fallback: manually apply speed boost using reflection
                        ApplyManualSpeedBoost(chaser);
                    }
                    break;
                }
            }
            else
            {
                Debug.LogWarning("assignedPlayer field not found on ChaserAI");
            }
        }
    }
    
    private void ApplyManualSpeedBoost(ChaserAI chaser)
    {
        // manually boost chaser speed if method doesn't exist
        UnityEngine.AI.NavMeshAgent agent = chaser.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            StartCoroutine(ManualSpeedBoost(agent));
        }
    }
    
    private System.Collections.IEnumerator ManualSpeedBoost(UnityEngine.AI.NavMeshAgent agent)
    {
        float originalSpeed = agent.speed;
        agent.speed += 2f; // boost speed by 2
        Debug.Log($"manually boosted chaser speed from {originalSpeed} to {agent.speed}");
        
        yield return new WaitForSeconds(3f);
        
        agent.speed = originalSpeed;
        Debug.Log($"manual speed boost ended - returned to {originalSpeed}");
    }
    
    private void ApplyTrapSideEffect(Collider player)
    {
        // apply disorientation effect to player
        Debug.Log($"applying trap disorientation to {player.name}");
        StartCoroutine(TrapDisorientation(player));
    }
    
    private System.Collections.IEnumerator TrapDisorientation(Collider player)
    {
        Debug.Log($"🕳️ trap disorientation started - reducing speed to {speedReduction * 100}%");
        
        // support both firstpersoncontrols and fpc2
        FirstPersonControls playerControls = player.GetComponent<FirstPersonControls>();
        FPC2 player2Controls = player.GetComponent<FPC2>();
        
        float originalSpeed = 0f;
        
        // apply speed reduction based on control type
        if (playerControls != null)
        {
            originalSpeed = playerControls.moveSpeed;
            playerControls.moveSpeed *= speedReduction;
            Debug.Log($"reduced firstpersoncontrols speed from {originalSpeed} to {playerControls.moveSpeed}");
        }
        else if (player2Controls != null)
        {
            originalSpeed = player2Controls.moveSpeed;
            player2Controls.moveSpeed *= speedReduction;
            Debug.Log($"reduced fpc2 speed from {originalSpeed} to {player2Controls.moveSpeed}");
        }
        else
        {
            Debug.LogWarning($"no supported player control script found on {player.name}");
            yield break;
        }
        
        // wait for disorientation duration
        yield return new WaitForSeconds(disoriententationDuration);
        
        // restore original speed
        if (playerControls != null)
        {
            playerControls.moveSpeed = originalSpeed;
            Debug.Log($"restored firstpersoncontrols speed to {originalSpeed}");
        }
        else if (player2Controls != null)
        {
            player2Controls.moveSpeed = originalSpeed;
            Debug.Log($"restored fpc2 speed to {originalSpeed}");
        }
        
        Debug.Log("🕳️ trap disorientation effect ended");
    }
    
    private void TriggerTrapEffect()
    {
        canTrigger = false;
        
        // play trap sound
        if (trapSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(trapSound);
            Debug.Log("played trap sound effect");
        }
        
        // show visual effect
        if (trapEffect != null)
        {
            GameObject effect = Instantiate(trapEffect, transform.position, transform.rotation);
            Destroy(effect, 2f);
            Debug.Log("triggered trap visual effect");
        }
        
        // start cooldown timer
        Invoke(nameof(ResetTrap), cooldownTime);
        Debug.Log($"trap on cooldown for {cooldownTime} seconds");
    }
    
    private void ResetTrap()
    {
        canTrigger = true;
        Debug.Log($"trap {gameObject.name} reset and ready to trigger again");
    }
    
    // public method to manually trigger trap (for testing)
    public void ManualTrigger()
    {
        if (canTrigger)
        {
            TriggerTrapEffect();
            Debug.Log("trap manually triggered");
        }
    }
    
    // public method to check if trap is ready
    public bool IsReady()
    {
        return canTrigger;
    }
}
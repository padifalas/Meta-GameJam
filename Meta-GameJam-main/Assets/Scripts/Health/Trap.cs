using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("TRAP SETTINGS")]
    [Space(5)]
    public float trapDamage = 20f;
    public float cooldownTime = 2f; 
    public GameObject trapEffect; 
    public AudioClip trapSound;
    
    private bool canTrigger = true;
    private AudioSource audioSource;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
       
        gameObject.tag = "Trap";
        
       
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!canTrigger) return;
        
        if (other.CompareTag("Player"))
        {
           
            ShieldSystem playerShield = other.GetComponent<ShieldSystem>();
            if (playerShield != null && playerShield.IsShieldActive())
            {
               
                TriggerTrapEffect(); 
                return; 
            }
            
           
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(trapDamage);
               
            }
            
            TriggerTrapEffect();
        }
    }
    
    private void TriggerTrapEffect()
    {
        canTrigger = false;
        
       
        if (trapSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(trapSound);
        }
        
       
        if (trapEffect != null)
        {
            GameObject effect = Instantiate(trapEffect, transform.position, transform.rotation);
            Destroy(effect, 2f);
        }
        
        
        Invoke(nameof(ResetTrap), cooldownTime);
    }
    
    private void ResetTrap()
    {
        canTrigger = true;
    }
}
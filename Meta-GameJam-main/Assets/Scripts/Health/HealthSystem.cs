using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [Header("HEALTH SETTINGS")]
    [Space(5)]
    public float maxHealth = 100f;
    [HideInInspector]
    public float currentHealth; 
    public Slider healthBar; 
    public float healthOrbHealAmount = 25f;
    
    [Header("SIDE EFFECTS")]
    [Space(5)]
    public float sideEffectDuration = 5f;
    private Camera playerCamera;
    private FirstPersonControls playerControls;
    private bool isAffectedBySideEffect = false;
    
    
    private Vector3 originalCameraPosition;
    private float originalMoveSpeed;
    private float shakeIntensity = 0.1f;
    
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
        
        
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();
            
        playerControls = GetComponent<FirstPersonControls>();
        
        if (playerCamera != null)
            originalCameraPosition = playerCamera.transform.localPosition;
            
        if (playerControls != null)
            originalMoveSpeed = playerControls.moveSpeed;
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float healAmount)
    {
        bool wasFullHealth = (currentHealth >= maxHealth);
        
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
        
        
        if (wasFullHealth && healAmount > 0)
        {
            
            ApplyStrangeSideEffect();
        }
        else if (healAmount > 0)
        {
            
            ApplyMinorSideEffect();
        }
    }
    
    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
    }
    
    private void ApplyStrangeSideEffect()
    {
        if (isAffectedBySideEffect) return;
        
        Debug.Log("player big");
        StartCoroutine(RegenerativeChaos());
    }
    
    private void ApplyMinorSideEffect()
    {
        if (isAffectedBySideEffect) return;
        
        Debug.Log("cam shake");
        StartCoroutine(HealingDisorientation());
    }
    
    private IEnumerator RegenerativeChaos()
    {
        isAffectedBySideEffect = true;
        
        
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.3f;
        
        
        float elapsed = 0;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed);
            yield return null;
        }
        
        
        yield return new WaitForSeconds(sideEffectDuration - 2f);
        
        
        elapsed = 0;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed);
            yield return null;
        }
        
        transform.localScale = originalScale;
        isAffectedBySideEffect = false;
    }
    
    private IEnumerator HealingDisorientation()
    {
        isAffectedBySideEffect = true;
        
        
        if (playerControls != null)
        {
            playerControls.moveSpeed *= 0.8f; 
        }
        
        float elapsed = 0;
        while (elapsed < sideEffectDuration)
        {
            elapsed += Time.deltaTime;
            
           
            if (playerCamera != null)
            {
                Vector3 shake = Random.insideUnitSphere * shakeIntensity;
                playerCamera.transform.localPosition = originalCameraPosition + shake;
            }
            
            yield return null;
        }
        
        
        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = originalCameraPosition;
        }
        
        if (playerControls != null)
        {
            playerControls.moveSpeed = originalMoveSpeed;
        }
        
        isAffectedBySideEffect = false;
    }
    
    private void Die()
    {
        Debug.Log("plauer die");
       
    }
    
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("HealthOrb"))
        {
            Heal(healthOrbHealAmount);
            Destroy(other.gameObject);
        }
    }
}
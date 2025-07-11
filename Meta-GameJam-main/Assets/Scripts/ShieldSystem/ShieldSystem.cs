using System.Collections;
using UnityEngine;

public class ShieldSystem : MonoBehaviour
{
    [Header("SHIELD SETTINGS")]
    [Space(5)]
    public float shieldDuration = 10f;
    public GameObject shieldBubble; 
    public float knockbackForce = 5f; 
    public float sideEffectDuration = 5f;
    
    [Header("SIDE EFFECTS")]
    [Space(5)]
    public float staticIntensity = 0.5f;
    public Color staticColor = Color.cyan;
    
    private bool isShieldActive = false;
    private bool isSideEffectActive = false;
    private Coroutine shieldCoroutine;
    private Coroutine sideEffectCoroutine;
    
  
    private CharacterController characterController;
    private Camera playerCamera;
    private HealthSystem healthSystem;
    private FirstPersonControls playerControls; 
    
   
    private Material originalSkybox;
    private Color originalCameraBackground;
    
    private void Start()
    {
        
        characterController = GetComponent<CharacterController>();
        healthSystem = GetComponent<HealthSystem>();
        playerControls = GetComponent<FirstPersonControls>();
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();
        
       
        if (shieldBubble != null)
        {
            shieldBubble.SetActive(false);
        }
        
     
        if (playerCamera != null)
        {
            originalCameraBackground = playerCamera.backgroundColor;
        }
    }
    
    public void ActivateShield()
    {
        if (isShieldActive)
        {
          
            ApplyStrangeSideEffect();
            return;
        }
        

        isShieldActive = true;
        
     
        if (shieldBubble != null)
        {
            shieldBubble.SetActive(true);
        }
        
      
        ApplyMinorSideEffect();
        
       
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }
        shieldCoroutine = StartCoroutine(ShieldTimer());
        
        Debug.Log("shield on");
    }
    
    private IEnumerator ShieldTimer()
    {
        yield return new WaitForSeconds(shieldDuration);
        DeactivateShield();
    }
    
    public void DeactivateShield()
    {
        isShieldActive = false;
        
        
        if (shieldBubble != null)
        {
            shieldBubble.SetActive(false);
        }
        
        Debug.Log("shield off");
    }
    
    public bool IsShieldActive()
    {
        return isShieldActive;
    }
    
    public void OnChaserHit(Vector3 chaserPosition)
    {
        if (!isShieldActive) return;
        
       
        Vector3 knockbackDirection = (transform.position - chaserPosition).normalized;
        knockbackDirection.y = 0; 
        
        
        if (characterController != null)
        {
            StartCoroutine(ApplyKnockback(knockbackDirection));
        }
        
        
        DeactivateShield();
        
     
    }
    
    private IEnumerator ApplyKnockback(Vector3 direction)
    {
        float knockbackTime = 0.3f;
        float elapsed = 0;
        
        while (elapsed < knockbackTime)
        {
            elapsed += Time.deltaTime;
            float force = Mathf.Lerp(knockbackForce, 0, elapsed / knockbackTime);
            
            Vector3 movement = direction * force * Time.deltaTime;
            characterController.Move(movement);
            
            yield return null;
        }
    }
    
    private void ApplyStrangeSideEffect()
    {
        if (isSideEffectActive) return;
        
       
        if (sideEffectCoroutine != null)
        {
            StopCoroutine(sideEffectCoroutine);
        }
        sideEffectCoroutine = StartCoroutine(ArcaneFeedback());
    }
    
    private void ApplyMinorSideEffect()
    {
        if (isSideEffectActive) return;
        
        
        if (sideEffectCoroutine != null)
        {
            StopCoroutine(sideEffectCoroutine);
        }
        sideEffectCoroutine = StartCoroutine(MagicalStatic());
    }
    
    private IEnumerator ArcaneFeedback()
    {
        isSideEffectActive = true;
        
        
        float originalIntensity = staticIntensity;
        staticIntensity *= 2f;
        
        float elapsed = 0;
        while (elapsed < sideEffectDuration)
        {
            elapsed += Time.deltaTime;
            
            if (playerCamera != null)
            {
                
                float noise = Mathf.PerlinNoise(Time.time * 10f, 0) * staticIntensity;
                Color interferenceColor = Color.Lerp(originalCameraBackground, staticColor, noise);
                playerCamera.backgroundColor = interferenceColor;
                
              
                if (Random.value < 0.3f)
                {
                    playerCamera.enabled = false;
                    yield return new WaitForSeconds(0.05f);
                    playerCamera.enabled = true;
                }
            }
            
            yield return null;
        }
        
       
        staticIntensity = originalIntensity;
        if (playerCamera != null)
        {
            playerCamera.backgroundColor = originalCameraBackground;
        }
        
        isSideEffectActive = false;
    }
    
    private IEnumerator MagicalStatic()
    {
        isSideEffectActive = true;
        
        float elapsed = 0;
        while (elapsed < sideEffectDuration)
        {
            elapsed += Time.deltaTime;
            
            if (playerCamera != null)
            {
                
                float noise = Mathf.PerlinNoise(Time.time * 5f, 0) * staticIntensity;
                Color staticEffect = Color.Lerp(originalCameraBackground, staticColor, noise * 0.3f);
                playerCamera.backgroundColor = staticEffect;
            }
            
            yield return null;
        }
        
      
        if (playerCamera != null)
        {
            playerCamera.backgroundColor = originalCameraBackground;
        }
        
        isSideEffectActive = false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
      
        if (other.CompareTag("Trap") && isShieldActive)
        {
           
            return;
        }
    }
}
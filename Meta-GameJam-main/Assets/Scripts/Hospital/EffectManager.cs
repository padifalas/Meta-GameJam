using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [Header("effect settings")]
    [Space(5)]
    public float effectDuration = 15f; // how long effects last
    public Transform[] teleportPoints; // points for random teleportation
    
    [Header("visual effect references")]
    [Space(5)]
    public Camera playerCamera;
    public GameObject playerModel; // for invisibility effect
    
    private FirstPersonControls playerControls;
    private CharacterController characterController;
    private List<Coroutine> activeEffects = new List<Coroutine>();
    
    // original values for reset
    private float originalMoveSpeed;
    private bool originalCameraFlip = false;
    
    // effect tracking
    private bool isInvisible = false;
    private bool isSlowed = false;
    private bool hasFlippedScreen = false;
    private bool isShaking = false;
    
    public enum EffectType
    {
        ScreenFlip,
        ScreenBlur,
        SlowDown,
        CameraShake,
        RandomTeleport,
        Invisibility
    }
    
    private void Start()
    {
        playerControls = GetComponent<FirstPersonControls>();
        characterController = GetComponent<CharacterController>();
        
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        originalMoveSpeed = playerControls.moveSpeed;
    }
    
    public void ApplyRandomEffect()
    {
        // pick a random effect
        EffectType randomEffect = (EffectType)Random.Range(0, System.Enum.GetValues(typeof(EffectType)).Length);
        ApplyEffect(randomEffect);
    }
    
    public void ApplyEffect(EffectType effectType)
    {
        Debug.Log($"applying effect: {effectType}");
        
        switch (effectType)
        {
            case EffectType.ScreenFlip:
                if (!hasFlippedScreen)
                    activeEffects.Add(StartCoroutine(ScreenFlipEffect()));
                break;
            case EffectType.ScreenBlur:
                activeEffects.Add(StartCoroutine(ScreenBlurEffect()));
                break;
            case EffectType.SlowDown:
                if (!isSlowed)
                    activeEffects.Add(StartCoroutine(SlowDownEffect()));
                break;
            case EffectType.CameraShake:
                if (!isShaking)
                    activeEffects.Add(StartCoroutine(CameraShakeEffect()));
                break;
            case EffectType.RandomTeleport:
                activeEffects.Add(StartCoroutine(RandomTeleportEffect()));
                break;
            case EffectType.Invisibility:
                if (!isInvisible)
                    activeEffects.Add(StartCoroutine(InvisibilityEffect()));
                break;
        }
    }
    
    public void CureAllEffects()
    {
        Debug.Log("curing all effects!");
        
        // stop all active effect coroutines
        foreach (Coroutine effect in activeEffects)
        {
            if (effect != null)
                StopCoroutine(effect);
        }
        activeEffects.Clear();
        
        // reset all values to normal
        ResetToNormal();
    }
    
    private void ResetToNormal()
    {
        // reset camera
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.identity;
        }
        
        // reset speed
        playerControls.moveSpeed = originalMoveSpeed;
        isSlowed = false;
        
        // reset visibility
        if (playerModel != null)
        {
            Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
            }
        }
        isInvisible = false;
        
        // reset screen flip
        hasFlippedScreen = false;
        isShaking = false;
        
        Debug.Log("all effects reset to normal");
    }
    
    private IEnumerator ScreenFlipEffect()
    {
        hasFlippedScreen = true;
        Debug.Log("screen flip effect started");
        
        // flip camera vertically
        if (playerCamera != null)
        {
            playerCamera.transform.Rotate(0, 0, 180);
        }
        
        yield return new WaitForSeconds(effectDuration);
        
        // reset camera
        if (playerCamera != null)
        {
            playerCamera.transform.Rotate(0, 0, -180);
        }
        
        hasFlippedScreen = false;
        Debug.Log("screen flip effect ended");
    }
    
    private IEnumerator ScreenBlurEffect()
    {
        Debug.Log("screen blur effect started (would need post-processing)");
        
        // note: actual blur would need post-processing volume
        // for now we'll just do a color overlay
        if (playerCamera != null)
        {
            Color originalBackground = playerCamera.backgroundColor;
            
            float elapsed = 0f;
            while (elapsed < effectDuration)
            {
                elapsed += Time.deltaTime;
                // subtle color shift to simulate blur
                float blur = Mathf.Sin(Time.time * 2f) * 0.1f;
                playerCamera.backgroundColor = Color.Lerp(originalBackground, Color.gray, blur);
                yield return null;
            }
            
            playerCamera.backgroundColor = originalBackground;
        }
        
        Debug.Log("screen blur effect ended");
    }
    
    private IEnumerator SlowDownEffect()
    {
        isSlowed = true;
        Debug.Log("slow down effect started");
        
        // reduce movement speed
        playerControls.moveSpeed = originalMoveSpeed * 0.5f;
        
        yield return new WaitForSeconds(effectDuration);
        
        // restore speed
        playerControls.moveSpeed = originalMoveSpeed;
        isSlowed = false;
        Debug.Log("slow down effect ended");
    }
    
    private IEnumerator CameraShakeEffect()
    {
        isShaking = true;
        Debug.Log("camera shake effect started");
        
        Vector3 originalPosition = playerCamera.transform.localPosition;
        
        float elapsed = 0f;
        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            
            // random shake
            Vector3 shake = Random.insideUnitSphere * 0.1f;
            playerCamera.transform.localPosition = originalPosition + shake;
            
            yield return null;
        }
        
        // reset position
        playerCamera.transform.localPosition = originalPosition;
        isShaking = false;
        Debug.Log("camera shake effect ended");
    }
    
    private IEnumerator RandomTeleportEffect()
    {
        Debug.Log("random teleport effect started");
        
        float teleportInterval = 2f; // teleport every 2 seconds
        float elapsed = 0f;
        
        while (elapsed < effectDuration)
        {
            yield return new WaitForSeconds(teleportInterval);
            elapsed += teleportInterval;
            
            // teleport to random point
            if (teleportPoints != null && teleportPoints.Length > 0)
            {
                Transform randomPoint = teleportPoints[Random.Range(0, teleportPoints.Length)];
                characterController.enabled = false;
                transform.position = randomPoint.position;
                characterController.enabled = true;
                
                Debug.Log("player teleported!");
            }
        }
        
        Debug.Log("random teleport effect ended");
    }
    
    private IEnumerator InvisibilityEffect()
    {
        isInvisible = true;
        Debug.Log("invisibility effect started");
        
        // make player model invisible
        if (playerModel != null)
        {
            Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }
        }
        
        yield return new WaitForSeconds(effectDuration);
        
        // make player visible again
        if (playerModel != null)
        {
            Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
            }
        }
        
        isInvisible = false;
        Debug.Log("invisibility effect ended");
    }
}
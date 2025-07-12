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
    private FPC2 player2Controls; // for FPC2 support
    private CharacterController characterController;
    private List<Coroutine> activeEffects = new List<Coroutine>();
    
    // original values for reset
    private float originalMoveSpeed;
    private Vector3 originalCameraRotation;
    private Color originalCameraBackground;
    
    // effect tracking
    private bool isInvisible = false;
    private bool isSlowed = false;
    private bool hasFlippedScreen = false;
    private bool isShaking = false;
    private bool isBlurred = false;
    
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
        // try to get either control script
        playerControls = GetComponent<FirstPersonControls>();
        player2Controls = GetComponent<FPC2>();
        characterController = GetComponent<CharacterController>();
        
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        // store original values
        if (playerControls != null)
            originalMoveSpeed = playerControls.moveSpeed;
        else if (player2Controls != null)
            originalMoveSpeed = player2Controls.moveSpeed;
        
        if (playerCamera != null)
        {
            originalCameraRotation = playerCamera.transform.localEulerAngles;
            originalCameraBackground = playerCamera.backgroundColor;
        }
        
        Debug.Log($"effect manager initialized for {gameObject.name}");
    }
    
    public void ApplyRandomEffect()
    {
        // pick a random effect
        EffectType randomEffect = (EffectType)Random.Range(0, System.Enum.GetValues(typeof(EffectType)).Length);
        
        Debug.Log($" PILL COLLECTED! applying random effect: {randomEffect} to {gameObject.name}");
        
        ApplyEffect(randomEffect);
    }
    
    public void ApplyEffect(EffectType effectType)
    {
        Debug.Log($" starting {effectType} effect on {gameObject.name}");
        
        switch (effectType)
        {
            case EffectType.ScreenFlip:
                if (!hasFlippedScreen)
                {
                    Debug.Log(" screen flip effect - world turning upside down!");
                    activeEffects.Add(StartCoroutine(ScreenFlipEffect()));
                }
                else
                {
                    Debug.Log(" screen flip already active - skipping");
                }
                break;
                
            case EffectType.ScreenBlur:
                if (!isBlurred)
                {
                    Debug.Log("screen blur effect - vision becoming distorted!");
                    activeEffects.Add(StartCoroutine(ScreenBlurEffect()));
                }
                else
                {
                    Debug.Log(" screen blur already active - skipping");
                }
                break;
                
            case EffectType.SlowDown:
                if (!isSlowed)
                {
                    Debug.Log(" slowdown effect - movement becoming sluggish!");
                    activeEffects.Add(StartCoroutine(SlowDownEffect()));
                }
                else
                {
                    Debug.Log(" slowdown already active - skipping");
                }
                break;
                
            case EffectType.CameraShake:
                if (!isShaking)
                {
                    Debug.Log(" camera shake effect - intense tremors!");
                    activeEffects.Add(StartCoroutine(CameraShakeEffect()));
                }
                else
                {
                    Debug.Log(" camera shake already active - skipping");
                }
                break;
                
            case EffectType.RandomTeleport:
                Debug.Log(" random teleport effect - reality is shifting!");
                activeEffects.Add(StartCoroutine(RandomTeleportEffect()));
                break;
                
            case EffectType.Invisibility:
                if (!isInvisible)
                {
                    Debug.Log("invisibility effect - fading from existence!");
                    activeEffects.Add(StartCoroutine(InvisibilityEffect()));
                }
                else
                {
                    Debug.Log(" invisibility already active - skipping");
                }
                break;
        }
    }
    
    public void CureAllEffects()
    {
        Debug.Log($" CURE COLLECTED! resetting all effects for {gameObject.name}");
        
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
        Debug.Log(" resetting all effects to normal state");
        
        // reset camera rotation
        if (playerCamera != null)
        {
            playerCamera.transform.localEulerAngles = originalCameraRotation;
            playerCamera.backgroundColor = originalCameraBackground;
        }
        
        // reset speed
        if (playerControls != null)
            playerControls.moveSpeed = originalMoveSpeed;
        else if (player2Controls != null)
            player2Controls.moveSpeed = originalMoveSpeed;
        
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
        
        // reset all flags
        hasFlippedScreen = false;
        isShaking = false;
        isBlurred = false;
        
        Debug.Log(" all effects successfully reset");
    }
    
    private IEnumerator ScreenFlipEffect()
    {
        hasFlippedScreen = true;
        Debug.Log(" screen flip starting - rotating camera 180 degrees");
        
        // flip camera vertically (z-axis rotation)
        if (playerCamera != null)
        {
            Vector3 currentRotation = playerCamera.transform.localEulerAngles;
            playerCamera.transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, currentRotation.z + 180f);
            Debug.Log("screen flipped! controls are now inverted");
        }
        
        yield return new WaitForSeconds(effectDuration);
        
        // reset camera rotation
        if (playerCamera != null)
        {
            playerCamera.transform.localEulerAngles = originalCameraRotation;
            Debug.Log(" screen flip ended - vision restored to normal");
        }
        
        hasFlippedScreen = false;
    }
    
    private IEnumerator ScreenBlurEffect()
    {
        isBlurred = true;
        Debug.Log(" screen blur starting - vision becoming distorted");
        
        if (playerCamera != null)
        {
            float elapsed = 0f;
            while (elapsed < effectDuration)
            {
                elapsed += Time.deltaTime;
                
                // create pulsing blur effect by manipulating background color
                float blurIntensity = (Mathf.Sin(Time.time * 3f) + 1f) * 0.3f;
                Color blurColor = Color.Lerp(originalCameraBackground, Color.gray, blurIntensity);
                playerCamera.backgroundColor = blurColor;
                
                // add some random color shifts for more trippy effect
                if (Random.value < 0.1f)
                {
                    playerCamera.backgroundColor = Color.Lerp(blurColor, Random.ColorHSV(), 0.2f);
                }
                
                yield return null;
            }
            
            playerCamera.backgroundColor = originalCameraBackground;
            Debug.Log("screen blur ended - vision clarity restored");
        }
        
        isBlurred = false;
    }
    
    private IEnumerator SlowDownEffect()
    {
        isSlowed = true;
        Debug.Log(" slowdown starting - movement speed reduced to 40%");
        
        // reduce movement speed significantly
        float slowedSpeed = originalMoveSpeed * 0.4f;
        
        if (playerControls != null)
            playerControls.moveSpeed = slowedSpeed;
        else if (player2Controls != null)
            player2Controls.moveSpeed = slowedSpeed;
        
        Debug.Log($" movement speed changed from {originalMoveSpeed} to {slowedSpeed}");
        
        yield return new WaitForSeconds(effectDuration);
        
        // restore original speed
        if (playerControls != null)
            playerControls.moveSpeed = originalMoveSpeed;
        else if (player2Controls != null)
            player2Controls.moveSpeed = originalMoveSpeed;
        
        Debug.Log(" slowdown ended - normal movement speed restored");
        isSlowed = false;
    }
    
    private IEnumerator CameraShakeEffect()
    {
        isShaking = true;
        Debug.Log("camera shake starting - intense tremors for " + effectDuration + " seconds");
        
        Vector3 originalPosition = playerCamera.transform.localPosition;
        float shakeIntensity = 0.15f;
        
        float elapsed = 0f;
        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            
            // create random shake with varying intensity
            float currentIntensity = shakeIntensity * (1f - (elapsed / effectDuration * 0.5f)); // fade out over time
            Vector3 shake = Random.insideUnitSphere * currentIntensity;
            playerCamera.transform.localPosition = originalPosition + shake;
            
            yield return null;
        }
        
        // reset position
        playerCamera.transform.localPosition = originalPosition;
        Debug.Log(" camera shake ended - tremors subsided");
        isShaking = false;
    }
    
    private IEnumerator RandomTeleportEffect()
    {
        Debug.Log("random teleport starting - reality shifting every 2 seconds");
        
        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            Debug.LogWarning(" no teleport points assigned! creating random teleport positions");
            yield break;
        }
        
        float teleportInterval = 2f;
        float elapsed = 0f;
        int teleportCount = 0;
        
        while (elapsed < effectDuration)
        {
            yield return new WaitForSeconds(teleportInterval);
            elapsed += teleportInterval;
            teleportCount++;
            
            // teleport to random point
            Transform randomPoint = teleportPoints[Random.Range(0, teleportPoints.Length)];
            
            characterController.enabled = false;
            transform.position = randomPoint.position;
            characterController.enabled = true;
            
            Debug.Log($"teleport #{teleportCount} - warped to {randomPoint.name}!");
        }
        
        Debug.Log($"random teleport ended - total teleports: {teleportCount}");
    }
    
private IEnumerator InvisibilityEffect()
{
    isInvisible = true;
    Debug.Log("👻 invisibility starting - fading from the physical realm");
    
    List<Renderer> affectedRenderers = new List<Renderer>();
    List<Collider> affectedColliders = new List<Collider>();
    
    // make player model invisible
    if (playerModel != null)
    {
        // get all renderers
        Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.enabled)
            {
                renderer.enabled = false;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                affectedRenderers.Add(renderer);
            }
        }
        
        // optionally disable collision for true invisibility
        Collider[] colliders = playerModel.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.enabled && !col.isTrigger) // don't disable trigger colliders
            {
                col.enabled = false;
                affectedColliders.Add(col);
            }
        }
        
        Debug.Log($"👻 made {affectedRenderers.Count} renderers and {affectedColliders.Count} colliders invisible");
        
        // add ghostly particle effect at player position
        CreateGhostlyEffect();
    }
    else
    {
        Debug.LogWarning("⚠️ no player model assigned for invisibility effect!");
    }
    
    // visual feedback for the invisible player
    if (playerCamera != null)
    {
        StartCoroutine(InvisibilityFeedback());
    }
    
    yield return new WaitForSeconds(effectDuration);
    
    // make player visible again
    foreach (Renderer renderer in affectedRenderers)
    {
        if (renderer != null)
        {
            renderer.enabled = true;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
    
    // re-enable colliders
    foreach (Collider col in affectedColliders)
    {
        if (col != null)
        {
            col.enabled = true;
        }
    }
    
    Debug.Log("👻 invisibility ended - materialized back into reality");
    isInvisible = false;
}

private IEnumerator InvisibilityFeedback()
{
    // give visual feedback to invisible player
    Color originalColor = playerCamera.backgroundColor;
    
    float elapsed = 0f;
    while (elapsed < effectDuration && isInvisible)
    {
        elapsed += Time.deltaTime;
        
        // subtle blue tint to indicate invisibility
        float pulse = (Mathf.Sin(Time.time * 2f) + 1f) * 0.1f;
        Color invisibleTint = Color.Lerp(originalColor, Color.cyan, pulse);
        playerCamera.backgroundColor = invisibleTint;
        
        yield return null;
    }
    
    // restore original camera color
    playerCamera.backgroundColor = originalColor;
}

private void CreateGhostlyEffect()
{
    // create a subtle particle effect where player was
    GameObject ghostEffect = new GameObject("GhostlyEffect");
    ghostEffect.transform.position = transform.position;
    
    ParticleSystem particles = ghostEffect.AddComponent<ParticleSystem>();
    var main = particles.main;
    main.startColor = new Color(1f, 1f, 1f, 0.3f); // transparent white
    main.startSize = 0.1f;
    main.startSpeed = 1f;
    main.maxParticles = 20;
    
    var emission = particles.emission;
    emission.rateOverTime = 5f;
    
    var shape = particles.shape;
    shape.shapeType = ParticleSystemShapeType.Sphere;
    shape.radius = 1f;
    
    // destroy after effect
    Destroy(ghostEffect, effectDuration + 2f);
}
    
    // public method to check if any effects are active (for UI/debugging)
    public bool HasActiveEffects()
    {
        return activeEffects.Count > 0;
    }
    
    // public method to get current active effects (for debugging)
    public List<string> GetActiveEffects()
    {
        List<string> effects = new List<string>();
        if (hasFlippedScreen) effects.Add("Screen Flip");
        if (isBlurred) effects.Add("Screen Blur");
        if (isSlowed) effects.Add("Slowdown");
        if (isShaking) effects.Add("Camera Shake");
        if (isInvisible) effects.Add("Invisibility");
        return effects;
    }
}
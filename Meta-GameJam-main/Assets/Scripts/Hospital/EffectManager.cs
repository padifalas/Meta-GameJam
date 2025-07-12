using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [Header("effect settings")]
    [Space(5)]
    public float effectDuration = 15f; // how long effects last
    public Transform[] teleportPoints; // points for random teleportation
    
    [Header("slowdown effect")]
    [Space(5)]
    public float slowdownMultiplier = 0.4f; // multiply speed by this value
    
    [Header("camera shake effect")]
    [Space(5)]
    public float shakeIntensity = 0.15f; // how intense the shake is
    public bool shakeFadesOut = true; // whether shake intensity fades over time
    
    [Header("screen blur effect")]
    [Space(5)]
    public float blurPulseSpeed = 3f; // how fast the blur effect pulses
    public float blurIntensity = 0.3f; // strength of blur color mixing
    public float colorShiftChance = 0.1f; // chance per frame for random color shift
    public float colorShiftIntensity = 0.2f; // strength of color shifts
    
    [Header("teleport effect")]
    [Space(5)]
    public float teleportInterval = 2f; // time between teleports
    
    [Header("invisibility effect")]
    [Space(5)]
    public bool disableCollisionDuringInvisibility = false; // make player truly invisible
    public GameObject ghostlyParticlesPrefab; // custom particle effect for invisibility
    public Color invisibilityTint = Color.cyan; // camera tint when invisible
    public float invisibilityTintIntensity = 0.1f; // strength of camera tint
    
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
        
        Debug.Log($"🍄 PILL COLLECTED! applying random effect: {randomEffect} to {gameObject.name}");
        
        ApplyEffect(randomEffect);
    }
    
    public void ApplyEffect(EffectType effectType)
    {
        Debug.Log($"🎭 starting {effectType} effect on {gameObject.name}");
        
        switch (effectType)
        {
            case EffectType.ScreenFlip:
                if (!hasFlippedScreen)
                {
                    Debug.Log("🔄 screen flip effect - world turning upside down!");
                    activeEffects.Add(StartCoroutine(ScreenFlipEffect()));
                }
                else
                {
                    Debug.Log("⚠️ screen flip already active - skipping");
                }
                break;
                
            case EffectType.ScreenBlur:
                if (!isBlurred)
                {
                    Debug.Log("😵‍💫 screen blur effect - vision becoming distorted!");
                    activeEffects.Add(StartCoroutine(ScreenBlurEffect()));
                }
                else
                {
                    Debug.Log("⚠️ screen blur already active - skipping");
                }
                break;
                
            case EffectType.SlowDown:
                if (!isSlowed)
                {
                    Debug.Log($"🐌 slowdown effect - movement reduced to {slowdownMultiplier * 100}%!");
                    activeEffects.Add(StartCoroutine(SlowDownEffect()));
                }
                else
                {
                    Debug.Log("⚠️ slowdown already active - skipping");
                }
                break;
                
            case EffectType.CameraShake:
                if (!isShaking)
                {
                    Debug.Log($"📳 camera shake effect - intensity {shakeIntensity}!");
                    activeEffects.Add(StartCoroutine(CameraShakeEffect()));
                }
                else
                {
                    Debug.Log("⚠️ camera shake already active - skipping");
                }
                break;
                
            case EffectType.RandomTeleport:
                Debug.Log($"⚡ random teleport effect - teleporting every {teleportInterval} seconds!");
                activeEffects.Add(StartCoroutine(RandomTeleportEffect()));
                break;
                
            case EffectType.Invisibility:
                if (!isInvisible)
                {
                    Debug.Log("👻 invisibility effect - fading from existence!");
                    activeEffects.Add(StartCoroutine(InvisibilityEffect()));
                }
                else
                {
                    Debug.Log("⚠️ invisibility already active - skipping");
                }
                break;
        }
    }
    
    public void CureAllEffects()
    {
        Debug.Log($"💊 CURE COLLECTED! resetting all effects for {gameObject.name}");
        
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
        Debug.Log("🔄 resetting all effects to normal state");
        
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
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
            
            // re-enable colliders if they were disabled
            if (disableCollisionDuringInvisibility)
            {
                Collider[] colliders = playerModel.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                {
                    if (!col.isTrigger) // don't mess with trigger colliders
                        col.enabled = true;
                }
            }
        }
        isInvisible = false;
        
        // reset all flags
        hasFlippedScreen = false;
        isShaking = false;
        isBlurred = false;
        
        Debug.Log("✅ all effects successfully reset");
    }
    
    private IEnumerator ScreenFlipEffect()
    {
        hasFlippedScreen = true;
        Debug.Log("🔄 screen flip starting - rotating camera 180 degrees");
        
        // flip camera vertically (z-axis rotation)
        if (playerCamera != null)
        {
            Vector3 currentRotation = playerCamera.transform.localEulerAngles;
            playerCamera.transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, currentRotation.z + 180f);
            Debug.Log("🔄 screen flipped! controls are now inverted");
        }
        
        yield return new WaitForSeconds(effectDuration);
        
        // reset camera rotation
        if (playerCamera != null)
        {
            playerCamera.transform.localEulerAngles = originalCameraRotation;
            Debug.Log("🔄 screen flip ended - vision restored to normal");
        }
        
        hasFlippedScreen = false;
    }
    
    private IEnumerator ScreenBlurEffect()
    {
        isBlurred = true;
        Debug.Log($"😵‍💫 screen blur starting - pulse speed: {blurPulseSpeed}, intensity: {blurIntensity}");
        
        if (playerCamera != null)
        {
            float elapsed = 0f;
            while (elapsed < effectDuration)
            {
                elapsed += Time.deltaTime;
                
                // create pulsing blur effect by manipulating background color
                float blurValue = (Mathf.Sin(Time.time * blurPulseSpeed) + 1f) * blurIntensity;
                Color blurColor = Color.Lerp(originalCameraBackground, Color.gray, blurValue);
                playerCamera.backgroundColor = blurColor;
                
                // add some random color shifts for more trippy effect
                if (Random.value < colorShiftChance)
                {
                    playerCamera.backgroundColor = Color.Lerp(blurColor, Random.ColorHSV(), colorShiftIntensity);
                }
                
                yield return null;
            }
            
            playerCamera.backgroundColor = originalCameraBackground;
            Debug.Log("😵‍💫 screen blur ended - vision clarity restored");
        }
        
        isBlurred = false;
    }
    
    private IEnumerator SlowDownEffect()
    {
        isSlowed = true;
        Debug.Log($"🐌 slowdown starting - movement speed reduced to {slowdownMultiplier * 100}%");
        
        // reduce movement speed using the public multiplier
        float slowedSpeed = originalMoveSpeed * slowdownMultiplier;
        
        if (playerControls != null)
            playerControls.moveSpeed = slowedSpeed;
        else if (player2Controls != null)
            player2Controls.moveSpeed = slowedSpeed;
        
        Debug.Log($"🐌 movement speed changed from {originalMoveSpeed} to {slowedSpeed}");
        
        yield return new WaitForSeconds(effectDuration);
        
        // restore original speed
        if (playerControls != null)
            playerControls.moveSpeed = originalMoveSpeed;
        else if (player2Controls != null)
            player2Controls.moveSpeed = originalMoveSpeed;
        
        Debug.Log("🐌 slowdown ended - normal movement speed restored");
        isSlowed = false;
    }
    
    private IEnumerator CameraShakeEffect()
    {
        isShaking = true;
        Debug.Log($"📳 camera shake starting - intensity {shakeIntensity} for {effectDuration} seconds");
        
        Vector3 originalPosition = playerCamera.transform.localPosition;
        
        float elapsed = 0f;
        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            
            // calculate current shake intensity
            float currentIntensity = shakeIntensity;
            if (shakeFadesOut)
            {
                currentIntensity = shakeIntensity * (1f - (elapsed / effectDuration * 0.5f)); // fade out over time
            }
            
            // create random shake
            Vector3 shake = Random.insideUnitSphere * currentIntensity;
            playerCamera.transform.localPosition = originalPosition + shake;
            
            yield return null;
        }
        
        // reset position
        playerCamera.transform.localPosition = originalPosition;
        Debug.Log("📳 camera shake ended - tremors subsided");
        isShaking = false;
    }
    
    private IEnumerator RandomTeleportEffect()
    {
        Debug.Log($"⚡ random teleport starting - teleporting every {teleportInterval} seconds");
        
        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ no teleport points assigned! skipping teleport effect");
            yield break;
        }
        
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
            
            Debug.Log($"⚡ teleport #{teleportCount} - warped to {randomPoint.name}!");
        }
        
        Debug.Log($"⚡ random teleport ended - total teleports: {teleportCount}");
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
            if (disableCollisionDuringInvisibility)
            {
                Collider[] colliders = playerModel.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                {
                    if (col.enabled && !col.isTrigger) // don't disable trigger colliders
                    {
                        col.enabled = false;
                        affectedColliders.Add(col);
                    }
                }
            }
            
            Debug.Log($"👻 made {affectedRenderers.Count} renderers invisible");
            if (disableCollisionDuringInvisibility)
                Debug.Log($"👻 disabled {affectedColliders.Count} colliders");
            
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
        // give visual feedback to invisible player using custom tint
        float elapsed = 0f;
        while (elapsed < effectDuration && isInvisible)
        {
            elapsed += Time.deltaTime;
            
            // apply custom tint with custom intensity
            float pulse = (Mathf.Sin(Time.time * 2f) + 1f) * invisibilityTintIntensity;
            Color tintColor = Color.Lerp(originalCameraBackground, invisibilityTint, pulse);
            playerCamera.backgroundColor = tintColor;
            
            yield return null;
        }
        
        // restore original camera color
        playerCamera.backgroundColor = originalCameraBackground;
    }
    
    private void CreateGhostlyEffect()
    {
        GameObject ghostEffect;
        
        if (ghostlyParticlesPrefab != null)
        {
            // use custom prefab if assigned
            ghostEffect = Instantiate(ghostlyParticlesPrefab, transform.position, Quaternion.identity);
            Debug.Log("👻 spawned custom ghostly particle effect");
        }
        else
        {
            // create default particle effect
            ghostEffect = new GameObject("GhostlyEffect");
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
            
            Debug.Log("👻 created default ghostly particle effect");
        }
        
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
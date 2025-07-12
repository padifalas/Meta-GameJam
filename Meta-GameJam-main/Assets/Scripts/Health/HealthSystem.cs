using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [Header("HEALTH SETTINGS")]
    [Space(5)]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthBar; // UI health bar
    public float healthOrbHealAmount = 25f;

    [Header("SIDE EFFECTS")]
    [Space(5)]
    public float sideEffectDuration = 5f;
    private Camera playerCamera;
    private FirstPersonControls playerControls;
    private bool isAffectedBySideEffect = false;

    // Side effect variables
    private Vector3 originalCameraPosition;
    private float originalMoveSpeed;
    private float shakeIntensity = 0.1f;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        // Get references
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

        // Apply side effects based on health state
        if (wasFullHealth && healAmount > 0)
        {
            // Strange side effect when healing at full health
            ApplyStrangeSideEffect();
        }
        else if (healAmount > 0)
        {
            // Minor side effect when healing normally
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

        Debug.Log("Strange side effect: Regenerative Chaos!");
        StartCoroutine(RegenerativeChaos());
    }

    private void ApplyMinorSideEffect()
    {
        if (isAffectedBySideEffect) return;

        Debug.Log("Minor side effect: Healing Disorientation!");
        StartCoroutine(HealingDisorientation());
    }

    private IEnumerator RegenerativeChaos()
    {
        isAffectedBySideEffect = true;

        // Make player character grow larger (scale up)
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.3f;

        // Gradual scaling
        float elapsed = 0;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed);
            yield return null;
        }

        // Stay large for duration
        yield return new WaitForSeconds(sideEffectDuration - 2f);

        // Scale back down
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

        // Apply camera shake and slight movement speed reduction
        if (playerControls != null)
        {
            playerControls.moveSpeed *= 0.8f; // Reduce speed slightly
        }

        float elapsed = 0;
        while (elapsed < sideEffectDuration)
        {
            elapsed += Time.deltaTime;

            // Camera shake effect
            if (playerCamera != null)
            {
                Vector3 shake = Random.insideUnitSphere * shakeIntensity;
                playerCamera.transform.localPosition = originalCameraPosition + shake;
            }

            yield return null;
        }

        // Reset effects
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

    // public method to revive player (useful for respawning)
    public void Revive()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        // re-enable player controls
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script.GetType().Name == "FirstPersonControls" || script.GetType().Name == "FPC2")
            {
                script.enabled = true;
            }
        }

        Debug.Log($"✨ {gameObject.name} revived with full health!");
    }
    private void Die()
    {
        Debug.Log($"💀 {gameObject.name} died!");

        // notify round manager if we're in parking lot round
        RoundManager roundManager = FindObjectOfType<RoundManager>();
        if (roundManager != null)
        {
            // get the player script to pass to round manager
            MonoBehaviour playerScript = GetComponent<FirstPersonControls>();
            if (playerScript == null)
                playerScript = GetComponent<FPC2>();

            if (playerScript != null)
            {
                roundManager.OnPlayerDeath(playerScript);
            }
        }

        // disable player controls
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script.GetType().Name == "FirstPersonControls" || script.GetType().Name == "FPC2")
            {
                script.enabled = false;
            }
        }

        // optional: add death effects here (particle systems, sounds, etc.)
    }

    private void OnTriggerEnter(Collider other)
    {
        // Handle health orb collection
        if (other.CompareTag("HealthOrb"))
        {
            Heal(healthOrbHealAmount);
            Destroy(other.gameObject);
        }
    }
}

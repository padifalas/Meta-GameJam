using UnityEngine;

public class SpeedPowerUp : MonoBehaviour
{
    [Header("Boost Settings")]
    public float duration = 5f;          // How long the boost lasts
    public float respawnTime = 10f;      // Time until powerup reappears

    [Header("Visuals")]
    /*public GameObject visualObject;*/    // The visible powerup model

    private Collider powerupCollider;
    private bool isActive = true;

    private void Start()
    {
        powerupCollider = GetComponent<Collider>();
        // Ensure visual is properly initialized
        /*visualObject.SetActive(isActive);*/
        powerupCollider.enabled = isActive;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive || !other.CompareTag("Player")) return;

        FirstPersonControls playerControls = other.GetComponent<FirstPersonControls>();
        if (playerControls != null)
        {
            // Apply the boost (no multiplier needed - handled in controls)
            playerControls.ApplySpeedBoost(duration);

            // Hide and schedule respawn
            DisablePowerup();
            Invoke(nameof(EnablePowerup), respawnTime);

            // Optional: Add effects here
            // PlaySound();
            // SpawnParticles();
        }

        Debug.Log("Speed Powerup Collected");
    }

    private void DisablePowerup()
    {
        isActive = false;
        /*visualObject.SetActive(false);*/
        powerupCollider.enabled = false;
    }

    private void EnablePowerup()
    {
        isActive = true;
        /*visualObject.SetActive(true);*/
        powerupCollider.enabled = true;

        // Optional: Respawn effects
        // PlayRespawnSound();
    }
}

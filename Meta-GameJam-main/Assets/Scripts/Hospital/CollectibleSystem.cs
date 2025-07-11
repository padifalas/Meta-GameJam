using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleSystem : MonoBehaviour
{
    [Header("collectible settings")]
    [Space(5)]
    public CollectibleType collectibleType = CollectibleType.Pill;
    public float interactRange = 3f;
    public float floatSpeed = 2f;
    public GameObject interactUI; // ui image that shows when player is in range
    
    [Header("visual effects")]
    [Space(5)]
    public ParticleSystem collectEffect;
    public AudioClip collectSound;
    public Light glowLight;
    
    private bool playerInRange = false;
    private bool isCollected = false;
    private MonoBehaviour nearbyPlayer; // changed to MonoBehaviour to handle both script types
    private Vector3 startPosition;
    private AudioSource audioSource;
    
    // for floating animation
    private float bobOffset;
    
    public enum CollectibleType
    {
        Pill,
        Cure
    }
    
    private void Start()
    {
        startPosition = transform.position;
        bobOffset = Random.Range(0f, Mathf.PI * 2f); // random start for bobbing
        
        // setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // setup light based on type
        if (glowLight != null)
        {
            glowLight.color = collectibleType == CollectibleType.Pill ? Color.green : Color.blue;
            glowLight.intensity = 1f;
            glowLight.range = 3f;
        }
        
        // make sure ui starts hidden
        if (interactUI != null)
            interactUI.SetActive(false);
        
        // setup collider
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }
    
    private void Update()
    {
        if (isCollected) return;
        
        // floating animation
        float newY = startPosition.y + Mathf.Sin(Time.time + bobOffset) * 0.3f;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // rotation
        transform.Rotate(Vector3.up * 50f * Time.deltaTime);
        
        // pulsing light
        if (glowLight != null)
        {
            glowLight.intensity = 1f + Mathf.Sin(Time.time * 3f) * 0.3f;
        }
        
        // check for interact input when player is in range
        if (playerInRange && nearbyPlayer != null)
        {
            // the interact method in firstpersoncontrols will handle the raycast
            // but we need to check if player is looking at us
            CheckForInteraction();
        }
    }
    
    private void CheckForInteraction()
    {
        // get the player transform and camera based on script type
        Transform playerTransform = null;
        Transform playerCamera = null;
        
        if (nearbyPlayer.GetType().Name == "FirstPersonControls")
        {
            FirstPersonControls fpc = nearbyPlayer as FirstPersonControls;
            playerTransform = fpc.transform;
            playerCamera = fpc.player; // the camera transform
        }
        else if (nearbyPlayer.GetType().Name == "FPC2")
        {
            // assuming FPC2 has similar structure
            playerTransform = nearbyPlayer.transform;
            // get the player camera from FPC2 - adjust this based on your FPC2 script structure
            playerCamera = nearbyPlayer.transform.Find("player"); // or however you access camera in FPC2
            if (playerCamera == null)
            {
                // fallback - look for camera component
                Camera cam = nearbyPlayer.GetComponentInChildren<Camera>();
                if (cam != null) playerCamera = cam.transform;
            }
        }
        
        if (playerCamera == null) return;
        
        // raycast from player camera to see if theyre looking at this collectible
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactRange))
        {
            if (hit.collider.gameObject == gameObject)
            {
                // player is looking at this collectible
                if (interactUI != null)
                    interactUI.SetActive(true);
            }
            else
            {
                if (interactUI != null)
                    interactUI.SetActive(false);
            }
        }
        else
        {
            if (interactUI != null)
                interactUI.SetActive(false);
        }
    }
    
    public void OnInteracted(MonoBehaviour player)
    {
        if (isCollected) return;
        
        StartCoroutine(CollectItem(player));
    }
    
    private IEnumerator CollectItem(MonoBehaviour player)
    {
        isCollected = true;
        
        // hide ui
        if (interactUI != null)
            interactUI.SetActive(false);
        
        // float towards player
        Vector3 targetPosition = player.transform.position + Vector3.up * 1.5f;
        float journey = 0f;
        Vector3 startPos = transform.position;
        
        while (journey <= 1f)
        {
            journey += Time.deltaTime * floatSpeed;
            transform.position = Vector3.Lerp(startPos, targetPosition, journey);
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, journey);
            yield return null;
        }
        
        // trigger collection effect
        if (collectEffect != null)
        {
            collectEffect.transform.parent = null;
            collectEffect.Play();
            Destroy(collectEffect.gameObject, 2f);
        }
        
        // play sound
        if (collectSound != null && audioSource != null)
            audioSource.PlayOneShot(collectSound);
        
        // apply effect based on type
        EffectManager effectManager = player.GetComponent<EffectManager>();
        if (effectManager != null)
        {
            if (collectibleType == CollectibleType.Pill)
            {
                effectManager.ApplyRandomEffect();
                
                // notify round manager - pass the MonoBehaviour player
                RoundManager roundManager = FindObjectOfType<RoundManager>();
                if (roundManager != null)
                    roundManager.OnCollectibleGathered(player);
            }
            else if (collectibleType == CollectibleType.Cure)
            {
                effectManager.CureAllEffects();
            }
        }
        
        Debug.Log($"collected {collectibleType} - effect applied!");
        
        // destroy the collectible
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MonoBehaviour player = other.GetComponent<FirstPersonControls>();
            if (player == null)
                player = other.GetComponent<MonoBehaviour>(); // this will catch FPC2 or any other script
            
            if (player != null)
            {
                playerInRange = true;
                nearbyPlayer = player;
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            nearbyPlayer = null;
            
            // hide ui when player leaves
            if (interactUI != null)
                interactUI.SetActive(false);
        }
    }
}
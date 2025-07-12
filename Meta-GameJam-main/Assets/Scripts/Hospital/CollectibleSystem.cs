using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleSystem : MonoBehaviour
{
    [Header("collectible settings")]
    [Space(5)]
    public CollectibleType collectibleType = CollectibleType.Pill;
    public float interactRange = 3f;
    public float detectionRadius = 2f; // radius for detecting nearby players
    public float floatSpeed = 2f;
    public GameObject interactUI; // ui image that shows when player is in range
    
    [Header("visual effects")]
    [Space(5)]
    public ParticleSystem collectEffect;
    public AudioClip collectSound;
    public Light glowLight;
    
    private bool playerInRange = false;
    private bool isCollected = false;
    private MonoBehaviour nearbyPlayer;
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
        
        // no need for trigger collider anymore since we use radius detection
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
        
        // radius detection for nearby players
        CheckForNearbyPlayers();
        
        // check for interact input when player is in range
        if (playerInRange && nearbyPlayer != null)
        {
            CheckForInteraction();
        }
    }
    
    private void CheckForNearbyPlayers()
    {
        // find all player gameobjects within detection radius
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        
        bool foundPlayer = false;
        foreach (Collider col in nearbyColliders)
        {
            if (col.CompareTag("Player"))
            {
                MonoBehaviour player = col.GetComponent<FirstPersonControls>();
                if (player == null)
                    player = col.GetComponent<MonoBehaviour>(); // catch FPC2 or other scripts
                
                if (player != null)
                {
                    playerInRange = true;
                    nearbyPlayer = player;
                    foundPlayer = true;
                    break;
                }
            }
        }
        
        // if no player found in radius, clear references
        if (!foundPlayer && playerInRange)
        {
            playerInRange = false;
            nearbyPlayer = null;
            
            // hide ui when player leaves radius
            if (interactUI != null)
                interactUI.SetActive(false);
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
            // get the player camera from FPC2
            FPC2 fpc2 = nearbyPlayer as FPC2;
            playerCamera = fpc2.player; // the camera transform
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
        
        // hide ui immediately
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
                Debug.Log("pill collected - random effect applied!");
                
                // notify round manager
                RoundManager roundManager = FindObjectOfType<RoundManager>();
                if (roundManager != null)
                    roundManager.OnCollectibleGathered(player);
            }
            else if (collectibleType == CollectibleType.Cure)
            {
                effectManager.CureAllEffects();
                Debug.Log("cure collected - all effects reset!");
            }
        }
        
        // destroy the collectible gameobject
        Debug.Log($"destroying {collectibleType} gameobject");
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        // draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // draw interact range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactRange);
        
        // draw a line up to show collectible height
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }
    
    private void OnDrawGizmos()
    {
        // always visible gizmos for better visualization
        if (collectibleType == CollectibleType.Pill)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.blue;
        
        // draw a smaller sphere to show the collectible position
        Gizmos.DrawSphere(transform.position, 0.2f);
        
        // draw detection radius when not selected (more subtle)
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f); // transparent yellow
        Gizmos.DrawSphere(transform.position, detectionRadius);
    }
}
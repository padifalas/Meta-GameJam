using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleSystem : MonoBehaviour
{
    [Header("collectible settings")]
    [Space(5)]
    public CollectibleType collectibleType = CollectibleType.Pill;
    public float detectionRadius = 2f; // radius for showing UI
    public float floatSpeed = 2f;
    
    [Header("split screen ui")]
    [Space(5)]
    public GameObject player1InteractUI; // ui for player 1 side of screen
    public GameObject player2InteractUI; // ui for player 2 side of screen
    
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
        if (player1InteractUI != null)
            player1InteractUI.SetActive(false);
        if (player2InteractUI != null)
            player2InteractUI.SetActive(false);
        
        // setup trigger collider for collection
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
            Debug.Log($"{collectibleType} collider setup as trigger for collection");
        }
        else
        {
            Debug.LogWarning($"no collider found on {collectibleType}! adding sphere collider");
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 0.5f;
        }
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
        
        // radius detection for showing UI
        CheckForNearbyPlayers();
    }
    
    private void CheckForNearbyPlayers()
    {
        // find all player gameobjects within detection radius for UI display
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        
        bool foundPlayer = false;
        foreach (Collider col in nearbyColliders)
        {
            if (col.CompareTag("Player"))
            {
                MonoBehaviour player = col.GetComponent<FirstPersonControls>();
                if (player == null)
                    player = col.GetComponent<FPC2>(); // get FPC2 specifically
                if (player == null)
                    player = col.GetComponent<MonoBehaviour>(); // fallback
                
                if (player != null)
                {
                    playerInRange = true;
                    nearbyPlayer = player;
                    foundPlayer = true;
                    
                    // show appropriate UI based on which player is nearby
                    ShowUIForPlayer(player);
                    
                    break;
                }
            }
        }
        
        // if no player found in radius, clear references and hide UI
        if (!foundPlayer && playerInRange)
        {
            playerInRange = false;
            nearbyPlayer = null;
            
            // hide both UIs when player leaves radius
            HideAllUI();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        
        Debug.Log($"trigger collision detected with: {other.name} (tag: {other.tag})");
        
        if (other.CompareTag("Player"))
        {
            MonoBehaviour player = other.GetComponent<FirstPersonControls>();
            if (player == null)
                player = other.GetComponent<FPC2>(); // get FPC2 specifically
            if (player == null)
                player = other.GetComponent<MonoBehaviour>(); // fallback
            
            if (player != null)
            {
                Debug.Log($"player detected: {player.GetType().Name} - starting collection");
                StartCoroutine(CollectItem(player));
            }
            else
            {
                Debug.LogWarning("player tag detected but no valid player script found!");
            }
        }
    }
    
    private IEnumerator CollectItem(MonoBehaviour player)
    {
        isCollected = true;
        
        Debug.Log($"🎯 collecting {collectibleType}...");
        
        // hide all UI immediately
        HideAllUI();
        
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
        {
            audioSource.PlayOneShot(collectSound);
            Debug.Log($"🔊 playing {collectibleType} collection sound");
        }
        
        // apply effect based on type
        EffectManager effectManager = player.GetComponent<EffectManager>();
        if (effectManager != null)
        {
            if (collectibleType == CollectibleType.Pill)
            {
                effectManager.ApplyRandomEffect();
                Debug.Log("💊 pill collected - random effect applied!");
                
                // notify round manager
                RoundManager roundManager = FindObjectOfType<RoundManager>();
                if (roundManager != null)
                {
                    roundManager.OnCollectibleGathered(player);
                    Debug.Log("📊 round manager notified of pill collection");
                }
            }
            else if (collectibleType == CollectibleType.Cure)
            {
                effectManager.CureAllEffects();
                Debug.Log("🩺 cure collected - all effects reset!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ no effect manager found on player!");
        }
        
        // destroy the collectible gameobject
        Debug.Log($"💥 destroying {collectibleType} gameobject");
        Destroy(gameObject);
    }
    
    private void OnDrawGizmosSelected()
    {
        // draw detection radius (for UI display)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // draw collection trigger area
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = Color.red;
            if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawWireSphere(transform.position, sphere.radius);
            }
            else if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
                Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
        
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
    
    private void ShowUIForPlayer(MonoBehaviour player)
    {
        // hide both UIs first
        HideAllUI();
        
        // determine which player this is and show appropriate UI
        if (IsPlayer1(player))
        {
            if (player1InteractUI != null)
            {
                player1InteractUI.SetActive(true);
                Debug.Log("showing player 1 interact UI");
            }
        }
        else if (IsPlayer2(player))
        {
            if (player2InteractUI != null)
            {
                player2InteractUI.SetActive(true);
                Debug.Log("showing player 2 interact UI");
            }
        }
    }
    
    private void HideAllUI()
    {
        if (player1InteractUI != null)
            player1InteractUI.SetActive(false);
        if (player2InteractUI != null)
            player2InteractUI.SetActive(false);
    }
    
    private bool IsPlayer1(MonoBehaviour player)
    {
        // check if this is player 1 by script type or name
        return player.GetType().Name == "FirstPersonControls" || player.name.Contains("Player1");
    }
    
    private bool IsPlayer2(MonoBehaviour player)
    {
        // check if this is player 2 by script type or name
        return player.GetType().Name == "FPC2" || player.name.Contains("Player2");
    }
}
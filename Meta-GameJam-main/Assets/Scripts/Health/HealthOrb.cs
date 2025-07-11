using UnityEngine;

public class HealthOrb : MonoBehaviour
{
    [Header("HEALTH G.OBJ SETTINGS")]
    [Space(5)]
    public float rotationSpeed = 50f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;
    public float lifetime = 30f; //  disappears after 30 seconds
    
    [Header("VISUAL EFFECTS")]
    [Space(5)]
    public ParticleSystem glowEffect;
    public AudioClip collectSound;
    public Light orbLight;
    
    [HideInInspector]
    public HealthOrbSpawner spawner;
    
    private Vector3 startPosition;
    private AudioSource audioSource;
    private float timer;
    
    private void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        
        // Add audio source if not present
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        //  health light for
        if (orbLight != null)
        {
            orbLight.color = Color.green;
            orbLight.intensity = 1f;
            orbLight.range = 3f;
        }
        
        //  particle effect
        if (glowEffect != null)
        {
            glowEffect.Play();
        }
        
       
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        gameObject.tag = "HealthOrb";
    }
    
    private void Update()
    {
        
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        
        
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
       
        if (orbLight != null)
        {
            orbLight.intensity = 1f + Mathf.Sin(Time.time * 3f) * 0.3f;
        }
        
     
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            DestroyOrb();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag("Player"))
        {
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                
                if (collectSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(collectSound);
                }
                
                // heal the player..... will trigger side effects in HealthSystem)
                playerHealth.Heal(25f);
                
        
                
              
                if (glowEffect != null)
                {
                    glowEffect.Stop();
                    glowEffect.transform.parent = null;
                    Destroy(glowEffect.gameObject, 2f);
                }
                
              
                DestroyOrb();
            }
        }
    }
    
    private void DestroyOrb()
    {
       
        if (spawner != null)
        {
            spawner.OnOrbDestroyed();
        }
        
        Destroy(gameObject);
    }
}
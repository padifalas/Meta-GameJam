using UnityEngine;

public class ShieldOrb : MonoBehaviour
{
    [Header("ORB SETTINGS")]
    [Space(5)]
    public float rotationSpeed = 75f;
    public float bobSpeed = 3f;
    public float bobHeight = 0.3f;
    public float lifetime = 25f; 
    
    [Header("VISUAL EFFECTS")]
    [Space(5)]
    public ParticleSystem shieldEffect;
    public AudioClip collectSound;
    public Light orbLight;
    public Material shieldMaterial; 
    
    [HideInInspector]
    public ShieldOrbSpawner spawner;
    
    private Vector3 startPosition;
    private AudioSource audioSource;
    private float timer;
    private Renderer orbRenderer;
    
    private void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        orbRenderer = GetComponent<Renderer>();
        
       
        // if (audioSource == null)
        // {
        //     audioSource = gameObject.AddComponent<AudioSource>();
        // }
        
        
        if (orbLight != null)
        {
            orbLight.color = Color.cyan;
            orbLight.intensity = 1.2f;
            orbLight.range = 4f;
        }
        
       
        if (shieldMaterial != null && orbRenderer != null)
        {
            orbRenderer.material = shieldMaterial;
        }
        
       
        if (shieldEffect != null)
        {
            shieldEffect.Play();
        }
        
       
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        
        gameObject.tag = "ShieldOrb";
    }
    
    private void Update()
    {
      
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        transform.Rotate(Vector3.right * (rotationSpeed * 0.5f) * Time.deltaTime);
        
        
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
     
        if (orbLight != null)
        {
            orbLight.intensity = 1.2f + Mathf.Sin(Time.time * 4f) * 0.4f;
        }
        
       
        if (shieldMaterial != null && orbRenderer != null)
        {
            Color baseColor = Color.cyan;
            Color pulseColor = Color.white;
            float pulse = (Mathf.Sin(Time.time * 4f) + 1f) * 0.5f;
            orbRenderer.material.color = Color.Lerp(baseColor, pulseColor, pulse * 0.3f);
        }
        
       
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            DestroyOrb();
        }
        
        
        if (timer > lifetime - 3f)
        {
            float fadeAlpha = 1f - ((timer - (lifetime - 3f)) / 3f);
            Color currentColor = orbRenderer.material.color;
            currentColor.a = fadeAlpha;
            orbRenderer.material.color = currentColor;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {
            ShieldSystem playerShield = other.GetComponent<ShieldSystem>();
            if (playerShield != null)
            {
              
                if (collectSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(collectSound);
                }
                
               
                playerShield.ActivateShield();
                
                Debug.Log("Shield orb collected by player!");
                
               
                if (shieldEffect != null)
                {
                    shieldEffect.Stop();
                    shieldEffect.transform.parent = null;
                    
                
                    shieldEffect.Emit(20);
                    Destroy(shieldEffect.gameObject, 2f);
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
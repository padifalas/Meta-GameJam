using UnityEngine;

public class ForestFinishLine : MonoBehaviour
{
    [Header("finish line settings")]
    [Space(5)]
    public GameObject finishEffect; // particle effect when someone wins
    public AudioClip finishSound; // sound when crossing finish line
    
    private RoundManager roundManager;
    private AudioSource audioSource;
    private bool raceFinished = false;
    
    private void Start()
    {
        roundManager = FindObjectOfType<RoundManager>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // make sure this has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("finish line needs a trigger collider!");
        }
        
        Debug.Log("forest finish line initialized");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (raceFinished) return; // race already finished
        
        Debug.Log($"finish line collision with: {other.name} (tag: {other.tag})");
        
        if (other.CompareTag("Player"))
        {
            MonoBehaviour player = other.GetComponent<FirstPersonControls>();
            if (player == null)
                player = other.GetComponent<FPC2>();
            
            if (player != null && roundManager != null)
            {
                raceFinished = true;
                
                string playerName = (player.GetType().Name == "FirstPersonControls") ? "player 1" : "player 2";
                Debug.Log($"🏁 {playerName} crossed the finish line first!");
                
                // play finish sound
                if (finishSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(finishSound);
                }
                
                // spawn finish effect
                if (finishEffect != null)
                {
                    GameObject effect = Instantiate(finishEffect, transform.position, transform.rotation);
                    Destroy(effect, 3f);
                }
                
                // notify round manager
                roundManager.OnFinishLineReached(player);
            }
        }
    }
    
    // reset for new race
    public void ResetFinishLine()
    {
        raceFinished = false;
        Debug.Log("finish line reset for new race");
    }
}
using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    void PlaySound()
    {
        audioSource.PlayOneShot(clickSound);
    }
}


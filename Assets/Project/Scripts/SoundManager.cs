using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    [SerializeField] AudioSource _effectSound;

    public void PlaySound(AudioClip clip)
    {
        _effectSound.PlayOneShot(clip);
    }
}

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Âm thanh")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Nhạc Nền")]
    public AudioClip backgroundMusic;

    [Header("SFX")]
    public AudioClip buttonClick;
    public AudioClip collectCoin;
    public AudioClip chaChing;
    public AudioClip levelUp;
    public AudioClip placeFree;
    public AudioClip quickVegetable;

    [Header("Cài đặt Âm lượng")]
    [Range(0f, 1f)] public float musicVolume = 0.3f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                {
                    position = Input.mousePosition
                };

                var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);

                if (results.Count > 0)
                {
                    foreach (var res in results)
                    {
                        if (res.gameObject.GetComponentInParent<UnityEngine.UI.Button>() != null)
                        {
                            PlayClick();
                            break;
                        }
                    }
                }
            }
        }
    }
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    public void PlayClick() => PlaySFX(buttonClick);
    public void PlayCoin() => PlaySFX(collectCoin);
    public void PlayChaChing() => PlaySFX(chaChing);
    public void PlayLevelUp() => PlaySFX(levelUp);
    public void PlayPlace() => PlaySFX(placeFree);
    public void PlayVegetable() => PlaySFX(quickVegetable);
}
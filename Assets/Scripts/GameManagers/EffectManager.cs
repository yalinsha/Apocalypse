using UnityEngine;
using UnityEngine.UI;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; set; }
    private void Awake()
    {
        Instance = this;
    }
    public Image Red;
    public AudioSource soundEffect;
    public AudioSource bgm;
    int lastBGMIndex = -1;
    public int BGMCount = 2;
    private void Start()
    {
        EventManager.Instance.onSolarStormStart += () =>
        {
            Red.gameObject.SetActive(true);
            PlayStormBGM();
        };
        EventManager.Instance.onSolarStormEnd += () =>
        {
            Red.gameObject.SetActive(false);
            PlayRegularBGM();
        };
        EventManager.Instance.onStartConstruct += (value) => { PlayConstructionSound(); } ;
        var buttons = FindObjectsOfType<Button>(true); // true包含未激活的按钮
        foreach (var button in buttons)
        {
            button.onClick.AddListener(PlayClickSound);
        }
        PlayRegularBGM();
    }
    private void Update()
    {
        if (SolarStormManager.Instance.IsInStorm)
        {
            Camera.main.transform.Translate(new Vector3(Random.value-0.5f, Random.value - 0.5f) / 5);
        }
        else if (!bgm.isPlaying)
        {
            PlayRegularBGM();
        }
    }

    public void PlayClickSound()
    {
        soundEffect.clip = Resources.Load<AudioClip>("SoundEffects/sound1");
        soundEffect.Play();
    }
    public void PlayConstructionSound()
    {
        soundEffect.clip = Resources.Load<AudioClip>("SoundEffects/sound2");
        soundEffect.Play();
    }
    public void PlayRegularBGM()
    {
        int rand = Random.Range(0, BGMCount - 1);
        if (rand >= lastBGMIndex) ++rand;
        lastBGMIndex = rand;
        bgm.clip = Resources.Load<AudioClip>("Musics/BGM" + (rand + 1));
        bgm.Play();
    }
    public void PlayStormBGM()
    {
        bgm.clip = Resources.Load<AudioClip>("Musics/BGMStorm");
        bgm.Play();
    }
}

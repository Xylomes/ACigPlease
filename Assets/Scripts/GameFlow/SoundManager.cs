using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip gameAmbiantMusic;

    [SerializeField] private AudioClip fanSound;
    [SerializeField] private AudioClip fridgeSound;
    [SerializeField] private AudioClip carSound;
    [SerializeField] private AudioClip ratSound;
    [SerializeField] private AudioClip peopleSound;
    [SerializeField] private AudioClip playerBreathSound;
    [SerializeField] private AudioClip openCloset;

    [SerializeField] private AudioClip closeCloset;

    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

    [SerializeField] private AudioMixerGroup musicMixerGroup;

    [SerializeField] private AudioMixerGroup musicMixer;
    [SerializeField] private AudioMixerGroup sfxMixer;

    private Dictionary<SoundType, AudioClip> soundDictionary;
    public List<AudioSource> activeMusicSources = new List<AudioSource>();

    public enum SoundType
    {
        MenuMusic,
        GameMusic,
        Fridge,
        Fan,
        Car,
        Rat,
        People,
        PlayerBreath,
        OpenCloset,
        CloseCloset,
        AmbiantSound
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundDictionary();
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupAudioSources()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    private void InitializeSoundDictionary()
    {
        soundDictionary = new Dictionary<SoundType, AudioClip>
        {
            { SoundType.MenuMusic, menuMusic },
            { SoundType.GameMusic, gameMusic },
            { SoundType.Fridge, fridgeSound },
            { SoundType.Fan, fanSound },
            { SoundType.Car, carSound },
            { SoundType.Rat, ratSound },
            { SoundType.People, peopleSound },
            { SoundType.PlayerBreath, playerBreathSound },
            { SoundType.CloseCloset, closeCloset },
            { SoundType.OpenCloset, openCloset },
            { SoundType.AmbiantSound, gameAmbiantMusic }
        };
    }

    public void PlaySFX(SoundType pSoundType)
    {
        if (soundDictionary.TryGetValue(pSoundType, out AudioClip lClip) && lClip != null)
        {
            sfxSource.PlayOneShot(lClip, sfxVolume);
        }
    }

    public AudioSource PlayMusic(SoundType pMusicType, bool pLoop = true)
    {
        if (soundDictionary.TryGetValue(pMusicType, out AudioClip lClip) && lClip != null)
        {
            AudioSource lNewMusicSource = gameObject.AddComponent<AudioSource>();
            lNewMusicSource.clip = lClip;
            lNewMusicSource.loop = pLoop;
            lNewMusicSource.volume = musicVolume;
            lNewMusicSource.playOnAwake = false;
            lNewMusicSource.outputAudioMixerGroup = musicMixerGroup;
            lNewMusicSource.Play();

            activeMusicSources.Add(lNewMusicSource);
            return lNewMusicSource;
        }
        else
        {
            return null;
        }
    }

    public void StopMusic(AudioSource pMusicSource)
    {
        if (pMusicSource != null && activeMusicSources.Contains(pMusicSource))
        {
            pMusicSource.Stop();
            activeMusicSources.Remove(pMusicSource);
            Destroy(pMusicSource);
        }
    }

    public void StopAllMusic()
    {
        foreach (AudioSource lSource in activeMusicSources)
        {
            if (lSource != null)
            {
                lSource.Stop();
                Destroy(lSource);
            }
        }
        activeMusicSources.Clear();
    }

    public void PauseAllMusic()
    {
        foreach (AudioSource lSource in activeMusicSources)
        {
            if (lSource != null)
            {
                lSource.Pause();
            }
        }
    }

    public void ResumeAllMusic()
    {
        foreach (AudioSource lSource in activeMusicSources)
        {
            if (lSource != null)
            {
                lSource.UnPause();
            }
        }
    }

    public void SetMusicVolume(float pVolume)
    {
        musicVolume = Mathf.Clamp01(pVolume);
        foreach (AudioSource lSource in activeMusicSources)
        {
            if (lSource != null)
            {
                lSource.volume = musicVolume;
            }
        }
    }

    public void SetSFXVolume(float pVolume)
    {
        sfxVolume = Mathf.Clamp01(pVolume);
    }

    public void MuteAll(bool pMute)
    {
        AudioListener.volume = pMute ? 0f : 1f;
    }

    public AudioClip GetClip(SoundType pType)
    {
        soundDictionary.TryGetValue(pType, out AudioClip lClip);
        return lClip;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip gameAmbiantMusic;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip checkpointSound;
    [SerializeField] private AudioClip arrowLaunchSound;
    [SerializeField] private AudioClip clicSound;

    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Audio MixerGroup")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;

    [Header("Audio Mixer")]
    //[SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioMixerGroup musicMixer;
    [SerializeField] private AudioMixerGroup sfxMixer;

    private Dictionary<SoundType, AudioClip> soundDictionary;
    public List<AudioSource> activeMusicSources = new List<AudioSource>();

    public enum SoundType
    {
        MenuMusic,
        GameMusic,
        Jump,
        Dash,
        Death,
        Checkpoint,
        ArrowLaunch,
        Clic,
        AmiantMusic
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
            { SoundType.Jump, jumpSound },
            { SoundType.Dash, dashSound },
            { SoundType.Death, deathSound },
            { SoundType.Checkpoint, checkpointSound },
            { SoundType.ArrowLaunch, arrowLaunchSound },
            { SoundType.Clic, clicSound },
            { SoundType.AmiantMusic, gameAmbiantMusic }
        };
    }

    public void PlaySFX(SoundType soundType)
    {
        if (soundDictionary.TryGetValue(soundType, out AudioClip clip) && clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public AudioSource PlayMusic(SoundType musicType, bool loop = true)
    {
        if (soundDictionary.TryGetValue(musicType, out AudioClip clip) && clip != null)
        {
            AudioSource newMusicSource = gameObject.AddComponent<AudioSource>();
            newMusicSource.clip = clip;
            newMusicSource.loop = loop;
            newMusicSource.volume = musicVolume;
            newMusicSource.playOnAwake = false;
            newMusicSource.outputAudioMixerGroup = musicMixerGroup;
            newMusicSource.Play();

            activeMusicSources.Add(newMusicSource);
            return newMusicSource;
        }
        else
        {
            return null;
        }
    }

    public void StopMusic(AudioSource musicSource)
    {
        if (musicSource != null && activeMusicSources.Contains(musicSource))
        {
            musicSource.Stop();
            activeMusicSources.Remove(musicSource);
            Destroy(musicSource);
        }
    }

    public void StopAllMusic()
    {
        foreach (AudioSource source in activeMusicSources)
        {
            if (source != null)
            {
                source.Stop();
                Destroy(source);
            }
        }
        activeMusicSources.Clear();
    }

    public void PauseAllMusic()
    {
        foreach (AudioSource source in activeMusicSources)
        {
            if (source != null)
            {
                source.Pause();
            }
        }
    }

    public void ResumeAllMusic()
    {
        foreach (AudioSource source in activeMusicSources)
        {
            if (source != null)
            {
                source.UnPause();
            }
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        foreach (AudioSource source in activeMusicSources)
        {
            if (source != null)
            {
                source.volume = musicVolume;
                Debug.Log($"Music volume is {source.volume}");
            }
        }


    }


    //public void SetMusicVolume(AudioSource musicSource, float volume)
    //{
    //    if (musicSource != null)
    //    {
    //        musicSource.volume = Mathf.Clamp01(volume);
    //    }
    //}

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void MuteAll(bool mute)
    {
        AudioListener.volume = mute ? 0f : 1f;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

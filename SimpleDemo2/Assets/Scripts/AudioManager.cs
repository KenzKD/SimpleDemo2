using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public enum GameSfx
{
    Click,
    Start,
    Flip,
    Match,
    Wrong,
    Win
}

[Serializable]
public class SoundSfx
{
    public AudioClip clip;
    public GameSfx gameSfx;
}

public enum GameBgm
{
    Main
}

[Serializable]
public class SoundBgm
{
    public AudioClip clip;
    public GameBgm gameBgm;
}

public class AudioManager : MonoBehaviour
{
    // Constants
    private const string BGM_MIXER_VOLUME_NAME = "bgmMixerVolume";
    private const string SFX_MIXER_VOLUME_NAME = "sfxMixerVolume";

    // Audio mixer for managing audio channels
    public AudioMixer mixer;
    [Range(0, 100)] public int bgmVolume;
    [Range(0, 100)] public int sfxVolume;

    // Audio sources
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource menuSource;

    // Sfx Parent GameObjects
    [SerializeField] private GameObject sfxParent;
    [SerializeField] private GameObject sfxLoopingParent;

    // List of background music and sound effects
    [SerializeField] private List<SoundBgm> bgm;
    [SerializeField] private List<SoundSfx> sfx;

    // Sfx Indices
    private int _sfxIndex;
    private int _sfxLoopingIndex;

    // Sfx Lists
    private List<AudioSource> _sfxLoopingSources;
    private List<AudioSource> _sfxSources;

    // Singleton instance for easy access
    public static AudioManager Instance { get; private set; }


    // Initialize the audio manager
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        _sfxSources = sfxParent.GetComponentsInChildren<AudioSource>().ToList();
        _sfxLoopingSources = sfxLoopingParent.GetComponentsInChildren<AudioSource>().ToList();
    }

    // Start playing the BGM
    private void Start()
    {
        BgmVolume(bgmVolume * 0.01f);
        SfxVolume(sfxVolume * 0.01f);

        bgmSource.Stop();
        PlayBGM(GameBgm.Main);
    }

    private void PlayBGM(GameBgm bgmName)
    {
        SoundBgm bgmClip = bgm.FirstOrDefault(b => b.gameBgm == bgmName);
        if (bgmClip != null && bgmClip.clip != null)
        {
            bgmSource.clip = bgmClip.clip;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM Sound '{bgmName}' not found!");
        }
    }

    public void PlaySfx(GameSfx sfxName, bool sfxAllowOverlap = true, bool randomizePitch = true)
    {
        SoundSfx sfxClip = sfx.FirstOrDefault(s => s.gameSfx == sfxName);
        if (sfxClip != null && sfxClip.clip != null)
        {
            if (!sfxAllowOverlap) StopSfx();

            _sfxSources[_sfxIndex].pitch = randomizePitch ? Random.Range(0.95f, 1.05f) : 1f;

            _sfxSources[_sfxIndex].clip = sfxClip.clip;
            _sfxSources[_sfxIndex].Play();
            _sfxIndex = (_sfxIndex + 1) % _sfxSources.Count;
        }
        else
        {
            Debug.LogWarning($"SFX Sound '{sfxName}' not found!");
        }
    }

    public void PlayMenuSfx(GameSfx sfxName)
    {
        SoundSfx sfxClip = sfx.FirstOrDefault(s => s.gameSfx == sfxName);
        if (sfxClip != null && sfxClip.clip != null)
            menuSource.PlayOneShot(sfxClip.clip);
        else
            Debug.LogWarning($"SFX Sound '{sfxName}' not found!");
    }

    public void PlayLoopingSfx(GameSfx sfxName, bool sfxAllowOverlap = false)
    {
        SoundSfx sfxClip = sfx.FirstOrDefault(s => s.gameSfx == sfxName);
        if (sfxClip != null && sfxClip.clip != null)
        {
            if (!sfxAllowOverlap) StopLoopingSfx();

            _sfxLoopingSources[_sfxLoopingIndex].clip = sfxClip.clip;
            _sfxLoopingSources[_sfxLoopingIndex].Play();
            _sfxLoopingIndex = (_sfxLoopingIndex + 1) % _sfxLoopingSources.Count;
        }
        else
        {
            Debug.LogWarning($"SFX Sound '{sfxName}' not found!");
        }
    }

    private void StopLoopingSfx()
    {
        _sfxLoopingSources.All(s =>
        {
            s.Stop();
            return true;
        });
    }

    private void StopSfx()
    {
        _sfxSources.All(s =>
        {
            s.Stop();
            return true;
        });
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    private void BgmVolume(float volume)
    {
        mixer.SetFloat(BGM_MIXER_VOLUME_NAME, ConvertToDecibels(volume));
    }

    private void SfxVolume(float volume)
    {
        mixer.SetFloat(SFX_MIXER_VOLUME_NAME, ConvertToDecibels(volume));
    }

    private static float ConvertToDecibels(float volume)
    {
        // Ensure volume is not zero to avoid log(0) error
        volume = Mathf.Max(volume, 0.0001f);

        // Convert linear volume to decibels
        return 20f * Mathf.Log10(volume);
    }
}
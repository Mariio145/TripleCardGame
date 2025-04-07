using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UIElements.Experimental;
using System;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")] public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")] public List<AudioClip> musicClips;
    public AudioMixer mixer;
    public List<AudioClip> sfxClips;
    private readonly Dictionary<string, AudioClip> _musicDictionary = new();
    private readonly Dictionary<string, AudioClip> _sfxDictionary = new();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            InitializeDictionaries();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDictionaries()
    {
        foreach (AudioClip clip in musicClips)
        {
            _musicDictionary[clip.name] = clip;
        }

        foreach (AudioClip clip in sfxClips)
        {
            _sfxDictionary[clip.name] = clip;
        }
    }

    public void PlayMusic(string clipName, bool loop = true)
    {

        if (_musicDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"Music clip '{clipName}' not found!");
        }
    }

    public void PlaySfx(string clipName)
    {
        if (_sfxDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
            newSource.PlayOneShot(clip);
            StartCoroutine(DestroySource(newSource, clip.length));
        }
        else
        {
            Debug.LogWarning($"SFX clip '{clipName}' not found!");
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    IEnumerator DestroySource(AudioSource source, float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        Destroy(source);
    }

    public void SetMusicVolume(float volume)
    {
        if(volume < 0.001)
        {
            volume = 0.001f;
        }
        mixer.SetFloat("MusicVolume", MathF.Log10(volume) * 20f);
    }

    public void SetSfxVolume(float volume)
    {
        if(volume < 0.001)
        {
            volume = 0.001f;
        }
        mixer.SetFloat("SFXVolume", MathF.Log10(volume) * 20f);
    }

    public float GetMusicVolume()
    {
        float volume;
        mixer.GetFloat("MusicVolume", out volume);
        return volume;
    }

    public float GetSFXVolume()
    {
        float volume;
        mixer.GetFloat("SFXVolume", out volume);
        return volume;
    }
}
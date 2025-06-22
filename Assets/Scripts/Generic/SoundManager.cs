using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UIElements.Experimental;
using System;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")] public AudioSource musicSource;

    [Header("Audio Clips")] public List<AudioClip> musicClips;
    public List<AudioClip> sfxClips;
    private readonly Dictionary<string, AudioClip> _musicDictionary = new();
    private readonly Dictionary<string, AudioClip> _sfxDictionary = new();

    public float sfxValue = 0.1f;
    public float musicValue = 0.1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.volume = musicValue;
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
            newSource.volume = sfxValue;
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

    public void SetMusicVolume(float value)
    {
        musicValue = value;
        if(musicValue < 0.001)
        {
            musicValue = 0.001f;
        }
        
        if (musicSource != null) musicSource.volume = musicValue;
    }

    public void SetSfxVolume(float value)
    {
        sfxValue = value;
        if(sfxValue < 0.001)
        {
            sfxValue = 0.001f;
        }
    }

    public float GetMusicVolume()
    {
        return musicValue;
    }

    public float GetSfxVolume()
    {
        return sfxValue;
    }
}
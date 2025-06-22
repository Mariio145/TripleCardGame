using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private string _sceneName;
    [SerializeField] private GameObject options;
    [SerializeField] private GameObject credits;
    
    [FormerlySerializedAs("volumeSlider")] [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    

    private void Start()
    {
        UpdateSliders();
        SoundManager.Instance.PlayMusic("MusicaMenu");
    }

    public void ChangeVolume()
    {
        SoundManager.Instance.SetSfxVolume(sfxSlider.value);
        SoundManager.Instance.SetMusicVolume(musicSlider.value);
    }
    
    private void UpdateSliders()
    {
        float sfxValue = SoundManager.Instance.sfxValue;
        float musicValue = SoundManager.Instance.musicValue;
        sfxSlider.value = sfxValue;
        musicSlider.value = musicValue;
    }

    public void SelectGame(string gameName)
    {
        SoundManager.Instance.PlaySfx("Button");
        _sceneName = gameName;
        ChangeScene();
    }

    private void ChangeScene()
    {
        SceneManager.LoadScene(_sceneName);
    }

    public void ShowOptions()
    {
        SoundManager.Instance.PlaySfx("Button");
        options.SetActive(true);
    }
    
    public void HideOptions()
    {
        SoundManager.Instance.PlaySfx("Button");
        options.SetActive(false);
    }
    
    public void ShowCredits()
    {
        SoundManager.Instance.PlaySfx("Button");
        credits.SetActive(true);
    }
    
    public void HideCredits()
    {
        SoundManager.Instance.PlaySfx("Button");
        credits.SetActive(false);
    }
    public void OpenWeb(string url)
    {
        SoundManager.Instance.PlaySfx("Button");
        Application.OpenURL(url);
    }
    
    public void ExitGame()
    {
        SoundManager.Instance.PlaySfx("Button");
        DOTween.KillAll();
        Application.Quit();
    }
}

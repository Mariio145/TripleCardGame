using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private string _sceneName;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        for (int i = 0; i < SceneManager.loadedSceneCount; i++)
        {
            //if (SceneManager.GetSceneByBuildIndex(i) != SceneManager.GetActiveScene())SceneManager.UnloadSceneAsync(i);
        }
    }

    public void SelectGame(string gameName)
    {
        _sceneName = gameName;
        ChangeScene();
        // TODO: StartCoroutine(ChangeSceneAnim());
    }
    
    private IEnumerator ChangeSceneAnim()
    {
        _animator.Play("FadeInWithSelect"); //TODO: Poner nombre de la animacion de cierre
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length + 0.5f);
        ChangeScene();
    }

    private void ChangeScene()
    {
        SceneManager.LoadScene(_sceneName);
    }

    public void ShowOptions()
    {
        
    }

    public void HideOptions()
    {
        
    }

    public void ExitGame()
    {
        DOTween.KillAll();
        Application.Quit();
    }
}

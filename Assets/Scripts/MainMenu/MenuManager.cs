using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private string _sceneName;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SelectGame(string gameName)
    {
        _sceneName = gameName;
        StartCoroutine(ChangeSceneAnim());
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
}

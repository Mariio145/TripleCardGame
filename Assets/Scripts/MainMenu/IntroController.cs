using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoController;

    private void Start()
    {
        SceneManager.LoadScene(1);
        videoController.loopPointReached += EndReached;
    }

    private void EndReached(VideoPlayer vp)
    {
        SceneManager.LoadScene(1);
    }
}

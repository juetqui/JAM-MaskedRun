using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayMainMenu : MonoBehaviour
{
    public void Play()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("julian");
    }
}

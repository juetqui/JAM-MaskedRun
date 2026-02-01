using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayMainMenu : MonoBehaviour
{
    public void Play()
    {
        GameTimeManager.Instance.ResetTime();

        SceneManager.LoadScene("julian");
    }
}

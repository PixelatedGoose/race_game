using UnityEngine;
using UnityEngine.SceneManagement;

public class winmenu : MonoBehaviour
{

    public void RestartGame()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
    
    public void MainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }
}

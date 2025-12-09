using UnityEngine;
using UnityEngine.SceneManagement;

public class winmenu : MonoBehaviour
{

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

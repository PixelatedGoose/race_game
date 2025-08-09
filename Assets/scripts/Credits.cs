using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
    public Selectable target;
    public string prefix;
    public string[] tasks;
    private Text thetext;

    void OnEnable()
    {
        target.Select();

        thetext = GameObject.Find("Canvas/whatiswhat").GetComponent<Text>();
        tasks = new string[] {
            "PixelatedGoose\nproject lead\ngraphical design, ai coding, 3d models, map design, shaders",
            "Vizl87\nlead programmer\ncar controller, game data handling",
            "ThatOneGuy\ncomposer, programmer\nall music and sound design, tutorial",
            "Leobold\nassisting programmer\ncertain menus, racing mechanics",
            "rojp\nassisting programmer\nother help, early 3d models"
        };
    }

    void FixedUpdate()
    {
        Selectable current = EventSystem.current.currentSelectedGameObject?.GetComponent<Selectable>();
        prefix = current.name.Substring(0, 1);
        int index;
        if (int.TryParse(prefix, out index) && index >= 0 && index < tasks.Length)
        {
            thetext.text = tasks[index];
        }
        else
        {
            thetext.text = "";
        }
    }

    public void Back()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
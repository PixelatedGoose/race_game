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
            "PixelatedGoose\nPROJECT LEAD\ngraphical design, ai coding, 3d models, map design, shaders",
            "Vizl87\nLEAD PROGRAMMER\ncar controller, game data handling",
            "ThatOneGuy\nCOMPOSER, PROGRAMMER\nall music and sound design, tutorial",
            "Leobold\nASSISTING PROGRAMMER\ncertain menus, racing mechanics",
            "rojp\nASSISTING PROGRAMMER\nother help, early 3d models"
        };
    }

    void FixedUpdate()
    {
        Selectable current = EventSystem.current.currentSelectedGameObject?.GetComponent<Selectable>();
        prefix = current.name.Substring(0, 1);
        int index;
        //there is no god here
        if (prefix == null || prefix == "")
        {
            current = null;
            target = null;
            thetext.text = "";
        }
        else
        {
            if (int.TryParse(prefix, out index) && index >= 0 && index < tasks.Length)
            {
                thetext.text = tasks[index];
            }
            else
            {
                thetext.text = "";
            }
        }
    }

    public void Back()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
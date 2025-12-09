using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
    int index = 0;

    [Header("UI")]
    [SerializeField] private Text thetext;
    [SerializeField] private Selectable target;
    [SerializeField] private GameObject buttonContainer; // The parent GameObject containing all buttons

    [Header("Data")]
    [TextArea] public string[] tasks;

    [Header("Specific Info Popup")]
    public string[] whatHeDo;
    [SerializeField] private Text popupInfo;

    CarInputActions Controls;

    private void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();

        if (tasks == null || tasks.Length == 0)
        {
            tasks = new string[]
            {
                "PixelatedGoose\nPROJECT LEAD\ngraphical design, ai coding, 3d models, map design, shaders",
                "Vizl87\nLEAD PROGRAMMER\ncar controller, game data handling",
                "ThatOneGuy\nCOMPOSER, PROGRAMMER\nall music and sound design, tutorial",
                "Leobold\nASSISTING PROGRAMMER\ncertain menus, racing mechanics",
                "lamelemon\nPROGRAMMER\nreworking scripts, bug fixing",
                "rojp\nASSISTING PROGRAMMER\nother help, early 3d models",
            };
        }
        whatHeDo = new string[]
        {
            "He is a great man",
            "You could say he controlled the car",
            "Who the fuck is this motherfucker",
            "Bold Leobold but he's not bold",
            "He smoked fentanyl and fixed code",
            "Our code is full of rojping" 
        };
    }

    public void UpdateIconSelection()
    {        
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        target = currentSelected.GetComponent<Selectable>();
        if (!int.TryParse(target.name.Substring(0, 1), out index))
        {
            Debug.LogWarning($"Could not parse index from name: {target.name}");
            return;
        }

        if (index >= 0 && index < tasks.Length)
        {
            thetext.text = tasks[index];
        }
        else
        {
            thetext.text = "";
        }
        if (index >= 0 && index < whatHeDo.Length)
        {
            popupInfo.text = whatHeDo[index];
        }
    }
}
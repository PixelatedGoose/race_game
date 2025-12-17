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
                "PixelatedGoose\nPROJECT LEAD\ngraphical design, ai coding, map design, shaders",
                "Vizl87\nLEAD PROGRAMMER\ncar controller, game data handling",
                "ThatOneGuy\nCOMPOSER, PROGRAMMER\nall music and sound design, tutorial",
                "Leobold\nPROGRAMMER\ncertain menus, racing mechanics",
                "lamelemon\nPROGRAMMER\nrefactoring scripts, bug fixing",
                "rojp\nUNPAID INTERN\nother help, car textures",
            };
        }
        whatHeDo = new string[]
        {
            "He is a great man sometimes known as the goose lord",
            "You could he is a vizlly good programmer",
            "Who the flip is ThatOneGuy?",
            "Bold Leobold but he's not bold but he is bald",
            "He is lemony fresh",
            "Our code contains 0% rojp" 
        };
    }

    void OnDisable()
    {
        Controls.Disable();
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
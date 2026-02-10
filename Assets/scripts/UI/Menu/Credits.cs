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
    [SerializeField] private AudioSource creditsTrack;

    private int musictweenIDstart = -1;
    private int musictweenIDend = -1;

    CarInputActions Controls;

    private void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();

        if (tasks == null || tasks.Length == 0)
        {
            tasks = new string[]
            {
                "PixelatedGoose\nPROJECT LEAD\ngraphical design, map design, shaders",
                "Vizl87\nLEAD PROGRAMMER\ncar controller, game data handling",
                "ThatOneGuy\nCOMPOSER, PROGRAMMER\nmusic and sfx, selection menus",
                "Leobold\nPROGRAMMER\ncertain menus, racing mechanics",
                "lamelemon\nPROGRAMMER\nai coding, refactoring scripts",
                "rojp\nDESIGNER\nother help, car textures",
            };
        }
        whatHeDo = new string[]
        {
            //lol
            "- all pixel art graphics\n- all map design\n- all shaders\n- leaderboard system",
            "- car controller\n- save data system\n- wheel support\n- score multiplier",
            "- all music and sound effects\n- selection menus\n- some graphics assets\n- bug fixing",
            "- multiplayer\n- score system\n- lap and time system",
            "- AI car code\n- user input check system",
            "- car textures\n- ideas",
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

    public void CreditsMusic(bool active)
    {
        switch (active)
        {
            case true:
                if (musictweenIDend != -1)
                    LeanTween.cancel(musictweenIDend);
                creditsTrack.Stop();
                creditsTrack.Play();

                musictweenIDstart = LeanTween.value(creditsTrack.volume, 0.27f, 0.9f)
                .setOnUpdate(val => creditsTrack.volume = val).id;
                break;
            case false:
                if (musictweenIDstart != -1)
                    LeanTween.cancel(musictweenIDstart);
                musictweenIDend = LeanTween.value(creditsTrack.volume, 0.0f, 0.9f)
                .setOnUpdate(val => creditsTrack.volume = val).id;
                break;
        }
    }
}
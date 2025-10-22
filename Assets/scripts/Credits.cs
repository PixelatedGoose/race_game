using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text thetext;          // Assign in Inspector
    [SerializeField] private Selectable target;     // Default selection (assign in Inspector)

    [Header("Data")]
    [TextArea] public string[] tasks;

    private void Awake()
    {
        // Ensure tasks exist even if this object starts inactive
        if (tasks == null || tasks.Length == 0)
        {
            tasks = new string[] {
                "PixelatedGoose\nPROJECT LEAD\ngraphical design, ai coding, 3d models, map design, shaders",
                "Vizl87\nLEAD PROGRAMMER\ncar controller, game data handling",
                "ThatOneGuy\nCOMPOSER, PROGRAMMER\nall music and sound design, tutorial",
                "Leobold\nASSISTING PROGRAMMER\ncertain menus, racing mechanics",
                "rojp\nASSISTING PROGRAMMER\nother help, early 3d models"
            };
        }

        // Try to find the text under this object even if inactive (fallback if not assigned)
        if (thetext == null)
        {
            var t = transform.Find("Credits/whatiswhat");
            if (t != null) thetext = t.GetComponent<Text>();
            if (thetext == null) thetext = GetComponentInChildren<Text>(true);
        }
    }

    private void OnEnable()
    {
        // Set selection when shown
        if (target != null && target.IsActive() && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(target.gameObject);
        }
        UpdateTextFromSelection(); // update immediately
    }

    private void Update()
    {
        UpdateTextFromSelection();
    }

    private void UpdateTextFromSelection()
    {
        if (thetext == null) return;

        GameObject selGO = (EventSystem.current != null) ? EventSystem.current.currentSelectedGameObject : null;

        // Fallback to default target when nothing is selected
        if (selGO == null && target != null && target.IsActive())
            selGO = target.gameObject;

        int index = -1;

        if (selGO != null)
        {
            string name = selGO.name;
            if (!string.IsNullOrEmpty(name))
            {
                char c = name[0];
                if (char.IsDigit(c)) index = c - '0';
            }
        }

        if (index >= 0 && index < tasks.Length)
            thetext.text = tasks[index];
        else
            thetext.text = string.Empty;
    }

    public void Back()
    {
        SceneManager.LoadSceneAsync(0);
    }
}
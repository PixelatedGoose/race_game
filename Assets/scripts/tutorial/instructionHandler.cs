using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[System.Serializable]
public class instructionListClass
{
    public string[] intro;
    public string[] driving;
    public string[] driving_2;
}

public class instructionHandler : MonoBehaviour
{
    private int index = -1;
    private GameObject instructionBox;
    public Text instructionText;

    public TextAsset instructionListJSON;
    public instructionListClass instructionListData;

    public AudioSource[] instructSounds;

    void OnEnable()
    {
        instructionBox = GameObject.Find("instructionBox");
        //SETUPPAA MANUAALISESTI instructionText INSPECTORISSA TAI HEITTÄÄ NULLREFERENCEEXCEPTIONIN!!!

        instructionListJSON = Resources.Load<TextAsset>("instructionList");
        instructionListData = JsonUtility.FromJson<instructionListClass>(instructionListJSON.text);

        instructSounds = new AudioSource[] {
            GameObject.Find("instructionOpen").GetComponent<AudioSource>(),
            GameObject.Find("instructionClose").GetComponent<AudioSource>(),
            GameObject.Find("instructionText").GetComponent<AudioSource>()
        };
        instructSounds = instructSounds.OrderBy(a => a.name).ToArray();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowNextInstructionInCategory("intro", false, false, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ShowNextInstructionInCategory("intro", false, false, 1);
        }
    }

    public void ShowInstruction(string instructText, bool manualSkippingEnabled = false, int anim = 1)
    {
        switch (anim)
        {
            case 0:
                Debug.Log("stays open instruction: " + instructText);
                instructionText.text = instructText;

                instructSounds[1].Play();

                break;
            case 1:
                Debug.Log("open instruction: " + instructText);
                instructionText.text = instructText;

                LeanTween.scaleX(instructionBox, 1.0f, 0.5f).setEaseOutCubic();
                instructSounds[1].Play();

                break;
            case 2:
                Debug.Log("close instruction: " + instructText);

                LeanTween.scaleX(instructionBox, 0.0f, 0.5f).setEaseOutCubic();
                instructSounds[0].Play();

                break;
        }

        // do animation (0 = none (stays open), 1 = open, 2 = close)
        // replace text with the next one

        // if instruction == null, show error message on instruction text instead

        // KÄYTTÖ:
        // ShowInstruction(GetInstruction("intro", 0), true);
    }

    public void ShowNextInstructionInCategory(string category, bool reset = false, bool skippable = false, int anim = 1)
    {
        if (reset == true)
        {
            index = 0;
            Debug.Log("continuing ShowNextInstructionInCategory with reset");
        }
        else
        {
            index++;
            Debug.Log("continuing ShowNextInstructionInCategory without reset");
        }

        if (GetInstruction(category, index).StartsWith("Instruction with id") || GetInstruction(category, index).StartsWith("Category"))
        {
            anim = 2;
            Debug.LogWarning("No valid instruction");
        }

        Debug.Log(index);

        ShowInstruction(GetInstruction(category, index), skippable, anim);
    }

    public string GetInstruction(string category, int id)
    {
        string[] texts;
        switch (category)
        {
            case "intro":
                texts = instructionListData.intro;
                break;
            case "driving":
                texts = instructionListData.driving;
                break;
            case "driving_2":
                texts = instructionListData.driving_2;
                break;
            default:
                Debug.LogError($"Category '{category}' not found");
                return $"Category '{category}' not found";
        }

        if (texts != null && id >= 0 && id < texts.Length)
        {
            return texts[id];
        }
        else
        {
            return $"Instruction with id {id} not found in '{category}'";
        }
    }
}

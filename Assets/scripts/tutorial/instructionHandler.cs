using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[System.Serializable]
public class instructionListClass
{
    public string[] intro;
    public string[] driving;
    public string[] driving_2;
    public string[] controls;
    public string[] drifting;
    public string[] turbe;
    public string[] final;
}

public class instructionHandler : MonoBehaviour
{
    private int index = -1;
    private GameObject instructionBox;
    public Text instructionText;

    public TextAsset instructionListJSON;
    public instructionListClass instructionListData;

    public AudioSource[] instructSounds;

    private bool allowAdvancing;
    //lisää kunnollinen skippaaminen
    //eli allowAdvancing = anim 3
    //varmista että kaikki eri vaihtoehot toimii



    void OnEnable()
    {
        instructionBox = GameObject.Find("instructionBox");
        //SETUPPAA MANUAALISESTI instructionText INSPECTORISSA TAI HEITTÄÄ NULLREFERENCEEXCEPTIONIN!!!

        instructionListJSON = Resources.Load<TextAsset>("instructionList");
        instructionListData = JsonUtility.FromJson<instructionListClass>(instructionListJSON.text);

        instructSounds = new AudioSource[] {
            GameObject.Find("instructionClose").GetComponent<AudioSource>(),
            GameObject.Find("instructionOpen").GetComponent<AudioSource>(),
            GameObject.Find("instructionText").GetComponent<AudioSource>()
        };
        instructSounds = instructSounds.OrderBy(a => a.name).ToArray();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowNextInstructionInCategory("intro", false, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ShowNextInstructionInCategory("intro", false, 1);
        }
    }

    /// <summary>
    /// näyttää tekstiohjeen. toimii omien tekstien näyttämiseen ja osana funktiota ShowNextInstructionInCategory
    /// </summary>
    /// <param name="instructText">teksti, joka näytetään</param>
    /// <param name="anim">animaatio, joka toistetaan, kun ohje näkyy.
    /// [0 = ei mitään, 1 = aukeaa, 2 = sulkeutuu, 3 = pois päältä]</param>
    public void ShowInstruction(string instructText, int anim = 1)
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
            case 3:
                Debug.Log("stays closed instruction: " + instructText);

                break;
        }
    }

    /// <summary>
    /// käsittelee kategoriassa seuraavana olevana tekstiohjeen näyttämisen
    /// </summary>
    /// <param name="category">kategoria, josta etitään</param>
    /// <param name="reset">asettaa indeksin takasi nollaksi</param>
    /// <param name="anim">animaatio, joka toistetaan, kun ohje vaihtuu. viittaa ShowInstructioniin animaatioita varte</param>
    public void ShowNextInstructionInCategory(string category, bool reset = false, int anim = 1)
    {
        string[] texts = GetInstructionListByCategory(category);

        if (reset == true)
        {
            index = 0;
            Debug.Log("continuing ShowNextInstructionInCategory with reset");

            if (anim == 2)
            {
                Debug.LogError("NOTE: don't try to close and reset an instruction at the same time");
            }
            else
            {
                ShowInstruction(GetInstruction(category, index), anim);
            }
        }
        else
        {
            Debug.Log("continuing ShowNextInstructionInCategory without reset");

            if (index == texts.Length - 1)
            {
                anim = 2;
            }

            if (anim == 2)
            {
                Debug.Log("NOT ADVANCING TEXT; CLOSING");
                ShowInstruction(GetInstruction(category, index), 2);
            }
            else
            {
                Debug.Log("ADVANCING TEXT");
                index++;
                ShowInstruction(GetInstruction(category, index), anim);
            }
        }

        if (GetInstruction(category, index).StartsWith("Instruction with id")
        || GetInstruction(category, index).StartsWith("Category"))
        {
            Debug.LogWarning("instruction not found");
        }
    }

    /// <summary>
    /// palauttaa oikean tekstiohjeen kategorian mukaan
    /// </summary>
    /// <param name="category">kategoria, josta etitään</param>
    /// <param name="id">instructionList.json mukaine listan indeksi. jos ei löydy, palauttaa virheen tekstin paikalle</param>
    public string GetInstruction(string category, int id)
    {
        string[] texts = GetInstructionListByCategory(category);

        if (texts != null && id >= 0 && id < texts.Length)
        {
            return texts[id];
        }
        else
        {
            return $"Instruction with id {id} not found in '{category}'";
        }
    }

    //lazy
    private string[] GetInstructionListByCategory(string category)
    {
        switch (category)
        {
            case "intro":
                return instructionListData.intro;
            case "driving":
                return instructionListData.driving;
            case "driving_2":
                return instructionListData.driving_2;
            case "controls":
                return instructionListData.controls;
            case "drifting":
                return instructionListData.drifting;
            case "turbe":
                return instructionListData.turbe;
            case "final":
                return instructionListData.final;
            default:
                Debug.LogError($"Category '{category}' not found");
                return null;
        }
    }
}

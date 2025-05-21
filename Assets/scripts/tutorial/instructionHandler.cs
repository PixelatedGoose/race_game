using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

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
    [Header("kategoriat")]
    public string[] categories;
    [Tooltip("kategorian indeksi")]
    public int idx;
    public string nextCategory;
    public string curCategory; //ei default kategoriaa jotta ei tuu jotai fuck uppeja
    private int index = -1;

    private GameObject instructionBox;

    [Header("instruction box")]
    [Tooltip("aseta manuaalisesti. löytyy instructionBoxin sisältä")]
    public Text instructionText;
    public bool boxOpen = false;

    [Header("data")]
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
            GameObject.Find("instructionClose").GetComponent<AudioSource>(),
            GameObject.Find("instructionOpen").GetComponent<AudioSource>(),
            GameObject.Find("instructionText").GetComponent<AudioSource>()
        };
        instructSounds = instructSounds.OrderBy(a => a.name).ToArray();

        categories = System.Array.ConvertAll(instructionListData
            .GetType()
            .GetFields(),
            field => field.Name
        );
    }

    void Start()
    {
        ShowNextInstructionInCategory("intro", false, 1);
    }



    public Dictionary<string, int> instructionAnimOverrides = new Dictionary<string, int>
    {
        { "intro:2", 3 }, //
        { "driving:3", 2 }, //
        { "driving_2:4", 2 }, //
        { "drifting:1", 2 }, //hasu kohta
        { "final:3", 3 }
    };
    
    /// <summary>
    /// näyttää tekstiohjeen. toimii omien tekstien näyttämiseen ja osana funktiota ShowNextInstructionInCategory
    /// </summary>
    /// <param name="instructText">teksti, joka näytetään</param>
    /// <param name="anim">animaatio, joka toistetaan, kun ohje näkyy.
    /// [0 = ei mitään, 1 = aukeaa, 2 = sulkeutuu, 3 = ei skippaamista]</param>
    public void ShowInstruction(string instructText, int anim = 1)
    {
        switch (anim)
        {
            case 0:
                Debug.Log("stays open instruction: " + instructText);
                boxOpen = true;
                instructionText.text = instructText;

                instructSounds[1].Play();

                break;
            case 1:
                Debug.Log("open instruction: " + instructText);
                instructionText.text = instructText;

                LeanTween.scaleX(instructionBox, 1.0f, 0.5f).setEaseOutCubic();
                boxOpen = true;
                instructSounds[1].Play();

                break;
            case 2:
                Debug.Log("close instruction: " + instructText);

                LeanTween.scaleX(instructionBox, 0.0f, 0.5f).setEaseOutCubic();
                boxOpen = false;
                instructSounds[0].Play();

                break;
            case 3:
                Debug.Log("stays closed instruction: " + instructText);
                instructionText.text = instructText;
                boxOpen = false;

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
        
        if (reset)
        {
            index = 0;

            string key = $"{category}:{index}";
            if (instructionAnimOverrides.ContainsKey(key))
                anim = instructionAnimOverrides[key];

            if (anim == 2)
            {
                Debug.LogError("NOTE: don't try to close and reset an instruction at the same time");
            }
            else
            {
                Debug.Log("ADVANCING TEXT / RESET");
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

                string key = $"{category}:{index}";
                if (instructionAnimOverrides.ContainsKey(key))
                    anim = instructionAnimOverrides[key];

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
        curCategory = category;
        idx = System.Array.FindIndex(categories, category => category == curCategory);

        if (!(idx + 1 >= categories.Length))
        {
            nextCategory = categories[idx + 1];
        }
        else
        {
            Debug.Log("HEWGJUYGR4YRGFKUY4EWGFUYESF8WIF43F8IU3F874497H4");
        }

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

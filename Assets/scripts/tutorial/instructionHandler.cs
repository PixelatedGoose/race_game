using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Logitech;
using UnityEngine.InputSystem.HID;
using NUnit.Framework.Internal.Commands;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.iOS;
using UnityEngine.InputSystem.XInput;

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

    public string[] controller_intro;
    public string[] controller_driving;
    public string[] controller_driving_2;
    public string[] controller_controls;
    public string[] controller_drifting;
    public string[] controller_turbe;
    public string[] controller_final;

    public string[] wheel_intro;
    public string[] wheel_driving;
    public string[] wheel_driving_2;
    public string[] wheel_controls;
    public string[] wheel_drifting;
    public string[] wheel_turbe;
    public string[] wheel_final;
}

public class instructionHandler : MonoBehaviour
{
    [Header("kategoriat")]
    public string[] categories;
    private string[] deviceSpecificCategories;
    [Tooltip("kategorian indeksi")]
    public int idx;
    public string nextCategory;
    public string curCategory; //ei default kategoriaa jotta ei tuu jotai fuck uppeja
    public int index = -1;

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

        InputSystem.onDeviceChange += SetCategoriesByDevice;
        
        // Find the wheel device if present, otherwise fallback
        var wheelDevice = InputSystem.devices.FirstOrDefault(d => d.name == "Logitech G923 Racing Wheel for PlayStation and PC");
        var controllerDevice = InputSystem.devices.FirstOrDefault(d => d is Gamepad);
        if (wheelDevice != null)
            SetCategoriesByDevice(wheelDevice, InputDeviceChange.Added);
        else if (controllerDevice != null)
            SetCategoriesByDevice(controllerDevice, InputDeviceChange.Added);
        else
            SetCategoriesByDevice(InputSystem.devices.FirstOrDefault(), InputDeviceChange.Added);
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= SetCategoriesByDevice;
    }

    private void SetCategoriesByDevice(InputDevice device, InputDeviceChange change)
    {
        if (change != InputDeviceChange.Added && change != InputDeviceChange.Removed)
            return;

        if (change is InputDeviceChange.Added)
        {
            if (device is Keyboard)
                deviceSpecificCategories = categories.Take(7).ToArray();
            else if (device is Gamepad)
                deviceSpecificCategories = categories.Skip(7).Take(7).ToArray();
            else if (device.name == "Logitech G923 Racing Wheel for PlayStation and PC")
                deviceSpecificCategories = categories.Skip(14).Take(7).ToArray();
            else
                deviceSpecificCategories = categories.Take(7).ToArray();

            Debug.Log($"controls added on {device}");
        }
        if (change is InputDeviceChange.Removed)
        {
            var currentDevice = InputSystem.devices.FirstOrDefault();
            
            if (currentDevice is Keyboard)
                deviceSpecificCategories = categories.Take(7).ToArray();
            else if (currentDevice is Gamepad)
                deviceSpecificCategories = categories.Skip(7).Take(7).ToArray();
            else if (currentDevice.name == "Logitech G923 Racing Wheel for PlayStation and PC")
                deviceSpecificCategories = categories.Skip(14).Take(7).ToArray();
            else
                deviceSpecificCategories = categories.Take(7).ToArray();

            Debug.Log($"controls removed on {device}");
            Debug.Log($"controls have swapped to {currentDevice}");
        }

        /* päivitä teksti seuraavasti:
        1. etsi device kategorioista (eli laitetta koskien, KAIKKI)
        2. idx mukaan (se yks kategoria, missä se itse teksti myös on)
        3. tekstin index */
        if (deviceSpecificCategories != null && idx >= 0 && idx < deviceSpecificCategories.Length)
        {
            instructionText.text = GetInstruction(deviceSpecificCategories[idx], index);
        }
        else
        {
            Debug.LogError("Device-specific categories are null or index is out of range.");
            instructionText.text = "Error: Unable to retrieve instruction.";
        }
    }

    void Start()
    {
        //OH MY GOD FUCKING FINALLY!!!!!!21
        ShowNextInstructionInCategory(deviceSpecificCategories[idx], false, 1);
    }


    //pitää poistaa myöhemmin
    public Dictionary<string, int> instructionAnimOverrides = new Dictionary<string, int>
    {
        { "intro:2", 4 }, //
        { "driving:3", 2 }, //
        { "driving_2:3", 2 }, //
        { "drifting:1", 2 }, //hasu kohta
        { "final:2", 4 },
        { "controller_intro:2", 4 }, //
        { "controller_driving:3", 2 }, //
        { "controller_driving_2:3", 2 }, //
        { "controller_drifting:1", 2 }, //hasu kohta
        { "controller_final:2", 4 },
        { "wheel_intro:2", 4 }, //
        { "wheel_driving:3", 2 }, //
        { "wheel_driving_2:3", 2 }, //
        { "wheel_drifting:1", 2 }, //hasu kohta
        { "wheel_final:2", 4 }
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
                //Debug.Log("stays open instruction: " + instructText);
                boxOpen = true;
                instructionText.text = instructText;

                instructSounds[1].Play();

                break;
            case 1:
                //Debug.Log("open instruction: " + instructText);
                instructionText.text = instructText;

                LeanTween.scaleX(instructionBox, 1.0f, 0.5f).setEaseOutCubic();
                boxOpen = true;
                instructSounds[1].Play();

                break;
            case 2:
                //Debug.Log("close instruction: " + instructText);

                LeanTween.scaleX(instructionBox, 0.0f, 0.5f).setEaseOutCubic();
                boxOpen = false;
                instructSounds[0].Play();

                break;
            case 3:
                //Debug.Log("stays closed instruction: " + instructText);
                instructionText.text = instructText;
                boxOpen = false;

                break;
            case 4:
                //Debug.Log("stays closed with sound: " + instructText);
                instructionText.text = instructText;
                boxOpen = false;
                instructSounds[1].Play();

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
            //Debug.Log("continuing ShowNextInstructionInCategory without reset");

            if (index == texts.Length - 1)
            {
                anim = 2;
            }

            if (anim == 2)
            {
                //Debug.Log("NOT ADVANCING TEXT; CLOSING");
                ShowInstruction(GetInstruction(category, index), 2);
            }
            else
            {
                //Debug.Log("ADVANCING TEXT");
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
        Debug.Log($"{curCategory}, {category}, {deviceSpecificCategories[6]}, {idx}, {nextCategory}");
        idx = System.Array.FindIndex(deviceSpecificCategories, category => category == curCategory);

        if (!(idx + 1 >= deviceSpecificCategories.Length))
        {
            nextCategory = deviceSpecificCategories[idx + 1];
        }
        else
        {
            Debug.Log("HEWGJUYGR4YRGFKUY4EWGFUYESF8WIF43F8IU3F874497H4");
        }

        switch (category)
        {
            //hell
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

            case "controller_intro":
                return instructionListData.controller_intro;
            case "controller_driving":
                return instructionListData.controller_driving;
            case "controller_driving_2":
                return instructionListData.controller_driving_2;
            case "controller_controls":
                return instructionListData.controller_controls;
            case "controller_drifting":
                return instructionListData.controller_drifting;
            case "controller_turbe":
                return instructionListData.controller_turbe;
            case "controller_final":
                return instructionListData.controller_final;

            case "wheel_intro":
                return instructionListData.wheel_intro;
            case "wheel_driving":
                return instructionListData.wheel_driving;
            case "wheel_driving_2":
                return instructionListData.wheel_driving_2;
            case "wheel_controls":
                return instructionListData.wheel_controls;
            case "wheel_drifting":
                return instructionListData.wheel_drifting;
            case "wheel_turbe":
                return instructionListData.wheel_turbe;
            case "wheel_final":
                return instructionListData.wheel_final;
            
            default:
                Debug.LogError($"Category '{category}' not found");
                return null;
        }
    }
}

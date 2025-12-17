using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class userDataInput : MonoBehaviour
{
    private TMP_InputField inputField;
    public string userName;
    private TextAsset jsonText;
    private string[] bannedNamesArray;
    private string[] bannedNamePopups;
    [SerializeField] private Text bannedPopup;
    private Button enter;
    private RacerScript racerscript;

    void OnEnable()
    {
        inputField = GameObject.Find("userDataInput").GetComponent<TMP_InputField>();
        enter = GameObject.Find("Enter").GetComponent<Button>();
        jsonText = Resources.Load<TextAsset>("bannedNames");
        bannedNamesArray = JsonUtility.FromJson<BannedNames>(jsonText.text).names.ToArray();
        bannedNamePopups = new string[]
        {
            "Name cannot be empty!",
            "Invalid name!"
        };
        racerscript = FindFirstObjectByType<RacerScript>();
    }
    
    [Serializable]
    public class BannedNames
    {
        public string[] names;
    }

    public void UpdateUserName()
    {
        userName = inputField.text;
        Debug.Log($"edited; new name: {userName}");
    }

    public void CheckForInvalidName()
    {
        if (userName.Length == 0)
        {
            bannedPopup.text = bannedNamePopups[0];
            enter.interactable = false;
        }
        else if (bannedNamesArray.Contains(userName.ToLower()))
        {
            bannedPopup.text = bannedNamePopups[1];
            enter.interactable = false;
        }
        else
        {
            bannedPopup.text = "";
            enter.interactable = true;
        }
    }

    public void SaveDataWithUserName()
    {
        RaceResultCollector.instance.SaveRaceResult(userName);
        racerscript.FinalizeRaceAndSaveData();
    }
}

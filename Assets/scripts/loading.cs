using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class loadText_data
{
    public string[] general;
    public string[] uncommon;
    public string[] rare;
    public string[] obscure;
    public string[] special;
}

public class loading : MonoBehaviour
{
    public Text loadText_text;
    public TextAsset loadTexts;
    public loadText_data textData;

    void OnEnable()
    {
        loadTexts = Resources.Load<TextAsset>("loading/loadTexts");
        textData = JsonUtility.FromJson<loadText_data>(loadTexts.text);
    }
    void Start()
    {
        loadingTexts();
    }

    public void loadingTexts()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        int chance = Random.Range(1, 101);

        string loadTextRarity;

        if (chance <= 2) //2%
        {
            loadTextRarity = "obscure";
        }
        else if (chance <= 8) //6%
        {
            loadTextRarity = "rare";
        }
        else if (chance <= 26) //18%
        {
            loadTextRarity = "uncommon";
        }
        else //74%
        {
            loadTextRarity = "general";
        }

        //lisää tähän erillinen mahollisuus special teksteille
        //overwritaa mahollisuuet käyttämällä .json filen lukua ja kirjotusta (kirjotuksen tarvii aarren special tekstiä varten)

        switch (loadTextRarity)
        {
            case "general":
                Debug.Log("GENERAL");
                loadText_text.text = textData.general[Random.Range(0, textData.general.Length)];

                break;
            case "uncommon":
                Debug.Log("UNCOMMON");
                loadText_text.text = textData.uncommon[Random.Range(0, textData.uncommon.Length)];

                break;
            case "rare":
                Debug.Log("RARE");
                loadText_text.text = textData.rare[Random.Range(0, textData.rare.Length)];

                break;
            case "obscure":
                Debug.Log("OBSCURE");
                loadText_text.text = textData.obscure[Random.Range(0, textData.obscure.Length)];

                break;
        }
    }

    /* public void specialLoadingTexts()
    {
        TextAsset specialTextChancesFile = Resources.Load<TextAsset>("loading/specialTextChances");
        Dictionary<string, float> specialChances = JsonUtility.FromJson<Dictionary<string, float>>(specialTextChancesFile.text);

        Random.InitState(System.DateTime.Now.Millisecond);

        foreach (string text in textData.special)
        {
            int textIndex = System.Array.IndexOf(textData.special, text);
            Debug.Log(textIndex);

            if (specialChances.TryGetValue(text, out float value))
            {
                float sChance = Random.Range(1, 101f);

                if (sChance <= value)
                {
                    Debug.Log("special message chance achieved");
                    loadText_text.text = textData.special[textIndex];
                }
            }
        }
    } */
}

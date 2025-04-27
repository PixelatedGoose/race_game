using UnityEngine;
using UnityEngine.UI;

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

    void Start()
    {
        loadingTexts();
    }

    void loadingTexts()
    {
        TextAsset loadTexts = Resources.Load<TextAsset>("loading/loadTexts");
        loadText_data textData = JsonUtility.FromJson<loadText_data>(loadTexts.text);

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
}

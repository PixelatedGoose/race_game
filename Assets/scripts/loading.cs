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

[System.Serializable]
public class specialTextChances
{
    public float error;
    public float allthetexts;
    public float play1;
    public float play2;
    public float play3;
    public float chance;
    public float i_tried;
    public float outoftime;
    public float juud7;
    public float grass;
    public float reallyspecial;
}

public class loading : MonoBehaviour
{
    public Text loadText_text;
    public TextAsset loadTexts;
    public loadText_data textData;
    public int index = -1;

    void OnEnable()
    {
        loadTexts = Resources.Load<TextAsset>("loading/loadTexts");
        textData = JsonUtility.FromJson<loadText_data>(loadTexts.text);
    }
    void Start()
    {
        // pitäs toimia
        loadingTexts();
        specialLoadingTexts();
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

        switch (loadTextRarity)
        {
            case "general":
                loadText_text.text = textData.general[Random.Range(0, textData.general.Length)];

                break;
            case "uncommon":
                loadText_text.text = textData.uncommon[Random.Range(0, textData.uncommon.Length)];

                break;
            case "rare":
                loadText_text.text = textData.rare[Random.Range(0, textData.rare.Length)];

                break;
            case "obscure":
                loadText_text.text = textData.obscure[Random.Range(0, textData.obscure.Length)];

                break;
        }
    }

    public void specialLoadingTexts()
    {
        TextAsset specialTextChancesFile = Resources.Load<TextAsset>("loading/specialTextChances");
        specialTextChances specialChances = JsonUtility.FromJson<specialTextChances>(specialTextChancesFile.text);

        Random.InitState(System.DateTime.Now.Millisecond);

        var fields = typeof(specialTextChances).GetFields();
        foreach (var field in fields)
        {
            index += 1;

            string key = field.Name;
            float value = (float)field.GetValue(specialChances);

            float sChance = Random.Range(value * 1000, 100000);
            if (sChance <= value * 1000)
            {
                loadText_text.text = textData.special[index];
                Debug.Log(key);

                loadtextbehaviour loadtextbehaviour = gameObject.GetComponent<loadtextbehaviour>();
                loadtextbehaviour.SetSpecialLoadTextBehaviour(key);

                break;
            }
            else
            {
                Debug.Log("special load text not triggered");
            }
        }
    }
}
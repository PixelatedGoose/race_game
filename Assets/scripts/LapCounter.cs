using UnityEngine;
using UnityEngine.UI;

public class LapCounter : MonoBehaviour
{
    public Sprite[] numberSprites; // Sprites for digits 0-9
    public GameObject lapContainer; // Parent GameObject for the lap UI
    public GameObject digitPrefab; // Prefab for a single digit (with an Image component)

    private const int digitCount = 1; // Only one digit for lap
    private Image[] digitImages;
    private string lastLapString = "";
    private RacerScript racer; // Now private, found at runtime

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Find the active car and its RacerScript
        GameObject carParent = GameObject.FindGameObjectWithTag("thisisacar");
        if (carParent != null)
        {
            Transform carChild = carParent.transform.Find("car");
            if (carChild != null)
            {
                racer = carChild.GetComponent<RacerScript>();
            }
        }

        // Instantiate digit object and cache its Image component
        digitImages = new Image[digitCount];
        for (int i = 0; i < digitCount; i++)
        {
            GameObject digitGO = Instantiate(digitPrefab, lapContainer.transform);
            digitImages[i] = digitGO.GetComponent<Image>();
        }
    }

    // Update is called once per frame aaa
    void Update()
    {
        int lap = racer != null ? racer.CurrentLap : 0;
        string lapString = lap.ToString().PadLeft(digitCount, '0');

        // Only update UI if the lap string has changed
        if (lapString != lastLapString)
        {
            UpdateLapUI(lapString, lastLapString);
            lastLapString = lapString;
        }
    }

    void UpdateLapUI(string lapString, string prevLapString)
    {
        for (int i = 0; i < digitCount; i++)
        {
            if (digitImages[i] == null)
                continue;

            char digitChar = lapString[i];
            int digit = digitChar - '0';

            // Only update if this digit has changed
            if (prevLapString.Length != digitCount || prevLapString[i] != digitChar)
            {
                if (digit >= 0 && digit <= 9 && numberSprites != null && numberSprites.Length > digit)
                    digitImages[i].sprite = numberSprites[digit];
            }
        }
    }
}

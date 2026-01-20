using UnityEngine;
using UnityEngine.UI;

public class TimeSprite : MonoBehaviour
{
    [Tooltip("Sprites for digits 0-9.")]
    public Sprite[] numberSprites;
    [Tooltip("Sprites for digits 0-9 (red, for milliseconds).")]
    public Sprite[] redNumberSprites;
    [Tooltip("Parent GameObject for the time UI.")]
    public GameObject timeContainer;
    [Tooltip("Prefab for a single digit (with an Image component).")]
    public GameObject digitPrefab;
    [Tooltip("How many digits to show (should be 4 for SSSd, e.g. 0123 = 12.3s).")]
    public int digitCount = 4;

    private RacerScript racerScript;
    private Image[] digitImages;
    private string lastTimeString = "";

    void Start()
    {
        // Instantiate digit objects once and cache their Image components
        digitImages = new Image[digitCount];
        for (int i = 0; i < digitCount; i++)
        {
            GameObject digitGO = Instantiate(digitPrefab, timeContainer.transform);
            digitImages[i] = digitGO.GetComponent<Image>();
        }
    }

    void Update()
    {
        // Try to fetch the RacerScript if not already cached
        if (racerScript == null)
        {
            if (GameManager.instance == null || GameManager.instance.currentCar == null)
                return;

            racerScript = GameManager.instance.currentCar.GetComponentInChildren<RacerScript>();
            if (racerScript == null)
                return;
        }

        if (!racerScript.racestarted)
            return;

        float time = racerScript.laptime;
        int seconds = Mathf.FloorToInt(time);
        int tenths = Mathf.FloorToInt((time - seconds) * 10f);

        // Combine into a 4-digit string: SSSd (e.g. 0123 for 12.3s)
        string timeString = seconds.ToString().PadLeft(3, '0') + tenths.ToString();

        // Only update UI if the time string has changed
        if (timeString != lastTimeString)
        {
            UpdateTimeUI(timeString);
            lastTimeString = timeString;
        }
    }

    void UpdateTimeUI(string timeString)
    {
        for (int i = 0; i < digitCount; i++)
        {
            if (i >= timeString.Length || digitImages[i] == null)
                continue;

            char digitChar = timeString[i];
            int digit = digitChar - '0';
            if (digit < 0 || digit > 9)
                continue;

            // Use red sprites for the last digit, normal otherwise
            if (i == timeString.Length - 1 && redNumberSprites != null && redNumberSprites.Length >= 10)
                digitImages[i].sprite = redNumberSprites[digit];
            else if (numberSprites != null && numberSprites.Length >= 10)
                digitImages[i].sprite = numberSprites[digit];
        }
    }
}

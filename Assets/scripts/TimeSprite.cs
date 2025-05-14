using UnityEngine;
using UnityEngine.UI;

public class TimeSprite : MonoBehaviour
{
    [Tooltip("Sprites for digits 0-9.")]
    public Sprite[] numberSprites;
    [Tooltip("Parent GameObject for the time UI.")]
    public GameObject timeContainer;
    [Tooltip("Prefab for a single digit (with an Image component).")]
    public GameObject digitPrefab;
    [Tooltip("How many digits to show (should be 4 for SSSd, e.g. 0123 = 12.3s).")]
    public int digitCount = 4;

    private RacerScript racerScript;

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
        UpdateTimeUI(timeString);
    }

    void UpdateTimeUI(string timeString)
    {
        // Clear existing digits
        foreach (Transform child in timeContainer.transform)
            Destroy(child.gameObject);

        // Create UI digits
        foreach (char digitChar in timeString)
        {
            int digit = digitChar - '0';
            if (digit < 0 || digit > 9)
                continue;
            GameObject digitGO = Instantiate(digitPrefab, timeContainer.transform);
            Image digitImage = digitGO.GetComponent<Image>();
            if (digitImage == null || numberSprites == null || numberSprites.Length < 10)
                continue;
            digitImage.sprite = numberSprites[digit];
        }
    }
}

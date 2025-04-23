using UnityEngine;
using UnityEngine.UI;

public class numbercounter : MonoBehaviour
{
    public Sprite[] numberSprites; // Array to hold sprites for digits 0-9
    public GameObject scoreContainer; // Parent GameObject for the score UI
    public GameObject digitPrefab; // Prefab for a single digit (with an Image component)

    void Update()
    {
        // Read the score from the GameManager
        int score = GameManager.instance != null ? GameManager.instance.score : 0;

        // Update the score UI
        UpdateScoreUI(score);
    }

    void UpdateScoreUI(int score)
    {
        // Clear existing digits
        foreach (Transform child in scoreContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Convert score to a string with leading zeros to ensure 7 digits
        string scoreString = score.ToString().PadLeft(7, '0'); // Pads with zeros to make it 7 characters long

        // Create UI digits
        foreach (char digitChar in scoreString)
        {
            int digit = digitChar - '0'; // Convert char to int
            GameObject digitGO = Instantiate(digitPrefab, scoreContainer.transform);
            Image digitImage = digitGO.GetComponent<Image>();
            digitImage.sprite = numberSprites[digit];
        }
    }
}

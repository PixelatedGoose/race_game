using UnityEngine;
using System.IO;

[RequireComponent(typeof(LeaderBoard))]
public class leaderboardClearer : MonoBehaviour
{
    private string filePath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "race_result.json");
        GameManager.Controls.CarControls.Debug_ClearLeaderboard.performed += context => ClearLeaderboardData();
    }
    void OnDisable() => GameManager.Controls.CarControls.Debug_ClearLeaderboard.performed -= context => ClearLeaderboardData();
    void OnDestroy() => GameManager.Controls.CarControls.Debug_ClearLeaderboard.performed -= context => ClearLeaderboardData();

    private void ClearLeaderboardData()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            var LeaderboardManager = GetComponent<LeaderBoard>();
            LeaderboardManager.ClearLeaderboard();
            Debug.Log("Leaderboard data cleared!");
        }
        else Debug.LogWarning("No leaderboard data file found to delete.");
    }
}

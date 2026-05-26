using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class LeaderBoard : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI[] racerNameTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] scoreTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] mapTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] carTexts = new TextMeshProUGUI[5];

    [Header("Settings")]
    [SerializeField] private float updateInterval = 7f;
    [SerializeField] private bool showMapColumn = true;
    [SerializeField] private bool showScoreColumn = true;
    [SerializeField] private bool loadCar = false;
    private List<string> maps;
    [SerializeField] private string mapName = "Shoreline Day";

    private RaceResultHandler resultHandler;
    private Coroutine updateCoroutine;

    void Start()
    {
        //TODO: vaiha coroutine startissa asetukseksi
        maps = GameManager.maps.ToList();
        resultHandler = new RaceResultHandler(Application.persistentDataPath, "race_result.json");
        UpdateLeaderboard();
        updateCoroutine = StartCoroutine(UpdateLeaderboardRoutine());
        GameManager.Controls.CarControls.carskinleft.performed += context => ChangeLeaderboardMap(false);
        GameManager.Controls.CarControls.carskinright.performed += context => ChangeLeaderboardMap(true);
    }
    void OnDestroy()
    {
        if (updateCoroutine != null) StopCoroutine(updateCoroutine);
        GameManager.Controls.CarControls.carskinleft.performed -= context => ChangeLeaderboardMap(false);
        GameManager.Controls.CarControls.carskinright.performed -= context => ChangeLeaderboardMap(true);
    }

    private void ChangeLeaderboardMap(bool forward)
    {
        int current = maps.IndexOf(mapName);
        mapName = forward ? maps[current + 1] : maps[current - 1];
        UpdateLeaderboard();
    }

    private IEnumerator UpdateLeaderboardRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);
            UpdateLeaderboard();
        }
    }

    public void UpdateLeaderboard()
    {
        RaceResultCollection collection = resultHandler.Load();

        if (collection == null || collection.results.Count == 0)
        {
            ClearLeaderboard();
            Debug.LogWarning("leaderboard failed to load data, or no race results exist!");
            return;
        }

        List<RaceResultData> sortedResults = collection.results.OrderBy(r => r.score).Reverse().Take(5).ToList();
        foreach (var r in sortedResults) Debug.Log(r.map);
        for (int i = 0; i < sortedResults.Count; i++) DisplayResult(i, sortedResults[i]);
    }

    private void DisplayResult(int index, RaceResultData result)
    {
        Debug.Log($"{result.racerName} {result.time:F2} {result.score} {result.map} {result.carName}");
        racerNameTexts[index].text = racerNameTexts[index] != null ? result.racerName : "";
        if (showScoreColumn) scoreTexts[index].text = scoreTexts[index] != null ? result.score.ToString() : "";
        if (showMapColumn) mapTexts[index].text = mapTexts[index] != null ? result.map : "";
        if (loadCar) carTexts[index].text = carTexts[index] != null ? result.carName : "";
    }

    private void ClearSlot(int index)
    {
        if (racerNameTexts[index] != null) racerNameTexts[index].text = "---";
        if (scoreTexts[index] != null) scoreTexts[index].text = "---";
        if (mapTexts[index] != null) mapTexts[index].text = "---";
        if (loadCar && carTexts[index] != null) carTexts[index].text = "---";
    }

    public void ClearLeaderboard()
    {
        for (int i = 0; i < 5; i++) ClearSlot(i);
    }
}
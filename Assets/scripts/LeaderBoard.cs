using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LeaderBoard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI[] racerNameTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] timeTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] scoreTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] mapTexts = new TextMeshProUGUI[5];

    [Header("Settings")]
    [SerializeField] private float updateInterval = 7f;
    [SerializeField] private bool showMapColumn = true;
    [SerializeField] private bool showScoreColumn = true;

    private RaceResultHandler resultHandler;
    private Coroutine updateCoroutine;

    void Start()
    {
        resultHandler = new RaceResultHandler(Application.persistentDataPath, "race_result.json");
        UpdateLeaderboard();
        updateCoroutine = StartCoroutine(UpdateLeaderboardRoutine());
    }

    void OnDestroy()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }
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
            return;
        }

        List<RaceResultData> sortedResults = collection.results
            .OrderBy(r => r.time)
            .Take(5)
            .ToList();

        for (int i = 0; i < 5; i++)
        {
            if (i < sortedResults.Count)
            {
                DisplayResult(i, sortedResults[i]);
            }
            else
            {
                ClearSlot(i);
            }
        }
    }

    private void DisplayResult(int index, RaceResultData result)
    {
        if (racerNameTexts[index] != null)
        {
            racerNameTexts[index].text = result.racerName;
        }

        if (timeTexts[index] != null)
        {
            timeTexts[index].text = result.time.ToString("F2") + "s";
        }

        if (showScoreColumn && scoreTexts[index] != null)
        {
            scoreTexts[index].text = result.score.ToString();
        }

        if (showMapColumn && mapTexts[index] != null)
        {
            mapTexts[index].text = result.map;
        }
    }

    private void ClearSlot(int index)
    {
        if (racerNameTexts[index] != null)
            racerNameTexts[index].text = "---";

        if (timeTexts[index] != null)
            timeTexts[index].text = "---";

        if (scoreTexts[index] != null)
            scoreTexts[index].text = "---";

        if (mapTexts[index] != null)
            mapTexts[index].text = "---";
    }

    private void ClearLeaderboard()
    {
        for (int i = 0; i < 5; i++)
        {
            ClearSlot(i);
        }
    }

    private string GetPositionSuffix(int position)
    {
        switch (position)
        {
            case 1: return "1st";
            case 2: return "2nd";
            case 3: return "3rd";
            default: return position + "th";
        }
    }
}
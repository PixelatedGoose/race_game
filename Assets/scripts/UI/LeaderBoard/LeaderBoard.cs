using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

public class LeaderBoard : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI[] racerNameTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] scoreTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] mapTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI[] carTexts = new TextMeshProUGUI[5];
    [SerializeField] private TextMeshProUGUI leaderboardSelectedMap;

    [Header("Settings")]
    [SerializeField] private bool useUpdateCoroutine = false;
    [SerializeField] private float updateInterval = 7f;
    [SerializeField] private bool allowSwitching = false;
    private List<string> maps;
    public string selectedMap;

    private RaceResultHandler resultHandler;
    private Coroutine updateCoroutine;

    void Awake()
    {
        maps = FormattedMapNames();
        selectedMap = maps[0];
        resultHandler = new RaceResultHandler(Application.persistentDataPath, "race_result.json");

        UpdateLeaderboard();
        if (useUpdateCoroutine) updateCoroutine = StartCoroutine(UpdateLeaderboardRoutine());
        if (!allowSwitching && GameManager.maps.Contains(GameManager.SceneSelected))
        {
            selectedMap = FormatMapName(GameManager.SceneSelected, PlayerPrefs.GetInt("SpawnAI") == 1);
            return;
        }
        
        GameManager.Controls.CarControls.carskinleft.performed += context => ChangeLeaderboardMap(false);
        GameManager.Controls.CarControls.carskinright.performed += context => ChangeLeaderboardMap(true);
    }
    void OnDestroy()
    {
        if (updateCoroutine != null) StopCoroutine(updateCoroutine);
        GameManager.Controls.CarControls.carskinleft.performed -= context => ChangeLeaderboardMap(false);
        GameManager.Controls.CarControls.carskinright.performed -= context => ChangeLeaderboardMap(true);
    }

    //olen pahoillani tästä
    public void ChangeLeaderboardMap(bool forward)
    {
        if (!gameObject.activeInHierarchy) return;

        int current = maps.IndexOf(selectedMap);
        int next = forward ? (current + 1) % maps.Count :
        (current - 1) < 0 ? maps.Count - 1 : current - 1 ;
        selectedMap = maps[next];
        UpdateLeaderboard();
    }

    private string FormatMapName(string name, bool appendAI)
    {
        StringBuilder normal = new(name);
        normal.Replace('_', ' ');

        if (!normal.ToString().Contains("night")) normal.Append(" day");
        if (appendAI) normal.Append(" [AI]");

        string formattedName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(normal.ToString());
        return formattedName;
    }
    private List<string> FormattedMapNames()
    {
        List<string> poopoohead = new();
        List<string> baseMaps = GameManager.maps.ToList();
        foreach (string map in baseMaps)
        {
            string formattedBaseName = FormatMapName(map, false);

            StringBuilder ai = new(formattedBaseName);
            ai.Append(" [AI]");
            
            poopoohead.Add(formattedBaseName);
            poopoohead.Add(ai.ToString());
        }
        return poopoohead;
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

        List<RaceResultData> sortedResults = collection.results.Where(r => r.map == selectedMap).OrderByDescending(r => r.score).Take(5).ToList();
        if (leaderboardSelectedMap != null) leaderboardSelectedMap.text = selectedMap;
        int remainder = sortedResults.Count;
        for (int i = 0; i < remainder; i++) DisplayResult(i, sortedResults[i]);
        for (int i = remainder; i < 5; i++) ClearSlot(i);
    }

    private void DisplayResult(int index, RaceResultData result)
    {
        racerNameTexts[index].text = racerNameTexts[index] != null ? result.racerName : "";
        scoreTexts[index].text = scoreTexts[index] != null ? result.score.ToString() : "";
        mapTexts[index].text = mapTexts[index] != null ? result.map : "";
        carTexts[index].text = carTexts[index] != null ? result.carName : "";
    }

    private void ClearSlot(int index)
    {
        if (racerNameTexts[index] != null) racerNameTexts[index].text = "---";
        if (scoreTexts[index] != null) scoreTexts[index].text = "---";
        if (mapTexts[index] != null) mapTexts[index].text = "---";
        if (carTexts[index] != null) carTexts[index].text = "---";
    }

    public void ClearLeaderboard()
    {
        for (int i = 0; i < 5; i++) ClearSlot(i);
    }
}
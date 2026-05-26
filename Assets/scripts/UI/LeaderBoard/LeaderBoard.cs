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
    }
    void Start()
    {
        //TODO: vaiha coroutine startissa asetukseksi
        resultHandler = new RaceResultHandler(Application.persistentDataPath, "race_result.json");
        UpdateLeaderboard();
        if (useUpdateCoroutine) updateCoroutine = StartCoroutine(UpdateLeaderboardRoutine());
        if (!allowSwitching)
        {
            //TODO: implementtaa defaultiksi mappi jossa pelaaja on atm; jos ei löydy, käyttää ekaa indeksiä (Shoreline Day)
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

    private void ChangeLeaderboardMap(bool forward)
    {
        int current = maps.IndexOf(selectedMap);
        //olen pahoillani tästä
        int next = forward ? (current + 1) % maps.Count :
        (current - 1) < 0 ? maps.Count - 1 : current - 1 ;
        selectedMap = maps[next];
        UpdateLeaderboard();
    }

    private List<string> FormattedMapNames()
    {
        List<string> poopoohead = new();
        List<string> baseMaps = GameManager.maps.ToList();
        foreach (string map in baseMaps)
        {
            StringBuilder normal = new(map);
            normal.Replace('_', ' ');
            if (!normal.ToString().Contains("night")) normal.Append(" day");
            string formattedBaseName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(normal.ToString());

            StringBuilder ai = new(formattedBaseName);
            ai.Append(" [AI]");
            
            poopoohead.Add(formattedBaseName);
            poopoohead.Add(ai.ToString());
        }
        foreach (var a in poopoohead) Debug.Log($"hi my name is {a}");
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
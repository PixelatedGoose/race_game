using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RaceResultData
{
    public int score;
    public float time;
    public string map;
    public string dateTime;
    public string racerName;

    public RaceResultData(int score, float time, string map, string racerName)
    {
        this.score = score;
        // Round time to 2 decimal places when creating the object
        this.time = Mathf.Round(time * 100f) / 100f;
        this.map = map;
        this.racerName = racerName;
        this.dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

[Serializable]
public class RaceResultCollection
{
    public List<RaceResultData> results = new List<RaceResultData>();
}
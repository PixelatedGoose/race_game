using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RankThreshold
{
    public float minValue;
    public float maxValue; 
    public string rankName; 
}

public class RankManager : MonoBehaviour
{
    [Header("Rank Thresholds")]
    public List<RankThreshold> rankThresholds; 

    public string GetRank(float value)
    {

        foreach (var threshold in rankThresholds)
        {

            if (value >= threshold.minValue - 0.0001f && value <= threshold.maxValue + 0.0001f)
            {
                return threshold.rankName;
            }

        }
        return "N/A";
    }
}

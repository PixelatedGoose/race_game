using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class GameData
{
    public float besttime;
    public int scored;
    public List<SceneBestTime> bestTimesByMap;

    public GameData()
    {
        this.besttime = 0.0f;
        this.scored = 0;
        bestTimesByMap = new List<SceneBestTime>();
    }

}
[System.Serializable]
public class SceneBestTime
{
    public string sceneName;
    public float bestTime;

    public SceneBestTime(string sceneName, float bestTime)
    {
        this.sceneName = sceneName;
        this.bestTime = bestTime;
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class DatapersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string filename;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    public static DatapersistenceManager instance { get; private set; }


    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Found more than one Data Persistence Manager in the scene");
        }
        instance = this;
    }

    private void Start()
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, filename);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    
    public void NewGame()
    {
        this.gameData = new GameData();
        this.gameData.bestTimesByMap = new List<SceneBestTime>();
    }

    public void LoadGame()
    {
        this.gameData = dataHandler.Load();
        if (this.gameData == null)
        {
            Debug.Log("No save data found. Starting a new game.");
            NewGame();
        }
        else
        {
            Debug.Log("Game loaded successfully.");
            foreach (var scene in gameData.bestTimesByMap)
            {
                Debug.Log($"Loaded best time for scene {scene.sceneName}: {scene.bestTime}");
            }
        }

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);
        }

        foreach (var scene in gameData.bestTimesByMap)
        {
            Debug.Log($"Saving best time for scene {scene.sceneName}: {scene.bestTime}");
        }

        dataHandler.Save(gameData);
        Debug.Log("Game saved successfully.");
    }

    public void UpdateBestTime(string sceneName, float time)
    {
        if (gameData == null || gameData.bestTimesByMap == null)
        {
            return;
        }
        var sceneBestTime = gameData.bestTimesByMap
            .FirstOrDefault(scene => scene.sceneName.ToLower() == sceneName.ToLower());

        if (sceneBestTime != null)
        {
    
            if (sceneBestTime.bestTime == 0 || time < sceneBestTime.bestTime)
            {
                sceneBestTime.bestTime = time;
            }
        }
        else
        {
            gameData.bestTimesByMap.Add(new SceneBestTime(sceneName, time));
        }
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        var dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using System.Collections;
using System.Reflection;
using PurrNet;



public class GameManager : MonoBehaviour, IProvidePrefabInstantiated
{
    public static GameManager instance;
    public static RacerScript racerscript;
    public GameObject CarUI;
    public Action<GameObject> OnCurrentCarChanged;

    [Header("menut")]
    public bool isPaused = false;

    [Header("car selection")]
    public GameObject CurrentCar { get; private set; }
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Transform reverse_playerSpawn;
    [SerializeField] private GameObject[] cars;
    [SerializeField] private PlayerSpawner playerSpawner;

    [Header("scene asetukset")]
    public string sceneSelected;
    private string[] maps = new string[]
    {
        "haukipudas",
        "haukipudas_night",
        "ai_haukipudas",
        "ai_haukipudas_night",
        "tutorial",
        "canyon",
        "canyon_night",
        "ai_canyon",
        "ai_canyon_night"
    };
    
    [Header("auto")]
    public float carSpeed;
    public bool turbeActive = false;
    private Coroutine autoAssignCoroutine;

    public void SetCurrentCar(GameObject spawnedObject)
    {
        CurrentCar = spawnedObject;
        racerscript = CurrentCar != null ? CurrentCar.GetComponentInChildren<RacerScript>() : null;
        OnCurrentCarChanged?.Invoke(CurrentCar);
    }

    void Awake()
    {
        instance = this;
        RegisterPlayerSpawnerProvider();

        sceneSelected = SceneManager.GetActiveScene().name;

        if (sceneSelected == "tutorial") SetCurrentCar(GameObject.Find("REALCAR"));
        else if (maps.Contains(sceneSelected) && cars.Length > 0)
        {
            GameObject selectedCar = cars.FirstOrDefault(c => c.name == PlayerPrefs.GetString("SelectedCar"));
            if (selectedCar == null) selectedCar = cars[0];

            if (playerSpawner != null)
            {
                TrySetSpawnerPlayerPrefab(selectedCar);
            }
            else
            {
                Transform spawn = PlayerPrefs.GetInt("Reverse") == 1 ? reverse_playerSpawn : playerSpawn;
                SetCurrentCar((GameObject)UnityProxy.InstantiateDirectly(selectedCar, spawn.position, spawn.rotation));
            }
        }
    }

    void Start()
    {
        if (CurrentCar != null) return;

        if (TryFindOwnedPlayerCar(out var localCar))
        {
            SetCurrentCar(localCar);
            return;
        }

        autoAssignCoroutine = StartCoroutine(AutoAssignCurrentCarWhenSpawned());
    }

    void OnEnable()
    {
        racerscript = CurrentCar != null ? CurrentCar.GetComponentInChildren<RacerScript>() : FindAnyObjectByType<RacerScript>();
    }

    void OnDisable()
    {
        if (playerSpawner != null)
            playerSpawner.ResetPrefabInstantiatedProvider();

        if (autoAssignCoroutine != null)
        {
            StopCoroutine(autoAssignCoroutine);
            autoAssignCoroutine = null;
        }
    }

    private void RegisterPlayerSpawnerProvider()
    {
        if (playerSpawner == null)
            playerSpawner = FindAnyObjectByType<PlayerSpawner>();

        if (playerSpawner != null)
            playerSpawner.SetPrefabInstantiatedProvider(this);
    }

    public void OnPrefabInstantiated(GameObject prefabInstance, PlayerID player, SceneID scene)
    {
        if (prefabInstance == null || CurrentCar != null)
            return;

        SetCurrentCar(prefabInstance);
    }

    private IEnumerator AutoAssignCurrentCarWhenSpawned()
    {
        while (CurrentCar == null)
        {
            if (TryFindOwnedPlayerCar(out var localCar))
            {
                SetCurrentCar(localCar);
                yield break;
            }

            yield return null;
        }
    }

    private bool TryFindOwnedPlayerCar(out GameObject ownedCar)
    {
        var localController = FindObjectsByType<PlayerCarController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .FirstOrDefault(controller => controller != null && controller.enabled);

        ownedCar = localController != null ? localController.gameObject : null;
        return ownedCar != null;
    }

    private void TrySetSpawnerPlayerPrefab(GameObject selectedCar)
    {
        if (playerSpawner == null || selectedCar == null)
            return;

        var spawnerType = playerSpawner.GetType();

        var setMethod = spawnerType.GetMethod("SetPlayerPrefab", BindingFlags.Instance | BindingFlags.Public);
        if (setMethod != null)
        {
            setMethod.Invoke(playerSpawner, new object[] { selectedCar });
            return;
        }

        var prefabField = spawnerType.GetField("_playerPrefab", BindingFlags.Instance | BindingFlags.NonPublic);
        if (prefabField != null)
            prefabField.SetValue(playerSpawner, selectedCar);
    }
}
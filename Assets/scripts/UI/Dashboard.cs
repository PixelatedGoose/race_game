using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dashboard : MonoBehaviour
{
    private CarInputActions Controls;
    [SerializeField] private bool ShouldDashboardOpen = false;
    private RectTransform rect;
    private Selectable[] dashboardButtons;
    [SerializeField] private Toggle shuffleToggle;
    [SerializeField] private Toggle loopToggle;
    private MusicManager musicManager;

    private Button firstSelectedButton;
    [SerializeField] private Button firstSelectedDashboardButton;
    private CarColors colors;

    private void Awake()
    {
        firstSelectedButton = EventSystem.current.firstSelectedGameObject.GetComponent<Button>();
        rect = GetComponent<RectTransform>();
        dashboardButtons = GetComponentsInChildren<Selectable>();
        musicManager = FindFirstObjectByType<MusicManager>();
        foreach (var b in dashboardButtons) b.interactable = false;
        Controls = new();
        Controls.CarControls.OpenDashboard.performed += ctx => ToggleDashboard();
    }
    private void OnEnable() => Controls.Enable();
    private void OnDisable()
    {
        Controls.CarControls.OpenDashboard.performed -= ctx => ToggleDashboard();
        Controls.Disable();
    }
    private void OnDestroy()
    {
        Controls.CarControls.OpenDashboard.performed -= ctx => ToggleDashboard();
        Controls.Disable();
    }

    private void ToggleDashboard()
    {
        LeanTween.cancel(rect);
        ShouldDashboardOpen = !ShouldDashboardOpen;
        if (ShouldDashboardOpen)
        {
            foreach (var b in dashboardButtons) b.interactable = true;
            firstSelectedDashboardButton.Select();
            LeanTween.value(rect.anchoredPosition.x, -5.0f, 0.4f).setOnUpdate((float val) => { rect.anchoredPosition = new Vector2(val, rect.anchoredPosition.y); }).setEaseInOutCirc().setIgnoreTimeScale(true);
        }
        else
        {
            foreach (var b in dashboardButtons) b.interactable = false;
            firstSelectedButton.Select();
            LeanTween.value(rect.anchoredPosition.x, 320.0f, 0.4f).setOnUpdate((float val) => { rect.anchoredPosition = new Vector2(val, rect.anchoredPosition.y); }).setEaseInOutCirc().setIgnoreTimeScale(true);
        }
    }

    //TODO: toggle shuffle ja yksittäisen trackin looppaus
    public void PlayAndPause()
    {
        musicManager.PauseSong();
    }
    public void PreviousTrack()
    {
        musicManager.PreviousSong();
    }
    public void NextTrack()
    {
        musicManager.NextSong();
    }
    public void Shuffle()
    {
        musicManager.shuffleSong = shuffleToggle.isOn;
    }
    public void LoopSingular()
    {
        musicManager.SetLoop(loopToggle.isOn);
    }
    public void Respawn()
    {
        GameManager.racerscript.FadeGameViewAndRespawn();
    }
    public void Lights()
    {
        if (colors == null) colors = GameManager.CurrentCar.GetComponent<CarColors>();
        colors.ToggleLights();
    }
}
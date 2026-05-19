using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dashboard : MonoBehaviour
{
    private CarInputActions Controls;
    private int dashboardOpenTween = -1;
    private int dashboardCloseTween = -1;
    [SerializeField] private bool ShouldDashboardOpen = false;
    private RectTransform rect;
    private Selectable[] dashboardButtons;
    [SerializeField] private Toggle shuffleToggle;
    [SerializeField] private Toggle loopToggle;
    private MusicManager musicManager;

    private Button firstSelectedButton;
    [SerializeField] private Button firstSelectedDashboardButton;

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
        ShouldDashboardOpen = !ShouldDashboardOpen;
        if (ShouldDashboardOpen)
        {
            LeanTween.cancel(dashboardOpenTween);
            foreach (var b in dashboardButtons) b.interactable = true;
            firstSelectedDashboardButton.Select();
            dashboardOpenTween = LeanTween.value(rect.anchoredPosition.x, -5.0f, 0.4f).setOnUpdate((float val) => { rect.anchoredPosition = new Vector2(val, rect.anchoredPosition.y); }).setEaseInOutCirc().setIgnoreTimeScale(true).id;
        }
        else
        {
            LeanTween.cancel(dashboardCloseTween);
            foreach (var b in dashboardButtons) b.interactable = false;
            firstSelectedButton.Select();
            dashboardCloseTween = LeanTween.value(rect.anchoredPosition.x, 265.0f, 0.4f).setOnUpdate((float val) => { rect.anchoredPosition = new Vector2(val, rect.anchoredPosition.y); }).setEaseInOutCirc().setIgnoreTimeScale(true).id;
        }
    }

    //TODO: toggle shuffle ja yksittäisen trackin looppaus
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
}
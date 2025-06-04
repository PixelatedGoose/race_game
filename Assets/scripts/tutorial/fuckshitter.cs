using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//tän scriptin ainoa tarkotus on tehä jotai kun se halutaan instructionCatInHand.cs:ssä
//en tosiaan haluais pidentää sitä enää lol
public class fuckshitter : MonoBehaviour
{
    CarInputActions Controls;
    private GameObject beginwall;
    private GameObject predriftwall;
    private AudioSource wallMovement_Lower;
    private AudioSource wallMovement_End;

    private musicControlTutorial musicControlTutorial;
    private CarController carController;

    private bool happened;

    private RawImage ui0, ui1, ui2, ui3, ui4, ui5;

    public void SetupFuckShit()
    {
        Controls = new CarInputActions();
        Controls.Enable();

        Controls.CarControls.Drift.performed += StartDriftTrack;
        Controls.CarControls.ui_advance.performed += CheckForOtherAssfuckery;

        beginwall = GameObject.Find("TERRAIN/walls_ground/beginwall");
        predriftwall = GameObject.Find("TERRAIN/walls_ground/course2after_endwall");
        wallMovement_Lower = GameObject.Find("wallMovement_Lower").GetComponent<AudioSource>();
        wallMovement_End = GameObject.Find("wallMovement_End").GetComponent<AudioSource>();
        musicControlTutorial = FindAnyObjectByType<musicControlTutorial>();
        carController = FindAnyObjectByType<CarController>();

        //oh good heavens
        ui0 = GameObject.Find("UIcanvas/uiShowcase/ui0").GetComponent<RawImage>();
        ui1 = GameObject.Find("UIcanvas/uiShowcase/ui1").GetComponent<RawImage>(); //speedometer
        ui2 = GameObject.Find("UIcanvas/uiShowcase/ui2").GetComponent<RawImage>(); //turbe meter
        ui3 = GameObject.Find("UIcanvas/uiShowcase/ui3").GetComponent<RawImage>(); //score
        ui4 = GameObject.Find("UIcanvas/uiShowcase/ui4").GetComponent<RawImage>(); //time
        ui5 = GameObject.Find("UIcanvas/uiShowcase/ui5").GetComponent<RawImage>(); //laps

        foreach (var ui in new[] { ui0, ui1, ui2, ui3, ui4, ui5 }) ui.enabled = false;
    }

    void OnDisable()
    {
        Controls.Disable();
        Controls.CarControls.Drift.performed -= StartDriftTrack;
        Controls.CarControls.ui_advance.performed -= CheckForOtherAssfuckery;
    }

    public void CheckForOtherAssfuckery(InputAction.CallbackContext context)
    {
        if (musicControlTutorial.mainTrack.clip.name == "5_FINAL_TUTORIAL_main")
        {
            instructionHandler instructionHandler = FindAnyObjectByType<instructionHandler>();
            switch (instructionHandler.index)
            {
                case 5:
                    ui0.enabled = true;
                    return;
                case 6:
                    ui0.enabled = false;
                    ui1.enabled = true;
                    return;
                case 7:
                    ui1.enabled = false;
                    ui3.enabled = true;
                    return;
                case 8:
                    ui3.enabled = false;
                    ui4.enabled = true;
                    return;
                case 9:
                    ui4.enabled = false;
                    ui5.enabled = true;
                    return;
                case 10:
                    ui5.enabled = false;
                    ui2.enabled = true;
                    return;
                case 11:
                    ui2.enabled = false;
                    Controls.CarControls.ui_advance.performed -= CheckForOtherAssfuckery;
                    return;
            }
        }
        else
        {
            return;
        }
    }

    public void DoSomeFuckShit(string value)
    {
        switch (value)
        {
            case "begin":
                LeanTween.moveY(beginwall, -5.5f, 2.5f).setEaseLinear().setOnComplete(() =>
                {
                    wallMovement_End.Play();
                    wallMovement_Lower.Stop();
                });
                wallMovement_Lower.Play();
                return;
            case "predriftfadeout":
                if (happened)
                    return;
                happened = true;

                musicControlTutorial.TrackedTween_Start(
                    musicControlTutorial.mainTrack.volume, 0.0f, 5.0f, val =>
                    musicControlTutorial.mainTrack.volume = val);

                wallMovement_Lower.transform.position = new Vector3(
                    predriftwall.transform.position.x,
                    wallMovement_Lower.transform.position.y,
                    predriftwall.transform.position.z);
                wallMovement_End.transform.position = new Vector3(
                    predriftwall.transform.position.x,
                    wallMovement_End.transform.position.y,
                    predriftwall.transform.position.z);

                LeanTween.moveY(predriftwall, -5.5f, 2.5f).setEaseLinear().setOnComplete(() =>
                {
                    wallMovement_End.Play();
                    wallMovement_Lower.Stop();
                });
                wallMovement_Lower.Play();
                return;
            default:
                Debug.LogWarning($"case {value} not found");
                return;
        }
    }

    void StartDriftTrack(InputAction.CallbackContext context)
    {
        if (carController.isDrifting)
        {
            Controls.CarControls.Drift.performed -= StartDriftTrack;
            musicControlTutorial.MusicSections("6_FINAL_TUTORIAL_main");
        }
    }
}
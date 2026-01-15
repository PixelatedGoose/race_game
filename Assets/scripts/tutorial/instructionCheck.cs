using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class instructionCheck : MonoBehaviour
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

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
        //kokeile alusta lähtien, voiko pelaaja driftaa
        Controls.CarControls.Drift.performed += DriftTrack;
        Controls.CarControls.ui_advance.performed += CheckInstructionConditions;

        musicControlTutorial = FindAnyObjectByType<musicControlTutorial>();
        carController = FindAnyObjectByType<CarController>();

        //oh good heavens
        //tälle paskalle on VARMASTI parempi tapa
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
        Controls.CarControls.Drift.performed -= DriftTrack;
        Controls.CarControls.ui_advance.performed -= CheckInstructionConditions;
    }

    /// <summary>
    /// myös edistää tekstiohjeita
    /// </summary>
    public void CheckInstructionConditions(InputAction.CallbackContext context)
    {
        instructionHandler instructionHandler = FindAnyObjectByType<instructionHandler>();

        if (!GameManager.instance.isPaused)
        {
            switch (instructionHandler.boxOpen)
            {
                case true:
                    instructionHandler.ShowNextInstructionInCategory(
                    instructionHandler.curCategory, false, 0);
                    break;

                case false:
                    Debug.Log("no voi vittu");
                    break;
            }
        }

        if (musicControlTutorial.mainTrack.clip.name == "5_FINAL_TUTORIAL_main")
        {
            switch (instructionHandler.index)
            {
                //huomio itelleni: KORJAA TÄMÄ VITUN SYSTEEMI PAREMMAKSI
                //esim näin:
                //ettii 2 ui elementtiä, seuraavan ja edellisen.
                //jos löytää seuraavan, laittaa sen näkyviin
                //jos löytää edellisen, laittaa sen poissa näkyvistä
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
                    return;
            }
        }
        else
        {
            return;
        }
    }

    void DriftTrack(InputAction.CallbackContext context)
    {
        Debug.Log("FUCK YOU");
        if (!carController.canDrift) return;

        musicControlTutorial.EnableDriftFunctions();
        Debug.Log("actually i'll take that back");
        Controls.CarControls.Drift.performed -= DriftTrack;
        //musicControlTutorial.BeginDriftSection();
        //pitää synkronisoida, että se fade ei kuulosta paskalta
    }
}

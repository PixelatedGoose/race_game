using UnityEngine;
using UnityEngine.InputSystem;

public class instructionCheck : MonoBehaviour
{
    CarInputActions Controls;

    private musicControlTutorial musicControlTutorial;
    private CarController carController;


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
        //plot twist: just delete it, they don't care or give a shit regards oneguy vanukas
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

                    //ei kategoriaa - mikään kategoria ei yllä tähän paitsi controls
                    if (instructionHandler.index == 11)
                    {
                        LeanTween.value(musicControlTutorial.mainTrack.volume,
                        0.0f, 4.0f)
                        .setOnUpdate((float val) => {
                            musicControlTutorial.mainTrack.volume = val;
                        });
                    }
                    break;

                case false:
                    Debug.Log("no voi vittu");
                    break;
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
        musicControlTutorial.BeginDriftSection();
        //pitää synkronisoida, että se fade ei kuulosta paskalta
    }
}

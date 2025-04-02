using UnityEngine;

public class backCameraState : MonoBehaviour
{
    CarInputActions Controls;
    
    public GameObject backViewImage;
    public GameObject backViewCamera;
    bool backCamera = false;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
    }

    void Update()
    {
        if (Controls.CarControls.backcam.triggered)
        {
            LeanTween.cancel(backViewImage);
            LeanTween.moveLocalY(backViewImage, backCamera ? 0.0f : -164.0f, 0.4f).setEase(LeanTweenType.easeInOutCirc);
            //backcameran mukaan: jos tosi, laita 0.0f; jos epätosi, laita -164.0f
            backCamera = !backCamera;
        }
    }
}
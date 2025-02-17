using UnityEngine;

public class backCameraState : MonoBehaviour
{

    public GameObject backViewImage;
    public GameObject backViewCamera;

    bool backCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        backCamera = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            LeanTween.cancel(backViewImage);

            if(backCamera == false)
            {
                backCamera = true;
                LeanTween.moveLocalY(backViewImage, -164.0f, 0.4f).setEase(LeanTweenType.easeInOutCirc);
            }

            else
            {
                backCamera = false;
                LeanTween.moveLocalY(backViewImage, 0.0f, 0.4f).setEase(LeanTweenType.easeInOutCirc);
            }
        }
    }
}
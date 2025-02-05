using UnityEngine;

public class backCameraState : MonoBehaviour
{

    public GameObject backViewImage;
    public GameObject backViewCamera;

    bool backCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // backViewImage.SetActive(false);
        // backViewCamera.SetActive(false);
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
                // backViewImage.SetActive(true);
                // backViewCamera.SetActive(true);
                backCamera = true;
                LeanTween.moveLocalY(backViewImage, 380.0f, 0.4f).setEase(LeanTweenType.easeInOutCirc);
            }

            else
            {
                // backViewImage.SetActive(false);
                // backViewCamera.SetActive(false);
                backCamera = false;
                LeanTween.moveLocalY(backViewImage, 675.0f, 0.4f).setEase(LeanTweenType.easeInOutCirc);
            }
        }
    }
}

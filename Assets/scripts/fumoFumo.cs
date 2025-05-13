using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class fumoFumo : MonoBehaviour
{
    CarInputActions Controls;
    
    public GameObject fumoImage;
    int randomNumber;

    void Awake()
    {
        Controls = new CarInputActions();
        Controls.Enable();
    }

    /* void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu" || SceneManager.GetActiveScene().name == "test_mountain" || SceneManager.GetActiveScene().name == "test_mountain_night")
        {
            fumoRandomizer(Random.Range(1, 6));
            fumo();
        }
    }

    void fumo()
    {
        LeanTween.moveLocalX(fumoImage, -546.0f, 2.5f).setOnComplete(() =>
        {
            randomNumber = Random.Range((int)57.5f, (int)498.5f);
            fumoImage.transform.position = new Vector3(1092.0f, randomNumber, default);
            fumo();
            fumoRandomizer(Random.Range(1, 6));
        }).setIgnoreTimeScale(true);
    }

    void fumoRandomizer(int fumoValue = 1)
    {
        Texture2D texture1 = Resources.Load<Texture2D>("fumoTexture/fumo" + fumoValue);

        if (texture1 != null)
        {
            fumoImage.GetComponent<RawImage>().texture = texture1;
        }
        // else
        // {
        //     Debug.LogError("ei löytyny kuvaa tai polkua siihe");
        // }
    }

    void LateUpdate()
    {
        if (Controls.CarControls.pausemenu.triggered && GameManager.instance.isPaused == true)
        {
            fumoRandomizer(Random.Range(1, 6));
        }
    } */

    private void OnDisable()
    {
        Controls.Disable();
    }
}
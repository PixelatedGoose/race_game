using UnityEngine;
using UnityEngine.SceneManagement;

public class cameramove : MonoBehaviour
{
    public Camera freeCamera;

    // Removed unused private fields x, y, and z

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        freeCamera = GameObject.Find("freeCamera").GetComponent<Camera>();

        GameObject.Find("Managers").SetActive(false);
        if (SceneManager.GetActiveScene().name == "test_mountain" || SceneManager.GetActiveScene().name == "haukipudas")
            GameObject.Find("WinCanvas").SetActive(false);
        GameObject.Find("UIcanvas").SetActive(false);
        GameObject.Find("cars").SetActive(false);
        GameObject.Find("musicControl").SetActive(false);
        GameObject.Find("soundControl").SetActive(false);
        if (SceneManager.GetActiveScene().name == "test_mountain_night")
            GameObject.Find("pixel_kuusi").SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var x = freeCamera.transform.position.x;
        var y = freeCamera.transform.position.y;
        var z = freeCamera.transform.position.z;

        var rx = freeCamera.transform.rotation.eulerAngles.x;
        var ry = freeCamera.transform.rotation.eulerAngles.y;
        var rz = freeCamera.transform.rotation.eulerAngles.z; 

        if (Input.GetKey(KeyCode.Alpha7))
        {
            SceneManager.LoadScene("test_mountain");
        }
        if (Input.GetKey(KeyCode.Alpha8))
        {
            SceneManager.LoadScene("test_mountain_night");
        }
        if (Input.GetKey(KeyCode.Alpha9))
        {
            SceneManager.LoadScene("haukipudas");
        }

        if (Input.GetKey(KeyCode.L))
        {
            freeCamera.transform.position += freeCamera.transform.right * 2f;
        }
        if (Input.GetKey(KeyCode.J))
        {
            freeCamera.transform.position -= freeCamera.transform.right * 2f;
        }
        if (Input.GetKey(KeyCode.I))
        {
            freeCamera.transform.position += freeCamera.transform.forward * 2f;
        }
        if (Input.GetKey(KeyCode.K))
        {
            freeCamera.transform.position -= freeCamera.transform.forward * 2f;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            freeCamera.transform.position += freeCamera.transform.up * 2f;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            freeCamera.transform.position -= freeCamera.transform.up * 2f;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            freeCamera.transform.rotation = Quaternion.Euler(rx + 2f, ry, rz);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            freeCamera.transform.rotation = Quaternion.Euler(rx - 2f, ry, rz);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            freeCamera.transform.rotation = Quaternion.Euler(rx, ry + 2f, rz);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            freeCamera.transform.rotation = Quaternion.Euler(rx, ry - 2f, rz);
        }

        if (Input.GetKey(KeyCode.Alpha1))
        {
            freeCamera.transform.position = new Vector3(0f, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            freeCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}

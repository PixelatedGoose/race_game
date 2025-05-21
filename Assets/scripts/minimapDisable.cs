using UnityEngine;

public class minimapDisable : MonoBehaviour
{

    public GameObject MiniMapCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
	{
		MinimapDisable();
	}

    public void MinimapDisable()
	{
		// Toggle minimap with M key
		if (Input.GetKeyDown(KeyCode.M))
		{
			MiniMapCamera.SetActive(!MiniMapCamera.activeSelf);
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionCategories : MonoBehaviour
{
    private List<Transform> CategoryContents;
    private List<Button> CategoryButtonList;
    private GameObject currentlySelected => EventSystem.current.currentSelectedGameObject;
    private int index = 0;
    private Transform currentCategory => CategoryContents[index];
    CarInputActions Controls;

    void Awake()
    {
        //TODO: joku parempi tapa tälle sillä tää on täynnä conditioneita ja paskaa
        CategoryContents = GetComponentsInChildren<Transform>().OrderBy(a => a.name[^1]).Where(i => char.IsDigit(i.name[^1])).ToList();
        CategoryButtonList = GetComponentsInChildren<Button>().OrderBy(a => a.name[^1]).ToList();
        foreach (var a in CategoryContents) if (a != currentCategory) a.gameObject.SetActive(false);
        Controls = new CarInputActions();
    }
    public void OnOpen()
    {
        Controls.CarControls.carskinright.performed += ctx => ChangeCategory(true);
        Controls.CarControls.carskinleft.performed += ctx => ChangeCategory(false);
    }
    public void OnClose()
    {
        Controls.CarControls.carskinright.performed -= ctx => ChangeCategory(true);
        Controls.CarControls.carskinleft.performed -= ctx => ChangeCategory(false);
    }
    void OnEnable()
    {
        Controls.Enable();
        OnOpen();
    }
    void OnDestroy()
    {
        Controls.Disable();
        OnClose();
    }
    private void ChangeCategory(bool change)
    {
        if (index > CategoryButtonList.Count - 1 || index < 0 || !gameObject.activeInHierarchy) return;
        try
        {
            if (change) CategoryButtonList[index + 1].Select();
            else CategoryButtonList[index - 1].Select();
            currentCategory.gameObject.SetActive(true);
            foreach (var a in CategoryContents) if (a != currentCategory) a.gameObject.SetActive(false);
        }
        catch
        {
            Debug.Log("You prehistoric pancake, you pigeon pistachio, you absconded acolyte, you corrupted crisis. You think you can index me, you don't know that I invented indexing");
        }
    }

    public void ChangeCategory()
    {
        int previousButtonIndex = index;
        index = CategoryButtonList.IndexOf(currentlySelected.GetComponent<Button>());
        if (previousButtonIndex == index) return;

        currentCategory.gameObject.SetActive(true);
        foreach (var a in CategoryContents) if (a != currentCategory) a.gameObject.SetActive(false);
    }

    //TODO: parempi tapa ylös liikkumisen tarkistamiseen (OnMove callback todennäkösesti)
    public void SelectNearestOption()
    {
        if (Controls.CarControls.UIMove.ReadValue<Vector2>().y <= 0f) return;

        Selectable nearestOption = currentCategory.GetChild(currentCategory.childCount - 1).GetComponentInChildren<Selectable>();
        nearestOption.Select();
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.InputSystem.Utilities;
using System;

[Serializable] public class ButtonListing
{
    public string keyboard;
    public string controller;
    public string wheel;
}
[Serializable] public class TranslationEntry
{
    public TMP_Text text;
    public ButtonListing buttonListings;
}
public class UINavigationTranslations : MonoBehaviour
{
    //parempi mutta vituttaa että leaderboard menu on ainoa asia joka pakotti mut tekee näin
    [SerializeField] List<TranslationEntry> translationEntries;
    private InputDevice wheel;
    void Awake()
    {
        wheel = InputSystem.devices.FirstOrDefault(d => d.name == "Logitech G923 Racing Wheel for PlayStation and PC");
        CheckInputDevice();
        InputSystem.onEvent.Call((_) => { CheckInputDevice(InputSystem.GetDeviceById(_.deviceId)); });
    }
    //varmaan saa jotenki tän pois
    private void CheckInputDevice()
    {
        if (wheel != null)
        {
            foreach (var i in translationEntries) i.text.text = i.buttonListings.wheel;
            Debug.Log("wheel");
            return;
        }
        else if (Gamepad.current != null)
        {  
            foreach (var i in translationEntries) i.text.text = i.buttonListings.controller;
            Debug.Log("controller");
            return;
        }
        foreach (var i in translationEntries) i.text.text = i.buttonListings.keyboard;
        Debug.Log("no controller or wheel found; defaulting to keyboard");
    }
    private void CheckInputDevice(InputDevice currentDevice)
    {
        if (currentDevice is Keyboard || currentDevice is Mouse) foreach (var i in translationEntries) i.text.text = i.buttonListings.keyboard;
        else if (currentDevice is Gamepad) foreach (var i in translationEntries) i.text.text = i.buttonListings.controller;
        else if (wheel != null) foreach (var i in translationEntries) i.text.text = i.buttonListings.wheel;
    }
}
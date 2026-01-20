using UnityEngine;
using UnityEngine.UI;

public class DefaultButtonSelection : MonoBehaviour
{
    public Button target;

    void OnEnable()
    {
        target.Select();
    }
}
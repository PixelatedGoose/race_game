using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class RainbowColorTexture : MonoBehaviour
{
    private Button buttoning;
    private void Start()
    {
        buttoning = GetComponent<Button>();
        //mitä vittua tää koodi rehellisesti tekee
        LeanTween.value(buttoning.gameObject, new Color(1f, 0f, 0.8509805f, 1f), new Color(0f, 0f, 1f, 1f), 2.0f).setOnUpdate((Color val) =>
        {
            ColorBlock block = new() { normalColor = new(val.r, val.g, val.b, 0f), highlightedColor = val, pressedColor = new(val.r - 0.2f, val.g - 0.2f, val.b - 0.2f, 1f), selectedColor = val, disabledColor = buttoning.colors.disabledColor, colorMultiplier = buttoning.colors.colorMultiplier, fadeDuration = buttoning.colors.fadeDuration };
            buttoning.colors = block;
        })
        .setLoopPingPong();
    }
}
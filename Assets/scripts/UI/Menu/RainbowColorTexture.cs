using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class RainbowColorTexture : MonoBehaviour
{
    private Button buttoning;
    private ColorBlock block;
    private void Start()
    {
        buttoning = GetComponent<Button>();
        block = new() { normalColor = new(0f, 0f, 0f, 0f), disabledColor = buttoning.colors.disabledColor, colorMultiplier = buttoning.colors.colorMultiplier, fadeDuration = buttoning.colors.fadeDuration };

        //mitä vittua tää koodi rehellisesti tekee
        LeanTween.value(buttoning.gameObject, new Color(1f, 0f, 0.8509805f, 1f), new Color(0f, 0f, 1f, 1f), 1.0f).setOnUpdate((Color val) =>
        {
            block.highlightedColor = val;
            block.pressedColor = new(val.r - 0.3f, val.g - 0.15f, val.b - 0.3f, 1f);
            block.selectedColor = val;
            buttoning.colors = block;
        })
        .setLoopPingPong();
    }
}
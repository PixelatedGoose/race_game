using UnityEngine;
using UnityEngine.UI;

public class ScrollingImageEffect : MonoBehaviour
{
    [SerializeField] private RawImage targetImage;
    [SerializeField] private Vector2 scrollSpeed = new Vector2(0.1f, 0f);

    private Vector2 offset;

    private void Update()
    {
        offset += scrollSpeed * Time.deltaTime;
        targetImage.uvRect = new Rect(offset, targetImage.uvRect.size);
    }
}
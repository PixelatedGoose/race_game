using UnityEngine;

public class materialColorer : MonoBehaviour
{
    //LISÄÄ KUUSKIRJAIMINEN HEX KOODI OBJEKTIN NIMEN LOPPUUN
    private string hexColor;
    private string suffix;
    [Tooltip("käytä tätä värin hex koodin laittamisee")]
    public Color previewColor = new Color(1f, 1f, 1f, 1f);

    void Awake()
    {
        suffix = gameObject.name.Substring(gameObject.name.Length - 6, 6);
        hexColor = "#" + suffix;
    }

    void OnEnable()
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material = Instantiate(Resources.Load("scriptMaterials/glowing_trigger-smooth") as Material);

            Color color;
            if (ColorUtility.TryParseHtmlString(hexColor, out color))
            {
                renderer.material.color = color;
            }
        }
    }
}
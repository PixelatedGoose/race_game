using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class test : MonoBehaviour
{

     public Volume volume;
     public UnityEngine.Rendering.Universal.DepthOfField dof;

    void Start()
    {
        InitializeDepthOfField();
    }

    void Update()
    {

    }

    public void BlurredBG()
    {
        dof.focusDistance.value = 0.1f;
    }

    public void ClearBG()
    {
        dof.focusDistance.value = 10f;
    }

    void InitializeDepthOfField()
    {

        var profile = volume.profile;

        if (!profile.TryGet(out dof))
        {
            dof = profile.Add<DepthOfField>(true);
        }
    }
}

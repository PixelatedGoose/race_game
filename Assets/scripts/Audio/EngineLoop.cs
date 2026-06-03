using UnityEngine;

public class EngineLoop : MonoBehaviour
{
    private AudioSource engine;
    private BaseCarController controller;
    [SerializeField] float volumeMin = 0.3f, volumeMax = 0.83f;
    [SerializeField] float pitchMin = 0.3f, pitchMax = 1.5f;
    private void Awake()
    {
        engine = GetComponent<AudioSource>();
        controller = GameManager.CurrentCar.GetComponentInChildren<BaseCarController>();
    }
    private void Update()
    {
        bool carNotControllable = !GameManager.racerscript.racestarted || GameManager.racerscript.raceFinished;
        float fard = controller.CarRb.linearVelocity.sqrMagnitude / (controller.BaseMpsMaxSpeed * controller.BaseMpsMaxSpeed);
        engine.pitch = Mathf.Clamp(fard * (pitchMax - pitchMin) + 0.2f, carNotControllable ? 0f : pitchMin, pitchMax);
        engine.volume = carNotControllable ? 0f : (fard * volumeMax) + 0.2f;
    }
}
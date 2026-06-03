using UnityEngine;

public class EngineLoop : MonoBehaviour
{
    private AudioSource engine;
    private BaseCarController controller;
    [SerializeField] float volumeMin = 0.2f, volumeMax = 0.83f;
    [SerializeField] float pitchMin = 0.2f, pitchMax = 1.5f;
    private void Awake()
    {
        engine = GetComponent<AudioSource>();
        controller = GameManager.CurrentCar.GetComponentInChildren<BaseCarController>();
    }
    private void Update()
    {
        bool carControllable = !GameManager.racerscript.racestarted || GameManager.racerscript.raceFinished;
        float fard = controller.CarRb.linearVelocity.sqrMagnitude / (controller.BaseMpsMaxSpeed * controller.BaseMpsMaxSpeed);
        engine.pitch = Mathf.Clamp(fard * pitchMax, carControllable ? 0f : pitchMin, pitchMax);
        engine.volume = Mathf.Clamp(fard * volumeMax, carControllable ? 0f : volumeMin, volumeMax);
    }
}
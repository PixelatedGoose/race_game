using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float moveSmoothness;
    public float rotSmoothness;


    public Vector3 moveOffset; 
    public Vector3 rotOffset;

    public Transform carTarget;
    
    private Camera Cam;
    private PlayerCarController carController;
    float normalFOV = 60;
    float ZoomFOV = 70;
    public float smoothTime = 0.3f;
    public bool setTutorialValues = false;



    private void Start()
    {
        Cam = GetComponent<Camera>();
        carController = carTarget.GetComponent<PlayerCarController>();
    }

    private void FixedUpdate()
    {
        FollowTarget();
    }

    void FollowTarget()
    {
        HandleMovement();
        HandleRotation();
        CameraFovChanger();
    }

    void HandleMovement()
    {
        Vector3 targetPos = carTarget.TransformPoint(moveOffset); 
    
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);

    }
    void HandleRotation()
    {
        var direction = carTarget.position - transform.position;
        var rotation = Quaternion.LookRotation(direction + rotOffset, Vector3.up);
        transform.rotation = rotation;
    }
    void CameraFovChanger()
    {
        float speed;
        float maxSpeed;
        speed = carController.GetSpeed();
        maxSpeed = carController.GetMaxSpeed();
        float speedRatio = Mathf.Clamp01(speed / maxSpeed);
        float targetFov = Mathf.Lerp(normalFOV, ZoomFOV, speedRatio);
        //how did you know i was listening to lorna shore while writing this? 
        //answer: i didn't, i just know you're a metalhead                           
        Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView, targetFov, Time.deltaTime * moveSmoothness);
    }
}

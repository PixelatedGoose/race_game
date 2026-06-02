using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Extend this class to create a new type of turbo
// Override the Use() function to apply logic when player is using turbo

[RequireComponent(typeof(BaseCarController))]
public abstract class AbstractTurbo : MonoBehaviour
{
    [Tooltip("How strong the turbo is")]
    [SerializeField] protected float strenght = 10f;
    [Tooltip("Maximum amount of turbo")]
    [SerializeField] protected float maxAmount = 100f;
    [Tooltip("Starting % amount of turbo.")]
    [Range(0f, 100f)]
    [SerializeField] protected float startingAmount = 100.0f;
    [Tooltip("How much turbo is consumed per second")]
    [SerializeField] protected float consumeRate = 20f;
    [Tooltip("How much turbo is regenerated per second")]
    [SerializeField] protected float regenerationRate = 20f;
    [Tooltip("How long to wait to start recharging turbo")]
    [SerializeField] protected float waitTime = 1f;
    [SerializeField] protected float maxSpeedMultiplier = 1.3f;
    protected WaitForSeconds waiter; // Waiter! Waiter! May I ask for seconds?
    private float _amount;
    public float Amount
    {
        get { return _amount; } 
        protected set
        {
            _amount = value;
            TurboBar.fillAmount = value / maxAmount;
        }
    }
    protected BaseCarController carController;
    protected Coroutine turboCoroutine;
    protected Image TurboBar;

    // Runs the specific turbo type's logic
    protected abstract void Use();

    protected virtual void Awake()
    {
        carController = GetComponent<BaseCarController>();
        TurboBar = GameManager.instance.CarUI.transform.Find("TurbeDisplay").GetComponentInChildren<Image>();

        Amount = startingAmount / 100f * maxAmount;
        waiter = new WaitForSeconds(waitTime);
    }

    public virtual void Activate()
    {
        if (Amount <= 0 || carController.IsTurboActive) return;

        carController.IsTurboActive = true;
        carController.MaxSpeed *= maxSpeedMultiplier;

        if (turboCoroutine != null) StopCoroutine(turboCoroutine);
        turboCoroutine = StartCoroutine(Consume());
    }

    public virtual void Stop()
    {
        carController.IsTurboActive = false;
        carController.DecayMaxSpeed(0);

        if (turboCoroutine != null) StopCoroutine(turboCoroutine);
        turboCoroutine = StartCoroutine(Regenerate());
    }

    protected virtual IEnumerator Consume()
    {
        while (Amount > 0)
        {
            Amount -= consumeRate * Time.deltaTime;
            Use();
            yield return null;
        }

        Amount = 0;
        Stop();
        yield break;
    }

    protected virtual IEnumerator Regenerate()
    {
        yield return waiter;

        while (Amount < maxAmount)
        {
            Amount += regenerationRate * Time.deltaTime;
            yield return null;
        }
        
        Amount = maxAmount;
        yield break;
    }
}

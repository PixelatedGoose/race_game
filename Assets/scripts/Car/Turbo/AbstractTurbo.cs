using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Extend this class to create a new type of turbo
// Override the Use() function to apply logic when player is using turbo

[RequireComponent(typeof(BaseCarController))]
public abstract class AbstractTurbo : MonoBehaviour
{
    [Tooltip("How strong the turbo is")]
    [SerializeField] protected float strength = 10f;
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
    protected WaitForSeconds waiter; // Waiter! Waiter! May I ask for seconds?
    protected float amount;
    protected BaseCarController carController;
    protected Coroutine turboCoroutine;
    protected bool consuming;
    protected Image TurboBar;

    // Used for running the specific turbo's logic when the player wants to use turbo.
    protected abstract void Use();

    protected virtual void Awake()
    {
        carController = GetComponent<BaseCarController>();
        TurboBar = GameManager.instance.CarUI.transform.Find("TurbeDisplay").GetComponentInChildren<Image>();

        amount = startingAmount / 100f * maxAmount;
        waiter = new WaitForSeconds(waitTime);
    }

    public virtual void Activate()
    {
        if (amount <= 0 || carController.IsTurboActive) return;

        carController.IsTurboActive = true;

        if (turboCoroutine != null) StopCoroutine(turboCoroutine);
        turboCoroutine = StartCoroutine(Consume());
    }

    public virtual void Stop()
    {
        carController.IsTurboActive = false;
        if (turboCoroutine != null) StopCoroutine(turboCoroutine);
        turboCoroutine = StartCoroutine(Regenerate());
    }

    protected virtual IEnumerator Consume()
    {
        while (amount > 0)
        {
            if (amount > 0.1f) amount -= consumeRate * Time.deltaTime;
            else amount = 0;
            TurboBar.fillAmount = amount / maxAmount;
            Use();
            yield return null;
        }

        Stop();
        yield break;
    }

    protected virtual IEnumerator Regenerate()
    {
        yield return waiter;

        while (amount < maxAmount)
        {
            if (amount < maxAmount - 0.1f) amount += regenerationRate * Time.deltaTime;
            else amount = maxAmount;
            TurboBar.fillAmount = amount / maxAmount;
            yield return null;
        }
        
        yield break;
    }
}

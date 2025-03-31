using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HiderController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 120f;

    [Header("Transformation Settings")]
    [SerializeField] private int currentFormIndex = 0; // Tracks the current form
    [SerializeField] private float transformCooldown = 5f;
    [SerializeField] private float transformDuration = 10f; // Time before reverting
    private float lastTransformTime = -Mathf.Infinity;

    private Transform[] formObjects;
    private Coroutine revertCoroutine;

    private Vector3 previousPosition;
    private Vector3 velocity;

    void Start()
    {

        // Store all child objects (forms)
        int childCount = transform.childCount;
        formObjects = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            formObjects[i] = transform.GetChild(i);
        }

        ActivateForm(currentFormIndex);
    }

    private void Update()
    {
        // Calculate velocity manually
        velocity = (transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;
    }

    public void Move(float direction)
    {
        transform.position += transform.forward * moveSpeed * direction * Time.deltaTime;
    }

    public void Turn(float direction)
    {
        transform.Rotate(Vector3.up, rotationSpeed * direction * Time.deltaTime);
    }

    public void TransformHider(int formIndex)
    {
        if (Time.time - lastTransformTime < transformCooldown || formIndex == currentFormIndex)
            return;

        if (formIndex >= 0 && formIndex < formObjects.Length)
        {
            ActivateForm(formIndex);
            lastTransformTime = Time.time;

            // Start revert timer if transformed into another form
            if (formIndex != 0)
            {
                if (revertCoroutine != null)
                    StopCoroutine(revertCoroutine);

                revertCoroutine = StartCoroutine(RevertToOriginalAfterDelay(transformDuration));
            }
        }
    }

    private void ActivateForm(int formIndex)
    {
        for (int i = 0; i < formObjects.Length; i++)
        {
            formObjects[i].gameObject.SetActive(i == formIndex);
        }
        currentFormIndex = formIndex;
    }

    private IEnumerator RevertToOriginalAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TransformHider(0); // Revert back to original form
    }

    public float GetTransformCooldownRemaining()
    {
        return Mathf.Max(0, transformCooldown - (Time.time - lastTransformTime));
    }

    public int GetCurrentFormIndex()
    {
        return currentFormIndex;
    }

    public float GetCurrentTransformationDurationRemaining()
    {
        if (currentFormIndex == 0) return 0f; // Not transformed
        return Mathf.Max(0, transformCooldown - (Time.time - lastTransformTime));
    }

    // Getter for velocity so RaycastSensor can read it
    public Vector3 GetVelocity()
    {
        return velocity;
    }
}

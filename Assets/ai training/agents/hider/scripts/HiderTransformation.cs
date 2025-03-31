using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiderTransformation : MonoBehaviour
{
    [Header("Transformation Settings")]
    [SerializeField] private float transformationDuration = 5f;  // Time the Hider stays transformed
    [SerializeField] private float transformationCooldown = 3f;  // Time before it can transform again
    [SerializeField] private float transformationTimeRemaining = 0f;
    [SerializeField] private float cooldownTimeRemaining = 0f;

    private List<GameObject> transformationForms = new List<GameObject>();
    [SerializeField] private int currentFormIndex = 0;  // 0 is the original Hider form

    void Start()
    {
        // Populate the list with all child objects
        foreach (Transform child in transform)
        {
            transformationForms.Add(child.gameObject);
        }

        // Ensure the original form is active and others are inactive
        SetTransformation(0);
    }

    void Update()
    {
        // Update timers
        if (transformationTimeRemaining > 0)
        {
            transformationTimeRemaining -= Time.deltaTime;

            // If time runs out, revert to original form
            if (transformationTimeRemaining <= 0)
            {
                SetTransformation(0); // Return to original form
                cooldownTimeRemaining = transformationCooldown; // Start cooldown
            }
        }

        if (cooldownTimeRemaining > 0)
        {
            cooldownTimeRemaining -= Time.deltaTime;
        }
    }

    public bool CanTransform()
    {
        return cooldownTimeRemaining <= 0;
    }

    public void TransformInto(int propIndex)
    {
        if (CanTransform() && propIndex >= 0 && propIndex < transformationForms.Count)
        {
            SetTransformation(propIndex);
            transformationTimeRemaining = transformationDuration; // Start transformation timer
        }
    }

    private void SetTransformation(int index)
    {
        for (int i = 0; i < transformationForms.Count; i++)
        {
            transformationForms[i].SetActive(i == index);
        }
        currentFormIndex = index;
    }

    public int GetCurrentFormIndex()
    {
        return currentFormIndex;
    }

    public float GetTransformationTimeRemaining()
    {
        return transformationTimeRemaining;
    }

    public float GetCooldownTimeRemaining()
    {
        return cooldownTimeRemaining;
    }
}

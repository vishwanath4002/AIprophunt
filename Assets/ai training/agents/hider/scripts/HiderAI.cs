using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.AI;

public class HiderAI : Agent
{
    [Header("References")]
    public NavMeshAgent agent;
    public RaycastSensor raycastSensor;

    [Header("Transformation Settings")]
    public GameObject[] propForms; // Child objects representing different props
    private int currentFormIndex = 0; // Index of the active prop
    public float transformCooldown = 5f; // Cooldown before transforming again
    private float transformTimer = 0f;
    private float transformDuration = 10f; // Time the Hider stays transformed
    private float transformDurationTimer = 0f;
    private bool isTransformed = false;

    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 180f;

    void Start()
    {
        agent.speed = moveSpeed;
        EnableForm(0); // Start in original form
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EnableForm(0); // Original form
        if (Input.GetKeyDown(KeyCode.Alpha2)) EnableForm(1); // First prop
        if (Input.GetKeyDown(KeyCode.Alpha3)) EnableForm(2); // Second prop
    }
    public override void OnEpisodeBegin()
    {
        transformTimer = 0f;
        transformDurationTimer = 0f;
        isTransformed = false;
        EnableForm(0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Continuous: Time left before transformation expires
        sensor.AddObservation(transformDurationTimer / transformDuration);

        // Continuous: Time left before it can transform again
        sensor.AddObservation(transformTimer / transformCooldown);

        // Discrete: Current prop type index
        sensor.AddObservation(currentFormIndex);

        // Discrete Array: Count of props of each type in view
        List<RaycastSensor.RaycastHitData> detectedObjects = raycastSensor.CastRays();
        Dictionary<string, int> propCounts = new Dictionary<string, int>();

        foreach (var obj in detectedObjects)
        {
            if (!propCounts.ContainsKey(obj.tag))
                propCounts[obj.tag] = 0;
            propCounts[obj.tag]++;
        }

        foreach (var propType in propForms)
        {
            int count = propCounts.ContainsKey(propType.tag) ? propCounts[propType.tag] : 0;
            sensor.AddObservation(count);
        }

        // Discrete Array: Discretized minimum distance from each prop type
        foreach (var propType in propForms)
        {
            float minDist = float.MaxValue;
            foreach (var obj in detectedObjects)
            {
                if (obj.tag == propType.tag)
                    minDist = Mathf.Min(minDist, obj.distance);
            }
            sensor.AddObservation(minDist / raycastSensor.raycastLength);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Move forward or not
        if (actions.DiscreteActions[0] == 1)
            agent.Move(transform.forward * moveSpeed * Time.deltaTime);

        // Turn left/right
        if (actions.DiscreteActions[1] == 1)
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        else if (actions.DiscreteActions[1] == 2)
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Transformation action
        int transformAction = actions.DiscreteActions[2];

        if (transformAction != currentFormIndex && transformTimer <= 0f)
        {
            EnableForm(transformAction);
            transformTimer = transformCooldown;
            transformDurationTimer = transformDuration;
            isTransformed = transformAction != 0;
        }

        // Handle transformation duration countdown
        if (isTransformed)
        {
            transformDurationTimer -= Time.deltaTime;
            if (transformDurationTimer <= 0f)
            {
                EnableForm(0); // Revert to original form
                isTransformed = false;
            }
        }

        // Handle transformation cooldown countdown
        if (transformTimer > 0f)
        {
            transformTimer -= Time.deltaTime;
        }
    }

    private void EnableForm(int index)
    {
        for (int i = 0; i < propForms.Length; i++)
        {
            propForms[i].SetActive(i == index);
        }
        currentFormIndex = index;
    }
}

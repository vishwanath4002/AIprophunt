using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class HiderAI : Agent
{
    [Header("References")]
    [SerializeField] private HiderController hiderController;
    [SerializeField] private RaycastSensor raycastSensor;

    [Header("Tags & Detection Settings")]
    [SerializeField] private List<string> detectablePropTags;
    [SerializeField] private string seekerTag;

    [Header("Debug Info")]
    [SerializeField] private int currentFormIndex; // To track current form in Inspector
    [SerializeField] private bool debugMode = true; // Enable keyboard control for testing

    private void Update()
    {
        if (!debugMode) return;

        // Movement (W = forward, S = stop)
        float move = Input.GetKey(KeyCode.W) ? 1f : 0f;

        // Turning (A = left, D = right)
        float turn = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;

        // Transform using number keys (1 = original, 2, 3, etc. for props)
        int transformIndex = -1;
        for (int i = 0; i < 9; i++) // Supports up to 9 transformations
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                transformIndex = i;
                break;
            }
        }

        // Apply actions
        hiderController.Move(move);
        hiderController.Turn(turn);
        if (transformIndex >= 0) hiderController.TransformHider(transformIndex);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Transformation duration remaining (continuous)
        sensor.AddObservation(hiderController.GetCurrentTransformationDurationRemaining());

        // 2. Transformation cooldown remaining (continuous)
        sensor.AddObservation(hiderController.GetTransformCooldownRemaining());

        // 3. Prop count per type in view (discrete array)
        List<RaycastSensor.RaycastHitData> detectedObjects = raycastSensor.CastRays();
        Dictionary<string, int> propTypeCount = new Dictionary<string, int>();
        Dictionary<string, float> minDistances = new Dictionary<string, float>();

        foreach (var propTag in detectablePropTags)
        {
            propTypeCount[propTag] = 0;
            minDistances[propTag] = float.MaxValue;
        }

        float seekerDistance = float.MaxValue;

        foreach (var hit in detectedObjects)
        {
            if (hit.tag == seekerTag)
            {
                seekerDistance = hit.distance;
            }
            else if (detectablePropTags.Contains(hit.tag))
            {
                propTypeCount[hit.tag]++;
                if (hit.distance < minDistances[hit.tag])
                {
                    minDistances[hit.tag] = hit.distance;
                }
            }
        }

        foreach (var propTag in detectablePropTags)
        {
            sensor.AddObservation(propTypeCount[propTag]);
            sensor.AddObservation(minDistances[propTag] == float.MaxValue ? raycastSensor.raycastLength : minDistances[propTag]);
        }

        // 4. Seeker distance (discrete)
        sensor.AddObservation(seekerDistance == float.MaxValue ? raycastSensor.raycastLength : seekerDistance);

        // 5. Current form index
        currentFormIndex = hiderController.GetCurrentFormIndex();
        sensor.AddObservation(currentFormIndex);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float move = actions.DiscreteActions[0] == 1 ? 1f : 0f;
        float turn = actions.DiscreteActions[1] == 1 ? 1f : actions.DiscreteActions[1] == 2 ? -1f : 0f;
        int transformIndex = actions.DiscreteActions[2];

        hiderController.Move(move);
        hiderController.Turn(turn);
        hiderController.TransformHider(transformIndex);
    }
}

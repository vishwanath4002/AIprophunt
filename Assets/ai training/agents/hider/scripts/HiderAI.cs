using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class HiderAI : Agent
{
    public float moveSpeed = 3f;
    public float disguiseTime = 5f;
    public LayerMask propLayer;
    public GameObject disguiseIndicator;
    public List<GameObject> propPrefabs; // List of prefabs to use for disguises
    public float maxEpisodeTime = 10f;  // Max episode time in seconds

    public float detectionDistance = 5f; // Distance for raycasting detection
    public int numberOfDetectionRays = 8; // Number of rays to cast around the Hider

    private bool isDisguised = false;
    private float disguiseTimer = 0f;
    private GameObject currentDisguise;
    private NavMeshAgent navMeshAgent;
    private Vector3 targetDestination;
    private float episodeTimer;
    private MeshRenderer meshRenderer;
    private Transform seekerTransform;

    void Start()
    {
        // Cache components and ensure they exist
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent is missing on the HiderAI GameObject.");
        }

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer is missing on the HiderAI GameObject.");
        }

        disguiseIndicator.SetActive(false);

        // Optionally, find the Seeker's transform
        GameObject seeker = GameObject.FindWithTag("Player");
        if (seeker != null)
        {
            seekerTransform = seeker.transform;
        }
    }

    void Update()
    {
        // Continuously check for nearby props using raycasting
        CheckForPropsWithRaycasting();

        // Check if the episode timer has run out
        episodeTimer -= Time.deltaTime;
        if (episodeTimer <= 0)
        {
            AddReward(-1.0f); // Penalty for running out of time
            EndEpisode();
        }
    }

    private bool CheckForPropsWithRaycasting()
    {
        // Cast rays in multiple directions to detect props
        Vector3[] rayDirections = GenerateRayDirections(numberOfDetectionRays);

        foreach (Vector3 direction in rayDirections)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, detectionDistance, propLayer))
            {
                Debug.Log("Hider: Prop detected via raycast!");
                return true; // Return true if a prop is detected
            }
        }
        return false; // No props detected
    }


    private Vector3[] GenerateRayDirections(int numberOfRays)
    {
        Vector3[] directions = new Vector3[numberOfRays];
        float angleStep = 360f / numberOfRays;

        for (int i = 0; i < numberOfRays; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            directions[i] = direction;
        }

        return directions;
    }

    public override void OnEpisodeBegin()
    {
        // Reset parameters and components at the beginning of an episode
        isDisguised = false;
        disguiseTimer = 0f;
        episodeTimer = maxEpisodeTime; // Reset the episode timer

        if (currentDisguise) Destroy(currentDisguise);

        transform.position = new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
        navMeshAgent.ResetPath();
        disguiseIndicator.SetActive(false);
        meshRenderer.enabled = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observations for the agent: whether it's disguised and the remaining disguise time
        sensor.AddObservation(isDisguised ? 1 : 0);
        sensor.AddObservation(disguiseTimer / disguiseTime);

        // Additional observations for learning
        if (seekerTransform != null)
        {
            sensor.AddObservation(Vector3.Distance(transform.position, seekerTransform.position)); // Distance to Seeker
            sensor.AddObservation(seekerTransform.position); // Seeker's position
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        int disguiseAction = Mathf.FloorToInt(actions.DiscreteActions[0]);

        // Movement
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;
        if (moveDirection.magnitude > 0)
        {
            targetDestination = transform.position + moveDirection * moveSpeed;
            navMeshAgent.SetDestination(targetDestination);
        }

        // Handle disguise action
        if (disguiseAction == 1 && !isDisguised)
        {
            TryDisguise();
        }

        // Handle disguise timer
        if (isDisguised)
        {
            disguiseTimer -= Time.deltaTime;
            if (disguiseTimer <= 0)
            {
                RevertDisguise();
            }
        }

        AddReward(-0.001f); // Small time penalty to encourage efficiency
    }

    private void TryDisguise()
    {
        // Use raycasting to detect nearby props
        if (CheckForPropsWithRaycasting() && propPrefabs.Count > 0)
        {
            // Randomly select a prop from the list and transform into it
            int randomIndex = Random.Range(0, propPrefabs.Count);
            TransformIntoProp(propPrefabs[randomIndex]);
            AddReward(0.2f); // Reward for disguising successfully
        }
    }


    private void TransformIntoProp(GameObject propPrefab)
    {
        if (currentDisguise) Destroy(currentDisguise);

        currentDisguise = Instantiate(propPrefab, transform.position, propPrefab.transform.rotation);
        currentDisguise.transform.parent = transform;

        isDisguised = true;
        disguiseTimer = disguiseTime;
        disguiseIndicator.SetActive(true);

        meshRenderer.enabled = false;
    }

    private void RevertDisguise()
    {
        if (currentDisguise)
        {
            Destroy(currentDisguise);
        }

        isDisguised = false;
        disguiseIndicator.SetActive(false);
        meshRenderer.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AddReward(-1.0f); // Large penalty for getting caught
            EndEpisode();
        }
    }

    public bool IsDisguised()
    {
        return isDisguised;
    }
}
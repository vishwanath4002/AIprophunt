using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Transform hider;
    public Transform seeker;
    public float roundTime = 60f;
    private float timer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        StartNewRound();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Debug.Log("Time's up! Restarting round...");
            StartNewRound();
        }
    }

    public void HiderCaught()
    {
        Debug.Log("Hider was caught! Restarting round...");
        StartNewRound();
    }

    public void StartNewRound()
    {
        timer = roundTime;
        RandomizePosition(hider);
        RandomizePosition(seeker);
    }

    void RandomizePosition(Transform obj)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(new Vector3(Random.Range(-10, 10), 1, Random.Range(-10, 10)), out hit, 10f, NavMesh.AllAreas))
        {
            obj.position = hit.position;
            obj.GetComponent<NavMeshAgent>().Warp(hit.position);
        }
    }
}

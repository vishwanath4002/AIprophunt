using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerFinal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HiderAI hiderAgent;
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private UIController uiController;

    [Header("Game Settings")]
    [SerializeField] private float roundDuration = 60f;

    private float timer;
    private bool caught;
    private bool isFrozen = false;

    private void Start()
    {
        uiController.OnRestartGame += RestartRound; // Listen for restart button
        StartNewRound();
    }

    private void Update()
    {
        if (!isFrozen)
        {
            timer -= Time.deltaTime;
            uiController.UpdateTimer(timer);

            if (caught)
            {
                EndGame("You Lose!", false);
            }
            else if (timer <= 0)
            {
                EndGame("You Win!", true);
            }
        }
    }

    public void OnHiderCaught()
    {
        caught = true;
    }

    private void RestartRound()
    {
        uiController.HideResult();
        ResetPositions();
        StartNewRound();
    }

    private void StartNewRound()
    {
        caught = false;
        timer = roundDuration;
        ResetPositions();
        StartCoroutine(StartCountdown());
    }

    private void EndGame(string message, bool won)
    {
        uiController.ShowResult(message, won);
        FreezeAgent();
    }

    private void ResetPositions()
    {
        if (spawnPoints.Length < 2)
        {
            Debug.LogError("Not enough spawn points! At least 2 required.");
            return;
        }

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            availableIndices.Add(i);
        }

        int hiderSpawnIndex = Random.Range(0, availableIndices.Count);
        hiderAgent.transform.position = spawnPoints[availableIndices[hiderSpawnIndex]].position;
        availableIndices.RemoveAt(hiderSpawnIndex);

        int playerSpawnIndex = Random.Range(0, availableIndices.Count);
        player.transform.position = spawnPoints[availableIndices[playerSpawnIndex]].position;
    }

    public void FreezeAgent()
    {
        isFrozen = true;
        hiderAgent.enabled = false; // Disables the agent (stops decision-making)
    }

    public void UnfreezeAgent()
    {
        isFrozen = false;
        hiderAgent.enabled = true; // Enables the agent (resumes decision-making)
    }

    private IEnumerator StartCountdown()
    {
        FreezeAgent();
        uiController.UpdateCountdown("3");
        yield return new WaitForSeconds(1f);
        uiController.UpdateCountdown("2");
        yield return new WaitForSeconds(1f);
        uiController.UpdateCountdown("1");
        yield return new WaitForSeconds(1f);
        uiController.UpdateCountdown("Go!", true);
        yield return new WaitForSeconds(1f);
        uiController.HideCountdown();
        UnfreezeAgent();
    }
}

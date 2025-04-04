using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Add this for UI elements

public class GameManagerFinal : MonoBehaviour
{
    public Transform player;
    public GameObject[] hiders; // Assign all Hiders in the Inspector
    public Transform[] spawnPoints;
    public UIController uiController;
    public float roundDuration = 60f;

    public TMPro.TMP_Text countdownText; // Separate UI text for countdown

    private float timer;
    private int hidersCaught = 0;
    private int totalHiders = 0;
    public bool gamePaused = true;
    public bool onMenu = true;

    private void Start()
    {
        gamePaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Ensure countdown UI is hidden at the start
        countdownText.gameObject.SetActive(false);
    }

    public void StartGameplay()
    {
        onMenu = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        uiController.gameUI.SetActive(true);
        totalHiders = Mathf.Clamp(PlayerPrefs.GetInt("HiderCount", 1), 1, hiders.Length);
        hidersCaught = 0;
        timer = roundDuration;

        SpawnPlayerAndHiders();
        FreezeAgents();
        StartCoroutine(StartCountdown());

        uiController.UpdateScore(hidersCaught, totalHiders);
        uiController.UpdateTimer(timer);
    }

    private void Update()
    {
        if (gamePaused) return;

        if (timer > 0)
        {
            timer -= Time.deltaTime;
            uiController.UpdateTimer(timer);

            if (timer <= 0)
                EndGame(false);
        }
    }

    void SpawnPlayerAndHiders()
    {
        if (spawnPoints.Length < totalHiders + 1)
        {
            Debug.LogError("Not enough spawn points! Assign more.");
            return;
        }

        // Move player to first spawn point
        player.position = spawnPoints[0].position;

        // Position and enable only the selected number of hiders
        for (int i = 0; i < hiders.Length; i++)
        {
            if (i < totalHiders)
            {
                hiders[i].transform.position = spawnPoints[i + 1].position;
                hiders[i].SetActive(false); // Will be activated after countdown
            }
            else
            {
                hiders[i].SetActive(false); // Ensure unused hiders stay disabled
            }
        }
    }

    public void HiderCaught()
    {
        hidersCaught++;
        uiController.UpdateScore(hidersCaught, totalHiders);

        if (hidersCaught >= totalHiders)
            EndGame(true);
    }

    void EndGame(bool won)
    {
        gamePaused = true;
        onMenu = true;
        uiController.gameUI.SetActive(false);
        FreezeAgents();
        uiController.ShowEndScreen(won, roundDuration - timer);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true); // Show countdown UI
        player.gameObject.SetActive(true);
        countdownText.text = "3";
        yield return new WaitForSeconds(0.5f);
        countdownText.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        countdownText.gameObject.SetActive(true);
        countdownText.text = "2";
        yield return new WaitForSeconds(0.5f);
        countdownText.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        countdownText.gameObject.SetActive(true);
        countdownText.text = "1";
        yield return new WaitForSeconds(0.5f);
        countdownText.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        countdownText.gameObject.SetActive(true);
        countdownText.text = "Go!";
        yield return new WaitForSeconds(1f);


        countdownText.gameObject.SetActive(false); // Hide countdown after start

        gamePaused = false;
        UnfreezeAgents();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FreezeAgents()
    {
        gamePaused = true;
        player.gameObject.SetActive(false);

        for (int i = 0; i < totalHiders; i++)
        {
            hiders[i].SetActive(false);
        }
    }

    private void UnfreezeAgents()
    {
        gamePaused = false;
        player.gameObject.SetActive(true);

        for (int i = 0; i < totalHiders; i++)
        {
            hiders[i].SetActive(true);
        }
    }
}

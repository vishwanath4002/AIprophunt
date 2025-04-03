using System.Collections;
using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerFinal : MonoBehaviour
{
    public Transform player;
    public GameObject hiderAI;
    public Transform[] spawnPoints;
    public UIController uiController;
    public float roundDuration = 60f;

    private float timer;
    private int hidersCaught = 0;
    private int totalHiders;
    public bool gamePaused = true; // Start with game paused
    public bool onMenu = true;

    private void Start()
    {
        gamePaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void StartGameplay()
    {
        onMenu = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        uiController.gameUI.SetActive(true);
        totalHiders = PlayerPrefs.GetInt("HiderCount", 1);
        SpawnPlayerAndHiders();
        FreezeAgents(); // Freeze player and hiders at the start
        StartCoroutine(StartCountdown());
    }

    private void Update()
    {
        if (gamePaused) return; // Stop everything if the game is paused

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
        player.position = spawnPoints[0].position; // Player spawns at first position

        for (int i = 1; i < totalHiders; i++)
        {
            Instantiate(hiderAI, spawnPoints[i].position, Quaternion.identity);
        }
    }

    public void HiderCaught()
    {
        hidersCaught++;
        uiController.UpdateScore(hidersCaught, totalHiders);

        if (hidersCaught == totalHiders)
            EndGame(true);
    }

    void EndGame(bool won)
    {
        gamePaused = true;
        onMenu = true;
        uiController.gameUI.SetActive(false);
        FreezeAgents(); // Freeze movement on game over
        uiController.ShowEndScreen(won, roundDuration - timer);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator StartCountdown()
    {
        uiController.gameUI.SetActive(true);
        uiController.UpdateCountdown("3");
        yield return new WaitForSeconds(1f);
        uiController.UpdateCountdown("2");
        yield return new WaitForSeconds(1f);
        uiController.UpdateCountdown("1");
        yield return new WaitForSeconds(1f);
        uiController.UpdateCountdown("Go!");
        yield return new WaitForSeconds(1f);
        uiController.HideCountdown();

        gamePaused = false;
        UnfreezeAgents();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FreezeAgents()
    {
        gamePaused = true;

        // Disable player movement script
        player.gameObject.SetActive(false);
        player.GetComponent<ExamplePlayer>().enabled = false;
        player.GetComponent<ExampleCharacterController>().enabled = false;

        // Disable hider AI scripts
        foreach (GameObject hider in GameObject.FindGameObjectsWithTag("Hider"))
        {
            //hider.GetComponent<HiderController>().enabled = false;
            //hider.GetComponent<HiderAI>().enabled = false;
            Debug.Log(hider);
            hider.SetActive(false);
        }
    }

    private void UnfreezeAgents()
    {
        gamePaused = false;

        // Enable player movement script
        player.gameObject.SetActive(true);
        player.GetComponent<ExamplePlayer>().enabled = true;
        player.GetComponent<ExampleCharacterController>().enabled = true;

        // Enable hider AI scripts
        foreach (GameObject hider in GameObject.FindGameObjectsWithTag("Hider"))
        {
            //hider.GetComponent<HiderController>().enabled = true;
            //hider.GetComponent<HiderAI>().enabled = true;
            Debug.Log(hider);
            hider.SetActive(true);
        }
    }
}

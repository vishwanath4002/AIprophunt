using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameManagerFinal gameManagerFinal;
    public MainMenuController mainMenuController;

    public TMPro.TMP_Text timerText;
    public TMPro.TMP_Text scoreText;
    public GameObject pauseMenu;
    public GameObject endScreen;
    public TMPro.TMP_Text endMessage;
    public TMPro.TMP_Text timeTakenText;
    public Button resumeButton;
    public Button pauseMenuExitToMenuButton;
    public Button endScreenExitToMenuButton;
    public Button restartButton;

    private bool paused;
    private void Start()
    {
        paused = false;
    }

    public void ExitToMenu()
    {
        endScreen.SetActive(false);
        pauseMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; // Hide cursor
        Cursor.visible = false;
        mainMenuController.GoBackToMainMenu();
    }

    public void Restart()
    {
        endScreen.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; // Hide cursor
        Cursor.visible = false;
        gameManagerFinal.StartGameplay();
    }
    public void UpdateTimer(float time)
    {
        timerText.text = "Time: " + Mathf.Ceil(time);
    }

    public void UpdateScore(int caught, int total)
    {
        scoreText.text = "Hiders Caught: " + caught + "/" + total;
    }

    public void ShowEndScreen(bool won, float timeTaken)
    {
        endScreen.SetActive(true);
        restartButton.onClick.AddListener(() => Restart());
        endScreenExitToMenuButton.onClick.AddListener(() => ExitToMenu());

        endMessage.text = won ? "You Win!" : "Game Over!";
        timeTakenText.text = "Time Taken: " + timeTaken.ToString("F1") + "s";
    }

    public void UpdateCountdown(string text)
    {
        timerText.text = text;
    }

    public void HideCountdown()
    {
        timerText.text = "";
    }

    public void TogglePauseMenu()
    {
        paused = !paused;
        pauseMenu.SetActive(paused);
        Time.timeScale = paused ? 0 : 1;

        if (paused)
        {
            Cursor.lockState = CursorLockMode.None; // Show cursor
            Cursor.visible = true;
            gameManagerFinal.gamePaused = true;
            resumeButton.onClick.AddListener(() => TogglePauseMenu());
            pauseMenuExitToMenuButton.onClick.AddListener(() => ExitToMenu());
        }
        else
        {
            gameManagerFinal.gamePaused = false;
            Cursor.lockState = CursorLockMode.Locked; // Hide cursor
            Cursor.visible = false;

        }
    }
}

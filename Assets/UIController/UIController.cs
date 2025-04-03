using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Image redScreenEffect;
    [SerializeField] private Button restartButton; // New Restart Button

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip countdownBeep;
    [SerializeField] private AudioClip goSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    public delegate void RestartGame();
    public event RestartGame OnRestartGame;

    private void Start()
    {
        restartButton.onClick.AddListener(() =>
        {
            restartButton.gameObject.SetActive(false);
            OnRestartGame?.Invoke();
        });
        restartButton.gameObject.SetActive(false);
    }

    public void UpdateCountdown(string text, bool isFinal = false)
    {
        countdownText.text = text;
        countdownText.gameObject.SetActive(true);

        if (isFinal)
            audioSource.PlayOneShot(goSound);
        else
            audioSource.PlayOneShot(countdownBeep);
    }

    public void HideCountdown()
    {
        countdownText.gameObject.SetActive(false);
    }

    public void UpdateTimer(float timeRemaining)
    {
        timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        redScreenEffect.gameObject.SetActive(timeRemaining <= 10);
    }

    public void ShowResult(string message, bool won)
    {
        resultText.text = message;
        resultText.gameObject.SetActive(true);
        audioSource.PlayOneShot(won ? winSound : loseSound);
        restartButton.gameObject.SetActive(true);
    }

    public void HideResult()
    {
        resultText.gameObject.SetActive(false);
    }
}

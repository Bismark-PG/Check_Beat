using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("In-Game UI")]
    public Text stageText;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public Text finalScoreText;     

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (StageManager.Instance != null)
        {
            UpdateStageText(StageManager.Instance.currentStage);
        }
    }

    // In Game UI
    public void UpdateStageText(int stage)
    {
        if (stageText != null)
        {
            stageText.text = "STAGE: " + stage; 
        }
    }

    // Game Over UI
    public void ShowGameOver(int finalStage)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Stage: " + finalStage;
            }
        }

        Time.timeScale = 0f;
    }

    // Retry Button
    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Main Menu Button
    public void GoToMain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }
}
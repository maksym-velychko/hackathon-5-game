using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Меню смерти / Паузы")]
    public GameObject gameOverCanvas;

    [Header("Названия сцен для кнопок")]
    [Tooltip("Точное название сцены для рестарта")]
    public string restartSceneName = "OriginalScene";
    [Tooltip("Точное название сцены главного меню")]
    public string menuSceneName = "Menu";

    [Header("Приближение камеры при смерти")]
    public Camera mainCamera;
    public float targetZoomSize = 2.5f;
    public float zoomSpeed = 3f;

    [Header("Настройки паузы")]
    public float pauseDelay = 1f;

    private bool isPlayerDead = false;
    // Новая переменная, чтобы отслеживать, открыто ли меню вручную через ESC
    private bool isMenuOpen = false; 
    private float originalCameraSize;

    void Start()
    {
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
        if (mainCamera == null) mainCamera = Camera.main;
        
        if (mainCamera != null)
        {
            originalCameraSize = mainCamera.orthographicSize;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPlayerDead)
            {
                ToggleMenu();
            }
        }

        if (isPlayerDead && mainCamera != null)
        {
            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize, 
                targetZoomSize, 
                Time.unscaledDeltaTime * zoomSpeed
            );
        }
    }

    private void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;

        if (isMenuOpen)
        {
            if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        isMenuOpen = false;
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ShowGameOverMenu()
    {
        if (isPlayerDead) return;
        isPlayerDead = true;
        isMenuOpen = true;

        if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
        StartCoroutine(PauseGameRoutine());
    }

    IEnumerator PauseGameRoutine()
    {
        yield return new WaitForSecondsRealtime(pauseDelay);
        Time.timeScale = 0f; 
    }

    // ==========================================
    // МЕТОДЫ ДЛЯ ТВОИХ ТРЕХ КНОПОК
    // ==========================================

    // 1. Кнопка "Рестарт"
    public void RestartGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(restartSceneName); 
    }

    // 2. Кнопка "Меню"
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(menuSceneName); 
    }

    // 3. Кнопка "Выход"
    public void QuitGame()
    {
        Debug.Log("Выход из игры (Application.Quit)");
        Application.Quit(); 
    }
}
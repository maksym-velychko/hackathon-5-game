using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Меню смерти")]
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

    void Start()
    {
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        if (isPlayerDead && mainCamera != null)
        {
            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize, 
                targetZoomSize, 
                Time.unscaledDeltaTime * zoomSpeed
            );
        }
    }

    public void ShowGameOverMenu()
    {
        if (isPlayerDead) return;
        isPlayerDead = true;

        if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
        StartCoroutine(PauseGameRoutine());
    }

    IEnumerator PauseGameRoutine()
    {
        yield return new WaitForSecondsRealtime(pauseDelay);
        Time.timeScale = 0f; // Ставим на паузу
    }

    // ==========================================
    // МЕТОДЫ ДЛЯ ТВОИХ ТРЕХ КНОПОК
    // ==========================================

    // 1. Кнопка "Рестарт"
    public void RestartGame()
    {
        Time.timeScale = 1f; // Обязательно снимаем с паузы!
        SceneManager.LoadScene(restartSceneName); // Грузит "OriginalScene"
    }

    // 2. Кнопка "Меню"
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Обязательно снимаем с паузы!
        SceneManager.LoadScene(menuSceneName); // Грузит "Menu"
    }

    // 3. Кнопка "Выход"
    public void QuitGame()
    {
        Debug.Log("Выход из игры (Application.Quit)");
        Application.Quit(); // Закрывает игру (работает только в скомпилированном билде, не в редакторе Unity)
    }
}
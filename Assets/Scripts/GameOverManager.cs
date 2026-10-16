using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Меню смерти")]
    [Tooltip("Перетащи сюда весь GameObject Canvas с менюшкой смерти")]
    public GameObject gameOverCanvas;

    [Header("Приближение камеры при смерти")]
    [Tooltip("Ссылка на основную камеру (если пусто, найдет Camera.main)")]
    public Camera mainCamera;
    [Tooltip("Целевой размер камеры при приближении (например, 2.5 или 3)")]
    public float targetZoomSize = 2.5f;
    [Tooltip("Скорость приближения камеры")]
    public float zoomSpeed = 3f;

    [Header("Настройки паузы")]
    [Tooltip("Через сколько секунд после смерти игра ставится на паузу")]
    public float pauseDelay = 1f;

    private float defaultZoomSize;
    private bool isPlayerDead = false;

    void Start()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(false);
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            defaultZoomSize = mainCamera.orthographicSize;
        }
    }

    void Update()
    {
        // Если игрок погиб, плавно приближаем камеру.
        // Используем unscaledDeltaTime, чтобы зум работал даже после остановки времени (Time.timeScale = 0)
        if (isPlayerDead && mainCamera != null)
        {
            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize, 
                targetZoomSize, 
                Time.unscaledDeltaTime * zoomSpeed
            );
        }
    }

    // Метод, который вызывает PlayerHealth при гибели
    public void ShowGameOverMenu()
    {
        if (isPlayerDead) return;
        isPlayerDead = true;

        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
        }

        // Запускаем отсчет 1 секунды перед постановкой игры на паузу
        StartCoroutine(PauseGameRoutine());
    }

    // Корутина ожидания перед паузой
    IEnumerator PauseGameRoutine()
    {
        // Ждем 1 секунду реального времени (даже если кадры идут)
        yield return new WaitForSecondsRealtime(pauseDelay);

        // Ставим игру на паузу. 
        // Физика, враги и скрипты персонажа остановятся, но кнопки в Canvas продолжат работать!
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        // Обязательно возвращаем нормальное время перед перезагрузкой уровня
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu(string menuSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
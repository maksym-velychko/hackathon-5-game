using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChaoticShipFlight : MonoBehaviour
{
    [Header("Зона спокойного полета")]
    public float swayRangeX = 2.5f;
    public float swayRangeY = 1.5f;
    public float minMoveDuration = 1.5f;
    public float maxMoveDuration = 3.0f;

    [Header("Плавный наклон носа")]
    public bool enableTilt = true;
    public float maxTiltAngle = 12f;
    public float tiltSmoothTime = 0.4f;

    [Header("Запуск игры и Разогрев (Tap to Play)")]
    [Tooltip("Перетащи сюда объект текста 'Tap to Play' из Canvas")]
    public GameObject tapToPlayUI;
    [Tooltip("Перетащи сюда скрытый объект огня позади корабля")]
    public GameObject fireEffectObject;
    [Tooltip("Сколько секунд корабль сильно качает с включенным огнем перед уходом вниз")]
    public float boostDuration = 1.5f;
    [Tooltip("Во сколько раз увеличиваются колебания при включении огня")]
    public float boostMultiplier = 2f;

    [Header("Настройки ухода вниз за экран")]
    public float diveSpeedY = 18f;
    public float diveSpeedX = 5f;
    public float diveTargetAngle = -55f;
    [Tooltip("Время плавного наклона носа вниз при падении")]
    public float diveTiltSmoothTime = 0.35f;
    public float loadNextSceneDelay = 1.5f;
    public string nextSceneName = "";

    private Vector3 startPosition;
    private float currentAngle = 0f;
    private float angleVelocity = 0f;
    
    private bool isStarted = false;
    private float currentSwayMultiplier = 1f; // 1 - обычный полет, 2 - усиленные колебания
    private Coroutine flightCoroutine;

    void Start()
    {
        startPosition = transform.position;

        if (fireEffectObject != null)
        {
            fireEffectObject.SetActive(false);
        }

        if (tapToPlayUI != null)
        {
            tapToPlayUI.SetActive(true);
        }

        // Запускаем плавный полет со старта
        flightCoroutine = StartCoroutine(SmoothFlightRoutine());
    }

    void Update()
    {
        // Ждем клика мышки или нажатия Пробела
        if (!isStarted && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            StartCoroutine(LaunchSequenceRoutine());
        }
    }

    // Пошаговый сценарий после нажатия Tap to Play
    IEnumerator LaunchSequenceRoutine()
    {
        isStarted = true;

        // 1. Прячем надпись "Tap to Play" и включаем огонь
        if (tapToPlayUI != null) tapToPlayUI.SetActive(false);
        if (fireEffectObject != null) fireEffectObject.SetActive(true);

        // 2. Увеличиваем колебания в 2 раза и перезапускаем маневр, чтобы корабль сразу отреагировал
        currentSwayMultiplier = boostMultiplier;
        if (flightCoroutine != null) StopCoroutine(flightCoroutine);
        flightCoroutine = StartCoroutine(SmoothFlightRoutine());

        // 3. Ждем 1.5 секунды, пока корабль активно раскачивается с работающим огнем
        yield return new WaitForSeconds(boostDuration);

        // 4. Останавливаем качку и плавно уводим корабль вниз за экран
        if (flightCoroutine != null) StopCoroutine(flightCoroutine);
        StartCoroutine(DiveDownRoutine());
    }

    // Корутина полета (работает и в спокойном режиме, и в усиленном в 2 раза)
    IEnumerator SmoothFlightRoutine()
    {
        while (true)
        {
            Vector3 fromPos = transform.position;

            // Размах умножается на currentSwayMultiplier (в 2 раза больше после клика)
            float rangeX = swayRangeX * currentSwayMultiplier;
            float rangeY = swayRangeY * currentSwayMultiplier;

            Vector3 toPos = startPosition + new Vector3(
                Random.Range(-rangeX, rangeX),
                Random.Range(-rangeY, rangeY),
                0f
            );

            // При усилении двигателей маневры проходят в 2 раза энергичнее
            float duration = Random.Range(minMoveDuration, maxMoveDuration) / currentSwayMultiplier;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float smoothT = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                Vector3 newPos = Vector3.Lerp(fromPos, toPos, smoothT);

                if (enableTilt && Time.deltaTime > 0f)
                {
                    float verticalDiff = toPos.y - transform.position.y;
                    float maxAngle = maxTiltAngle * currentSwayMultiplier;
                    float targetAngle = Mathf.Clamp(verticalDiff * 8f, -maxAngle, maxAngle);
                    
                    // Слегка ускоряем наклон носа во время разогрева, сохраняя плавность
                    float smoothTime = tiltSmoothTime / currentSwayMultiplier;
                    currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angleVelocity, smoothTime);
                    transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
                }

                transform.position = newPos;
                yield return null;
            }
        }
    }

    // Корутина плавного ухода корабля вниз за экран
    IEnumerator DiveDownRoutine()
    {
        float elapsed = 0f;
        float currentVelocityY = 0f;
        float currentVelocityX = 0f;

        while (true)
        {
            elapsed += Time.deltaTime;

            // Мягкий, постепенный разгон через SmoothStep (без резкого рывка)
            float accelProgress = Mathf.SmoothStep(0f, 1f, elapsed / 0.8f);
            currentVelocityY = Mathf.Lerp(0f, diveSpeedY, accelProgress);
            currentVelocityX = Mathf.Lerp(0f, diveSpeedX, accelProgress);

            transform.position += new Vector3(currentVelocityX, -currentVelocityY, 0f) * Time.deltaTime;

            // Плавно наклоняем нос вниз по дуге
            currentAngle = Mathf.SmoothDampAngle(currentAngle, diveTargetAngle, ref angleVelocity, diveTiltSmoothTime);
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            if (loadNextSceneDelay > 0f && elapsed >= loadNextSceneDelay && !string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
                yield break;
            }

            yield return null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        
        // Зеленая рамка — обычные колебания
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, new Vector3(swayRangeX * 2f, swayRangeY * 2f, 0f));

        // Желтая рамка — увеличенные в 2 раза колебания после включения огня
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, new Vector3(swayRangeX * boostMultiplier * 2f, swayRangeY * boostMultiplier * 2f, 0f));
    }
}
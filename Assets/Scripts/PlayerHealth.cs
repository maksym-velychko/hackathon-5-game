using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Здоровье игрока")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Авто-регенерация ХП")]
    public float regenDelay = 20f;
    public int regenAmount = 5;
    public float regenInterval = 4f;

    [Header("UI Полоска ХП над головой")]
    public Image healthBarFill;
    public Transform healthBarCanvas;
    public Vector3 healthBarOffset = new Vector3(0f, 1.5f, 0f);
    public float smoothDuration = 0.3f;

    [Header("Меню смерти")]
    public GameOverManager gameOverManager;

    private Animator anim;
    private Coroutine smoothHealthCoroutine;
    private Coroutine regenCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;

            if (healthBarCanvas == null)
            {
                Canvas parentCanvas = healthBarFill.GetComponentInParent<Canvas>();
                if (parentCanvas != null)
                {
                    healthBarCanvas = parentCanvas.transform;
                }
            }
        }

        if (healthBarCanvas != null)
        {
            healthBarOffset = healthBarCanvas.position - transform.position;
            healthBarCanvas.SetParent(null);
            healthBarCanvas.rotation = Quaternion.identity;
        }
    }

    void LateUpdate()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.position = transform.position + healthBarOffset;
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0); // Не даем ХП уйти ниже 0
        UpdateHealthUI();

        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }

        if (currentHealth <= 0)
        {
            isDeath();
        }
        else
        {
            regenCoroutine = StartCoroutine(RegenerateHealthRoutine());
        }
    }

    // Мгновенная смерть при столкновении с объектом, у которого тег "Smert"
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Smert"))
        {
            TakeDamage(1000); // Наносим 1000 урона, что мгновенно убивает игрока
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Smert"))
        {
            TakeDamage(1000); // То же самое, если зона со смертельным тегом — это триггер
        }
    }

    IEnumerator RegenerateHealthRoutine()
    {
        yield return new WaitForSeconds(regenDelay);

        while (currentHealth > 0 && currentHealth < maxHealth)
        {
            currentHealth = Mathf.Clamp(currentHealth + regenAmount, 0, maxHealth);
            UpdateHealthUI();

            if (currentHealth >= maxHealth)
            {
                break;
            }

            yield return new WaitForSeconds(regenInterval);
        }

        regenCoroutine = null;
    }

    void UpdateHealthUI()
    {
        if (healthBarFill != null && gameObject.activeInHierarchy)
        {
            float targetFill = (float)currentHealth / maxHealth;

            if (smoothHealthCoroutine != null)
            {
                StopCoroutine(smoothHealthCoroutine);
            }

            smoothHealthCoroutine = StartCoroutine(SmoothFillRoutine(targetFill));
        }
    }

    IEnumerator SmoothFillRoutine(float targetFill)
    {
        float startFill = healthBarFill.fillAmount;
        float elapsedTime = 0f;

        while (elapsedTime < smoothDuration)
        {
            elapsedTime += Time.deltaTime;
            healthBarFill.fillAmount = Mathf.Lerp(startFill, targetFill, elapsedTime / smoothDuration);
            yield return null;
        }

        healthBarFill.fillAmount = targetFill;
    }

    void isDeath()
    {
        Debug.Log("Игрок погиб!");

        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }

        if (anim != null)
        {
            anim.SetTrigger("isDeath");
        }
        
        // Прячем полоску ХП над головой
        if (healthBarCanvas != null)
        {
            Destroy(healthBarCanvas.gameObject, smoothDuration);
        }

        // ВЫЗЫВАЕМ ПОКАЗ КАНВАСА СМЕРТИ
        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOverMenu();
        }

        // Отключаем управление движением
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;
    }
}
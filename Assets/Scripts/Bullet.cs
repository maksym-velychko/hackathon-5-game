using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 30f;
    public float lifetime = 5f;
    public int damage = 10;

    [Header("Заготовка под эфекты")]
    public GameObject hitEffectPrefab; 
    
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Двигаем пулю вперед по её локальной оси X (куда её повернула винтовка при выстреле)
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 1. Игнорируем самого игрока и другие пули
        if (hitInfo.CompareTag("Player") || hitInfo.GetComponent<Bullet>() != null)
        {
            return;
        }

        // 2. Если попали во врага — наносим урон и уничтожаем пулю
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, transform.rotation);
            }
            Destroy(gameObject);
            return;
        }

        // 3. Если попали в стену, землю или застрявшее копье — уничтожаем пулю
        if (hitInfo.CompareTag("Wall") || hitInfo.CompareTag("Ground") || hitInfo.CompareTag("StuckSpear"))
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, transform.rotation);
            }
            Destroy(gameObject);
        }
    }
}
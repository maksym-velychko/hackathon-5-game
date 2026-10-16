using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [Header("Настройки периодического урона")]
    [Tooltip("Количество урона за один тик")]
    public int damageAmount = 2;

    [Tooltip("Интервал нанесения урона в секундах")]
    public float damageInterval = 0.5f;

    [Tooltip("True: урон идет только пока игрок касается объекта. False: игрок коснулся один раз и горит/кровоточит вечно")]
    public bool stopWhenExit = true;

    private float nextDamageTime = 0f;
    private PlayerHealth burningPlayer; // Для режима вечного урона после одного касания

    void Update()
    {
        // Если включен режим "вечного урона после первого касания" (stopWhenExit = false)
        if (!stopWhenExit && burningPlayer != null && burningPlayer.currentHealth > 0)
        {
            if (Time.time >= nextDamageTime)
            {
                burningPlayer.TakeDamage(damageAmount);
                nextDamageTime = Time.time + damageInterval;
            }
        }
    }

    // 1. Если на объекте стоит галочка Is Trigger (например, огонь, газ, кислота)
    void OnTriggerStay2D(Collider2D other)
    {
        HandleDamage(other.gameObject);
    }

    // 2. Если это твердый объект без Is Trigger (например, шипы или горящий корпус корабля)
    void OnCollisionStay2D(Collision2D collision)
    {
        HandleDamage(collision.gameObject);
    }

    void HandleDamage(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null && playerHealth.currentHealth > 0)
            {
                if (!stopWhenExit)
                {
                    burningPlayer = playerHealth;
                    return;
                }

                // Наносим урон строго раз в 0.5 секунды
                if (Time.time >= nextDamageTime)
                {
                    playerHealth.TakeDamage(damageAmount);
                    nextDamageTime = Time.time + damageInterval;
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 30f;
    public float lifetime = 5f;
    // public int damage = 10;

    [Header("Заготовка под эфекты")]
    public GameObject hitEffectPrefab; 
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
    if (hitInfo.CompareTag("Player") || hitInfo.CompareTag("StuckSpear")){
            return;
        }

        /*
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null){
            enemy.TakeDamage(damage);
        }
        */

        if (hitEffectPrefab != null){
            Instantiate(hitEffectPrefab, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }
}
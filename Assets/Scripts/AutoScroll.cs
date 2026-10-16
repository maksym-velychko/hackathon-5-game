using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoScrollBackground : MonoBehaviour
{
    [Header("Настройки движения")]
    [Tooltip("Скорость движения фона влево")]
    public float scrollSpeed = 2f;
    
    [Tooltip("Зацикливать ли фон бесконечно")]
    public bool isLooping = true;

    private Vector3 startPosition;
    private float spriteWidth;

    void Start()
    {
        startPosition = transform.position;

        // Автоматически вычисляем ширину одной картинки
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            spriteWidth = (sr.sprite.texture.width / sr.sprite.pixelsPerUnit) * transform.localScale.x;
        }
    }

    void Update()
    {
        // Двигаем фон строго влево
        transform.Translate(Vector2.left * scrollSpeed * Time.deltaTime);

        // Если фон уехал влево на ширину одной картинки — незаметно сдвигаем его обратно
        if (isLooping && spriteWidth > 0)
        {
            if (transform.position.x <= startPosition.x - spriteWidth)
            {
                transform.position = new Vector3(transform.position.x + spriteWidth, transform.position.y, transform.position.z);
            }
        }
    }
}

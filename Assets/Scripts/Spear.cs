using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))] 
public class Spear : MonoBehaviour
{
    public float lifetime = 5f;
    
    private Rigidbody2D rb;
    private bool hasHit = false; 
    private Collider2D spearCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spearCollider = GetComponent<Collider2D>();
        
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (!hasHit && rb.velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;

        if (collision.transform.CompareTag("Ground"))
        {
            Stick();
        }
    }
void Stick()
    {
        hasHit = true;
        
        // Останавливаем физику копья
        rb.velocity = Vector2.zero; 
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rb.isKinematic = true;      
        
        transform.position += transform.right * 0.1f; 
        gameObject.tag = "Ground"; // Делаем копье "землей"
        
        // --- ВАЖНО: СНОВА ДЕЛАЕМ КОПЬЕ ТВЕРДЫМ ДЛЯ ИГРОКА ---
        
        // Если вы настраивали слои (Способ 1), возвращаем копье на базовый слой (Default),
        // который сталкивается с игроком.
        gameObject.layer = 0; 

        // Если вы отключали коллизию через код (Способ 2), 
        // находим скрипт игрока и снова включаем столкновение:
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (spearCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(spearCollider, playerCollider, false);
            }
        }
        // ----------------------------------------------------

        CancelInvoke(); 
        Destroy(gameObject, 10f); 
    }
}
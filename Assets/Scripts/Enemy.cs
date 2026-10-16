using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Здоровье врага")]
    public int maxHealth = 50;
    private int currentHealth;
    private bool isDead = false;

    [Header("Движение и Патрулирование")]
    public float moveSpeed = 1f; 
    public float patrolDistance = 3f;
    private Vector2 startPosition;
    private int patrolDirection = 1;
    public bool isFacingRight = false;

    [Header("Обнаружение игрока (Зона и Луч)")]
    public float detectionRadius = 6f; 
    public float loseSightDelay = 2f;  
    public LayerMask obstacleLayer;    
    private Transform playerTransform;
    private float lostPlayerTimer = 0f;

    [Header("Урон и Атака")]
    public int contactDamage = 25;
    public float damageCooldown = 1.2f;  // Перезарядка между ударами
    private float nextDamageTime = 0f;
    [Tooltip("Через сколько секунд после начала анимации враг наносит урон (например, 0.7с)")]
    public float attackDelay = 0.7f;

    [Header("Настройка Анимаций")]
    public Animator anim;
    public string runAnimName = "Enemy1_run";
    public string attackAnimName = "Enemy1_attack";
    public string deathAnimName = "Enemy1_death";
    
    public float attackAnimDuration = 1.0f; // Общая длительность анимации удара
    public float deathDestroyDelay = 2f;    
    private float attackTimer = 0f;
    private bool isAttacking = false;

    private enum State { Patrolling, Chasing, WaitingToReturn, Returning }
    private State currentState = State.Patrolling;

    private Rigidbody2D body;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();

        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        currentHealth = maxHealth;
        startPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                isAttacking = false;
            }
        }

        bool canSee = CanSeePlayer();

        if (canSee)
        {
            currentState = State.Chasing;
            lostPlayerTimer = loseSightDelay;
        }
        else
        {
            if (currentState == State.Chasing)
            {
                currentState = State.WaitingToReturn;
            }
            else if (currentState == State.WaitingToReturn)
            {
                lostPlayerTimer -= Time.deltaTime;
                if (lostPlayerTimer <= 0f)
                {
                    currentState = State.Returning;
                }
            }
        }

        UpdateAnimations();
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        // Если идет анимация атаки — не перебиваем её бегом
        if (isAttacking) return;

        bool isMoving = Mathf.Abs(body.velocity.x) > 0.05f && currentState != State.WaitingToReturn;

        if (isMoving)
        {
            anim.speed = 1f;
            anim.Play(runAnimName);
        }
        else
        {
            anim.speed = 0f; 
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Во время атаки враг останавливается
        if (isAttacking)
        {
            body.velocity = new Vector2(0f, body.velocity.y);
            return;
        }

        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                break;

            case State.Chasing:
                if (playerTransform != null)
                {
                    MoveTowardsX(playerTransform.position.x);
                }
                break;

            case State.WaitingToReturn:
                body.velocity = new Vector2(0f, body.velocity.y);
                break;

            case State.Returning:
                MoveTowardsX(startPosition.x);
                
                if (Mathf.Abs(transform.position.x - startPosition.x) < 0.2f)
                {
                    currentState = State.Patrolling;
                }
                break;
        }
    }

    void Patrol()
    {
        if (patrolDistance <= 0.05f)
        {
            body.velocity = new Vector2(0f, body.velocity.y);
            return;
        }

        float targetX = startPosition.x + (patrolDirection * patrolDistance);
        MoveTowardsX(targetX);

        if (Mathf.Abs(transform.position.x - targetX) < 0.2f)
        {
            patrolDirection *= -1;
        }
    }

    void MoveTowardsX(float targetX)
    {
        float diff = targetX - transform.position.x;

        if (Mathf.Abs(diff) < 0.1f)
        {
            body.velocity = new Vector2(0f, body.velocity.y);
            return;
        }

        float dir = Mathf.Sign(diff);
        body.velocity = new Vector2(dir * moveSpeed, body.velocity.y);

        if (dir > 0 && !isFacingRight) Flip();
        else if (dir < 0 && isFacingRight) Flip();
    }

    bool CanSeePlayer()
    {
        if (playerTransform == null) return false;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > detectionRadius) return false;

        RaycastHit2D hit = Physics2D.Linecast(transform.position, playerTransform.position, obstacleLayer);
        if (hit.collider != null) return false;

        return true;
    }

    // При касании игрока запускаем подготовку к удару
    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player") && Time.time >= nextDamageTime && !isAttacking)
        {
            StartCoroutine(PerformAttackRoutine(collision.gameObject));
        }
    }

    // Корутина задержки удара (урон наносится ровно через attackDelay секунд)
    IEnumerator PerformAttackRoutine(GameObject playerObj)
    {
        isAttacking = true;
        attackTimer = attackAnimDuration;
        nextDamageTime = Time.time + damageCooldown;

        // 1. Включаем анимацию удара (исправлен вызов без индекса слоя -1)
        if (anim != null)
        {
            anim.speed = 1f;
            anim.Play(attackAnimName);
        }

        // 2. Ждем ровно столько, сколько нужно для замаха (например, 0.7 сек)
        yield return new WaitForSeconds(attackDelay);

        // 3. Проверяем: если игрок всё еще рядом (не убежал из зоны поражения) — наносим урон
        if (!isDead && playerObj != null)
        {
            float distance = Vector2.Distance(transform.position, playerObj.transform.position);
            // Если игрок в пределах досягаемости (например, ближе чем 1.5 метра)
            if (distance <= 1.8f)
            {
                PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(contactDamage);
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        body.velocity = Vector2.zero;
        body.isKinematic = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (anim != null)
        {
            anim.speed = 1f;
            anim.Play(deathAnimName);
        }

        Destroy(gameObject, deathDestroyDelay);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Vector2 center = Application.isPlaying ? startPosition : (Vector2)transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(center - Vector2.right * patrolDistance, center + Vector2.right * patrolDistance);
        Gizmos.DrawWireSphere(center - Vector2.right * patrolDistance, 0.2f);
        Gizmos.DrawWireSphere(center + Vector2.right * patrolDistance, 0.2f);

        if (Application.isPlaying && playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist <= detectionRadius)
            {
                Gizmos.color = CanSeePlayer() ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position, playerTransform.position);
            }
        }
    }
}
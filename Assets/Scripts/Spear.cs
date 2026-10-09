using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))] 
public class Spear : MonoBehaviour
{
    public float LifeTime = 6f;
    public float TimeInWall = 10f;
    
    private Rigidbody2D rb;
    private bool hasHit = false; 
    private Collider2D spearCollider;
    private Coroutine lifetimeCoroutine;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        spearCollider = GetComponent<Collider2D>();
        
        lifetimeCoroutine = StartCoroutine(LifetimeSequence(LifeTime));
    }

    void FixedUpdate(){
        if (!hasHit && rb.velocity.magnitude > 0.1f){
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;

        Switch button = collision.gameObject.GetComponent<Switch>();
        if (button != null)
        {
            button.Activate();
            Stick();
            return;
        }

        if (collision.transform.CompareTag("Wall"))
        {
            Vector2 wallNormal = collision.GetContact(0).normal;
            Vector2 spearDirection = transform.right;

            float angle = Vector2.Angle(spearDirection, -wallNormal);

            if (angle <= 35f)
            {
                Stick();
            }
            else
            {
                hasHit = true;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
        else if (collision.transform.CompareTag("Ground") || collision.transform.CompareTag("StuckSpear"))
        {
            hasHit = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void Stick() {
        hasHit = true;
        
        if (lifetimeCoroutine != null) StopCoroutine(lifetimeCoroutine);

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rb.isKinematic = true;
        transform.position += transform.right * 0.1f;
        gameObject.tag = "StuckSpear"; 
        
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player != null){
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (spearCollider != null && playerCollider != null){
                Physics2D.IgnoreCollision(spearCollider, playerCollider, false);
            }
        }

        StartCoroutine(LifetimeSequence(TimeInWall));
    }

    private IEnumerator LifetimeSequence(float delay) {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
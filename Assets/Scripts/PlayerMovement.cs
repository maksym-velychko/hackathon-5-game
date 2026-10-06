using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour 
{
    public enum ProjectAxis { onlyX = 0, xAndY = 1 };
    
    [Header("Movement Settings")]
    public ProjectAxis projectAxis = ProjectAxis.onlyX;
    public float speed = 150;
    public float jumpForce = 10;
    public KeyCode leftButton = KeyCode.A;
    public KeyCode rightButton = KeyCode.D;
    public KeyCode upButton = KeyCode.W;
    public KeyCode downButton = KeyCode.S;
    public KeyCode jumpButton = KeyCode.Space;
    public bool isFacingRight = true;

    [Header("Dash Settings")]
    public float lungeDistance = 20f;
    public float lungeDuration = 0.3f;
    public float lungeCooldown = 1f;
    public KeyCode dashButton = KeyCode.LeftControl;
    
    [Header("Spear Settings")]
    public GameObject spearPrefab;
    public Transform throwPoint;
    public float throwForce = 20f;
    public KeyCode throwButton = KeyCode.Mouse0;
    public float throwCooldown = 1f;

    private bool lockLunge = false;
    private bool isLunging = false;
    private float nextThrowTime = 1f;
    private Vector2 moveDirection;
    private Rigidbody2D body;
    private bool isGrounded;
    private Animator anim;

    void Start() 
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (projectAxis == ProjectAxis.xAndY) 
        {
            body.gravityScale = 0;
            body.drag = 10; 
        }
    }

    void OnCollisionStay2D(Collision2D coll) 
    {
        if (coll.transform.CompareTag("Ground") || coll.transform.CompareTag("StuckSpear"))
        {
            body.drag = 10;
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D coll) 
    {
        if (coll.transform.CompareTag("Ground") || coll.transform.CompareTag("StuckSpear"))
        {
            body.drag = 0;
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        if (isLunging) return;

        body.AddForce(moveDirection * body.mass * speed);

        if (Mathf.Abs(body.velocity.x) > speed / 100f)
        {
            body.velocity = new Vector2(Mathf.Sign(body.velocity.x) * speed / 100f, body.velocity.y);
        }

        if (projectAxis == ProjectAxis.xAndY)
        {
            if (Mathf.Abs(body.velocity.y) > speed / 100f)
            {
                body.velocity = new Vector2(body.velocity.x, Mathf.Sign(body.velocity.y) * speed / 100f);
            }
        }
        else
        {
            if (Input.GetKey(jumpButton) && isGrounded)
            {
                body.velocity = new Vector2(body.velocity.x, jumpForce);
            }
        }
    }

    void Update() 
    {
        if (isLunging) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
        mousePos.z = 0;
        if (Input.GetKey(throwButton) && Time.time >= nextThrowTime && spearPrefab != null)
        {
            ThrowSpear(mousePos);
            nextThrowTime = Time.time + throwCooldown;
        }

        float horizontal = 0;
        float vertical = 0;

        if (Input.GetKey(leftButton)) horizontal = -1;
        else if (Input.GetKey(rightButton)) horizontal = 1;

        if (Input.GetKey(upButton)) vertical = 1;
        else if (Input.GetKey(downButton)) vertical = -1;

        if (anim != null)
        {
            bool moving = (horizontal != 0);
            if (projectAxis == ProjectAxis.xAndY) moving = (horizontal != 0 || vertical != 0);
            anim.SetBool("isRunning", moving); 
        }

        if (projectAxis == ProjectAxis.onlyX) 
        {
            moveDirection = new Vector2(horizontal, 0); 
        }
        else 
        {
            moveDirection = new Vector2(horizontal, vertical).normalized; 
        }

        if (horizontal > 0 && !isFacingRight) Flip();
        else if (horizontal < 0 && isFacingRight) Flip();

        if (Input.GetKeyDown(dashButton))
        {
            StartCoroutine(PerformLunge());
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f); 
    }
    
    void ThrowSpear(Vector3 targetPos)
    {
        Transform spawnPoint = (throwPoint != null) ? throwPoint : transform;
        Vector2 throwDirection = ((Vector2)targetPos - (Vector2)spawnPoint.position).normalized;
        Vector2 forwardVector = isFacingRight ? Vector2.right : Vector2.left;
        
        float currentAngle = Vector2.Angle(forwardVector, throwDirection);
        if (currentAngle > 85f)
        {
            float crossZ = (forwardVector.x * throwDirection.y) - (forwardVector.y * throwDirection.x);
            float sign = Mathf.Sign(crossZ);
            float baseAngle = isFacingRight ? 0f : 180f;
            float clampedAngle = baseAngle + (85f * sign);
            throwDirection = new Vector2(Mathf.Cos(clampedAngle * Mathf.Deg2Rad), Mathf.Sin(clampedAngle * Mathf.Deg2Rad));
        }

        float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        GameObject spear = Instantiate(spearPrefab, spawnPoint.position, rotation);
        
        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D spearCollider = spear.GetComponent<Collider2D>();
        if (playerCollider != null && spearCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, spearCollider);
        }

        Rigidbody2D spearRb = spear.GetComponent<Rigidbody2D>();
        if (spearRb != null)
        {
            spearRb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
        }
    }

    IEnumerator PerformLunge()
    {
        if (lockLunge) yield break;

        lockLunge = true;
        isLunging = true;

        if (anim != null) anim.SetTrigger("Dash");

        float originalGravity = body.gravityScale;
        body.gravityScale = 0f;

        Vector2 lungeDirection = isFacingRight ? Vector2.right : Vector2.left;
        body.velocity = lungeDirection * lungeDistance;

        yield return new WaitForSeconds(lungeDuration);

        body.gravityScale = originalGravity;
        body.velocity = Vector2.zero;
        isLunging = false;

        yield return new WaitForSeconds(lungeCooldown);
        lockLunge = false;
    }
}
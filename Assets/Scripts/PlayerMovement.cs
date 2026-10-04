using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour {

    public enum ProjectAxis {onlyX = 0, xAndY = 1};
    public ProjectAxis projectAxis = ProjectAxis.onlyX;
    public float speed = 150;
    public float addForce = 7;
    public bool lookAtCursor;
    public KeyCode leftButton = KeyCode.A;
    public KeyCode rightButton = KeyCode.D;
    public KeyCode upButton = KeyCode.W;
    public KeyCode downButton = KeyCode.S;
    public KeyCode addForceButton = KeyCode.Space;
    public bool isFacingRight = true;
    
    [Header("Spear Settings")]
    public GameObject spearPrefab;
    public Transform throwPoint;
    public float throwForce = 20f;
    public KeyCode throwButton = KeyCode.Mouse0;
    public float throwCooldown = 1f;
    
    private float nextThrowTime = 1f;
    private Vector3 direction;
    private float vertical;
    private float horizontal;
    private Rigidbody2D body;
    private float rotationY;
    private bool jump;

    private Animator anim;

    void Start () 
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        body.fixedAngle = true;

        if(projectAxis == ProjectAxis.xAndY) 
        {
            body.gravityScale = 0;
            body.drag = 10;
        }
    }

    void OnCollisionStay2D(Collision2D coll) 
    {
        if(coll.transform.tag == "Ground")
        {
            body.drag = 10;
            jump = true;
        }
    }
    
    void OnCollisionExit2D(Collision2D coll) 
    {
        if(coll.transform.tag == "Ground")
        {
            body.drag = 0;
            jump = false;
        }
    }
    
    void FixedUpdate()
    {
        body.AddForce(direction * body.mass * speed);

        if(Mathf.Abs(body.velocity.x) > speed/100f)
        {
            body.velocity = new Vector2(Mathf.Sign(body.velocity.x) * speed/100f, body.velocity.y);
        }

        if(projectAxis == ProjectAxis.xAndY)
        {
            if(Mathf.Abs(body.velocity.y) > speed/100f)
            {
                body.velocity = new Vector2(body.velocity.x, Mathf.Sign(body.velocity.y) * speed/100f);
            }
        }
        else
        {
            if(Input.GetKey(addForceButton) && jump)
            {
                body.velocity = new Vector2(0, addForce);
            }
        }
    }

    void Flip()
    {
        if(projectAxis == ProjectAxis.onlyX)
        {
            isFacingRight = !isFacingRight;
                Vector3 theScale = transform.localScale;
                theScale.x *= -1;
                transform.localScale = theScale;
        }
    }
    
    void Update () 
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
        mousePos.z = 0;
        if (Input.GetKey(throwButton) && Time.time >= nextThrowTime && spearPrefab != null)
        {
            ThrowSpear(mousePos);
            nextThrowTime = Time.time + throwCooldown;
        }
        if(Input.GetKey(upButton)) vertical = 1;
        else if(Input.GetKey(downButton)) vertical = -1; else vertical = 0;

        if(Input.GetKey(leftButton)) horizontal = -1;
        else if(Input.GetKey(rightButton)) horizontal = 1; else horizontal = 0;

        if (anim != null)
        {
            bool moving = (horizontal != 0);
            if (projectAxis == ProjectAxis.xAndY)
            {
                moving = (horizontal != 0 || vertical != 0);
            }

            anim.SetBool("isRunning", moving);
        }

        if (projectAxis == ProjectAxis.onlyX) 
        {
            direction = new Vector2(horizontal, 0); 
        }
        else 
        {
            if(Input.GetKeyDown(addForceButton)) speed += addForce; else if(Input.GetKeyUp(addForceButton)) speed -= addForce;
            direction = new Vector2(horizontal, vertical);
        }
        if (horizontal > 0 && !isFacingRight) 
        {
            Flip(); 
        }
        else if (horizontal < 0 && isFacingRight) 
        {
            Flip();
        }
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
}
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

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
    
    private bool lockLunge = false;
    private bool isLunging = false;
    public bool isAiming = false;
    private Vector2 moveDirection;
    private Rigidbody2D body;
    private bool isGrounded;
    private Animator anim;
    private bool jumpRequested = false;

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

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.CompareTag("Ground") || coll.gameObject.CompareTag("StuckSpear")){
            isGrounded = true;
            if (anim != null){
                anim.SetBool("isGrounded", true); 
            }
        }
    }

    void OnCollisionStay2D(Collision2D coll) 
    {
        if (coll.gameObject.CompareTag("Ground") || coll.gameObject.CompareTag("StuckSpear")){
            body.drag = 10;
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D coll) 
    {
        if (coll.gameObject.CompareTag("Ground") || coll.gameObject.CompareTag("StuckSpear")){
            body.drag = 0;
            isGrounded = false;

            if (anim != null) {
                anim.SetBool("isGrounded", false);
            }
        }
    }

    void Update() 
    {
        if (isLunging) return;

        bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
        mousePos.z = 0;
        isAiming = Input.GetMouseButton(1);

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

        if (projectAxis == ProjectAxis.onlyX) {
            if (Input.GetKeyDown(jumpButton) && isGrounded){
                jumpRequested = true;
                if (anim != null) {
                    anim.SetTrigger("isJumping");
                    anim.SetBool("isGrounded", false);
                }
            }
        }

        if (projectAxis == ProjectAxis.onlyX) {
            moveDirection = new Vector2(horizontal, 0); 
        }
        else {
            moveDirection = new Vector2(horizontal, vertical).normalized; 
        }
        
        if (isAiming){
            if (mousePos.x > transform.position.x && !isFacingRight) Flip();
            else if (mousePos.x < transform.position.x && isFacingRight) Flip();
        }
        else{
            if (horizontal > 0 && !isFacingRight) Flip();
            else if (horizontal < 0 && isFacingRight) Flip();
        }

        if (Input.GetKeyDown(dashButton)){
            StartCoroutine(PerformLunge());
        }
    }

    void FixedUpdate()
    {
        if (isLunging) return;
        float currentSpeed = isAiming ? speed / 2f : speed;
        body.AddForce(moveDirection * body.mass * currentSpeed);
        if (Mathf.Abs(body.velocity.x) > currentSpeed / 100f){
            body.velocity = new Vector2(Mathf.Sign(body.velocity.x) * currentSpeed / 100f, body.velocity.y);
        }

        if (projectAxis == ProjectAxis.xAndY){
            if (Mathf.Abs(body.velocity.y) > currentSpeed / 100f){
                body.velocity = new Vector2(body.velocity.x, Mathf.Sign(body.velocity.y) * currentSpeed / 100f);
            }
        }
        else{
            if (jumpRequested){
                body.velocity = new Vector2(body.velocity.x, jumpForce);
                jumpRequested = false;
            }
        }
    }

    public void Flip() 
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f); 
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
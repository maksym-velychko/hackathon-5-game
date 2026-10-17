using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))] // Автоматически добавит AudioSource на игрока, если его нет
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

    [Header("Audio Settings (Звуки)")]
    public AudioSource audioSource;
    public AudioClip jumpSound;       
    public AudioClip[] footstepSounds; 
    public AudioClip dashSound;
    public AudioClip landSound;
    public float stepRate = 0.35f;
    private float nextStepTime = 0f;
    
    private bool lockLunge = false;
    private bool isLunging = false;
    public bool isAiming = false;
    private Vector2 moveDirection;
    private Rigidbody2D body;
    private bool isGrounded;
    private Animator anim;
    private bool jumpRequested = false;

    // Флаг для отслеживания момента приземления
    private bool wasGroundedLastFrame;

    void Start() 
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (projectAxis == ProjectAxis.xAndY) 
        {
            body.gravityScale = 0;
            body.drag = 10; 
        }

        wasGroundedLastFrame = isGrounded;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.CompareTag("Ground") || coll.gameObject.CompareTag("StuckSpear")){
            isGrounded = true;
            if (anim != null){
                anim.SetBool("isGrounded", true); 
            }

            // Проигрываем звук приземления только если мы до этого летели/падали
            if (!wasGroundedLastFrame && landSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(landSound);
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
        
        bool moving = (horizontal != 0);
        if (projectAxis == ProjectAxis.xAndY) moving = (horizontal != 0 || vertical != 0);

        if (anim != null)
        {
            anim.SetBool("isRunning", moving); 
        }

        // --- ЛОГИКА ЗВУКОВ ХОДЬБЫ ---
        bool canPlayStep = (projectAxis == ProjectAxis.onlyX) ? isGrounded : true;
        if (moving && canPlayStep && Time.time >= nextStepTime)
        {
            PlayFootstepSound();
            // Если игрок целится (идет медленнее), увеличиваем паузу между шагами в 1.5 раза
            float currentStepRate = isAiming ? stepRate * 1.5f : stepRate;
            nextStepTime = Time.time + currentStepRate;
        }

        // --- ЛОГИКА ПРЫЖКА И ЗВУКА ПРЫЖКА ---
        if (projectAxis == ProjectAxis.onlyX) {
            if (Input.GetKeyDown(jumpButton) && isGrounded){
                jumpRequested = true;
                
                // Проигрываем звук прыжка
                if (jumpSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(jumpSound);
                }

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

        // Запоминаем текущее состояние земли для следующего кадра
        wasGroundedLastFrame = isGrounded;
    }

    // Метод для воспроизведения шага со случайным выбором звука и легким изменением тона
    void PlayFootstepSound()
    {
        if (footstepSounds != null && footstepSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, footstepSounds.Length);
            
            // Слегка меняем высоту звука (pitch), чтобы один и тот же звук не звучал как пулемет
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(footstepSounds[randomIndex]);
            audioSource.pitch = 1f; // Возвращаем стандартный питч
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
        if (dashSound != null && audioSource != null)
        {
    audioSource.PlayOneShot(dashSound);
        }

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
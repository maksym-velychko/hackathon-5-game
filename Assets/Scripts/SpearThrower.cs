using UnityEngine;
using UnityEngine.EventSystems;

public class SpearThrower : MonoBehaviour
{
    public GameObject spearPrefab;
    public Transform throwPoint;
    public float throwForce = 20f;
    public float throwCooldown = 1f;

    public float maxAimAngle = 80f;

    private float nextThrowTime = 1f;
    private bool isFacingRight = true;

    void Update()
    {
        AimAtCursor();

        bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (!isPointerOverUI && Input.GetMouseButtonDown(0) && Time.time >= nextThrowTime && spearPrefab != null){
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            ThrowSpear(mousePos);
            nextThrowTime = Time.time + throwCooldown;
        }
    }

    void AimAtCursor()
    {
        PlayerMovement player = GetComponentInParent<PlayerMovement>();
        if (player != null)
        {
            isFacingRight = player.isFacingRight;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector2 aimDirection = (mousePos - transform.position).normalized;
        Vector2 forwardVector = isFacingRight ? Vector2.right : Vector2.left;

        float currentAngle = Vector2.Angle(forwardVector, aimDirection);

        if (currentAngle > maxAimAngle)
        {
            float crossZ = (forwardVector.x * aimDirection.y) - (forwardVector.y * aimDirection.x);
            float sign = Mathf.Sign(crossZ);
            
            float baseAngle = isFacingRight ? 0f : 180f;
            float clampedAngle = baseAngle + (maxAimAngle * sign);
            
            aimDirection = new Vector2(Mathf.Cos(clampedAngle * Mathf.Deg2Rad), Mathf.Sin(clampedAngle * Mathf.Deg2Rad));
        }

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        if (!isFacingRight)
        {
            angle = 180f - angle;
        }
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    void ThrowSpear(Vector3 targetPos)
    {
        Transform spawnPoint = (throwPoint != null) ? throwPoint : transform;
        Vector2 throwDirection = ((Vector2)targetPos - (Vector2)spawnPoint.position).normalized;
        Vector2 forwardVector = isFacingRight ? Vector2.right : Vector2.left;
        
        float currentAngle = Vector2.Angle(forwardVector, throwDirection);
        
        // Используем maxAimAngle вместо жестко заданных 85 градусов
        if (currentAngle > maxAimAngle)
        {
            float crossZ = (forwardVector.x * throwDirection.y) - (forwardVector.y * throwDirection.x);
            float sign = Mathf.Sign(crossZ);
            float baseAngle = isFacingRight ? 0f : 180f;
            float clampedAngle = baseAngle + (maxAimAngle * sign);
            throwDirection = new Vector2(Mathf.Cos(clampedAngle * Mathf.Deg2Rad), Mathf.Sin(clampedAngle * Mathf.Deg2Rad));
        }

        float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        GameObject spear = Instantiate(spearPrefab, spawnPoint.position, rotation);
        
        Collider2D playerCollider = GetComponentInParent<Collider2D>();
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
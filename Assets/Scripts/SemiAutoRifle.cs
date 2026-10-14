using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SemiAutoRifle : MonoBehaviour
{
    [Header("Задержка между выстрелом")]
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;

    public float maxAimAngle = 80f;
    public bool isFacingRight = true;
    public Animator anim;
    public Transform firePoint;
    public GameObject bulletPrefab;

    void Update()
    {
        AimAtCursor();
        bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
                
        }
    }

    void AimAtCursor()
    {
        PlayerMovement player = GetComponentInParent<PlayerMovement>();
        if (player != null){
            isFacingRight = player.isFacingRight;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector2 aimDirection = (mousePos - transform.position).normalized;
        Vector2 forwardVector = isFacingRight ? Vector2.right : Vector2.left;
        float currentAngle = Vector2.Angle(forwardVector, aimDirection);

        if (currentAngle > maxAimAngle){
            float crossZ = (forwardVector.x * aimDirection.y) - (forwardVector.y * aimDirection.x);
            float sign = Mathf.Sign(crossZ);
            float baseAngle = isFacingRight ? 0f : 180f;
            float clampedAngle = baseAngle + (maxAimAngle * sign);
            aimDirection = new Vector2(Mathf.Cos(clampedAngle * Mathf.Deg2Rad), Mathf.Sin(clampedAngle * Mathf.Deg2Rad));
        }

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        if (!isFacingRight){
            angle = 180f - angle;
        }
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Shoot()
    {
        nextFireTime = Time.time + fireRate;
        if (anim != null){
            anim.SetTrigger("Shoot"); 
        }
        if (bulletPrefab != null && firePoint != null){
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }

    void Reload()
    {
        if (anim != null){
            anim.SetTrigger("Reload");
        }
    }
}
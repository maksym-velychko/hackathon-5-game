using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SemiAutoRifle : MonoBehaviour
{
    [Header("Задержка между выстрелом")]
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;

    [Header("Патроны и перезарядка")]
    public int maxAmmo = 10;          // Максимальное кол-во патронов в магазине
    public int currentAmmo;           // Текущее кол-во патронов
    public float reloadTime = 1.5f;   // Время перезарядки в секундах
    private bool isReloading = false; // Флаг: идет ли сейчас перезарядка

    [Header("Настройки прицеливания и стрельбы")]
    public float maxAimAngle = 80f;
    [Tooltip("Если спрайт винтовки визуально смотрит криво, подкрути этот угол (например, 15 или -15)")]
    public float aimAngleOffset = 0f; 
    public bool isFacingRight = true;
    public Animator anim;
    public Transform firePoint;
    public GameObject bulletPrefab;

    void Start()
    {
        currentAmmo = maxAmmo;
    }

    void OnDisable()
    {
        isReloading = false;
    }

    void Update()
    {
        // Если сейчас идет перезарядка, запрещаем стрелять и нажимать R повторно
        if (isReloading)
        {
            return;
        }

        // Автоматическая перезарядка, если патроны закончились
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // Ручная перезарядка на кнопку R (только если магазин не полный)
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        // Стреляем, только если есть патроны и мышка не наведена на UI
        if (!isPointerOverUI && Input.GetMouseButtonDown(0) && Time.time >= nextFireTime && currentAmmo > 0)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            Shoot(mousePos);
        }
    }

    // LateUpdate срабатывает ПОСЛЕ Animator, поэтому анимации не смогут сбить поворот оружия
    void LateUpdate()
    {
        AimAtCursor();
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

        // Применяем поворот вместе с поправкой (если она нужна для выравнивания спрайта)
        transform.localRotation = Quaternion.Euler(0f, 0f, angle + aimAngleOffset);
    }

    void Shoot(Vector3 targetPos)
    {
        nextFireTime = Time.time + fireRate;
        currentAmmo--;

        if (anim != null)
        {
            anim.SetTrigger("Shoot"); 
        }

        if (bulletPrefab != null)
        {
            // Точно как у гарпуна: считаем направление прямо от точки спавна (firePoint) до курсора мыши
            Transform spawnPoint = (firePoint != null) ? firePoint : transform;
            Vector2 shootDirection = ((Vector2)targetPos - (Vector2)spawnPoint.position).normalized;
            Vector2 forwardVector = isFacingRight ? Vector2.right : Vector2.left;

            float currentAngle = Vector2.Angle(forwardVector, shootDirection);

            if (currentAngle > maxAimAngle)
            {
                float crossZ = (forwardVector.x * shootDirection.y) - (forwardVector.y * shootDirection.x);
                float sign = Mathf.Sign(crossZ);
                float baseAngle = isFacingRight ? 0f : 180f;
                float clampedAngle = baseAngle + (maxAimAngle * sign);
                shootDirection = new Vector2(Mathf.Cos(clampedAngle * Mathf.Deg2Rad), Mathf.Sin(clampedAngle * Mathf.Deg2Rad));
            }

            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
            Quaternion bulletRotation = Quaternion.AngleAxis(angle, Vector3.forward);

            Instantiate(bulletPrefab, spawnPoint.position, bulletRotation);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;

        if (anim != null)
        {
            anim.SetTrigger("Reload");
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
    }
}
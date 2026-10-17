using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    [Header("Стартовая катсцена (Интро)")]
    [Tooltip("Показывать ли облет камеры при старте сцены")]
    public bool playIntroOnStart = true;
    [Tooltip("Точка, куда полетит камера (по умолчанию X:36, Y:65)")]
    public Vector2 introTargetCoords = new Vector2(36f, 65f);
    [Tooltip("Сколько секунд камера стоит на месте перед возвратом")]
    public float introHoldTime = 1f;
    [Tooltip("Скорость полета камеры во время интро (быстрее обычной)")]
    public float introFlySpeed = 10f; 

    [Header("Базовые настройки")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float offsetY = 2.5f;
    [SerializeField] private float startY = -1f;
    
    [SerializeField] private float deadzoneX = 3f; 
    [SerializeField] private float deadzoneY = 1.5f;

    [Header("Настройки прицеливания (ПКМ)")]
    [SerializeField] private float zoomFactor = 1.7f;
    [SerializeField] private float zoomSpeed = 5f;
    [Range(0f, 1f)]
    [SerializeField] private float cursorFollowWeight = 0.3f;
    [SerializeField] private float maxCameraOffset = 4f;

    private PlayerMovement _player;
    private Vector3 _targetPosition;
    
    private Camera _cam;
    private float _defaultZoom;
    private float _targetZoom;

    // Флаг, который показывает, идет ли сейчас катсцена
    private bool _isIntroPlaying = false;
    private Vector3 _introTargetPosition;

    void Start()
    {
        _targetPosition = transform.position;
        
        _cam = GetComponent<Camera>();
        if (_cam != null)
        {
            _defaultZoom = _cam.orthographicSize;
            _targetZoom = _defaultZoom;
        }

        // Если включено стартовое интро — запускаем катсцену
        if (playIntroOnStart)
        {
            StartCoroutine(IntroSequenceRoutine());
        }
    }

    IEnumerator IntroSequenceRoutine()
    {
        _isIntroPlaying = true;
        
        // Ждем 1 кадр, чтобы LateUpdate успел найти игрока через FindObjectOfType
        yield return null; 

        // 1. ОТКЛЮЧАЕМ УПРАВЛЕНИЕ И СТРЕЛЬБУ
        if (_player != null) 
        {
            // Отключаем передвижение
            _player.enabled = false;
            
            // Отключаем винтовку (ищет скрипт стрельбы на самом игроке или в его дочерних объектах)
            SemiAutoRifle rifle = _player.GetComponentInChildren<SemiAutoRifle>();
            if (rifle != null) rifle.enabled = false;
            
            // Если на старте игрок почему-то двигался, сбрасываем анимацию на стояние
            Animator anim = _player.GetComponent<Animator>();
            if (anim != null) anim.SetBool("isRunning", false);
        }

        // 2. ЛЕТИМ К ТОЧКЕ ИНТРО (X:36, Y:65)
        _introTargetPosition = new Vector3(introTargetCoords.x, introTargetCoords.y, transform.position.z);

        // Ждем, пока камера почти долетит до нужной точки (дистанция меньше 0.5)
        while (Vector2.Distance(transform.position, _introTargetPosition) > 0.5f)
        {
            yield return null;
        }

        // 3. ЖДЕМ 1 СЕКУНДУ НА МЕСТЕ
        yield return new WaitForSeconds(introHoldTime);

        // 4. ВОЗВРАЩАЕМСЯ К ИГРОКУ
        while (true)
        {
            if (_player != null)
            {
                // Во время полета обратно целимся в игрока с учетом стандартного смещения камеры вверх (offsetY)
                _introTargetPosition = new Vector3(_player.transform.position.x, _player.transform.position.y + offsetY, transform.position.z);
            }

            // Как только подлетели к игроку почти вплотную — выходим из цикла
            if (Vector2.Distance(transform.position, _introTargetPosition) < 0.2f)
            {
                break;
            }
            yield return null;
        }

        // 5. ВКЛЮЧАЕМ УПРАВЛЕНИЕ ОБРАТНО
        if (_player != null) 
        {
            _player.enabled = true;
            
            SemiAutoRifle rifle = _player.GetComponentInChildren<SemiAutoRifle>();
            if (rifle != null) rifle.enabled = true;
            
            // Синхронизируем мертвую зону с текущей позицией, чтобы камера не дернулась при возврате контроля
            _targetPosition = _player.transform.position;
        }

        _isIntroPlaying = false;
    }

    void LateUpdate()
    {
        if (_player == null)
        {
            _player = FindObjectOfType<PlayerMovement>();
            if (_player == null) return;
            
            // Если идет интро, не обновляем базовую цель мертвой зоны
            if (!_isIntroPlaying) 
            {
                _targetPosition = _player.transform.position;
            }
        }

        // --- ЛОГИКА ИНТРО-КАТСЦЕНЫ ---
        if (_isIntroPlaying)
        {
            // Во время катсцены камера плавно и быстро летит только к своей текущей цели (_introTargetPosition)
            transform.position = Vector3.Lerp(transform.position, _introTargetPosition, introFlySpeed * Time.deltaTime);
            return; // Дальнейшая сложная логика обычного слежения (с ПКМ и мертвой зоной) временно отключается
        }

        // --- СТАНДАРТНАЯ ЛОГИКА СЛЕЖЕНИЯ И МЕРТВОЙ ЗОНЫ ---
        Vector3 playerPos = _player.transform.position;

        if (playerPos.x > _targetPosition.x + deadzoneX)
        {
            _targetPosition.x = playerPos.x - deadzoneX;
        }
        else if (playerPos.x < _targetPosition.x - deadzoneX)
        {
            _targetPosition.x = playerPos.x + deadzoneX;
        }
        if (playerPos.y > _targetPosition.y + deadzoneY)
        {
            _targetPosition.y = playerPos.y - deadzoneY;
        }
        else if (playerPos.y < _targetPosition.y - deadzoneY)
        {
            _targetPosition.y = playerPos.y + deadzoneY;
        }

        Vector3 newPos = new Vector3(_targetPosition.x, _targetPosition.y + offsetY, transform.position.z);

        _targetZoom = _defaultZoom;
        Vector3 aimOffset = Vector3.zero;

        if (Input.GetMouseButton(1)) 
        {
            _targetZoom = _defaultZoom / zoomFactor;

            Vector3 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = playerPos.z;

            aimOffset = (mousePos - newPos) * cursorFollowWeight;
            aimOffset = Vector3.ClampMagnitude(aimOffset, maxCameraOffset);
            aimOffset.z = 0f;
        }

        newPos += aimOffset;

        if (newPos.y < startY)
        {
            newPos.y = startY;
        }

        transform.position = Vector3.Lerp(transform.position, newPos, speed * Time.deltaTime);

        if (_cam != null)
        {
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetZoom, zoomSpeed * Time.deltaTime);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying && !_isIntroPlaying ? new Vector3(_targetPosition.x, _targetPosition.y + offsetY, transform.position.z) : transform.position;
        Gizmos.DrawWireCube(center, new Vector3(deadzoneX * 2, deadzoneY * 2, 0));
    }
}
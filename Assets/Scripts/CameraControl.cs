using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
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

    void Start()
    {
        _targetPosition = transform.position;
        
        _cam = GetComponent<Camera>();
        if (_cam != null)
        {
            _defaultZoom = _cam.orthographicSize;
            _targetZoom = _defaultZoom;
        }
    }

    void LateUpdate()
    {
        if (_player == null)
        {
            _player = FindObjectOfType<PlayerMovement>();
            if (_player == null) return;
            
            _targetPosition = _player.transform.position;
        }

        Vector3 playerPos = _player.transform.position;

        // Логика мертвой зоны (обновляем базовую позицию цели)
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

        // Формируем базовую позицию с учетом оффсета
        Vector3 newPos = new Vector3(_targetPosition.x, _targetPosition.y + offsetY, transform.position.z);

        // --- НОВАЯ ЛОГИКА: Смещение к мыши и Зум ---
        _targetZoom = _defaultZoom;
        Vector3 aimOffset = Vector3.zero;

        if (Input.GetMouseButton(1)) // Если зажата ПКМ
        {
            _targetZoom = _defaultZoom / zoomFactor;

            // Вычисляем позицию мыши
            Vector3 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = playerPos.z;

            // Вычисляем смещение от базовой позиции (ограниченное)
            aimOffset = (mousePos - newPos) * cursorFollowWeight;
            aimOffset = Vector3.ClampMagnitude(aimOffset, maxCameraOffset);
            aimOffset.z = 0f;
        }

        // Применяем смещение от прицеливания
        newPos += aimOffset;

        // Ограничение по минимальной высоте (startY)
        if (newPos.y < startY)
        {
            newPos.y = startY;
        }

        // Плавное перемещение
        transform.position = Vector3.Lerp(transform.position, newPos, speed * Time.deltaTime);

        // Плавный зум
        if (_cam != null)
        {
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetZoom, zoomSpeed * Time.deltaTime);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? new Vector3(_targetPosition.x, _targetPosition.y + offsetY, transform.position.z) : transform.position;
        Gizmos.DrawWireCube(center, new Vector3(deadzoneX * 2, deadzoneY * 2, 0));
    }
}
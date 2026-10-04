using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float offsetY = 2.5f;
    [SerializeField] private float startY = -1f;
    
    [SerializeField] private float deadzoneX = 3f; 
    [SerializeField] private float deadzoneY = 1.5f;

    private PlayerMovement _player;
    private Vector3 _targetPosition;

    void Start()
    {
        _targetPosition = transform.position;
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
        if (newPos.y < startY)
        {
            newPos.y = startY;
        }

        transform.position = Vector3.Lerp(transform.position, newPos, speed * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? new Vector3(_targetPosition.x, _targetPosition.y + offsetY, transform.position.z) : transform.position;
        Gizmos.DrawWireCube(center, new Vector3(deadzoneX * 2, deadzoneY * 2, 0));
    }
}
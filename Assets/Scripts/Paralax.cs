using UnityEngine;

public class Paralax : MonoBehaviour
{
    [Tooltip("Kamera")]
    public Transform mainCamera;
    
    [Range(0f, 1f)]
    [Tooltip("0 - фон стоит на месте, 1 - фон жестко привязан к камере")]
    public float parallaxMultiplier;

    [Header("Настройки бесконечного фона")]
    public bool infiniteHorizontal = true;
    public bool infiniteVertical = false;

    private Vector3 startPosition;
    private Vector3 startCameraPosition;
    private float textureUnitSizeX;
    private float textureUnitSizeY;

    void Start()
    {
        if (mainCamera == null) 
        {
            mainCamera = Camera.main.transform;
        }
        
        startPosition = transform.position;
        startCameraPosition = mainCamera.position;

        // Автоматически вычисляем ширину и высоту ОДНОЙ оригинальной картинки в юнитах
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            Sprite sprite = sr.sprite;
            textureUnitSizeX = (sprite.texture.width / sprite.pixelsPerUnit) * transform.localScale.x;
            textureUnitSizeY = (sprite.texture.height / sprite.pixelsPerUnit) * transform.localScale.y;
        }
    }

    void LateUpdate()
    {
        Vector3 cameraMovement = mainCamera.position - startCameraPosition;
        Vector3 newPosition = startPosition + (cameraMovement * parallaxMultiplier);
        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);

        // Вычисляем, насколько камера уехала от фона
        Vector3 relativeCameraDist = mainCamera.position - transform.position;

        // Бесконечный повтор по горизонтали (Ось X)
        if (infiniteHorizontal && textureUnitSizeX > 0)
        {
            if (Mathf.Abs(relativeCameraDist.x) >= textureUnitSizeX)
            {
                float offsetPositionX = relativeCameraDist.x % textureUnitSizeX;
                startPosition.x = mainCamera.position.x - (cameraMovement.x * parallaxMultiplier) - offsetPositionX;
            }
        }

        // Бесконечный повтор по вертикали (Ось Y) — если уровень высокий
        if (infiniteVertical && textureUnitSizeY > 0)
        {
            if (Mathf.Abs(relativeCameraDist.y) >= textureUnitSizeY)
            {
                float offsetPositionY = relativeCameraDist.y % textureUnitSizeY;
                startPosition.y = mainCamera.position.y - (cameraMovement.y * parallaxMultiplier) - offsetPositionY;
            }
        }
    }
}
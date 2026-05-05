using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Minimap : MonoBehaviour
{
    [Header("Minimap Settings")]
    [SerializeField] private float mapHeight = 50f;
    [SerializeField] private float mapSize = 30f;
    [SerializeField] private bool rotateWithPlayer = true;

    [Header("Zoom")]
    [SerializeField] private float minZoom = 20f;
    [SerializeField] private float maxZoom = 50f;
    [SerializeField] private float zoomSpeed = 10f;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private Image playerIcon;
    [SerializeField] private GameObject enemyIconPrefab;

    [Header("Icon Colors")]
    [SerializeField] private Color playerColor = Color.cyan;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color teammateColor = Color.green;

    // Components
    private Camera minimapCamera;
    private RenderTexture renderTexture;

    // Enemy tracking
    private Dictionary<EnemyAI, RectTransform> enemyIcons = new Dictionary<EnemyAI, RectTransform>();

    void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null)
                player = pc.transform;
        }

        // Create minimap camera
        CreateMinimapCamera();

        // Create UI if not exists
        if (minimapImage == null)
            CreateMinimapUI();

        // Set player icon color
        if (playerIcon != null)
            playerIcon.color = playerColor;
    }

    void CreateMinimapCamera()
    {
        // Create camera object
        GameObject camObj = new GameObject("MinimapCamera");
        camObj.transform.SetParent(transform);

        minimapCamera = camObj.AddComponent<Camera>();
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = mapSize;
        minimapCamera.cullingMask = ~(1 << LayerMask.NameToLayer("Player")); // Don't render player layer
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        minimapCamera.depth = -10;

        // Create render texture
        renderTexture = new RenderTexture(256, 256, 16);
        renderTexture.Create();
        minimapCamera.targetTexture = renderTexture;

        // Position camera above player
        if (player != null)
        {
            camObj.transform.position = player.position + Vector3.up * mapHeight;
            camObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    void CreateMinimapUI()
    {
        // Find canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // Create minimap container
        GameObject container = new GameObject("MinimapContainer");
        container.transform.SetParent(canvas.transform);

        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(20, -20);
        containerRect.sizeDelta = new Vector2(180, 180);
        containerRect.localScale = Vector3.one;

        // Add mask for circular minimap
        Image maskImage = container.AddComponent<Image>();
        maskImage.color = new Color(0, 0, 0, 0.5f);
        Mask mask = container.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        // Create minimap image
        GameObject mapObj = new GameObject("MinimapImage");
        mapObj.transform.SetParent(container.transform);

        RectTransform mapRect = mapObj.AddComponent<RectTransform>();
        mapRect.anchorMin = Vector2.zero;
        mapRect.anchorMax = Vector2.one;
        mapRect.offsetMin = Vector2.zero;
        mapRect.offsetMax = Vector2.zero;
        mapRect.localScale = Vector3.one;

        minimapImage = mapObj.AddComponent<RawImage>();
        minimapImage.texture = renderTexture;

        // Create player icon (center)
        GameObject playerIconObj = new GameObject("PlayerIcon");
        playerIconObj.transform.SetParent(container.transform);

        RectTransform playerIconRect = playerIconObj.AddComponent<RectTransform>();
        playerIconRect.anchorMin = new Vector2(0.5f, 0.5f);
        playerIconRect.anchorMax = new Vector2(0.5f, 0.5f);
        playerIconRect.anchoredPosition = Vector2.zero;
        playerIconRect.sizeDelta = new Vector2(12, 12);
        playerIconRect.localScale = Vector3.one;

        playerIcon = playerIconObj.AddComponent<Image>();
        playerIcon.color = playerColor;

        // Create border
        GameObject border = new GameObject("Border");
        border.transform.SetParent(container.transform);

        RectTransform borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-3, -3);
        borderRect.offsetMax = new Vector2(3, 3);
        borderRect.localScale = Vector3.one;

        Image borderImage = border.AddComponent<Image>();
        borderImage.color = new Color(0.3f, 0.6f, 0.8f, 0.8f);
        borderImage.raycastTarget = false;

        // Move border behind
        border.transform.SetAsFirstSibling();

        // Store reference to container for enemy icons
        enemyIconPrefab = CreateEnemyIconPrefab(container.transform);
    }

    GameObject CreateEnemyIconPrefab(Transform parent)
    {
        GameObject iconObj = new GameObject("EnemyIconPrefab");
        iconObj.transform.SetParent(parent);
        iconObj.SetActive(false);

        RectTransform rect = iconObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(10, 10);
        rect.localScale = Vector3.one;

        Image img = iconObj.AddComponent<Image>();
        img.color = enemyColor;

        return iconObj;
    }

    void Update()
    {
        if (player == null || minimapCamera == null) return;

        UpdateCameraPosition();
        UpdateEnemyIcons();
        HandleZoom();
    }

    void UpdateCameraPosition()
    {
        // Follow player
        Vector3 newPos = player.position;
        newPos.y = mapHeight;
        minimapCamera.transform.position = newPos;

        // Rotate with player or stay north-up
        if (rotateWithPlayer)
        {
            minimapCamera.transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);

            // Keep player icon pointing up
            if (playerIcon != null)
                playerIcon.rectTransform.rotation = Quaternion.identity;
        }
        else
        {
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Rotate player icon to show direction
            if (playerIcon != null)
                playerIcon.rectTransform.rotation = Quaternion.Euler(0, 0, -player.eulerAngles.y);
        }
    }

    void UpdateEnemyIcons()
    {
        // Find all enemies
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();

        // Remove icons for dead/destroyed enemies
        List<EnemyAI> toRemove = new List<EnemyAI>();
        foreach (var pair in enemyIcons)
        {
            if (pair.Key == null || pair.Key.IsDead())
            {
                if (pair.Value != null)
                    Destroy(pair.Value.gameObject);
                toRemove.Add(pair.Key);
            }
        }
        foreach (var enemy in toRemove)
        {
            enemyIcons.Remove(enemy);
        }

        // Update/create icons for living enemies
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy.IsDead()) continue;

            RectTransform icon;
            if (!enemyIcons.TryGetValue(enemy, out icon))
            {
                // Create new icon
                GameObject newIcon = Instantiate(enemyIconPrefab, enemyIconPrefab.transform.parent);
                newIcon.SetActive(true);
                icon = newIcon.GetComponent<RectTransform>();
                enemyIcons[enemy] = icon;
            }

            // Update icon position
            UpdateIconPosition(icon, enemy.transform.position);
        }
    }

    void UpdateIconPosition(RectTransform icon, Vector3 worldPos)
    {
        if (minimapImage == null) return;

        // Calculate position relative to player
        Vector3 offset = worldPos - player.position;

        // Rotate offset if map rotates with player
        if (rotateWithPlayer)
        {
            offset = Quaternion.Euler(0, -player.eulerAngles.y, 0) * offset;
        }

        // Convert to minimap coordinates
        float mapScale = minimapImage.rectTransform.sizeDelta.x / (mapSize * 2f);
        Vector2 mapPos = new Vector2(offset.x, offset.z) * mapScale;

        // Clamp to minimap bounds
        float maxDist = minimapImage.rectTransform.sizeDelta.x / 2f - 10f;
        if (mapPos.magnitude > maxDist)
        {
            mapPos = mapPos.normalized * maxDist;
        }

        icon.anchoredPosition = mapPos;
    }

    void HandleZoom()
    {
        // Zoom with + and - keys
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
        {
            mapSize = Mathf.Max(minZoom, mapSize - zoomSpeed * Time.deltaTime);
            minimapCamera.orthographicSize = mapSize;
        }
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            mapSize = Mathf.Min(maxZoom, mapSize + zoomSpeed * Time.deltaTime);
            minimapCamera.orthographicSize = mapSize;
        }
    }

    // Public methods
    public void SetZoom(float zoom)
    {
        mapSize = Mathf.Clamp(zoom, minZoom, maxZoom);
        if (minimapCamera != null)
            minimapCamera.orthographicSize = mapSize;
    }

    public void ToggleRotation()
    {
        rotateWithPlayer = !rotateWithPlayer;
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}
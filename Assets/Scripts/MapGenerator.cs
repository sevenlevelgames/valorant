using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Size")]
    [SerializeField] private float mapWidth = 50f;
    [SerializeField] private float mapLength = 50f;
    [SerializeField] private float wallHeight = 5f;

    [Header("Ground Settings")]
    [SerializeField] private Color groundColor = new Color(0.3f, 0.3f, 0.35f);
    [SerializeField] private Color groundAccentColor = new Color(0.25f, 0.25f, 0.3f);

    [Header("Wall Settings")]
    [SerializeField] private Color wallColor = new Color(0.4f, 0.4f, 0.45f);
    [SerializeField] private Color wallAccentColor = new Color(0.5f, 0.3f, 0.2f);

    [Header("Cover Settings")]
    [SerializeField] private int boxCount = 15;
    [SerializeField] private int wallCoverCount = 8;
    [SerializeField] private Color coverColor = new Color(0.35f, 0.35f, 0.4f);

    [Header("Spawn Points")]
    [SerializeField] private int enemySpawnCount = 4;

    [Header("Lighting")]
    [SerializeField] private bool createLighting = true;
    [SerializeField] private Color ambientColor = new Color(0.6f, 0.65f, 0.75f);

    // References
    private GameObject mapParent;
    private List<Transform> enemySpawnPoints = new List<Transform>();
    private Transform playerSpawnPoint;

    void Start()
    {
        // Don't auto-generate, use context menu in editor instead
        // Map should be pre-generated and baked before play
    }

    [ContextMenu("1 - Generate Map")]
    public void GenerateMap()
    {
        // Clear existing map
        ClearMap();

        // Create parent object
        mapParent = new GameObject("GeneratedMap");
        mapParent.transform.position = Vector3.zero;

        // Generate map elements
        CreateGround();
        CreateOuterWalls();
        CreateMidStructure();
        CreateCoverBoxes();
        CreateWallCovers();
        CreateRamps();
        CreateSpawnPoints();

        if (createLighting)
            SetupLighting();

        // Setup NavMesh layer
        SetupNavMesh();

        Debug.Log("Map generated! Now add NavMeshSurface to Ground and Bake, then delete MapGenerator object.");
    }

    [ContextMenu("2 - Clear Map")]
    public void ClearMapManual()
    {
        ClearMap();
        Debug.Log("Map cleared!");
    }

    void ClearMap()
    {
        // Destroy old map
        GameObject oldMap = GameObject.Find("GeneratedMap");
        if (oldMap != null)
            DestroyImmediate(oldMap);

        enemySpawnPoints.Clear();
    }

    void CreateGround()
    {
        // Main ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.SetParent(mapParent.transform);
        ground.transform.position = new Vector3(0, -0.5f, 0);
        ground.transform.localScale = new Vector3(mapWidth, 1f, mapLength);

        ground.layer = LayerMask.NameToLayer("Ground");

        Renderer groundRenderer = ground.GetComponent<Renderer>();
        groundRenderer.material = CreateMaterial(groundColor);

        // Ground accent stripes
        CreateGroundAccents();
    }

    void CreateGroundAccents()
    {
        // Center line
        GameObject centerLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        centerLine.name = "CenterLine";
        centerLine.transform.SetParent(mapParent.transform);
        centerLine.transform.position = new Vector3(0, 0.01f, 0);
        centerLine.transform.localScale = new Vector3(mapWidth, 0.02f, 2f);

        centerLine.GetComponent<Renderer>().material = CreateMaterial(groundAccentColor);
        Destroy(centerLine.GetComponent<Collider>());

        // Side lines
        for (int i = -1; i <= 1; i += 2)
        {
            GameObject sideLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideLine.name = "SideLine";
            sideLine.transform.SetParent(mapParent.transform);
            sideLine.transform.position = new Vector3(0, 0.01f, i * (mapLength / 2 - 5));
            sideLine.transform.localScale = new Vector3(mapWidth - 10, 0.02f, 0.5f);

            sideLine.GetComponent<Renderer>().material = CreateMaterial(wallAccentColor);
            Destroy(sideLine.GetComponent<Collider>());
        }
    }

    void CreateOuterWalls()
    {
        float halfWidth = mapWidth / 2;
        float halfLength = mapLength / 2;
        float thickness = 1f;

        // North wall
        CreateWall("WallNorth", new Vector3(0, wallHeight / 2, halfLength),
            new Vector3(mapWidth, wallHeight, thickness));

        // South wall
        CreateWall("WallSouth", new Vector3(0, wallHeight / 2, -halfLength),
            new Vector3(mapWidth, wallHeight, thickness));

        // East wall
        CreateWall("WallEast", new Vector3(halfWidth, wallHeight / 2, 0),
            new Vector3(thickness, wallHeight, mapLength));

        // West wall
        CreateWall("WallWest", new Vector3(-halfWidth, wallHeight / 2, 0),
            new Vector3(thickness, wallHeight, mapLength));
    }

    void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(mapParent.transform);
        wall.transform.position = position;
        wall.transform.localScale = scale;

        wall.GetComponent<Renderer>().material = CreateMaterial(wallColor);

        // Add accent stripe
        GameObject accent = GameObject.CreatePrimitive(PrimitiveType.Cube);
        accent.name = "Accent";
        accent.transform.SetParent(wall.transform);
        accent.transform.localPosition = new Vector3(0, 0.3f, 0.51f);
        accent.transform.localScale = new Vector3(1f, 0.1f, 0.02f);
        accent.GetComponent<Renderer>().material = CreateMaterial(wallAccentColor);
        Destroy(accent.GetComponent<Collider>());
    }

    void CreateMidStructure()
    {
        // Central structure (like a small building)
        GameObject midStructure = new GameObject("MidStructure");
        midStructure.transform.SetParent(mapParent.transform);
        midStructure.transform.position = Vector3.zero;

        // Main block
        GameObject mainBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mainBlock.name = "MainBlock";
        mainBlock.transform.SetParent(midStructure.transform);
        mainBlock.transform.localPosition = new Vector3(0, 1.5f, 0);
        mainBlock.transform.localScale = new Vector3(8f, 3f, 8f);
        mainBlock.GetComponent<Renderer>().material = CreateMaterial(wallColor);

        // Roof
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.SetParent(midStructure.transform);
        roof.transform.localPosition = new Vector3(0, 3.1f, 0);
        roof.transform.localScale = new Vector3(10f, 0.2f, 10f);
        roof.GetComponent<Renderer>().material = CreateMaterial(groundAccentColor);

        // Openings (doorways)
        CreateDoorway(midStructure.transform, new Vector3(4f, 1f, 0), Quaternion.identity);
        CreateDoorway(midStructure.transform, new Vector3(-4f, 1f, 0), Quaternion.identity);
        CreateDoorway(midStructure.transform, new Vector3(0, 1f, 4f), Quaternion.Euler(0, 90, 0));
        CreateDoorway(midStructure.transform, new Vector3(0, 1f, -4f), Quaternion.Euler(0, 90, 0));

        // Side structures
        CreateSideStructure(new Vector3(15, 0, 10));
        CreateSideStructure(new Vector3(-15, 0, 10));
        CreateSideStructure(new Vector3(15, 0, -10));
        CreateSideStructure(new Vector3(-15, 0, -10));
    }

    void CreateDoorway(Transform parent, Vector3 position, Quaternion rotation)
    {
        // Door frame creates opening illusion
        GameObject doorFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorFrame.name = "DoorFrame";
        doorFrame.transform.SetParent(parent);
        doorFrame.transform.localPosition = position + Vector3.up * 1.5f;
        doorFrame.transform.localRotation = rotation;
        doorFrame.transform.localScale = new Vector3(0.3f, 0.5f, 3f);
        doorFrame.GetComponent<Renderer>().material = CreateMaterial(wallAccentColor);
    }

    void CreateSideStructure(Vector3 position)
    {
        GameObject structure = new GameObject("SideStructure");
        structure.transform.SetParent(mapParent.transform);
        structure.transform.position = position;

        // L-shaped wall
        GameObject wall1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall1.name = "Wall1";
        wall1.transform.SetParent(structure.transform);
        wall1.transform.localPosition = new Vector3(0, 1.5f, 0);
        wall1.transform.localScale = new Vector3(6f, 3f, 0.5f);
        wall1.GetComponent<Renderer>().material = CreateMaterial(wallColor);

        GameObject wall2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall2.name = "Wall2";
        wall2.transform.SetParent(structure.transform);
        wall2.transform.localPosition = new Vector3(2.75f, 1.5f, 2f);
        wall2.transform.localScale = new Vector3(0.5f, 3f, 4f);
        wall2.GetComponent<Renderer>().material = CreateMaterial(wallColor);
    }

    void CreateCoverBoxes()
    {
        for (int i = 0; i < boxCount; i++)
        {
            Vector3 position = GetRandomPosition();

            // Avoid center structure
            if (Vector3.Distance(position, Vector3.zero) < 8f)
                continue;

            CreateCoverBox(position);
        }
    }

    void CreateCoverBox(Vector3 position)
    {
        float height = Random.Range(1f, 2f);
        float width = Random.Range(1.5f, 3f);
        float depth = Random.Range(1.5f, 3f);

        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "CoverBox";
        box.transform.SetParent(mapParent.transform);
        box.transform.position = position + Vector3.up * (height / 2);
        box.transform.localScale = new Vector3(width, height, depth);
        box.transform.rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90f, 0);

        box.GetComponent<Renderer>().material = CreateMaterial(coverColor);

        // Add accent on top
        GameObject topAccent = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topAccent.name = "TopAccent";
        topAccent.transform.SetParent(box.transform);
        topAccent.transform.localPosition = new Vector3(0, 0.51f, 0);
        topAccent.transform.localScale = new Vector3(0.9f, 0.02f, 0.9f);
        topAccent.GetComponent<Renderer>().material = CreateMaterial(wallAccentColor);
        Destroy(topAccent.GetComponent<Collider>());
    }

    void CreateWallCovers()
    {
        for (int i = 0; i < wallCoverCount; i++)
        {
            float x = Random.Range(-mapWidth / 2 + 5, mapWidth / 2 - 5);
            float z = Random.Range(-mapLength / 2 + 5, mapLength / 2 - 5);

            // Avoid center
            if (Mathf.Abs(x) < 12f && Mathf.Abs(z) < 12f)
                continue;

            CreateWallCover(new Vector3(x, 0, z));
        }
    }

    void CreateWallCover(Vector3 position)
    {
        GameObject wallCover = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallCover.name = "WallCover";
        wallCover.transform.SetParent(mapParent.transform);
        wallCover.transform.position = position + Vector3.up * 1.5f;

        float length = Random.Range(4f, 8f);
        wallCover.transform.localScale = new Vector3(length, 3f, 0.5f);
        wallCover.transform.rotation = Quaternion.Euler(0, Random.Range(0, 180), 0);

        wallCover.GetComponent<Renderer>().material = CreateMaterial(wallColor);
    }

    void CreateRamps()
    {
        // Create ramps to elevated areas
        CreateRamp(new Vector3(10, 0, 0), 0);
        CreateRamp(new Vector3(-10, 0, 0), 180);
    }

    void CreateRamp(Vector3 position, float rotation)
    {
        GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = "Ramp";
        ramp.transform.SetParent(mapParent.transform);
        ramp.transform.position = position + new Vector3(0, 0.5f, 0);
        ramp.transform.localScale = new Vector3(3f, 0.2f, 6f);
        ramp.transform.rotation = Quaternion.Euler(15f, rotation, 0);

        ramp.GetComponent<Renderer>().material = CreateMaterial(groundAccentColor);

        // Platform at top
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "Platform";
        platform.transform.SetParent(mapParent.transform);

        Vector3 platformOffset = Quaternion.Euler(0, rotation, 0) * new Vector3(0, 0, -4f);
        platform.transform.position = position + platformOffset + new Vector3(0, 1.5f, 0);
        platform.transform.localScale = new Vector3(5f, 0.3f, 4f);
        platform.transform.rotation = Quaternion.Euler(0, rotation, 0);

        platform.GetComponent<Renderer>().material = CreateMaterial(coverColor);
    }

    void CreateSpawnPoints()
    {
        // Player spawn (one side)
        GameObject playerSpawn = new GameObject("PlayerSpawnPoint");
        playerSpawn.transform.SetParent(mapParent.transform);
        playerSpawn.transform.position = new Vector3(0, 1, -mapLength / 2 + 5);
        playerSpawnPoint = playerSpawn.transform;

        // Enemy spawns (other side and corners)
        Vector3[] enemySpawnPositions = {
            new Vector3(0, 1, mapLength / 2 - 5),
            new Vector3(mapWidth / 2 - 5, 1, 0),
            new Vector3(-mapWidth / 2 + 5, 1, 0),
            new Vector3(mapWidth / 2 - 8, 1, mapLength / 2 - 8),
            new Vector3(-mapWidth / 2 + 8, 1, mapLength / 2 - 8)
        };

        GameObject spawnPointsParent = new GameObject("EnemySpawnPoints");
        spawnPointsParent.transform.SetParent(mapParent.transform);

        for (int i = 0; i < enemySpawnCount && i < enemySpawnPositions.Length; i++)
        {
            GameObject enemySpawn = new GameObject($"SpawnPoint_{i}");
            enemySpawn.transform.SetParent(spawnPointsParent.transform);
            enemySpawn.transform.position = enemySpawnPositions[i];
            enemySpawnPoints.Add(enemySpawn.transform);

            // Visual indicator
            CreateSpawnIndicator(enemySpawn.transform, Color.red);
        }

        // Create spawn indicator for player
        CreateSpawnIndicator(playerSpawnPoint, Color.blue);

        Debug.Log($"Created {enemySpawnPoints.Count} enemy spawn points and 1 player spawn point.");
        Debug.Log("After baking NavMesh, manually assign spawn points to EnemySpawner and DeathScreen!");
    }

    void CreateSpawnIndicator(Transform parent, Color color)
    {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        indicator.name = "SpawnIndicator";
        indicator.transform.SetParent(parent);
        indicator.transform.localPosition = Vector3.zero;
        indicator.transform.localScale = new Vector3(1f, 0.05f, 1f);

        Renderer renderer = indicator.GetComponent<Renderer>();
        Material mat = CreateMaterial(color);
        mat.SetColor("_EmissionColor", color * 0.5f);
        mat.EnableKeyword("_EMISSION");
        renderer.material = mat;

        Destroy(indicator.GetComponent<Collider>());
    }

    void SetupLighting()
    {
        // Set ambient light
        RenderSettings.ambientLight = ambientColor;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        // Find or create directional light
        Light mainLight = FindObjectOfType<Light>();
        if (mainLight != null)
        {
            mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0);
            mainLight.intensity = 1.2f;
            mainLight.color = new Color(1f, 0.95f, 0.9f);
            mainLight.shadows = LightShadows.Soft;
        }

        // Add some point lights for atmosphere
        CreatePointLight(new Vector3(0, 4, 0), new Color(0.8f, 0.9f, 1f), 15f);
        CreatePointLight(new Vector3(15, 3, 15), new Color(1f, 0.8f, 0.6f), 10f);
        CreatePointLight(new Vector3(-15, 3, -15), new Color(1f, 0.8f, 0.6f), 10f);
    }

    void CreatePointLight(Vector3 position, Color color, float range)
    {
        GameObject lightObj = new GameObject("PointLight");
        lightObj.transform.SetParent(mapParent.transform);
        lightObj.transform.position = position;

        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = color;
        pointLight.range = range;
        pointLight.intensity = 0.8f;
    }

    void SetupNavMesh()
    {
        // Set ground layer for all walkable surfaces
        foreach (Transform child in mapParent.transform)
        {
            if (child.name.Contains("Ground") || child.name.Contains("Ramp") || child.name.Contains("Platform"))
            {
                child.gameObject.layer = LayerMask.NameToLayer("Ground");
            }
        }

        // Find ground object and add NavMeshSurface
        Transform groundTransform = mapParent.transform.Find("Ground");
        if (groundTransform != null)
        {
            // Add NavMeshSurface component if AI Navigation package is installed
            // User needs to manually add NavMeshSurface and bake
            Debug.Log("Map generated! Add 'NavMeshSurface' component to Ground object and click 'Bake' for enemy navigation.");
        }
    }

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(-mapWidth / 2 + 3, mapWidth / 2 - 3);
        float z = Random.Range(-mapLength / 2 + 3, mapLength / 2 - 3);
        return new Vector3(x, 0, z);
    }

    Material CreateMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.3f);
        return mat;
    }

    // Public getters
    public Transform GetPlayerSpawnPoint() => playerSpawnPoint;
    public List<Transform> GetEnemySpawnPoints() => enemySpawnPoints;

    // Gizmos
    void OnDrawGizmosSelected()
    {
        // Draw map bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(mapWidth, wallHeight, mapLength));

        // Draw spawn points
        if (playerSpawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerSpawnPoint.position, 1f);
        }

        foreach (Transform spawn in enemySpawnPoints)
        {
            if (spawn != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(spawn.position, 1f);
            }
        }
    }
}
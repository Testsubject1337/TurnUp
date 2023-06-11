using System.Collections.Generic;
using UnityEngine;

public class AnchorPointGenerator : MonoBehaviour
{
    [SerializeField] private GameObject defaultAnchorPrefab;
    [SerializeField] private GameObject movingAnchorPrefab;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float spawnHeightOffset = 1.5f;
    [SerializeField] private float despawnHeightOffset = 10f;
    [SerializeField] private float minSpawnDistance = 8f; // Umbenannt von startSpawnDistance
    [SerializeField] private float maxSpawnDistance = 15f; // Umbenannt von endSpawnDistance
    [SerializeField] private float interpolationStartHeight = 0f; // Umbenannt von startHeight
    [SerializeField] private float interpolationEndHeight = 3000f; // Umbenannt von endHeight
    [SerializeField] private float specialAnchorStartHeight = 500f; // Start-Höhe für den speziellen Anker
    [SerializeField] private int defaultAnchorPoolSize = 8;
    [SerializeField] private int movingAnchorPoolSize = 8;
    [SerializeField] private bool debugEnabled = false;

    private const float DespawnThreshold = -4f; // Threshold for despawning anchors

    private Queue<GameObject> defaultAnchorPointPool;
    private Queue<GameObject> movingAnchorPointPool;
    private List<GameObject> activeDefaultAnchorPoints;
    private List<GameObject> activeMovingAnchorPoints;
    private float lastSpawnHeight = 0f;
    private float specialAnchorSpawnProbability = 0.2f; // Wahrscheinlichkeit für den speziellen Anker

    private void Start()
    {
        lastSpawnHeight = mainCamera.transform.position.y + mainCamera.orthographicSize + spawnHeightOffset;

        // Initialize object pools
        defaultAnchorPointPool = new Queue<GameObject>();
        movingAnchorPointPool = new Queue<GameObject>();
        activeDefaultAnchorPoints = new List<GameObject>();
        activeMovingAnchorPoints = new List<GameObject>();

        for (int i = 0; i < defaultAnchorPoolSize; i++)
        {
            GameObject defaultAnchorPoint = Instantiate(defaultAnchorPrefab);
            defaultAnchorPoint.SetActive(false);
            defaultAnchorPoint.GetComponent<AnchorPoint>().ResetState();
            defaultAnchorPointPool.Enqueue(defaultAnchorPoint);
        }

        for (int i = 0; i < movingAnchorPoolSize; i++)
        {
            GameObject movingAnchorPoint = Instantiate(movingAnchorPrefab);
            movingAnchorPoint.SetActive(false);
            movingAnchorPoint.GetComponent<AnchorPoint>().ResetState();
            movingAnchorPointPool.Enqueue(movingAnchorPoint);
        }
    }

    private void Update()
    {
        // Only spawn points if game is running
        if (GameManager.Instance.IsGameRunning)
        {
            // Calculate current spawn distance based on camera height
            float t = Mathf.InverseLerp(interpolationStartHeight, interpolationEndHeight, mainCamera.transform.position.y);
            float currentSpawnDistance = Mathf.Lerp(minSpawnDistance, maxSpawnDistance, t);

            // If necessary, spawn new anchor points
            float screenTop = mainCamera.transform.position.y + mainCamera.orthographicSize;
            while (lastSpawnHeight < screenTop + spawnHeightOffset)
            {
                float spawnProbability = CalculateSpawnProbability();
                SpawnAnchorPoint(currentSpawnDistance, spawnProbability);
            }
        }

        // Recycle anchors under the screen
        RecycleAnchors();
    }

    private float CalculateSpawnProbability()
    {
        // Calculate spawn probability based on score
        float spawnProbability = 1f;
        int score = GameManager.Instance.getScore();

        if (score >= 500)
        {
            float scoreMultiplier = (score - 500) * specialAnchorSpawnProbability;
            spawnProbability += scoreMultiplier;
        }

        // Clamp spawn probability to a maximum of 1
        spawnProbability = Mathf.Clamp01(spawnProbability);

        return spawnProbability;
    }

    private void SpawnAnchorPoint(float currentSpawnDistance, float spawnProbability)
    {
        bool isSpecialAnchor = false;

        // Check Camera height for allowing SpecialAnchors
        if (mainCamera.transform.position.y >= specialAnchorStartHeight)
        {
            float modifiedSpawnProbability = Mathf.Clamp01(spawnProbability * (mainCamera.transform.position.y - specialAnchorStartHeight) / (interpolationEndHeight - specialAnchorStartHeight)); // Anpassung der Spawn-Wahrscheinlichkeit basierend auf der Kamera-Höhe und Begrenzung auf den Bereich [0, 1]
            isSpecialAnchor = Random.value <= modifiedSpawnProbability;
        }

        DebugLog("isSpecialAnchor: " + isSpecialAnchor);

        GameObject anchorPoint = null;
        AnchorPoint anchorPointComponent = null;

        if (isSpecialAnchor && movingAnchorPointPool.Count > 0)
        {
            anchorPoint = movingAnchorPointPool.Dequeue();
            anchorPointComponent = anchorPoint.GetComponent<AnchorPoint>();
            if (anchorPointComponent == null)
            {
                Debug.LogError("AnchorPoint component not found on Moving Anchor prefab!");
            }

            activeMovingAnchorPoints.Add(anchorPoint);

            DebugLog("Using movingAnchorPoint from the pool");
        }
        else
        {
            anchorPoint = defaultAnchorPointPool.Dequeue();
            anchorPointComponent = anchorPoint.GetComponent<AnchorPoint>();
            if (anchorPointComponent == null)
            {
                Debug.LogError("AnchorPoint component not found on Default Anchor prefab!");
            }

            activeDefaultAnchorPoints.Add(anchorPoint);

            DebugLog("Using defaultAnchorPoint from the pool");
        }

        if (anchorPoint == null || anchorPointComponent == null)
        {
            Debug.LogError("Failed to spawn anchorPoint or anchorPointComponent is null!");
            return;
        }

        // Calculate spawn position
        float spawnY = lastSpawnHeight + currentSpawnDistance;
        float spawnX = Random.Range(mainCamera.transform.position.x - mainCamera.aspect * mainCamera.orthographicSize + 1f,
                                     mainCamera.transform.position.x + mainCamera.aspect * mainCamera.orthographicSize - 1f);
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);

        // Set anchor point position and enable it
        anchorPoint.transform.position = spawnPosition;
        anchorPoint.SetActive(true);

        DebugLog("Spawned anchorPoint at position: " + spawnPosition);

        // Update last spawn height
        lastSpawnHeight = spawnY;
    }

    private void RecycleAnchors()
    {
        // Recycle default anchor points
        for (int i = activeDefaultAnchorPoints.Count - 1; i >= 0; i--)
        {
            GameObject anchorPoint = activeDefaultAnchorPoints[i];
            if (anchorPoint.transform.position.y < mainCamera.transform.position.y - mainCamera.orthographicSize + DespawnThreshold
                && !IsPointVisibleOnScreen(anchorPoint.transform.position))
            {
                RecycleAnchorPoint(anchorPoint, i);
            }
        }

        // Recycle moving anchor points
        for (int i = activeMovingAnchorPoints.Count - 1; i >= 0; i--)
        {
            GameObject anchorPoint = activeMovingAnchorPoints[i];
            if (anchorPoint.transform.position.y < mainCamera.transform.position.y - mainCamera.orthographicSize + DespawnThreshold
                && !IsPointVisibleOnScreen(anchorPoint.transform.position))
            {
                DebugLog("Trying to Recycle Moving Anchor...");
                RecycleAnchorPoint(anchorPoint, i);
            }
        }
    }

    private bool IsPointVisibleOnScreen(Vector3 point)
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(point);
        return screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1;
    }

    private void RecycleAnchorPoint(GameObject anchorPoint, int index)
    {
        // Disable the anchor point and put it back into the appropriate pool
        anchorPoint.SetActive(false);

        if (activeMovingAnchorPoints.Contains(anchorPoint))
        {
            activeMovingAnchorPoints.RemoveAt(index);
            movingAnchorPointPool.Enqueue(anchorPoint);
        }
        else if (activeDefaultAnchorPoints.Contains(anchorPoint))
        {
            activeDefaultAnchorPoints.RemoveAt(index);
            defaultAnchorPointPool.Enqueue(anchorPoint);
        }
    }

    private void DebugLog(string message)
    {
        if (debugEnabled)
        {
            Debug.Log(message);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class AnchorPointGenerator : MonoBehaviour
{
    // Required Game Objects
    [SerializeField] private Anchor defaultAnchorPrefab;
    [SerializeField] private Anchor movingAnchorPrefab;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameManager gameManager;


    // Gameplay Settings
    [SerializeField] private float spawnDistanceMin = 5f;
    [SerializeField] private float spawnDistanceMax = 15f;
    [SerializeField] private float spawnDistanceVariance = 2f;
    [SerializeField] private int spawnProbabilityIncreaseScore = 500;
    [SerializeField] private float movingAnchorInitialSpawnProbability = 0.1f;
    [SerializeField] private float movingAnchorMaxSpawnProbability = 0.4f;

    // Technical Settings
    private const float despawnTolerance = -4f;
    private const float spawnTolerance = 4f;

    // Debug Settings
    [SerializeField] private bool debugEnabled = false;

    // Other
    private ObjectPool<Anchor> defaultAnchorPointPool;
    private ObjectPool<Anchor> movingAnchorPointPool;
    private List<Anchor> activeAnchorPoints;
    private float lastSpawnHeight;
    private float currentSpawnDistance;
    private float currentSpawnProbability;
    private float screenTop;
    private void Start()
    {
        UpdateScreenTop();
        lastSpawnHeight = screenTop;

        // Initialize object pools
        defaultAnchorPointPool = new ObjectPool<Anchor>(defaultAnchorPrefab, 16);
        movingAnchorPointPool = new ObjectPool<Anchor>(movingAnchorPrefab, 16);

        activeAnchorPoints = new List<Anchor>();

        currentSpawnDistance = spawnDistanceMin;
        currentSpawnProbability = 0f;
    }


    private void Update()
    {
        UpdateScreenTop();
        // Only spawn points if game is running
        if (gameManager.IsGameRunning)
        {
            SpawnAnchorPoints();
        }

        // Recycle anchors under the screen
        RecycleAnchors();

        // Update spawn probability and distance based on score
        UpdateSpawnParameters();
    }

    private void SpawnAnchorPoints()
    {
        

        while (lastSpawnHeight < screenTop)
        {
            // Determine which anchor to spawn based on probability
            float spawnRoll = Random.value;
            Anchor newAnchorPoint;

            if (spawnRoll <= currentSpawnProbability)
            {
                newAnchorPoint = movingAnchorPointPool.GetObject();
            }
            else
            {
                newAnchorPoint = defaultAnchorPointPool.GetObject();
            }

            activeAnchorPoints.Add(newAnchorPoint);


            // Place newAnchorPoint at lastSpawnHeight and random x position
            float spawnX = Random.Range(-mainCamera.orthographicSize * mainCamera.aspect, mainCamera.orthographicSize * mainCamera.aspect);
            Vector3 newSpawnPoint = new Vector3(spawnX, lastSpawnHeight, 0);
            newAnchorPoint.UpdateSpawnPosition(newSpawnPoint);
            newAnchorPoint.transform.position = newSpawnPoint;
            newAnchorPoint.SetActive(true);

            // Update last spawn height based on a random value within the spawn distance variance
            spawnDistanceVariance = Random.Range(-spawnDistanceVariance, spawnDistanceVariance);
            lastSpawnHeight += Mathf.Min(currentSpawnDistance + spawnDistanceVariance, spawnDistanceMax);
        }
    }

    private void RecycleAnchors()
    {
        float screenBottom = mainCamera.transform.position.y - mainCamera.orthographicSize + despawnTolerance;

        for (int i = activeAnchorPoints.Count - 1; i >= 0; i--)
        {
            if (activeAnchorPoints[i].transform.position.y < screenBottom)
            {
                activeAnchorPoints[i].SetActive(false);
                // Depending on its type, return it to the correct pool
                if (activeAnchorPoints[i].GetType() == typeof(DefaultAnchor))
                {
                    defaultAnchorPointPool.ReturnObject(activeAnchorPoints[i]);
                    activeAnchorPoints.RemoveAt(i);
                }
                else if (activeAnchorPoints[i].GetType() == typeof(MovingAnchor))
                {
                    movingAnchorPointPool.ReturnObject(activeAnchorPoints[i]);
                    activeAnchorPoints.RemoveAt(i);
                }
                else
                {
                    DebugLog("RecycleAnchors failed at Count " + activeAnchorPoints.Count); 
                }
            }
        }
    }

    private void UpdateSpawnParameters()
    {
        int currentScore = gameManager.GetScore();
        float interpolationPosition = gameManager.GetInterpolationPosition();

        // Check if the current score is greater than spawnProbabilityIncreaseScore
        if (currentScore > spawnProbabilityIncreaseScore)
        {
            // Calculate new spawn probability
            float spawnProbabilityRatio = (currentScore - spawnProbabilityIncreaseScore) / (float)spawnProbabilityIncreaseScore;
            currentSpawnProbability = Mathf.Lerp(movingAnchorInitialSpawnProbability, movingAnchorMaxSpawnProbability, spawnProbabilityRatio);
        }

        // Calculate the new spawn distance
        float spawnDistanceRatio = currentScore / (float)interpolationPosition; // Normalize the ratio to [0,1]
        spawnDistanceRatio = Mathf.Clamp(spawnDistanceRatio, 0, 1); // Ensure the ratio is within the range [0,1]
        currentSpawnDistance = Mathf.Lerp(spawnDistanceMin, spawnDistanceMax, spawnDistanceRatio);
    }

    private void UpdateScreenTop()
    {
        screenTop = mainCamera.transform.position.y + mainCamera.orthographicSize + spawnTolerance;
    }

    private void DebugLog(string message)
    {
        if (debugEnabled)
        {
            Debug.Log("AnchorPointGenerator: " + message);
        }
    }
}

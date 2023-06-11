using UnityEngine;
using System.Collections.Generic;

public class BackgroundTileSpawner : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private float tileSize = 100.0f;
    [SerializeField] private float darkenFactor = 0.7f;  // Factor to darken the tile colors
    [SerializeField] private int poolSize = 5;  // Size of the object pool

    private Camera mainCamera;
    private Transform cameraTransform;
    private float nextTileSpawnY;
    private Color lastColor; // Stores the color of the last spawned tile
    private Queue<GameObject> tilePool; // Pool of tile objects

    private void Start()
    {
        mainCamera = Camera.main;
        cameraTransform = mainCamera.transform;

        // Calculate where the next tile should be spawned
        nextTileSpawnY = Mathf.Floor(cameraTransform.position.y / tileSize) * tileSize;

        // Initialize the tile pool
        tilePool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject newTile = Instantiate(tilePrefab, transform);
            newTile.SetActive(false);
            tilePool.Enqueue(newTile);
        }

        // Initially spawn tiles covering the visible area and the size of two additional tiles
        while (nextTileSpawnY - tileSize * 2 < cameraTransform.position.y + mainCamera.orthographicSize)
        {
            SpawnTile();
        }
    }

    private void Update()
    {
        // If the camera has passed the position of the second last tile, spawn a new tile at the top and recycle the oldest one at the bottom
        if (cameraTransform.position.y > nextTileSpawnY - tileSize)
        {
            SpawnTile();
            RecycleOldestTile();
        }
    }

    private void SpawnTile()
    {
        // Get the next tile from the pool
        GameObject newTile = tilePool.Dequeue();
        newTile.transform.position = new Vector3(0, nextTileSpawnY + tileSize / 2, 1.1f);
        newTile.GetComponent<SpriteRenderer>().sortingOrder = -3;

        Color newColor;

        // Generate a color in the opposite quarter of the color circle for the third tile onwards
        if (nextTileSpawnY > tileSize)
        {
            float h, s, v;
            Color.RGBToHSV(lastColor, out h, out s, out v);
            h = (h + 0.5f + Random.Range(-0.1f, 0.1f)) % 1; // Shift hue to the opposite quarter of the color circle with some random variation
            newColor = Color.HSVToRGB(h, Random.Range(0.6f, 0.8f), Random.Range(0.8f, 1f)); // Generate pastel colors
                                                                                            // Darken the color
            newColor *= darkenFactor;
        }
        else if (nextTileSpawnY == tileSize)  // Second tile (random color)
        {
            newColor = Random.ColorHSV(0, 1, 0.6f, 0.8f, 0.8f, 1f); // Generate pastel colors
                                                                    // Darken the color
            newColor *= darkenFactor;
        }
        else  // First tile (grey)
        {
            newColor = new Color32(0x3F, 0x3F, 0x3F, 0xFF);
        }

        newTile.GetComponent<SpriteRenderer>().color = newColor;

        // Scale the tile according to the size of the screen
        float width = mainCamera.aspect * 2f * mainCamera.orthographicSize;
        float height = tileSize;
        newTile.transform.localScale = new Vector3(width / newTile.GetComponent<SpriteRenderer>().sprite.bounds.size.x, height / newTile.GetComponent<SpriteRenderer>().sprite.bounds.size.y, 1);


        // Update the position where the next tile should be spawned
        nextTileSpawnY += tileSize;

        // Save this color for the next tile
        lastColor = newColor;

        // Reactivate the tile and put it back in the pool for later use
        newTile.SetActive(true);
        tilePool.Enqueue(newTile);
    }

    private void RecycleOldestTile()
    {
        // Recycle the oldest tile (the one at the bottom of the hierarchy)
        if (transform.childCount > 0)
        {
            GameObject oldestTile = transform.GetChild(0).gameObject;
            oldestTile.SetActive(false);
        }
    }
}

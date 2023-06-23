// Players Movement. Highly depending on the AnchorPoints.

using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private LayerMask anchorPointLayer;
    [SerializeField] private CameraFollow cameraFollow;

    private GameManager gameManager;
    private bool isHanging = false;
    private Anchor currentAnchorPoint;
    private Rigidbody2D rb;
    private LineRenderer lineRenderer;
    private Renderer renderer;

    // Cache for main camera
    private Camera mainCamera;

    private Vector3 screenBounds;
    private float topPercentageThreshold = 0.25f;
    private float topThresholdY;
    private float playerWidth;
    private float playerHeight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        gameManager = FindObjectOfType<GameManager>();
        renderer = GetComponent<Renderer>();
        mainCamera = Camera.main;

        rb.gravityScale = 0f;
        lineRenderer.enabled = false;

        screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z));
        playerWidth = renderer.bounds.extents.x;
        playerHeight = renderer.bounds.extents.y;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, anchorPointLayer);
            if (hit.collider != null)
            {
                SetAnchor(hit.collider.GetComponent<Anchor>());
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ReleaseAnchor();
        }

        if (isHanging)
        {
            UpdateLineRenderer();
        }
    }

    private void FixedUpdate()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        KeepPlayerWithinScreenBounds();

        if (gameManager.IsGameRunning && isHanging && currentAnchorPoint != null)
        {
            currentAnchorPoint.ApplyAnchorEffect(this);
        }
    }

    private void SetAnchor(Anchor anchorPoint)
    {
        currentAnchorPoint = anchorPoint;
        isHanging = true;
        lineRenderer.enabled = true;

        if (!gameManager.IsGameRunning)
        {
            StartGame();
        }
    }

    public void ReleaseAnchor()
    {
        currentAnchorPoint = null;
        isHanging = false;
        lineRenderer.enabled = false;
    }

    private void UpdateLineRenderer()
    {
        if (currentAnchorPoint != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, currentAnchorPoint.transform.position);
        }
    }
    private void KeepPlayerWithinScreenBounds()
    {
        float x = Mathf.Clamp(transform.position.x, -screenBounds.x + playerWidth, screenBounds.x - playerWidth);

        // Check if player has reached the edge of the screen
        if (transform.position.x <= -screenBounds.x + playerWidth || transform.position.x >= screenBounds.x - playerWidth)
        {
            // Calculate the new x velocity with 20% of the original velocity
            float newVelocityX = -rb.velocity.x * 0.2f;
            rb.velocity = new Vector2(newVelocityX, rb.velocity.y);

            // Apply torque proportional to the velocity at the moment of impact
            float torque = rb.velocity.magnitude;

            // Check the direction of the ball and adjust the torque accordingly
            if (transform.position.x >= screenBounds.x - playerWidth)
            {
                // If ball is moving upwards, rotate counter-clockwise, otherwise rotate clockwise
                torque *= rb.velocity.y > 0 ? -1 : 1;
            }
            else if (transform.position.x <= -screenBounds.x + playerWidth)
            {
                // If ball is moving upwards, rotate clockwise, otherwise rotate counter-clockwise
                torque *= rb.velocity.y > 0 ? 1 : -1;
            }

            rb.AddTorque(torque);
        }

        // Calculate the top threshold based on the visible area of the camera
        float topThresholdY = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1f - topPercentageThreshold, 0)).y;

        // Checks if camera has to speed up
        if (transform.position.y >= topThresholdY)
        {
            cameraFollow.speedup = true;
        }
        else
        {
            cameraFollow.speedup = false;
        }

        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    private void StartGame()
    {
        gameManager.StartGame();
        rb.gravityScale = 1.3f;
    }

}

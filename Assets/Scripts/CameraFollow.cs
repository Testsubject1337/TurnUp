using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 0.1f;
    [SerializeField] private float smoothTimeIncrease = 0.3f;
    [SerializeField] private float smoothTimeDecrease = 0.3f;
    [SerializeField] private float heightFactor = 0.001f;  // How much the height affects the speed
    [SerializeField] private float maxSpeedIncrease = 3f;  // Maximum additional speed from height
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform playerPosition;

    public bool speedup = false;

    private float currentSpeed;
    private float targetSpeed;
    private float speedVelocity;
    private float lastCameraY = 0f;
    private Rigidbody2D playerRigidbody;
    private Camera mainCamera;

    private void Start()
    {
        currentSpeed = baseSpeed;
        targetSpeed = baseSpeed;
        playerRigidbody = playerPosition.GetComponent<Rigidbody2D>();
        mainCamera = GetComponent<Camera>();
        mainCamera.backgroundColor = new Color32(0x3F, 0x3F, 0x3F, 0xFF);  // Set initial color to grey
    }

    private void FixedUpdate()
    {
        if (gameManager.IsGameRunning && !gameManager.IsPaused)
        {
            if (speedup)
            {
                targetSpeed = Mathf.Max(maxSpeed, playerRigidbody.velocity.y);
            }
            else
            {
                float speedIncrease = Mathf.Min(maxSpeedIncrease, heightFactor * Mathf.Pow(transform.position.y, 2));
                targetSpeed = baseSpeed + speedIncrease;
            }

            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, speedup ? smoothTimeIncrease : smoothTimeDecrease);

            float newCameraY = transform.position.y + currentSpeed * Time.deltaTime;
            if (newCameraY - lastCameraY >= 100f)
            {
                lastCameraY = newCameraY;
            }

            transform.Translate(Vector3.up * currentSpeed * Time.deltaTime);

            if (currentSpeed > maxSpeed)
            {
                currentSpeed = maxSpeed;
            }
        }
    }

    public void StopCamera()
    {
        currentSpeed = 0f;
    }

    public void SetSpeedup(bool isSpeedup)
    {
        speedup = isSpeedup;
    }
}

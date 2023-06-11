using UnityEngine;

public class MovingAnchor : AnchorPoint
{
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private Vector2 direction = Vector2.right;
    [SerializeField] private float range = 3.0f;

    private LineRenderer lineRenderer;
    private Vector2 startPosition;
    private Vector2 endPosition;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        anchorPointType = AnchorPointType.Special;
    }

    private void Start()
    {
        ResetState();
    }

    public override void ResetState()
    {
        startPosition = (Vector2)transform.position - (direction.normalized * range / 2);
        endPosition = (Vector2)transform.position + (direction.normalized * range / 2);

        // Check if lineRenderer is not null before trying to access it
        if (lineRenderer != null)
        {
            // Set the line renderer initial positions
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
        }
        else
        {
            Debug.LogError("LineRenderer component is missing on MovingAnchor GameObject.");
        }
    }


    private void Update()
    {
        // Update position based on direction, speed and time
        transform.position = startPosition + (direction.normalized * Mathf.PingPong(Time.time * speed, range));

        // Keep the object within the screen bounds
        var viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        viewportPos.x = Mathf.Clamp(viewportPos.x, 0.0f, 1.0f);
        //viewportPos.y = Mathf.Clamp(viewportPos.y, 0.0f, 1.0f);
        transform.position = Camera.main.ViewportToWorldPoint(viewportPos);
    }
}

using UnityEngine;

public class MovingAnchor : Anchor
{
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private Vector2 direction = Vector2.right;
    [SerializeField] private float range = 3.0f;

    private LineRenderer lineRenderer;
    private Vector2 startPosition;
    private Vector2 endPosition;

    MovingAnchor()
    {
        anchorType = AnchorPointType.Moving;

        debugEnabled = true;
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }


    public override void ResetState()
    {
        DebugLog("Reset State called. ");

        startPosition = (Vector2)spawnPoint - (direction.normalized * range / 2);
        endPosition = (Vector2)spawnPoint + (direction.normalized * range / 2);

        // Check if lineRenderer is not null before trying to access it
        if (lineRenderer != null)
        {
            // Set the line renderer initial positions
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
        }
        else
        {
            DebugLog("LineRenderer component is missing on MovingAnchor GameObject.", true);
        }
    }


    private void Update()
    {
        if (isActive)
        {
            // Update position based on direction, speed and time
            transform.position = startPosition + (direction.normalized * Mathf.PingPong(Time.time * speed, range));
        }



        //// Keep the object within the screen bounds
        //var viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        //viewportPos.x = Mathf.Clamp(viewportPos.x, 0.0f, 1.0f);
        //viewportPos.y = Mathf.Clamp(viewportPos.y, 0.0f, 1.0f);
        //transform.position = Camera.main.ViewportToWorldPoint(viewportPos);
    }
}

using UnityEngine;

public class MovingAnchor : Anchor
{

    public bool isRandomized = false;

    private LineRenderer lineRenderer;
    private float speed = 1.2f;
    private float range = 3.2f;
    private Vector2 direction = Vector2.right;
    private Vector2 startPosition;
    private Vector2 endPosition;
    

    MovingAnchor()
    {
        anchorType = AnchorPointType.Moving;
        debugEnabled = true;
        width = new Vector2(4.2f, 4.2f);
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

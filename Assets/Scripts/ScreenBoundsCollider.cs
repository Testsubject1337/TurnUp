using UnityEngine;

public class ScreenBoundsCollider : MonoBehaviour
{
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        UpdateCollider();
    }

    private void UpdateCollider()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenLimitX = Camera.main.ScreenToWorldPoint(new Vector3(screenWidth, 0f)).x;
        float screenLimitY = Camera.main.ScreenToWorldPoint(new Vector3(0f, screenHeight)).y;

        Vector2 colliderSize = new Vector2(screenLimitX * 2f, screenLimitY * 2f);
        boxCollider.size = colliderSize;

        transform.position = new Vector3(0f, screenLimitY + colliderSize.y / 2f, 0f);
    }
}

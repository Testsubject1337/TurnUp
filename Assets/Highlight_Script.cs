using UnityEngine;

public class Highlight_Script : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    private Vector3 playerPosition;

    private void Update()
    {
        if (playerTransform != null)
        {
            playerPosition = playerTransform.position;
            if (transform.position != playerPosition)
            {
                transform.position = playerPosition;
            }
        }
    }
}
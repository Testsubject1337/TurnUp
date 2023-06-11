using Newtonsoft.Json.Converters;
using UnityEngine;

public enum AnchorPointType { Default, Moving, Ghost };


public abstract class Anchor : MonoBehaviour
{
    // Gameplay-Settings
    protected float ropeReleaseRadius = 1f;
    protected float ropeForce = 75f;
    protected AnchorPointType anchorType;

    // Technical Temps
    protected Vector3 spawnPoint;
    protected bool isActive = false;

    // Debug
    protected bool debugEnabled = false;




    // Basic Movement
    public virtual void ApplyAnchorEffect(PlayerMovement player)
    {
        Vector2 ropeDirection = transform.position - player.transform.position;
        float distanceToAnchor = ropeDirection.magnitude;

        if (distanceToAnchor > ropeReleaseRadius)
        {
            player.GetComponent<Rigidbody2D>().AddForce(ropeDirection.normalized * ropeForce);
        }
        else
        {
            player.ReleaseAnchor();
        }
    }

    public virtual void ResetState()
    {
        // this is empty in the base class but can be overridden by derived classes
    }

    public void UpdateSpawnPosition(Vector3 transform)
    {
        spawnPoint = transform;
    }

    public AnchorPointType GetAnchorType()
    {
        return anchorType;
    }
    public float GetRopeReleaseRadius()
    {
        return ropeReleaseRadius;
    }

    public float GetRopeForce()
    {
        return ropeForce;
    }

    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
        if (isActive)
        {
            ResetState();
        }
        if (debugEnabled)
        {
            DebugLog("Set Active to " + isActive);
        }
    }



    //Editor Only
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }

    // Debug
    protected void DebugLog(string message)
    {
        if (debugEnabled)
        {
            Debug.Log("Anchor of type " + anchorType + ": " + message);
        }
        
    }

    protected void DebugLog(string message, bool isError)
    {
        if (debugEnabled)
        {
            if (isError)
            {
                Debug.LogError("Anchor of type " + anchorType + ": " + message);
            }
            else
            {
                Debug.Log("Anchor of type " + anchorType + ": " + message);
            }
            
        }

    }


}
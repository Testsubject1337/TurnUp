using UnityEngine;

public abstract class AnchorPoint : MonoBehaviour
{
    [SerializeField] protected float ropeReleaseRadius = 1f;
    [SerializeField] protected float ropeForce = 50f;

    public float RopeReleaseRadius { get { return ropeReleaseRadius; } }
    public float RopeForce { get { return ropeForce; } }
    public enum AnchorPointType { Default, Special };
    public AnchorPointType anchorPointType;


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }

    public virtual void ApplyAnchorEffect(PlayerMovement player)
    {
        Vector2 ropeDirection = transform.position - player.transform.position;
        float distanceToAnchor = ropeDirection.magnitude;

        if (distanceToAnchor > RopeReleaseRadius)
        {
            player.GetComponent<Rigidbody2D>().AddForce(ropeDirection.normalized * RopeForce);
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
}

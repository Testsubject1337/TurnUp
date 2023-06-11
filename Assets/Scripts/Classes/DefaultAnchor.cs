using UnityEngine;

public class DefaultAnchor : Anchor
{
    private void Awake()
    {
        anchorType = AnchorPointType.Default;
    }
}

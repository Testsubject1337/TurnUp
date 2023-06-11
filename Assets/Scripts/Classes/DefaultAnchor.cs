using UnityEngine;

public class DefaultAnchor : AnchorPoint
{
    private void Awake()
    {
        anchorPointType = AnchorPointType.Default;
    }
}

using UnityEngine;

[CreateAssetMenu(
    fileName = "PlacementVisualConfig",
    menuName = "Balance Game/Placement Visual Config"
)]
public class PlacementVisualConfig : ScriptableObject
{
    [SerializeField] private Material placeableGhostMaterial;
    [SerializeField] private Material notPlaceableGhostMaterial;

    public Material PlaceableGhostMaterial => placeableGhostMaterial;
    public Material NotPlaceableGhostMaterial => notPlaceableGhostMaterial;
}

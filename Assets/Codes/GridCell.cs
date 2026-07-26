using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField] private BalanceObject occupyingObject;
    [SerializeField] private int x;
    [SerializeField] private int z;

    public BalanceObject OccupyingObject => occupyingObject;
    public int X => x;
    public int Z => z;
    public bool IsOccupied => occupyingObject != null;

    public void Initialize(int newX, int newZ)
    {
        x = newX;
        z = newZ;
    }

    public void Occupy(BalanceObject placeableObject)
    {
        occupyingObject = placeableObject;
    }

    public void Clear()
    {
        occupyingObject = null;
    }
}
using UnityEngine;

public class Slot : MonoBehaviour
{
    public int gridX;
    public int gridY;
    public bool isEmpty = true;

    public PlateItem currentPlate;

    public void Initialize(int x, int y)
    {
        gridX = x;
        gridY = y;
    }
}
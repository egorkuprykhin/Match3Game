using UnityEngine;

public class TargetData
{
    public Sprite Type;
    public int Count;
    public bool Collected => Count == 0;

    public void CollectTarget(int count)
    {
        Count -= count;
        Count = Mathf.Max(0, Count);
    }
}
using UnityEngine;
public class RoomNode : Node
{
    public RoomNode(Vector2Int bottomLeftAreaCorner, Vector2Int topRightAreaCorner, Node parentNode, int index) : base(parentNode)
    {
        this.BottomLeftAreaCorner = bottomLeftAreaCorner;
        this.TopRightAreaCorner = topRightAreaCorner;
        this.BottomRightAreaCorner = new Vector2Int(topRightAreaCorner.x, bottomLeftAreaCorner.y);
        this.TopLeftAreaCorner = new Vector2Int(bottomLeftAreaCorner.x, TopRightAreaCorner.y);
        this.TreeLayerIndex = index;
        int rand1 = Random.Range(1, 4);        
        if (rand1 == 1)
        {
            this.heightPos = -1.5f;
        }
        else if (rand1 == 2)
        {
            this.heightPos = 0f;
        }
        else
        {
            this.heightPos = 1.5f;
        }

        this.height = Random.Range(1, 4);
        Vector2 sum = bottomLeftAreaCorner + topRightAreaCorner;
        Vector2 temp = sum / 2;
        this.midPoint = new Vector3(temp.x, this.heightPos, temp.y);
    }

    public int Width { get => (int)(TopRightAreaCorner.x - BottomLeftAreaCorner.x); }
    public int Length { get => (int)(TopRightAreaCorner.y - BottomLeftAreaCorner.y); }


}


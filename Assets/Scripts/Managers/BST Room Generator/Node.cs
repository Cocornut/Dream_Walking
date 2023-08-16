using System;
using System.Collections.Generic;
using UnityEngine;
public abstract class Node
{
    private List<Node> childrenNodeList;

    public List<Node> ChildrenNodeList { get => childrenNodeList; }

    public bool Visted { get; set; }
    public Vector2Int BottomLeftAreaCorner { get; set; }
    public Vector2Int BottomRightAreaCorner { get; set; }
    public Vector2Int TopRightAreaCorner { get; set; }
    public Vector2Int TopLeftAreaCorner { get; set; }
    public float heightPos { get; set; }
    public int diff { get; set; }
    public int height { get; set; }
    public bool lowerStructureWall { get; set; }

    public RoomType roomType { get; set; }

    public Vector3 midPoint { get; set; }

    public Vector3 terrace1BL { get; set; }
    public Vector3 terrace1TR { get; set; }
    public Vector3 terrace2BL { get; set; }
    public Vector3 terrace2TR { get; set; }

    public Node Parent { get; set; }


    public int TreeLayerIndex { get; set; }

    public Node(Node parentNode)
    {
        childrenNodeList = new List<Node>();
        this.Parent = parentNode;
        if (parentNode != null)
        {
            parentNode.AddChild(this);
        }
    }

    public void AddChild(Node node)
    {
        childrenNodeList.Add(node);

    }

    public void RemoveChild(Node node)
    {
        childrenNodeList.Remove(node);
    }
}

public enum RoomType
{
    PlayerRoom,
    FirstKeyRoom,
    ExitKeyRoom,
    EnemyRoom,
    ExitRoom,
    RegularRoom,
    Corridor
}
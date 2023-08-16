﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CorridorNode : Node
{
    private Node structure1;
    private Node structure2;
    private int corridorWidth;

    public CorridorNode(Node node1, Node node2, int corridorWidth) : base(null)
    {
        this.structure1 = node1;
        this.structure2 = node2;
        this.corridorWidth = corridorWidth;
        lowerStructureWall = false;
        roomType = RoomType.Corridor;
        GenerateCorridor();
    }

    private void GenerateCorridor()
    {
        var relativePositionOfStructure2 = CheckPositionStructure2AgainstStructure1();
        switch (relativePositionOfStructure2)
        {
            case RelativePosition.Up:
                ProcessRoomInRelationUpOrDown(this.structure1, this.structure2);
                break;
            case RelativePosition.Down:
                ProcessRoomInRelationUpOrDown(this.structure2, this.structure1);
                break;
            case RelativePosition.Right:
                ProcessRoomInRelationRightOrLeft(this.structure1, this.structure2);
                break;
            case RelativePosition.Left:
                ProcessRoomInRelationRightOrLeft(this.structure2, this.structure1);
                break;
            default:
                break;
        }
    }

    private void ProcessRoomInRelationRightOrLeft(Node structure1, Node structure2)
    {
        Node leftStructure = null;
        List<Node> leftStructureChildren = StructureHelper.TraverseGraphToExtractLowestLeafes(structure1);
        Node rightStructure = null;
        List<Node> rightStructureChildren = StructureHelper.TraverseGraphToExtractLowestLeafes(structure2);

        var sortedLeftStructure = leftStructureChildren.OrderByDescending(child => child.TopRightAreaCorner.x).ToList();
        if (sortedLeftStructure.Count == 1)
        {
            leftStructure = sortedLeftStructure[0];
        }
        else
        {
            int maxX = sortedLeftStructure[0].TopRightAreaCorner.x;
            sortedLeftStructure = sortedLeftStructure.Where(children => Math.Abs(maxX - children.TopRightAreaCorner.x) < 10).ToList();
            int index = UnityEngine.Random.Range(0, sortedLeftStructure.Count);
            leftStructure = sortedLeftStructure[index];
        }

        var possibleNeighboursInRightStructureList = rightStructureChildren.Where(
            child => GetValidYForNeighourLeftRight(
                leftStructure.TopRightAreaCorner,
                leftStructure.BottomRightAreaCorner,
                child.TopLeftAreaCorner,
                child.BottomLeftAreaCorner
                ) != -1
            ).OrderBy(child => child.BottomRightAreaCorner.x).ToList();

        if (possibleNeighboursInRightStructureList.Count <= 0)
        {
            rightStructure = structure2;
        }
        else
        {
            rightStructure = possibleNeighboursInRightStructureList[0];
        }
        int y = GetValidYForNeighourLeftRight(leftStructure.TopLeftAreaCorner, leftStructure.BottomRightAreaCorner,
            rightStructure.TopLeftAreaCorner,
            rightStructure.BottomLeftAreaCorner);
        while (y == -1 && sortedLeftStructure.Count > 1)
        {
            sortedLeftStructure = sortedLeftStructure.Where(
                child => child.TopLeftAreaCorner.y != leftStructure.TopLeftAreaCorner.y).ToList();
            leftStructure = sortedLeftStructure[0];
            y = GetValidYForNeighourLeftRight(leftStructure.TopLeftAreaCorner, leftStructure.BottomRightAreaCorner,
            rightStructure.TopLeftAreaCorner,
            rightStructure.BottomLeftAreaCorner);
        }
        BottomLeftAreaCorner = new Vector2Int(leftStructure.BottomRightAreaCorner.x, y);
        TopRightAreaCorner = new Vector2Int(rightStructure.TopLeftAreaCorner.x, y + this.corridorWidth);

        GetDifferenceForLeftRight(leftStructure, rightStructure);
    }

    private int GetDifferenceForLeftRight(Node leftStructure, Node rightStructure)
    {
        if (leftStructure.heightPos < rightStructure.heightPos)
        {
            heightPos = leftStructure.heightPos;
            if (leftStructure.heightPos == rightStructure.heightPos - 1.5f)
            {
                diff = 1;
            }
            if (leftStructure.heightPos == rightStructure.heightPos - 3f)
            {
                diff = 2;
            }
            if (leftStructure.height > 1)
            {
                lowerStructureWall = true;
            }
        }
        else
        {
            heightPos = rightStructure.heightPos;
            if (leftStructure.heightPos == rightStructure.heightPos + 1.5f)
            {
                diff = 3;
            }
            if (leftStructure.heightPos == rightStructure.heightPos + 3f)
            {
                diff = 4;
            }
            if (rightStructure.height > 1)
            {
                lowerStructureWall = true;
            }
        }
        return 0;
    }



    private int GetValidYForNeighourLeftRight(Vector2Int leftNodeUp, Vector2Int leftNodeDown, Vector2Int rightNodeUp, Vector2Int rightNodeDown)
    {
        leftNodeUp = new Vector2Int(leftNodeUp.x, leftNodeUp.y - corridorWidth);
        leftNodeDown = new Vector2Int(leftNodeDown.x, leftNodeDown.y);
        rightNodeUp = new Vector2Int(rightNodeUp.x, rightNodeUp.y - corridorWidth);
        rightNodeDown = new Vector2Int(rightNodeDown.x, rightNodeDown.y);

        if (rightNodeUp.y >= leftNodeUp.y && leftNodeDown.y >= rightNodeDown.y)
        {
            return Random.Range(leftNodeDown.y,leftNodeUp.y);                
        }
        if (rightNodeUp.y <= leftNodeUp.y && leftNodeDown.y <= rightNodeDown.y)
        {
            return Random.Range(rightNodeDown.y, rightNodeUp.y);
        }
        if (leftNodeUp.y >= rightNodeDown.y && leftNodeUp.y <= rightNodeUp.y)
        {
            return Random.Range(rightNodeDown.y, leftNodeUp.y);

        }
        if (leftNodeDown.y >= rightNodeDown.y && leftNodeDown.y <= rightNodeUp.y)
        {
            return Random.Range(leftNodeDown.y, rightNodeUp.y);
        }
        return -1;
    }

    private void ProcessRoomInRelationUpOrDown(Node structure1, Node structure2)
    {
        Node bottomStructure = null;
        List<Node> structureBottmChildren = StructureHelper.TraverseGraphToExtractLowestLeafes(structure1);
        Node topStructure = null;
        List<Node> structureAboveChildren = StructureHelper.TraverseGraphToExtractLowestLeafes(structure2);

        var sortedBottomStructure = structureBottmChildren.OrderByDescending(child => child.TopRightAreaCorner.y).ToList();

        if (sortedBottomStructure.Count == 1)
        {
            bottomStructure = structureBottmChildren[0];
        }
        else
        {
            int maxY = sortedBottomStructure[0].TopLeftAreaCorner.y;
            sortedBottomStructure = sortedBottomStructure.Where(child => Mathf.Abs(maxY - child.TopLeftAreaCorner.y) < 10).ToList();
            int index = UnityEngine.Random.Range(0, sortedBottomStructure.Count);
            bottomStructure = sortedBottomStructure[index];
        }

        var possibleNeighboursInTopStructure = structureAboveChildren.Where(
            child => GetValidXForNeighbourUpDown(
                bottomStructure.TopLeftAreaCorner,
                bottomStructure.TopRightAreaCorner,
                child.BottomLeftAreaCorner,
                child.BottomRightAreaCorner)
            != -1).OrderBy(child => child.BottomRightAreaCorner.y).ToList();
        if (possibleNeighboursInTopStructure.Count == 0)
        {
            topStructure = structure2;
        }
        else
        {
            topStructure = possibleNeighboursInTopStructure[0];
        }
        int x = GetValidXForNeighbourUpDown(
                bottomStructure.TopLeftAreaCorner,
                bottomStructure.TopRightAreaCorner,
                topStructure.BottomLeftAreaCorner,
                topStructure.BottomRightAreaCorner);
        while (x == -1 && sortedBottomStructure.Count > 1)
        {
            sortedBottomStructure = sortedBottomStructure.Where(child => child.TopLeftAreaCorner.x != topStructure.TopLeftAreaCorner.x).ToList();
            bottomStructure = sortedBottomStructure[0];
            x = GetValidXForNeighbourUpDown(
                bottomStructure.TopLeftAreaCorner,
                bottomStructure.TopRightAreaCorner,
                topStructure.BottomLeftAreaCorner,
                topStructure.BottomRightAreaCorner);
        }
        BottomLeftAreaCorner = new Vector2Int(x, bottomStructure.TopLeftAreaCorner.y);
        TopRightAreaCorner = new Vector2Int(x + this.corridorWidth, topStructure.BottomLeftAreaCorner.y);

        diff = GetDifferenceForUpDown(bottomStructure, topStructure);
    }

    private int GetDifferenceForUpDown(Node bottomStructure, Node topStructure)
    {
        if (bottomStructure.heightPos < topStructure.heightPos)
        {
            if (bottomStructure.height > 1)
            {
                lowerStructureWall = true;
            } 
            heightPos = bottomStructure.heightPos;
            if (bottomStructure.heightPos == topStructure.heightPos - 1.5f)
            {
                return 5;
            }
            if (bottomStructure.heightPos == topStructure.heightPos - 3f)
            {
                return 6;
            }
        }
        else
        {
            if (topStructure.height > 1)
            {
                lowerStructureWall = true;
            }
            heightPos = topStructure.heightPos;
            if (bottomStructure.heightPos == topStructure.heightPos + 1.5f)
            {
                return 7;
            }
            if (bottomStructure.heightPos == topStructure.heightPos + 3f)
            {
                return 8;
            }
        }
        return 0;
    }

    private int GetValidXForNeighbourUpDown(Vector2Int bottomNodeLeft, 
        Vector2Int bottomNodeRight, Vector2Int topNodeLeft, Vector2Int topNodeRight)
    {
        bottomNodeLeft = new Vector2Int(bottomNodeLeft.x, bottomNodeLeft.y);
        bottomNodeRight = new Vector2Int(bottomNodeRight.x - corridorWidth, bottomNodeRight.y);
        topNodeLeft = new Vector2Int(topNodeLeft.x, topNodeLeft.y);
        topNodeRight = new Vector2Int(topNodeRight.x - corridorWidth, topNodeRight.y);

        if (topNodeLeft.x <= bottomNodeLeft.x && bottomNodeRight.x <= topNodeRight.x)
        {
            return Random.Range(bottomNodeLeft.x, bottomNodeRight.x);
        }
        if (topNodeLeft.x >= bottomNodeLeft.x && bottomNodeRight.x >= topNodeRight.x)
        {
            return Random.Range(topNodeLeft.x, topNodeRight.x);
        }
        if (bottomNodeLeft.x >= topNodeLeft.x && bottomNodeLeft.x <= topNodeRight.x)
        {
            return Random.Range(bottomNodeLeft.x, topNodeRight.x);
        }
        if (bottomNodeRight.x <= topNodeRight.x && bottomNodeRight.x >= topNodeLeft.x)
        {
            return Random.Range(topNodeLeft.x, bottomNodeRight.x);
        }
        return -1;
    }

private RelativePosition CheckPositionStructure2AgainstStructure1()
    {
        Vector2 middlePointStructure1Temp = ((Vector2)structure1.TopRightAreaCorner + structure1.BottomLeftAreaCorner) / 2;
        Vector2 middlePointStructure2Temp = ((Vector2)structure2.TopRightAreaCorner + structure2.BottomLeftAreaCorner) / 2;
        float angle = CalculateAngle(middlePointStructure1Temp, middlePointStructure2Temp);
        if ((angle < 45 && angle >= 0) || (angle > -45 && angle < 0))
        {
            return RelativePosition.Right;
        }
        else if(angle > 45 && angle < 135)
        {
            return RelativePosition.Up;
        }
        else if(angle > -135 && angle < -45)
        {
            return RelativePosition.Down;
        }
        else
        {
            return RelativePosition.Left;
        }
    }

    private float CalculateAngle(Vector2 middlePointStructure1Temp, Vector2 middlePointStructure2Temp)
    {
        return Mathf.Atan2(middlePointStructure2Temp.y - middlePointStructure1Temp.y,
            middlePointStructure2Temp.x - middlePointStructure1Temp.x)*Mathf.Rad2Deg;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class DungeonCreatorScript : MonoBehaviour
{
    public int dungeonWidth, dungeonLength;
    public int roomWidthMin, roomLengthMin;
    public int maxIterations;
    public int corridorWidth;
    public Material floorMaterial;
    public Material ceilingMaterial;
    [Range(0.0f, 0.3f)]
    public float roomBottomCornerModifier;
    [Range(0.7f, 1f)]
    public float roomTopCornerModifier;
    [Range(0.0f, 2f)]
    public int roomOffset;

    //Walls - done
    public GameObject wallVertical, wallHorizontal, wallVerticalSmall, wallHorizontalSmall;
    //Door frames - done
    public GameObject entranceVertical, entranceHorizontal;
    //Doors - done
    public GameObject doorVertical, doorHorizontal;
    //Pillars - done
    public GameObject pillarPrefab;
    //Stairs - done
    public GameObject Stairs;
    //Ladders - done
    public GameObject ladder;
    //Shelves - done
    public GameObject shelf;
    //Candles
    public GameObject candles;
    //wardrobe
    public GameObject wardrobe;
    //Desk
    public GameObject desk;
    //Chair
    public GameObject chair;
    //Chest
    public GameObject chest;
    //Flag
    public GameObject flag;
    //Barrel
    public GameObject barrel;
    //Lamp
    public GameObject lamp;
    //DoorKey
    public GameObject doorKey;
    //ExitKey
    public GameObject exitKey;
    //Player
    public GameObject player;
    //Enemy
    public GameObject enemy;
    //Exit door
    public GameObject exitDoor;
    // Door Manager
    public GameObject doorManager;
    // Exit door manager
    public GameObject exitDoorManager;

    bool hasKey = false; // Max 1
    bool hasExitKey = false; // Max 1

    //Doors
    List<Vector3> possibleDoorVerticalPosition;
    List<Vector3> possibleDoorHorizontalPosition;

    //Walls
    List<Vector3> possibleWallHorizontalPosition;
    List<Vector3> possibleWallHorizontalSmallPosition;
    List<Vector3> possibleWallVerticalPosition;
    List<Vector3> possibleWallVerticalSmallPosition;

    //Pillars
    List<Vector3> possiblePillarPosition;

    //Ladders
    List<Vector3> possibleLadderPosition;
    List<Vector3> possibleLadderRotation;

    //Shelves
    List<Vector3> ShelfHorizontalPositions;
    List<Vector3> ShelfHorizontalRotations;
    List<Vector3> ShelfVerticalPositions;
    List<Vector3> ShelfVerticalRotations;

    // Potential Key Positions
    List<Vector3> keyPositions;
    List<Vector3> exitKeyPositions;

    // Player and enemy
    Vector3 playerPoint;
    Vector3 enemyPoint;

    public List<Vector3> midPoints;

    List<RoomNode> roomList; // New list of rooms that are not corridors
    List<RoomType> roomTypesForVerticalDoors; // Determines whether to instantiate doors in specific rooms
    List<RoomType> roomTypesForHorizontalDoors;

    public NavMeshSurface surface;


    private void Start()
    {        
        CreateDungeon();
    }

    public void CreateDungeon()
    {
        DestroyAllChildren();
        Instantiate(doorManager, transform);
        DungeonGenerator generator = new DungeonGenerator(dungeonWidth, dungeonLength);
        var listOfRooms = generator.CalculateDungeon(
            maxIterations,
            roomWidthMin,
            roomLengthMin,
            roomBottomCornerModifier,
            roomTopCornerModifier,
            roomOffset,
            corridorWidth);
        GameObject wallParent = new GameObject("wallParent");
        wallParent.transform.parent = transform;
        possibleDoorVerticalPosition = new List<Vector3>();
        possibleDoorHorizontalPosition = new List<Vector3>();
        possibleWallHorizontalPosition = new List<Vector3>();
        possibleWallHorizontalSmallPosition = new List<Vector3>();
        possibleWallVerticalPosition = new List<Vector3>();
        possibleWallVerticalSmallPosition = new List<Vector3>();
        roomList = new List<RoomNode>();
        roomTypesForVerticalDoors = new List<RoomType>();
        roomTypesForHorizontalDoors = new List<RoomType>();
        possiblePillarPosition = new List<Vector3>();
        possibleLadderPosition = new List<Vector3>();
        possibleLadderRotation = new List<Vector3>();

        //Shelves
        ShelfHorizontalPositions = new List<Vector3>();
        ShelfHorizontalRotations = new List<Vector3>();
        ShelfVerticalPositions = new List<Vector3>();
        ShelfVerticalRotations = new List<Vector3>();

        // Potential Key Positions
        keyPositions = new List<Vector3>();
        exitKeyPositions = new List<Vector3>();

        // Enemy walkPoints
        midPoints = new List<Vector3>();

        for (int i = 0; i < listOfRooms.Count; i++)
        {
            CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, listOfRooms[i].heightPos, listOfRooms[i].diff, listOfRooms[i].height, listOfRooms[i].midPoint, listOfRooms[i].roomType, wallParent);
            CreatePillarPos(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, listOfRooms[i].heightPos, listOfRooms[i].diff);
            if (listOfRooms[i].height >= 2)
            {
                CreatePillarPos(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, listOfRooms[i].heightPos + 3f, listOfRooms[i].diff);
            }
            if (listOfRooms[i].height >= 3)
            {
                CreatePillarPos(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, listOfRooms[i].heightPos + 6f, listOfRooms[i].diff);
            }
            if (listOfRooms[i].roomType == RoomType.RegularRoom)
            {
                roomList.Add((RoomNode)listOfRooms[i]);
            }

        }
        surface.BuildNavMesh();
        CreateRoomSystem();
        foreach (var room in listOfRooms)
        {
            GenerateWallPos(room.BottomLeftAreaCorner, room.TopRightAreaCorner, room.heightPos, room.diff, room.height, room.lowerStructureWall, room.roomType);
            if (room.height >= 2)
            {
                CreateTerrace(room.BottomLeftAreaCorner, room.TopRightAreaCorner, room.heightPos + 3f, room.height, room.roomType);
            }
            else
            {
                SpawnEntities(room.midPoint, room.roomType);
            }
        }
        CreateLadders(wallParent);
        CreateWalls(wallParent);
        CreateDoors(wallParent);
        CreatePillars(wallParent);
        foreach (var room in roomList)
        {
            GenerateFurniturePositions(room.BottomLeftAreaCorner, room.TopRightAreaCorner, room.heightPos, room.roomType);
        }
        CreateFurniture(wallParent);
        CreateKeys(wallParent);
        surface.BuildNavMesh();
        Instantiate(player, playerPoint, Quaternion.identity, transform);
        Instantiate(enemy, enemyPoint, Quaternion.identity, transform);
        Instantiate(exitDoorManager, transform);

    }

    private void SpawnEntities(Vector3 midPoint, RoomType roomType)
    {
        if (roomType == RoomType.PlayerRoom)
        {
            playerPoint = new Vector3(midPoint.x, midPoint.y + 1f, midPoint.z);
        }
        if (roomType == RoomType.EnemyRoom)
        {
            enemyPoint = new Vector3(midPoint.x, midPoint.y + 1f, midPoint.z);
        }
    }



    public void CreateRoomSystem()
    {
        int playerRoomIndex = 0;
        int exitRoomIndex = 0;
        int firstKeyIndex = 0;
        int exitKeyIndex = 0;
        int enemyRoomIndex = Random.Range(0, roomList.Count);

        // Find and set a random enemy room
        roomList[enemyRoomIndex].roomType = RoomType.EnemyRoom;
        Debug.Log("Type: " + roomList[enemyRoomIndex].roomType + ", Position: " + roomList[enemyRoomIndex].midPoint);

        float furthestDistanceFromEnemyRoom = 0f;

        // Find the furthest room from the EnemyRoom
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].roomType == RoomType.RegularRoom)
            {
                // Calculate the distance between the enemyRoom and the current room
                NavMeshPath path = new NavMeshPath();
                NavMesh.CalculatePath(roomList[enemyRoomIndex].midPoint, roomList[i].midPoint, NavMesh.AllAreas, path);

                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    Debug.Log("Complete");
                }

                // Find the furthest room
                float distance = CalculatePathDistance(path);
                if (distance > furthestDistanceFromEnemyRoom)
                {
                    furthestDistanceFromEnemyRoom = distance;
                    playerRoomIndex = i;
                }
            }
        }

        // Set the furthest room to PlayerRoom
        roomList[playerRoomIndex].roomType = RoomType.PlayerRoom;
        Debug.Log("Type: " + roomList[playerRoomIndex].roomType + ", Position: " + roomList[playerRoomIndex].midPoint);

        float furthestDistanceFromPlayerRoom = 0f;
        float closestDistanceToPlayerRoom = float.MaxValue;

        // Find the closest and furthest rooms from PlayerRoom
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].roomType == RoomType.RegularRoom)
            {
                // Calculate the distance between the playerRoom and the current room
                NavMeshPath path = new NavMeshPath();
                NavMesh.CalculatePath(roomList[playerRoomIndex].midPoint, roomList[i].midPoint, NavMesh.AllAreas, path);

                // Find the furthest room
                float distance = CalculatePathDistance(path);
                if (distance > furthestDistanceFromPlayerRoom)
                {
                    furthestDistanceFromPlayerRoom = distance;
                    exitKeyIndex = i;
                }
                // Find the closest room
                if (distance < closestDistanceToPlayerRoom)
                {
                    closestDistanceToPlayerRoom = distance;
                    firstKeyIndex = i;
                }
            }
        }

        // Set the furthest room to ExitRoom
        roomList[exitKeyIndex].roomType = RoomType.ExitKeyRoom;
        Debug.Log("Type: " + roomList[exitKeyIndex].roomType + ", Position: " + roomList[exitKeyIndex].midPoint);

        // Set the closest room to FirstKeyRoom
        roomList[firstKeyIndex].roomType = RoomType.FirstKeyRoom;
        Debug.Log("Type: " + roomList[firstKeyIndex].roomType + ", Position: " + roomList[firstKeyIndex].midPoint);


        float furthestRoomFromBoth1 = 0f;
        float furthestRoomFromBoth2 = 0f;
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].roomType == RoomType.RegularRoom)
            {
                // Calculate the distance between the playerRoom and the current room
                NavMeshPath playerPath = new NavMeshPath();
                NavMesh.CalculatePath(roomList[playerRoomIndex].midPoint, roomList[i].midPoint, NavMesh.AllAreas, playerPath);
                float playerDistance = CalculatePathDistance(playerPath);

                // Calculate the distance between the exitRoom and the current room
                NavMeshPath exitPath = new NavMeshPath();
                NavMesh.CalculatePath(roomList[exitKeyIndex].midPoint, roomList[i].midPoint, NavMesh.AllAreas, exitPath);
                float exitDistance = CalculatePathDistance(exitPath);

                // Find furthest room from both rooms
                if (playerDistance > furthestRoomFromBoth1 && exitDistance > furthestRoomFromBoth2)
                {
                    furthestRoomFromBoth1 = playerDistance;
                    furthestRoomFromBoth2 = exitDistance;
                    exitRoomIndex = i;
                }
            }
        }

        // Set the furthest room from both to ExitKeyRoom
        roomList[exitRoomIndex].roomType = RoomType.ExitRoom;
        Debug.Log("Type: " + roomList[exitRoomIndex].roomType + ", Position: " + roomList[exitRoomIndex].midPoint);

    }

    private float CalculatePathDistance(NavMeshPath path)
    {
        float distance = 0f;

        for (int i = 1; i < path.corners.Length; i++)
        {
            distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return distance;
    }



    private void CreatePillarPos(Vector2Int bottomLeftAreaCorner, Vector2Int topRightAreaCorner, float heightPos, int diff)
    {
        Vector3 bottomLeftV;
        Vector3 bottomRightV;
        Vector3 topLeftV;
        Vector3 topRightV;

        Vector3 blLampPos;
        Vector3 brLampPos;
        Vector3 tlLampPos;
        Vector3 trLampPos;

        Vector3 blLampRot;
        Vector3 brLampRot;
        Vector3 tlLampRot;
        Vector3 trLampRot;

        switch (diff)
        {
            case 1:
                bottomLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos, bottomLeftAreaCorner.y);
                bottomRightV = new Vector3(topRightAreaCorner.x, heightPos + 1.5f, bottomLeftAreaCorner.y);
                topLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos, topRightAreaCorner.y);
                topRightV = new Vector3(topRightAreaCorner.x, heightPos + 1.5f, topRightAreaCorner.y);
                break;
            case 2:
                bottomLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos, bottomLeftAreaCorner.y);
                bottomRightV = new Vector3(topRightAreaCorner.x, heightPos + 3f, bottomLeftAreaCorner.y);
                topLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos, topRightAreaCorner.y);
                topRightV = new Vector3(topRightAreaCorner.x, heightPos + 3f, topRightAreaCorner.y);
                break;
            case 3:
                bottomLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos + 1.5f, bottomLeftAreaCorner.y);
                bottomRightV = new Vector3(topRightAreaCorner.x, heightPos, bottomLeftAreaCorner.y);
                topLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos + 1.5f, topRightAreaCorner.y);
                topRightV = new Vector3(topRightAreaCorner.x, heightPos, topRightAreaCorner.y);
                break;
            case 4:
                bottomLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos + 3f, bottomLeftAreaCorner.y);
                bottomRightV = new Vector3(topRightAreaCorner.x, heightPos, bottomLeftAreaCorner.y);
                topLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos + 3f, topRightAreaCorner.y);
                topRightV = new Vector3(topRightAreaCorner.x, heightPos, topRightAreaCorner.y);
                break;
            case 5:
                bottomLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos, bottomLeftAreaCorner.y);
                bottomRightV = new Vector3(topRightAreaCorner.x, heightPos, bottomLeftAreaCorner.y);
                topLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos + 1.5f, topRightAreaCorner.y);
                topRightV = new Vector3(topRightAreaCorner.x, heightPos + 1.5f, topRightAreaCorner.y);
                break;
            case 6:
                bottomLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos, bottomLeftAreaCorner.y);
                bottomRightV = new Vector3(topRightAreaCorner.x, heightPos, bottomLeftAreaCorner.y);
                topLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos + 3f, topRightAreaCorner.y);
                topRightV = new Vector3(topRightAreaCorner.x, heightPos + 3f, topRightAreaCorner.y);
                break;
            case 7:
                bottomLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos + 1.5f, bottomLeftAreaCorner.y);
                bottomRightV = new Vector3(topRightAreaCorner.x, heightPos + 1.5f, bottomLeftAreaCorner.y);
                topLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos, topRightAreaCorner.y);
                topRightV = new Vector3(topRightAreaCorner.x, heightPos, topRightAreaCorner.y);
                break;
            case 8:
                bottomLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos + 3f, bottomLeftAreaCorner.y);
                bottomRightV = new Vector3(topRightAreaCorner.x, heightPos + 3f, bottomLeftAreaCorner.y);
                topLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos, topRightAreaCorner.y);
                topRightV = new Vector3(topRightAreaCorner.x, heightPos, topRightAreaCorner.y);
                break;
            default:
                bottomLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos, bottomLeftAreaCorner.y);
                bottomRightV = new Vector3(topRightAreaCorner.x, heightPos, bottomLeftAreaCorner.y);
                topLeftV = new Vector3(bottomLeftAreaCorner.x, heightPos, topRightAreaCorner.y);
                topRightV = new Vector3(topRightAreaCorner.x, heightPos, topRightAreaCorner.y);
                break;
        }

        possiblePillarPosition.Add(bottomLeftV);
        possiblePillarPosition.Add(bottomRightV);
        possiblePillarPosition.Add(topLeftV);
        possiblePillarPosition.Add(topRightV);

        if (diff > 0 && diff <= 4)
        {
            blLampPos = new Vector3(bottomLeftV.x - 0.25f, bottomLeftV.y + 1.5f, bottomLeftV.z);
            blLampRot = new Vector3(0, -90, 0);
            brLampPos = new Vector3(bottomRightV.x + 0.25f, bottomRightV.y + 1.5f, bottomRightV.z);
            brLampRot = new Vector3(0, 90, 0);
            tlLampPos = new Vector3(topLeftV.x - 0.25f, topLeftV.y + 1.5f, topLeftV.z);
            tlLampRot = new Vector3(0, -90, 0);
            trLampPos = new Vector3(topRightV.x + 0.25f, topRightV.y + 1.5f, topRightV.z);
            trLampRot = new Vector3(0, 90, 0);

            Instantiate(lamp, blLampPos, Quaternion.Euler(blLampRot), transform);
            Instantiate(lamp, brLampPos, Quaternion.Euler(brLampRot), transform);
            Instantiate(lamp, tlLampPos, Quaternion.Euler(tlLampRot), transform);
            Instantiate(lamp, trLampPos, Quaternion.Euler(trLampRot), transform);
        }
        else if (diff > 4 && diff <= 8)
        {
            blLampPos = new Vector3(bottomLeftV.x, bottomLeftV.y + 1.5f, bottomLeftV.z - 0.25f);
            blLampRot = new Vector3(0, 180, 0);
            brLampPos = new Vector3(bottomRightV.x, bottomRightV.y + 1.5f, bottomRightV.z - 0.25f);
            brLampRot = new Vector3(0, 180, 0);
            tlLampPos = new Vector3(topLeftV.x, topLeftV.y + 1.5f, topLeftV.z + 0.25f);
            tlLampRot = new Vector3(0, 0, 0);
            trLampPos = new Vector3(topRightV.x, topRightV.y + 1.5f, topRightV.z + 0.25f);
            trLampRot = new Vector3(0, 0, 0);
            Instantiate(lamp, blLampPos, Quaternion.Euler(blLampRot), transform);
            Instantiate(lamp, brLampPos, Quaternion.Euler(brLampRot), transform);
            Instantiate(lamp, tlLampPos, Quaternion.Euler(tlLampRot), transform);
            Instantiate(lamp, trLampPos, Quaternion.Euler(trLampRot), transform);
        }

    }

    private void CreateWalls(GameObject wallParent)
    {
        foreach (var wallPosition in possibleWallHorizontalPosition)
        {
            Vector3 wallPos = new Vector3(wallPosition.x + 1f, wallPosition.y, wallPosition.z);
            CreateWall(wallParent, wallPos, wallHorizontal);
        }
        foreach (var wallPosition in possibleWallVerticalPosition)
        {
            Vector3 wallPos = new Vector3(wallPosition.x, wallPosition.y, wallPosition.z);
            CreateWall(wallParent, wallPos, wallVertical);
        }
        foreach (var wallPosition in possibleWallHorizontalSmallPosition)
        {
            Vector3 wallPos = new Vector3(wallPosition.x + 1f, wallPosition.y, wallPosition.z);
            CreateWall(wallParent, wallPos, wallHorizontalSmall);
        }
        foreach (var wallPosition in possibleWallVerticalSmallPosition)
        {
            Vector3 wallPos = new Vector3(wallPosition.x, wallPosition.y, wallPosition.z);
            CreateWall(wallParent, wallPos, wallVerticalSmall);
        }
    }

    private void CreatePillars(GameObject wallParent)
    {
        foreach (var pillar in possiblePillarPosition)
        {
            Instantiate(pillarPrefab, pillar, Quaternion.identity,wallParent.transform);
        }
    }

    private void CreateLadders(GameObject wallParent)
    {
        for (int i = 0; i < possibleLadderPosition.Count; i++)
        {
            Instantiate(ladder, possibleLadderPosition[i], Quaternion.Euler(possibleLadderRotation[i]), wallParent.transform);
        }
    }

    private void CreateDoors(GameObject wallParent)
    {
        for (int i = 0; i < possibleDoorHorizontalPosition.Count; i += 2)
        {
            // Generate door frames
            var doorPosition = possibleDoorHorizontalPosition[i];
            doorPosition = new Vector3(doorPosition.x + 1, doorPosition.y, doorPosition.z);
            CreateWall(wallParent, doorPosition, entranceHorizontal);

            // Generate doors
            Vector3 doorPos = new Vector3(doorPosition.x - 0.57f, doorPosition.y + 1.25f, doorPosition.z - 0.01f);
            if (roomTypesForHorizontalDoors[i] == RoomType.PlayerRoom || roomTypesForHorizontalDoors[i] == RoomType.FirstKeyRoom)
            {
                continue;
            }
            else if (roomTypesForHorizontalDoors[i] == RoomType.ExitKeyRoom || roomTypesForHorizontalDoors[i] == RoomType.ExitRoom)
            {
                GameObject door = Instantiate(doorHorizontal, doorPos, doorHorizontal.transform.rotation, wallParent.transform);
                door.GetComponent<CorridorDoorScript>().doorID = 0;
            }
            else
            {
                if (Random.Range(0, 100) < 70)
                {
                    GameObject door = Instantiate(doorHorizontal, doorPos, doorHorizontal.transform.rotation, wallParent.transform);
                    door.GetComponent<CorridorDoorScript>().doorID = 0;
                }
            }
        }
        for (int i = 0; i < possibleDoorVerticalPosition.Count; i += 2)
        {
            // Generate door frames
            var doorPosition = possibleDoorVerticalPosition[i];
            doorPosition = new Vector3(doorPosition.x, doorPosition.y, doorPosition.z + 1);
            CreateWall(wallParent, doorPosition, entranceVertical);

            // Generate doors
            Vector3 doorPos = new Vector3(doorPosition.x + 0.04f, doorPosition.y + 1.25f, doorPosition.z + 0.57f);
            if (roomTypesForVerticalDoors[i] == RoomType.PlayerRoom || roomTypesForVerticalDoors[i] == RoomType.FirstKeyRoom)
            {
                continue;
            }
            else if (roomTypesForVerticalDoors[i] == RoomType.ExitKeyRoom || roomTypesForVerticalDoors[i] == RoomType.ExitRoom)
            {
                GameObject door = Instantiate(doorVertical, doorPos, doorVertical.transform.rotation, wallParent.transform);
                door.GetComponent<CorridorDoorScript>().doorID = 0;
            }
            else
            {
                if (Random.Range(0, 100) < 70)
                {
                    GameObject door = Instantiate(doorVertical, doorPos, doorVertical.transform.rotation, wallParent.transform);
                    door.GetComponent<CorridorDoorScript>().doorID = 0;
                }
            }
        }
    }


    private void CreateFurniture(GameObject wallParent)
    {
        for (int i = 0; i < ShelfHorizontalPositions.Count; i++)
        {
            Instantiate(shelf, ShelfHorizontalPositions[i], Quaternion.Euler(ShelfHorizontalRotations[i]), wallParent.transform);         
        }
        for (int i = 0; i < ShelfVerticalPositions.Count; i++)
        {
            Instantiate(shelf, ShelfVerticalPositions[i], Quaternion.Euler(ShelfVerticalRotations[i]), wallParent.transform);
        }
    }



    private void CreateWall(GameObject wallParent, Vector3 wallPosition, GameObject wallPrefab)
    {
        Instantiate(wallPrefab, wallPosition, wallPrefab.transform.rotation, wallParent.transform);
    }



    private void CreateMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner, float heightPos, int diff, int height, Vector3 midPoint, RoomType roomType, GameObject wallParent)
    {
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, heightPos, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, heightPos, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, heightPos, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, heightPos, topRightCorner.y);

        Vector3 thickness = Vector3.up * 0.05f;   

        Vector3[] vertices = new Vector3[] {
                // Top Face
                topLeftV, topRightV, bottomLeftV, bottomRightV,
                //Bottom Face
                topLeftV - thickness, topRightV - thickness, bottomLeftV - thickness, bottomRightV - thickness
            };

        // Define the faces (triangles) of the cube
        Face[] faces = new Face[] { new Face(new int[]
        {
            // Top face
            1,2,0,1,3,2,
            // Bottom face
            6,5,4,7,5,6,
            // Side faces
            0,4,1,1,4,5,
            1,5,3,3,5,7,
            3,7,2,2,7,6,
            2,6,0,0,6,4
        } 
        ) };

        Vector3 temp = (bottomLeftV + topRightV) / 2;
        midPoints.Add(new Vector3(temp.x, heightPos, temp.z));

        // ProBuilder Mesh
        GameObject dungeonFloor = new GameObject("Mesh: " + temp + ", Height: " + height, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));

        // Create a cube
        ProBuilderMesh pbMesh = dungeonFloor.AddComponent<ProBuilderMesh>();

        // Set the modified vertices back to the ProBuilder mesh
        pbMesh.RebuildWithPositionsAndFaces(vertices, faces);

        // Rebuild the mesh
        pbMesh.ToMesh();       
        dungeonFloor.GetComponent<MeshFilter>().mesh = pbMesh.gameObject.GetComponent<MeshFilter>().sharedMesh;
        dungeonFloor.GetComponent<MeshRenderer>().material = floorMaterial;
        dungeonFloor.GetComponent<MeshCollider>().sharedMesh = pbMesh.gameObject.GetComponent<MeshFilter>().sharedMesh;
        dungeonFloor.transform.parent = transform;
        dungeonFloor.layer = 11;


        pbMesh.Refresh();

        if (diff != 0)
        {
            if (diff % 2 == 0)
                CreateCeiling(bottomLeftCorner, topRightCorner, heightPos + 6f);
            else
                CreateCeiling(bottomLeftCorner, topRightCorner, heightPos + 4.5f);
        }
        else
        {
            if (height != 0)
                CreateCeiling(bottomLeftCorner, topRightCorner, heightPos + (height * 3f));
            else
                CreateCeiling(bottomLeftCorner, topRightCorner, heightPos + 3f);
        }

        Vector3 stairPos;
        Vector3 ladderPos;

        switch (diff)
        {
            case 1:
                stairPos = new Vector3(bottomRightV.x - 1, heightPos + 0.75f, topRightV.z - 1f);
                Instantiate(Stairs, stairPos, Quaternion.Euler(0, 90, 0), transform);
                break;
            case 2:
                for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
                {
                    var wallPosition = new Vector3(bottomRightV.x, heightPos - 0.01f, col);
                    CreateWall(wallParent, wallPosition, wallVertical);
                }
                ladderPos = new Vector3(bottomRightV.x - 0.15f, heightPos, topRightV.z - 1.66f);
                Instantiate(ladder, ladderPos, Quaternion.Euler(0, 90, 0), transform);
                break;
            case 3:
                stairPos = new Vector3(bottomLeftV.x + 1, heightPos + 0.75f, topLeftV.z - 1f);
                Instantiate(Stairs, stairPos, Quaternion.Euler(0, -90, 0), transform);
                break;
            case 4:
                for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
                {
                    var wallPosition = new Vector3(bottomLeftV.x, heightPos - 0.01f, col);
                    CreateWall(wallParent, wallPosition, wallVertical);
                }
                ladderPos = new Vector3(bottomLeftV.x + 0.15f, heightPos, topLeftV.z - 0.33f);
                Instantiate(ladder, ladderPos, Quaternion.Euler(0, -90, 0), transform);
                break;
            case 5:
                stairPos = new Vector3(topLeftV.x + 1, heightPos + 0.75f, topLeftV.z - 1);
                Instantiate(Stairs, stairPos, Quaternion.Euler(0, 0, 0), transform);
                break;
            case 6:
                for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
                {
                    var wallPosition = new Vector3(row + 1f, heightPos - 0.01f, topRightV.z);
                    CreateWall(wallParent, wallPosition, wallHorizontal);
                }
                ladderPos = new Vector3(topLeftV.x + 1.66f, heightPos, topRightV.z - 0.07f);
                Instantiate(ladder, ladderPos, Quaternion.Euler(0, 0, 0), transform);
                break;
            case 7:
                stairPos = new Vector3(bottomLeftV.x + 1, heightPos + 0.75f, bottomLeftV.z + 1);
                Instantiate(Stairs, stairPos, Quaternion.Euler(0, 180, 0), transform);
                break;
            case 8:
                for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
                {
                    var wallPosition = new Vector3(row + 1f, heightPos - 0.01f, bottomLeftV.z);
                    CreateWall(wallParent, wallPosition, wallHorizontal);
                }
                ladderPos = new Vector3(bottomLeftV.x + 0.33f, heightPos, bottomLeftV.z + 0.15f);
                Instantiate(ladder, ladderPos, Quaternion.Euler(0, 180, 0), transform);
                break;
            default:
                break;
        }

    }

    private void CreateKeys(GameObject wallParent)
    {
        int keyIndex = Random.Range(0, keyPositions.Count);
        int exitKeyIndex = Random.Range(0, exitKeyPositions.Count);

        GameObject dKey = Instantiate(doorKey, keyPositions[keyIndex], Quaternion.identity, wallParent.transform);
        GameObject eKey = Instantiate(exitKey, exitKeyPositions[exitKeyIndex], Quaternion.identity, wallParent.transform);
        
        eKey.gameObject.tag = "Pickup";
    }

    private void GenerateFurniturePositions(Vector2 bottomLeftCorner, Vector2 topRightCorner, float heightPos, RoomType roomType)
    {
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, heightPos, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, heightPos, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, heightPos, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, heightPos, topRightCorner.y);

        Vector3 bottomRotation = new Vector3(0, 0, 0);
        Vector3 rightRotation = new Vector3(0, -90, 0);
        Vector3 topRotation = new Vector3(0, 180, 0);
        Vector3 leftRotation = new Vector3(0, 90, 0);

        bool hasDesk = false; // Desk, Wardrobe, Chair - Max 1 per room
        bool hasFlags = false; // Max 1 per room

        // Random number to put either along one side of a room
        int flagChance = 29;
        int DeskChance = 64;
        int shelfChance = 99;

        // Corridors have no furnitre and exit room only has exit door
        if (roomType != RoomType.Corridor && roomType != RoomType.ExitRoom)
        {
            // Temporary lists
            List<Vector3> TempShelfList = new List<Vector3>();
            List<Vector3> TempDeskList = new List<Vector3>();

            // Bottom wall
            for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
            {
                // Check each wall position for validity
                var shelfPos = new Vector3(row, heightPos, bottomLeftV.z);
                if (ShelfValidity(shelfPos, possibleWallHorizontalPosition, true))
                {
                    // Add valid positions to the temporary list
                    TempShelfList.Add(shelfPos);
                }
                if (DeskValidity(shelfPos, possibleWallHorizontalPosition, true))
                {
                    TempDeskList.Add(shelfPos);
                }
            }

            // Pick a random number 0 - 99
            int rand = Random.Range(0, 100);
            // Decide between flags, desk, or shelves
            int set = 0;
            if (hasDesk && hasFlags)
            {
                set = 1; // Shelves
            }
            else if (hasDesk && !hasFlags)
            {
                if (rand >= 0 && rand < 50)
                {
                    set = 3; // flags
                    hasFlags = true;
                }
                else if (rand >= 50 && rand < 100)
                {
                    set = 1; // Shelves
                }
            }
            else if (!hasDesk && hasFlags)
            {
                if (rand >= 0 && rand < 50)
                {
                    set = 2; // Desk
                    hasDesk = true;
                }
                else if (rand >= 50 && rand < 100)
                {
                    set = 1; // Shelves
                }
            }
            else if (!hasDesk && !hasFlags)
            {
                if (rand >= 0 && rand <= flagChance)
                {
                    set = 3; // flags
                    hasFlags = true;
                }
                else if (rand > flagChance && rand <= DeskChance)
                {
                    set = 2; // Desk
                    hasDesk = true;
                }
                else if (rand > DeskChance && rand <= shelfChance)
                {
                    set = 1; // Shelf
                }
            }
            // Shelves - bottom
            if (set == 1 && TempShelfList.Count > 0)
            {
                // Pick a random position from the valid list
                int midShelf = Random.Range(0, TempShelfList.Count);
                // Create positions for bottom and top shelves
                Vector3 shelfPos1 = new Vector3(TempShelfList[midShelf].x - 0.5f, TempShelfList[midShelf].y + 1f, TempShelfList[midShelf].z + 0.35f);
                Vector3 shelfPos2 = new Vector3(TempShelfList[midShelf].x - 0.5f, TempShelfList[midShelf].y + 1.5f, TempShelfList[midShelf].z + 0.35f);
                // Add them to the lists
                ShelfHorizontalPositions.Add(shelfPos1);
                ShelfHorizontalRotations.Add(bottomRotation);
                ShelfHorizontalPositions.Add(shelfPos2);
                ShelfHorizontalRotations.Add(bottomRotation);

                // Generate potential key positions
                if (roomType == RoomType.FirstKeyRoom)
                {
                    Vector3 keyPos = new Vector3(shelfPos1.x, shelfPos1.y + 0.2f, shelfPos1.z);
                    Vector3 keyPos2 = new Vector3(shelfPos2.x, shelfPos2.y + 0.2f, shelfPos2.z);
                    keyPositions.Add(keyPos);
                    keyPositions.Add(keyPos2);
                }
                else if (roomType == RoomType.ExitKeyRoom)
                {
                    Vector3 keyPos = new Vector3(shelfPos1.x, shelfPos1.y + 0.3f, shelfPos1.z);
                    Vector3 keyPos2 = new Vector3(shelfPos2.x, shelfPos2.y + 0.3f, shelfPos2.z);
                    exitKeyPositions.Add(keyPos);
                    exitKeyPositions.Add(keyPos2);
                }

                // Neighbour positions are walls
                if (ShelfValidity(new Vector3(TempShelfList[midShelf].x + 1, TempShelfList[midShelf].y, TempShelfList[midShelf].z), possibleWallHorizontalPosition, true))
                {
                    Vector3 shelfRightPos1 = new Vector3(shelfPos1.x + 1, shelfPos1.y, shelfPos1.z);
                    Vector3 shelfRightPos2 = new Vector3(shelfPos1.x + 1, shelfPos1.y + 0.5f, shelfPos1.z);
                    ShelfHorizontalPositions.Add(shelfRightPos1);
                    ShelfHorizontalRotations.Add(bottomRotation);
                    ShelfHorizontalPositions.Add(shelfRightPos2);
                    ShelfHorizontalRotations.Add(bottomRotation);

                    // Generate potential key positions
                    if (roomType == RoomType.FirstKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfRightPos1.x, shelfRightPos1.y + 0.2f, shelfRightPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfRightPos2.x, shelfRightPos2.y + 0.2f, shelfRightPos2.z);
                        keyPositions.Add(keyPos);
                        keyPositions.Add(keyPos2);
                    }
                    else if (roomType == RoomType.ExitKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfRightPos1.x, shelfRightPos1.y + 0.3f, shelfRightPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfRightPos2.x, shelfRightPos2.y + 0.3f, shelfRightPos2.z);
                        exitKeyPositions.Add(keyPos);
                        exitKeyPositions.Add(keyPos2);
                    }
                }
                if (ShelfValidity(new Vector3(TempShelfList[midShelf].x - 1, TempShelfList[midShelf].y, TempShelfList[midShelf].z), possibleWallHorizontalPosition, true))
                {
                    Vector3 shelfLeftPos1 = new Vector3(shelfPos1.x - 1, shelfPos1.y, shelfPos1.z);
                    Vector3 shelfLeftPos2 = new Vector3(shelfPos1.x - 1, shelfPos1.y + 0.5f, shelfPos1.z);
                    ShelfHorizontalPositions.Add(shelfLeftPos1);
                    ShelfHorizontalRotations.Add(bottomRotation);
                    ShelfHorizontalPositions.Add(shelfLeftPos2);
                    ShelfHorizontalRotations.Add(bottomRotation);

                    // Generate potential key positions
                    if (roomType == RoomType.FirstKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfLeftPos1.x, shelfLeftPos1.y + 0.2f, shelfLeftPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfLeftPos2.x, shelfLeftPos2.y + 0.2f, shelfLeftPos2.z);
                        keyPositions.Add(keyPos);
                        keyPositions.Add(keyPos2);
                    }
                    else if (roomType == RoomType.ExitKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfLeftPos1.x, shelfLeftPos1.y + 0.3f, shelfLeftPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfLeftPos2.x, shelfLeftPos2.y + 0.3f, shelfLeftPos2.z);
                        exitKeyPositions.Add(keyPos);
                        exitKeyPositions.Add(keyPos2);
                    }
                }
            }
            // Desk
            else if (set == 2 && TempDeskList.Count > 0)
            {
                // Pick a random position from the valid list
                int midDesk = Random.Range(0, TempDeskList.Count);
                // Create position for Desk and chair
                Vector3 deskPos = new Vector3(TempDeskList[midDesk].x - 0.5f, TempDeskList[midDesk].y, TempDeskList[midDesk].z + 0.465f);
                Vector3 chairPos = new Vector3(deskPos.x, deskPos.y, TempDeskList[midDesk].z + 0.85f);
                Instantiate(desk, deskPos, Quaternion.Euler(bottomRotation), transform);
                Instantiate(chair, chairPos, Quaternion.Euler(topRotation), transform);

                if (roomType == RoomType.FirstKeyRoom && !hasKey)
                {
                    Vector3 keyPos = new Vector3(deskPos.x, deskPos.y + 0.85f, deskPos.z);
                    keyPositions.Add(keyPos);
                }
                else if (roomType == RoomType.ExitKeyRoom && !hasExitKey)
                {
                    Vector3 keyPos = new Vector3(deskPos.x, deskPos.y + 1f, deskPos.z);
                    exitKeyPositions.Add(keyPos);
                }

                // If neighbour wall is valid instantiate wardrobe
                if (TempDeskList.Contains(new Vector3(TempDeskList[midDesk].x - 1, TempDeskList[midDesk].y, TempDeskList[midDesk].z)))
                {
                    Vector3 wardrobePos = new Vector3(deskPos.x - 1.25f, deskPos.y, TempDeskList[midDesk].z + 0.4f);
                    Instantiate(wardrobe, wardrobePos, Quaternion.Euler(bottomRotation), transform);
                    Vector3 candlePos = new Vector3(deskPos.x + 0.75f, deskPos.y, TempDeskList[midDesk].z + 0.35f);
                    Instantiate(candles, candlePos, Quaternion.Euler(bottomRotation), transform);
                }
                else if (TempDeskList.Contains(new Vector3(TempDeskList[midDesk].x + 1, TempDeskList[midDesk].y, TempDeskList[midDesk].z)))
                {
                    Vector3 wardrobePos = new Vector3(deskPos.x + 1.25f, deskPos.y, TempDeskList[midDesk].z + 0.4f);
                    Instantiate(wardrobe, wardrobePos, Quaternion.Euler(bottomRotation), transform);
                    Vector3 candlePos = new Vector3(deskPos.x - 0.75f, deskPos.y, TempDeskList[midDesk].z + 0.35f);
                    Instantiate(candles, candlePos, Quaternion.Euler(bottomRotation), transform);
                }
                else
                {
                    Vector3 candlePos = new Vector3(deskPos.x + 0.75f, deskPos.y, TempDeskList[midDesk].z + 0.35f);
                    Instantiate(candles, candlePos, Quaternion.Euler(bottomRotation), transform);
                }
            }
            // Flags
            else if (set == 3 && TempDeskList.Count > 0)
            {
                // Create position
                for (int i = 0; i < TempDeskList.Count; i += 3)
                {
                    Vector3 flagPos = new Vector3(TempDeskList[i].x - 0.5f, TempDeskList[i].y + 2.8f, TempDeskList[i].z + 0.2f);
                    Instantiate(flag, flagPos, Quaternion.Euler(bottomRotation), transform);
                }
            }
            TempShelfList = new List<Vector3>();
            TempDeskList = new List<Vector3>();

            // Top wall
            for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
            {
                var shelfPos = new Vector3(row, heightPos, topLeftV.z);
                if (ShelfValidity(shelfPos, possibleWallHorizontalPosition, true))
                {
                    TempShelfList.Add(shelfPos);
                }
                if (DeskValidity(shelfPos, possibleWallHorizontalPosition, true))
                {
                    TempDeskList.Add(shelfPos);
                }
            }

            // Pick a random number 0 - 99
            rand = Random.Range(0, 100);
            // Decide between flags, desk, or shelves
            set = 0;
            if (hasDesk && hasFlags)
            {
                set = 1; // Shelves
            }
            else if (hasDesk && !hasFlags)
            {
                if (rand >= 0 && rand < 50)
                {
                    set = 3; // flags
                    hasFlags = true;
                }
                else if (rand >= 50 && rand < 100)
                {
                    set = 1; // Shelves
                }
            }
            else if (!hasDesk && hasFlags)
            {
                if (rand >= 0 && rand < 50)
                {
                    set = 2; // Desk
                    hasDesk = true;
                }
                else if (rand >= 50 && rand < 100)
                {
                    set = 1; // Shelves
                }
            }
            else if (!hasDesk && !hasFlags)
            {
                if (rand >= 0 && rand <= flagChance)
                {
                    set = 3; // flags
                    hasFlags = true;
                }
                else if (rand > flagChance && rand <= DeskChance)
                {
                    set = 2; // Desk
                    hasDesk = true;
                }
                else if (rand > DeskChance && rand <= shelfChance)
                {
                    set = 1; // Shelf
                }
            }
            // Top side shelves
            if (set == 1 && TempShelfList.Count > 0)
            {
                int midShelf = Random.Range(0, TempShelfList.Count);
                // Create positions for bottom and top shelves
                Vector3 shelfPos1 = new Vector3(TempShelfList[midShelf].x + 0.5f, TempShelfList[midShelf].y + 1f, TempShelfList[midShelf].z - 0.27f);
                Vector3 shelfPos2 = new Vector3(TempShelfList[midShelf].x + 0.5f, TempShelfList[midShelf].y + 1.5f, TempShelfList[midShelf].z - 0.27f);
                // Add them to the lists
                ShelfHorizontalPositions.Add(shelfPos1);
                ShelfHorizontalRotations.Add(topRotation);
                ShelfHorizontalPositions.Add(shelfPos2);
                ShelfHorizontalRotations.Add(topRotation);

                // Generate potential key positions
                if (roomType == RoomType.FirstKeyRoom)
                {
                    Vector3 keyPos = new Vector3(shelfPos1.x, shelfPos1.y + 0.2f, shelfPos1.z);
                    Vector3 keyPos2 = new Vector3(shelfPos2.x, shelfPos2.y + 0.2f, shelfPos2.z);
                    keyPositions.Add(keyPos);
                    keyPositions.Add(keyPos2);
                }
                else if (roomType == RoomType.ExitKeyRoom)
                {
                    Vector3 keyPos = new Vector3(shelfPos1.x, shelfPos1.y + 0.3f, shelfPos1.z);
                    Vector3 keyPos2 = new Vector3(shelfPos2.x, shelfPos2.y + 0.3f, shelfPos2.z);
                    exitKeyPositions.Add(keyPos);
                    exitKeyPositions.Add(keyPos2);
                }
                if (ShelfValidity(new Vector3(TempShelfList[midShelf].x + 1, TempShelfList[midShelf].y, TempShelfList[midShelf].z), possibleWallHorizontalPosition, true))
                {
                    Vector3 shelfRightPos1 = new Vector3(shelfPos1.x + 1, shelfPos1.y, shelfPos1.z);
                    Vector3 shelfRightPos2 = new Vector3(shelfPos1.x + 1, shelfPos1.y + 0.5f, shelfPos1.z);
                    ShelfHorizontalPositions.Add(shelfRightPos1);
                    ShelfHorizontalRotations.Add(topRotation);
                    ShelfHorizontalPositions.Add(shelfRightPos2);
                    ShelfHorizontalRotations.Add(topRotation);
                    // Generate potential key positions
                    if (roomType == RoomType.FirstKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfRightPos1.x, shelfRightPos1.y + 0.2f, shelfRightPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfRightPos2.x, shelfRightPos2.y + 0.2f, shelfRightPos2.z);
                        keyPositions.Add(keyPos);
                        keyPositions.Add(keyPos2);
                    }
                    else if (roomType == RoomType.ExitKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfRightPos1.x, shelfRightPos1.y + 0.3f, shelfRightPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfRightPos2.x, shelfRightPos2.y + 0.3f, shelfRightPos2.z);
                        exitKeyPositions.Add(keyPos);
                        exitKeyPositions.Add(keyPos2);
                    }
                }
                if (ShelfValidity(new Vector3(TempShelfList[midShelf].x - 1, TempShelfList[midShelf].y, TempShelfList[midShelf].z), possibleWallHorizontalPosition, true))
                {
                    Vector3 shelfLeftPos1 = new Vector3(shelfPos1.x - 1, shelfPos1.y, shelfPos1.z);
                    Vector3 shelfLeftPos2 = new Vector3(shelfPos1.x - 1, shelfPos1.y + 0.5f, shelfPos1.z);
                    ShelfHorizontalPositions.Add(shelfLeftPos1);
                    ShelfHorizontalRotations.Add(topRotation);
                    ShelfHorizontalPositions.Add(shelfLeftPos2);
                    ShelfHorizontalRotations.Add(topRotation);
                    // Generate potential key positions
                    if (roomType == RoomType.FirstKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfLeftPos1.x, shelfLeftPos1.y + 0.2f, shelfLeftPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfLeftPos2.x, shelfLeftPos2.y + 0.2f, shelfLeftPos2.z);
                        keyPositions.Add(keyPos);
                        keyPositions.Add(keyPos2);
                    }
                    else if (roomType == RoomType.ExitKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfLeftPos1.x, shelfLeftPos1.y + 0.3f, shelfLeftPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfLeftPos2.x, shelfLeftPos2.y + 0.3f, shelfLeftPos2.z);
                        exitKeyPositions.Add(keyPos);
                        exitKeyPositions.Add(keyPos2);
                    }
                }
            }
            // Desk - top side
            else if (set == 2 && TempDeskList.Count > 0)
            {
                // Pick a random position from the valid list
                int midDesk = Random.Range(0, TempDeskList.Count);
                // Create position for Desk and chair
                Vector3 deskPos = new Vector3(TempDeskList[midDesk].x - 0.5f, TempDeskList[midDesk].y, TempDeskList[midDesk].z - 0.39f);
                Vector3 chairPos = new Vector3(deskPos.x, deskPos.y, TempDeskList[midDesk].z - 0.75f);
                Instantiate(desk, deskPos, Quaternion.Euler(topRotation), transform);
                Instantiate(chair, chairPos, Quaternion.Euler(bottomRotation), transform);

                if (roomType == RoomType.FirstKeyRoom)
                {
                    Vector3 keyPos = new Vector3(deskPos.x, deskPos.y + 0.85f, deskPos.z);
                    keyPositions.Add(keyPos);
                }
                else if (roomType == RoomType.ExitKeyRoom)
                {
                    Vector3 keyPos = new Vector3(deskPos.x, deskPos.y + 1f, deskPos.z);
                    exitKeyPositions.Add(keyPos);
                }

                // If neighbour wall is valid instantiate wardrobe and candles
                if (TempDeskList.Contains(new Vector3(TempDeskList[midDesk].x + 1, TempDeskList[midDesk].y, TempDeskList[midDesk].z)))
                {
                    Vector3 wardrobePos = new Vector3(deskPos.x + 1.25f, deskPos.y, TempShelfList[midDesk].z - 0.3f);
                    Instantiate(wardrobe, wardrobePos, Quaternion.Euler(topRotation), transform);
                    Vector3 candlePos = new Vector3(deskPos.x - 0.75f, deskPos.y, TempDeskList[midDesk].z - 0.3f);
                    Instantiate(candles, candlePos, Quaternion.Euler(topRotation), transform);
                }
                else if (TempDeskList.Contains(new Vector3(TempDeskList[midDesk].x - 1, TempDeskList[midDesk].y, TempDeskList[midDesk].z)))
                {
                    Vector3 wardrobePos = new Vector3(deskPos.x - 1.25f, deskPos.y, TempShelfList[midDesk].z - 0.3f);
                    Instantiate(wardrobe, wardrobePos, Quaternion.Euler(topRotation), transform);
                    Vector3 candlePos = new Vector3(deskPos.x + 0.75f, deskPos.y, TempDeskList[midDesk].z - 0.3f);
                    Instantiate(candles, candlePos, Quaternion.Euler(topRotation), transform);
                }
                else
                {
                    Vector3 candlePos = new Vector3(deskPos.x - 0.75f, deskPos.y, TempDeskList[midDesk].z - 0.3f);
                    Instantiate(candles, candlePos, Quaternion.Euler(topRotation), transform);
                }
            }
            // Flags
            else if (set == 3 && TempDeskList.Count > 0)
            {
                // Create position
                for (int i = 0; i < TempDeskList.Count; i += 3)
                {
                    Vector3 flagPos = new Vector3(TempDeskList[i].x - 0.5f, TempDeskList[i].y + 2.8f, TempDeskList[i].z - 0.125f);
                    Instantiate(flag, flagPos, Quaternion.Euler(topRotation), transform);
                }             
            }
            TempShelfList = new List<Vector3>();
            TempDeskList = new List<Vector3>();

            // Left wall
            for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
            {
                var shelfPos = new Vector3(bottomLeftV.x, heightPos, col);
                if (ShelfValidity(shelfPos, possibleWallVerticalPosition, false))
                {
                    TempShelfList.Add(shelfPos);
                }
                if (DeskValidity(shelfPos, possibleWallVerticalPosition, false))
                {
                    TempDeskList.Add(shelfPos);
                }
            }

            // Pick a random number 0 - 99
            rand = Random.Range(0, 100);
            // Decide between flags, desk, or shelves
            set = 0;
            if (hasDesk && hasFlags)
            {
                set = 1; // Shelves
            }
            else if (hasDesk && !hasFlags)
            {
                if (rand >= 0 && rand < 50)
                {
                    set = 3; // flags
                    hasFlags = true;
                }
                else if (rand >= 50 && rand < 100)
                {
                    set = 1; // Shelves
                }
            }
            else if (!hasDesk && hasFlags)
            {
                if (rand >= 0 && rand < 50)
                {
                    set = 2; // Desk
                    hasDesk = true;
                }
                else if (rand >= 50 && rand < 100)
                {
                    set = 1; // Shelves
                }
            }
            else if (!hasDesk && !hasFlags)
            {
                if (rand >= 0 && rand <= flagChance)
                {
                    set = 3; // flags
                    hasFlags = true;
                }
                else if (rand > flagChance && rand <= DeskChance)
                {
                    set = 2; // Desk
                    hasDesk = true;
                }
                else if (rand > DeskChance && rand <= shelfChance)
                {
                    set = 1; // Shelf
                }
            }
            // Left side shelves
            if (set == 1 && TempShelfList.Count > 0)
            {
                int midShelf = Random.Range(0, TempShelfList.Count);
                Vector3 shelfPos1 = new Vector3(TempShelfList[midShelf].x + 0.35f, TempShelfList[midShelf].y + 1f, TempShelfList[midShelf].z + 0.5f);
                Vector3 shelfPos2 = new Vector3(TempShelfList[midShelf].x + 0.35f, TempShelfList[midShelf].y + 1.5f, TempShelfList[midShelf].z + 0.5f);
                ShelfVerticalPositions.Add(shelfPos1);
                ShelfVerticalRotations.Add(leftRotation);
                ShelfVerticalPositions.Add(shelfPos2);
                ShelfVerticalRotations.Add(leftRotation);

                // Generate potential key positions
                if (roomType == RoomType.FirstKeyRoom)
                {
                    Vector3 keyPos = new Vector3(shelfPos1.x, shelfPos1.y + 0.2f, shelfPos1.z);
                    Vector3 keyPos2 = new Vector3(shelfPos2.x, shelfPos2.y + 0.2f, shelfPos2.z);
                    keyPositions.Add(keyPos);
                    keyPositions.Add(keyPos2);
                }
                else if (roomType == RoomType.ExitKeyRoom)
                {
                    Vector3 keyPos = new Vector3(shelfPos1.x, shelfPos1.y + 0.3f, shelfPos1.z);
                    Vector3 keyPos2 = new Vector3(shelfPos2.x, shelfPos2.y + 0.3f, shelfPos2.z);
                    exitKeyPositions.Add(keyPos);
                    exitKeyPositions.Add(keyPos2);
                }
                if (ShelfValidity(new Vector3(TempShelfList[midShelf].x, TempShelfList[midShelf].y, TempShelfList[midShelf].z + 1), possibleWallVerticalPosition, false))                 
                {
                    Vector3 shelfRightPos1 = new Vector3(shelfPos1.x, shelfPos1.y, shelfPos1.z + 1);
                    Vector3 shelfRightPos2 = new Vector3(shelfPos1.x, shelfPos1.y + 0.5f, shelfPos1.z + 1);
                    ShelfVerticalPositions.Add(shelfRightPos1);
                    ShelfVerticalRotations.Add(leftRotation);
                    ShelfVerticalPositions.Add(shelfRightPos2);
                    ShelfVerticalRotations.Add(leftRotation);
                    // Generate potential key positions
                    if (roomType == RoomType.FirstKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfRightPos1.x, shelfRightPos1.y + 0.2f, shelfRightPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfRightPos2.x, shelfRightPos2.y + 0.2f, shelfRightPos2.z);
                        keyPositions.Add(keyPos);
                        keyPositions.Add(keyPos2);
                    }
                    else if (roomType == RoomType.ExitKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfRightPos1.x, shelfRightPos1.y + 0.3f, shelfRightPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfRightPos2.x, shelfRightPos2.y + 0.3f, shelfRightPos2.z);
                        exitKeyPositions.Add(keyPos);
                        exitKeyPositions.Add(keyPos2);
                    }
                }
                if (ShelfValidity(new Vector3(TempShelfList[midShelf].x, TempShelfList[midShelf].y, TempShelfList[midShelf].z - 1), possibleWallVerticalPosition, false))
                {
                    Vector3 shelfLeftPos1 = new Vector3(shelfPos1.x, shelfPos1.y, shelfPos1.z - 1);
                    Vector3 shelfLeftPos2 = new Vector3(shelfPos1.x, shelfPos1.y + 0.5f, shelfPos1.z - 1);
                    ShelfVerticalPositions.Add(shelfLeftPos1);
                    ShelfVerticalRotations.Add(leftRotation);
                    ShelfVerticalPositions.Add(shelfLeftPos2);
                    ShelfVerticalRotations.Add(leftRotation);
                    // Generate potential key positions
                    if (roomType == RoomType.FirstKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfLeftPos1.x, shelfLeftPos1.y + 0.2f, shelfLeftPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfLeftPos2.x, shelfLeftPos2.y + 0.2f, shelfLeftPos2.z);
                        keyPositions.Add(keyPos);
                        keyPositions.Add(keyPos2);
                    }
                    else if (roomType == RoomType.ExitKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfLeftPos1.x, shelfLeftPos1.y + 0.3f, shelfLeftPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfLeftPos2.x, shelfLeftPos2.y + 0.3f, shelfLeftPos2.z);
                        exitKeyPositions.Add(keyPos);
                        exitKeyPositions.Add(keyPos2);
                    }
                }
            }
            // Desk - Left side
            else if (set == 2 && TempDeskList.Count > 0)
            {
                // Pick a random position from the valid list
                int midDesk = Random.Range(0, TempDeskList.Count);
                // Create position for Desk and chair
                Vector3 deskPos = new Vector3(TempDeskList[midDesk].x + 0.465f, TempDeskList[midDesk].y, TempDeskList[midDesk].z + 0.5f);
                Vector3 chairPos = new Vector3(TempDeskList[midDesk].x + 0.85f, deskPos.y, deskPos.z);
                Instantiate(desk, deskPos, Quaternion.Euler(leftRotation), transform);
                Instantiate(chair, chairPos, Quaternion.Euler(rightRotation), transform);

                if (roomType == RoomType.FirstKeyRoom)
                {
                    Vector3 keyPos = new Vector3(deskPos.x, deskPos.y + 0.85f, deskPos.z);
                    keyPositions.Add(keyPos);
                }
                else if (roomType == RoomType.ExitKeyRoom)
                {
                    Vector3 keyPos = new Vector3(deskPos.x, deskPos.y + 1f, deskPos.z);
                    exitKeyPositions.Add(keyPos);
                }

                // If neighbour wall is valid instantiate wardrobe
                if (TempDeskList.Contains(new Vector3(TempDeskList[midDesk].x, TempDeskList[midDesk].y, TempDeskList[midDesk].z + 1)))
                {
                    Vector3 wardrobePos = new Vector3(TempDeskList[midDesk].x + 0.4f, deskPos.y, deskPos.z + 1.25f);
                    Instantiate(wardrobe, wardrobePos, Quaternion.Euler(leftRotation), transform);
                    Vector3 candlePos = new Vector3(TempDeskList[midDesk].x + 0.3f, deskPos.y, deskPos.z - 0.75f);
                    Instantiate(candles, candlePos, Quaternion.Euler(leftRotation), transform);
                }
                else if (TempDeskList.Contains(new Vector3(TempDeskList[midDesk].x, TempDeskList[midDesk].y, TempDeskList[midDesk].z - 1)))
                {
                    Vector3 wardrobePos = new Vector3(TempDeskList[midDesk].x + 0.4f, deskPos.y, deskPos.z - 1.25f);
                    Instantiate(wardrobe, wardrobePos, Quaternion.Euler(leftRotation), transform);
                    Vector3 candlePos = new Vector3(TempDeskList[midDesk].x + 0.3f, deskPos.y, deskPos.z + 0.75f);
                    Instantiate(candles, candlePos, Quaternion.Euler(leftRotation), transform);
                }
                else
                {
                    Vector3 candlePos = new Vector3(TempDeskList[midDesk].x + 0.3f, deskPos.y, deskPos.z - 0.75f);
                    Instantiate(candles, candlePos, Quaternion.Euler(leftRotation), transform);
                }
            }
            // Flags - left side
            else if (set == 3 && TempDeskList.Count > 0)
            {
                // Create position
                for (int i = 0; i < TempDeskList.Count; i += 3)
                {
                    Vector3 flagPos = new Vector3(TempDeskList[i].x + 0.2f, TempDeskList[i].y + 2.8f, TempDeskList[i].z + 0.5f);
                    Instantiate(flag, flagPos, Quaternion.Euler(leftRotation), transform);
                }
            }
            TempShelfList = new List<Vector3>();
            TempDeskList = new List<Vector3>();

            // Right wall
            for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
            {
                var shelfPos = new Vector3(bottomRightV.x, heightPos, col);
                if (ShelfValidity(shelfPos, possibleWallVerticalPosition, false))
                {
                    TempShelfList.Add(shelfPos);
                }
                if (DeskValidity(shelfPos, possibleWallVerticalPosition, false))
                {
                    TempDeskList.Add(shelfPos);
                }
            }
            // Pick a random number 0 - 99
            rand = Random.Range(0, 100);
            // Decide between flags, desk, or shelves
            set = 0;
            if (hasDesk && hasFlags)
            {
                set = 1; // Shelves
            }
            else if (hasDesk && !hasFlags)
            {
                if (rand >= 0 && rand < 50)
                {
                    set = 3; // flags
                    hasFlags = true;
                }
                else if (rand >= 50 && rand < 100)
                {
                    set = 1; // Shelves
                }
            }
            else if (!hasDesk && hasFlags)
            {
                if (rand >= 0 && rand < 50)
                {
                    set = 2; // Desk
                    hasDesk = true;
                }
                else if (rand >= 50 && rand < 100)
                {
                    set = 1; // Shelves
                }
            }
            else if (!hasDesk && !hasFlags)
            {
                if (rand >= 0 && rand <= flagChance)
                {
                    set = 3; // flags
                    hasFlags = true;
                }
                else if (rand > flagChance && rand <= DeskChance)
                {
                    set = 2; // Desk
                    hasDesk = true;
                }
                else if (rand > DeskChance && rand <= shelfChance)
                {
                    set = 1; // Shelf
                }
            }
            // Right side shelves
            if (set == 1 && TempShelfList.Count > 0)
            {
                int midShelf = Random.Range(0, TempShelfList.Count);
                Vector3 shelfPos1 = new Vector3(TempShelfList[midShelf].x - 0.25f, TempShelfList[midShelf].y + 1f, TempShelfList[midShelf].z + 0.5f);
                Vector3 shelfPos2 = new Vector3(TempShelfList[midShelf].x - 0.25f, TempShelfList[midShelf].y + 1.5f, TempShelfList[midShelf].z + 0.5f);
                ShelfVerticalPositions.Add(shelfPos1);
                ShelfVerticalRotations.Add(rightRotation);
                ShelfVerticalPositions.Add(shelfPos2);
                ShelfVerticalRotations.Add(rightRotation);

                // Generate potential key positions
                if (roomType == RoomType.FirstKeyRoom)
                {
                    Vector3 keyPos = new Vector3(shelfPos1.x, shelfPos1.y + 0.2f, shelfPos1.z);
                    Vector3 keyPos2 = new Vector3(shelfPos2.x, shelfPos2.y + 0.2f, shelfPos2.z);
                    keyPositions.Add(keyPos);
                    keyPositions.Add(keyPos2);
                }
                else if (roomType == RoomType.ExitKeyRoom)
                {
                    Vector3 keyPos = new Vector3(shelfPos1.x, shelfPos1.y + 0.3f, shelfPos1.z);
                    Vector3 keyPos2 = new Vector3(shelfPos2.x, shelfPos2.y + 0.3f, shelfPos2.z);
                    exitKeyPositions.Add(keyPos);
                    exitKeyPositions.Add(keyPos2);
                }

                if (ShelfValidity(new Vector3(TempShelfList[midShelf].x, TempShelfList[midShelf].y, TempShelfList[midShelf].z + 1), possibleWallVerticalPosition, false))
                {
                    Vector3 shelfRightPos1 = new Vector3(shelfPos1.x, shelfPos1.y, shelfPos1.z + 1);
                    Vector3 shelfRightPos2 = new Vector3(shelfPos1.x, shelfPos1.y + 0.5f, shelfPos1.z + 1);
                    ShelfVerticalPositions.Add(shelfRightPos1);
                    ShelfVerticalRotations.Add(rightRotation);
                    ShelfVerticalPositions.Add(shelfRightPos2);
                    ShelfVerticalRotations.Add(rightRotation);
                    // Generate potential key positions
                    if (roomType == RoomType.FirstKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfRightPos1.x, shelfRightPos1.y + 0.2f, shelfRightPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfRightPos2.x, shelfRightPos2.y + 0.2f, shelfRightPos2.z);
                        keyPositions.Add(keyPos);
                        keyPositions.Add(keyPos2);
                    }
                    else if (roomType == RoomType.ExitKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfRightPos1.x, shelfRightPos1.y + 0.3f, shelfRightPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfRightPos2.x, shelfRightPos2.y + 0.3f, shelfRightPos2.z);
                        exitKeyPositions.Add(keyPos);
                        exitKeyPositions.Add(keyPos2);
                    }
                }
                if (ShelfValidity(new Vector3(TempShelfList[midShelf].x, TempShelfList[midShelf].y, TempShelfList[midShelf].z - 1), possibleWallVerticalPosition, false))
                {
                    Vector3 shelfLeftPos1 = new Vector3(shelfPos1.x, shelfPos1.y, shelfPos1.z - 1);
                    Vector3 shelfLeftPos2 = new Vector3(shelfPos1.x, shelfPos1.y + 0.5f, shelfPos1.z - 1);
                    ShelfVerticalPositions.Add(shelfLeftPos1);
                    ShelfVerticalRotations.Add(rightRotation);
                    ShelfVerticalPositions.Add(shelfLeftPos2);
                    ShelfVerticalRotations.Add(rightRotation);
                    // Generate potential key positions
                    if (roomType == RoomType.FirstKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfLeftPos1.x, shelfLeftPos1.y + 0.2f, shelfLeftPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfLeftPos2.x, shelfLeftPos2.y + 0.2f, shelfLeftPos2.z);
                        keyPositions.Add(keyPos);
                        keyPositions.Add(keyPos2);
                    }
                    else if (roomType == RoomType.ExitKeyRoom)
                    {
                        Vector3 keyPos = new Vector3(shelfLeftPos1.x, shelfLeftPos1.y + 0.3f, shelfLeftPos1.z);
                        Vector3 keyPos2 = new Vector3(shelfLeftPos2.x, shelfLeftPos2.y + 0.3f, shelfLeftPos2.z);
                        exitKeyPositions.Add(keyPos);
                        exitKeyPositions.Add(keyPos2);
                    }
                }
            }
            // Desk - right side
            else if (set == 2 && TempDeskList.Count > 0)
            {
                // Pick a random position from the valid list
                int midDesk = Random.Range(0, TempDeskList.Count);
                // Create position for Desk and chair
                Vector3 deskPos = new Vector3(TempDeskList[midDesk].x - 0.39f, TempDeskList[midDesk].y, TempDeskList[midDesk].z + 0.5f);
                Vector3 chairPos = new Vector3(TempDeskList[midDesk].x - 0.75f, deskPos.y, deskPos.z);
                Instantiate(desk, deskPos, Quaternion.Euler(rightRotation), transform);
                Instantiate(chair, chairPos, Quaternion.Euler(leftRotation), transform);

                if (roomType == RoomType.FirstKeyRoom)
                {
                    Vector3 keyPos = new Vector3(deskPos.x, deskPos.y + 0.85f, deskPos.z);
                    keyPositions.Add(keyPos);
                }
                else if (roomType == RoomType.ExitKeyRoom)
                {
                    Vector3 keyPos = new Vector3(deskPos.x, deskPos.y + 1f, deskPos.z);
                    exitKeyPositions.Add(keyPos);
                }

                // If neighbour wall is valid instantiate wardrobe
                if (TempDeskList.Contains(new Vector3(TempDeskList[midDesk].x, TempDeskList[midDesk].y, TempDeskList[midDesk].z - 1)))
                {
                    Vector3 wardrobePos = new Vector3(TempDeskList[midDesk].x - 0.3f, deskPos.y, deskPos.z - 1.25f);
                    Instantiate(wardrobe, wardrobePos, Quaternion.Euler(rightRotation), transform);
                    Vector3 candlePos = new Vector3(TempDeskList[midDesk].x - 0.3f, deskPos.y, deskPos.z + 0.75f);
                    Instantiate(candles, candlePos, Quaternion.Euler(rightRotation), transform);
                }
                else if (TempDeskList.Contains(new Vector3(TempDeskList[midDesk].x, TempDeskList[midDesk].y, TempDeskList[midDesk].z + 1)))
                {
                    Vector3 wardrobePos = new Vector3(TempDeskList[midDesk].x - 0.3f, deskPos.y, deskPos.z + 1.25f);
                    Instantiate(wardrobe, wardrobePos, Quaternion.Euler(rightRotation), transform);
                    Vector3 candlePos = new Vector3(TempDeskList[midDesk].x - 0.3f, deskPos.y, deskPos.z - 0.75f);
                    Instantiate(candles, candlePos, Quaternion.Euler(rightRotation), transform);
                }
                else
                {
                    Vector3 candlePos = new Vector3(TempDeskList[midDesk].x - 0.3f, deskPos.y, deskPos.z + 0.75f);
                    Instantiate(candles, candlePos, Quaternion.Euler(rightRotation), transform);
                }
            }
            // Flags - Right side
            else if (set == 3 && TempDeskList.Count > 0)
            {
                // Create position
                for (int i = 0; i < TempDeskList.Count; i += 3)
                {
                    Vector3 flagPos = new Vector3(TempDeskList[i].x - 0.1f, TempDeskList[i].y + 2.8f, TempDeskList[i].z + 0.5f);
                    Instantiate(flag, flagPos, Quaternion.Euler(rightRotation), transform);
                }               
            }
        }
        else if (roomType == RoomType.ExitRoom)
        {
            bool top = true;
            bool right = true;
            bool bottom = true;
            bool left = true;
            // top
            for (int row = (int)topLeftV.x; row < (int)topRightV.x; row++)
            {
                var doorPos = new Vector3(row, heightPos, topLeftV.z);
                if (possibleDoorHorizontalPosition.Contains(doorPos))
                {
                    top = false;
                }
            }
            // right
            for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
            {
                var doorPos = new Vector3(bottomRightV.x, heightPos, col);
                if (possibleDoorVerticalPosition.Contains(doorPos))
                {
                    right = false;
                }
            }
            // bottom
            for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
            {
                var doorPos = new Vector3(row, heightPos, bottomLeftV.z);
                if (possibleDoorHorizontalPosition.Contains(doorPos))
                {
                    bottom = false;
                }
            }
            // left
            for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
            {
                var doorPos = new Vector3(bottomLeftV.x, heightPos, col);
                if (possibleDoorHorizontalPosition.Contains(doorPos))
                {
                    left = false;
                }
            }

            if (top)
            {
                var doorPos = new Vector3(Random.Range(topLeftV.x + 1, topRightV.x - 1), heightPos, topLeftV.z - 0.06f);
                Instantiate(exitDoor, doorPos, Quaternion.identity, transform);
            }
            if (right)
            {
                var doorPos = new Vector3(bottomRightV.x - 0.06f, heightPos, Random.Range(bottomRightV.z + 1, topRightV.z - 1));
                Instantiate(exitDoor, doorPos, Quaternion.Euler(0,90,0), transform);
            }
            if (bottom)
            {
                var doorPos = new Vector3(Random.Range(bottomLeftV.x + 1, bottomRightV.x - 1), heightPos, bottomLeftV.z + 0.15f);
                Instantiate(exitDoor, doorPos, Quaternion.identity, transform);
            }
            if (left)
            {
                var doorPos = new Vector3(bottomLeftV.x + 0.15f, heightPos, Random.Range(bottomLeftV.z + 1, topLeftV.z - 1));
                Instantiate(exitDoor, doorPos, Quaternion.Euler(0, 90, 0), transform);
            }
           
        }
    }

    private bool DeskValidity(Vector3 deskPos, List<Vector3> wallList, bool horizontal)
    {
        if (!wallList.Contains(deskPos))
        {
            return false;
        }
        if (horizontal)
        {
            int index = wallList.IndexOf(deskPos);
            Vector3 plusPos = new Vector3(deskPos.x + 2, deskPos.y, deskPos.z);
            Vector3 minusPos = new Vector3(deskPos.x - 2, deskPos.y, deskPos.z);
            // Neighbour positions are walls
            if (index < wallList.Count - 2 && index > 2)
            {
                if ((wallList[index + 2] == plusPos || wallList[index + 2] == minusPos) && (wallList[index - 2] == plusPos || wallList[index - 2] == minusPos))
                {
                    return true;
                }
            }
        }
        else
        {
            int index = wallList.IndexOf(deskPos);
            Vector3 plusPos = new Vector3(deskPos.x, deskPos.y, deskPos.z + 2);
            Vector3 minusPos = new Vector3(deskPos.x, deskPos.y, deskPos.z - 2);
            // Neighbour positions are walls
            if (index < wallList.Count - 2 && index > 2)
            {
                if ((wallList[index + 2] == plusPos || wallList[index + 2] == minusPos) && (wallList[index - 2] == plusPos || wallList[index - 2] == minusPos))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool ShelfValidity(Vector3 shelfPos, List<Vector3> wallList, bool horizontal)
    {
        if (!wallList.Contains(shelfPos))
        {
            return false;
        }
        if (horizontal)
        {
            int index = wallList.IndexOf(shelfPos);
            Vector3 plusPos = new Vector3(shelfPos.x + 1, shelfPos.y, shelfPos.z);
            Vector3 minusPos = new Vector3(shelfPos.x - 1, shelfPos.y, shelfPos.z);
            // Neighbour positions are walls
            if (index < wallList.Count - 1 && index > 1)
            {
                if ((wallList[index + 1] == plusPos || wallList[index + 1] == minusPos) && (wallList[index - 1] == plusPos || wallList[index - 1] == minusPos))
                {
                    return true;
                }
            }
        }
        else
        {
            int index = wallList.IndexOf(shelfPos);
            Vector3 plusPos = new Vector3(shelfPos.x, shelfPos.y, shelfPos.z + 1);
            Vector3 minusPos = new Vector3(shelfPos.x, shelfPos.y, shelfPos.z - 1);
            // Neighbour positions are walls
            if (index < wallList.Count - 1 && index > 1)
            {
                if ((wallList[index + 1] == plusPos || wallList[index + 1] == minusPos) && (wallList[index - 1] == plusPos || wallList[index - 1] == minusPos))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void GenerateWallPos(Vector2 bottomLeftCorner, Vector2 topRightCorner, float heightPos, int diff, int height, bool floors, RoomType roomType)
    {
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, heightPos, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, heightPos, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, heightPos, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, heightPos, topRightCorner.y);



        switch (diff)
        {
            // Left room is lower by 1.5
            case 1:
                for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, bottomLeftV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    var wallPosition2nd = new Vector3(row, heightPos + 3f, bottomLeftV.z);
                    AddWallPositionToList(wallPosition2nd, possibleWallHorizontalSmallPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, topRightV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    var wallPosition2nd = new Vector3(row, heightPos + 3f, topRightV.z);
                    AddWallPositionToList(wallPosition2nd, possibleWallHorizontalSmallPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
                {
                    var wallPosition = new Vector3(bottomLeftV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    if (!floors)
                    {
                        var wallPosition2nd = new Vector3(bottomLeftV.x, heightPos + 3f, col);
                        AddWallPositionToList(wallPosition2nd, possibleWallVerticalSmallPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    }
                }
                for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
                {
                    var wallPosition = new Vector3(bottomRightV.x, heightPos + 1.5f, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                break;
            // Left room is lower by 3
            case 2:
                for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, bottomLeftV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    var wallPosition2nd = new Vector3(row, heightPos + 3f, bottomLeftV.z);
                    AddWallPositionToList(wallPosition2nd, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, topRightV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    var wallPosition2nd = new Vector3(row, heightPos + 3f, topRightV.z);
                    AddWallPositionToList(wallPosition2nd, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
                {
                    var wallPosition = new Vector3(bottomLeftV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    if (!floors)
                    {
                        var wallPosition2nd = new Vector3(bottomLeftV.x, heightPos + 3f, col);
                        AddWallPositionToList(wallPosition2nd, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    }
                }
                for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
                {
                    var wallPosition2nd = new Vector3(bottomRightV.x, heightPos + 3f, col);
                    AddWallPositionToList(wallPosition2nd, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                break;
            // Left room is higher by 1.5
            case 3:
                for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, bottomLeftV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    var wallPosition2nd = new Vector3(row, heightPos + 3f, bottomLeftV.z);
                    AddWallPositionToList(wallPosition2nd, possibleWallHorizontalSmallPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, topRightV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    var wallPosition2nd = new Vector3(row, heightPos + 3f, topRightV.z);
                    AddWallPositionToList(wallPosition2nd, possibleWallHorizontalSmallPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
                {
                    var wallPosition = new Vector3(bottomLeftV.x, heightPos + 1.5f, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
                {
                    var wallPosition = new Vector3(bottomRightV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    if (!floors)
                    {
                        var wallPosition2nd = new Vector3(bottomRightV.x, heightPos + 3f, col);
                        AddWallPositionToList(wallPosition2nd, possibleWallVerticalSmallPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    }
                }
                break;
            // Left room is higher by 3
            case 4:
                for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, bottomLeftV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    var wallPosition2nd = new Vector3(row, heightPos + 3f, bottomLeftV.z);
                    AddWallPositionToList(wallPosition2nd, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, topRightV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    var wallPosition2nd = new Vector3(row, heightPos + 3f, topRightV.z);
                    AddWallPositionToList(wallPosition2nd, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
                {
                    var wallPosition2nd = new Vector3(bottomLeftV.x, heightPos + 3f, col);
                    AddWallPositionToList(wallPosition2nd, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
                {
                    var wallPosition = new Vector3(bottomRightV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    if (!floors)
                    {
                        var wallPosition2nd = new Vector3(bottomRightV.x, heightPos + 3f, col);
                        AddWallPositionToList(wallPosition2nd, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    }
                }
                break;
            // Bottom room is lower by 1.5
            case 5:
                for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, bottomLeftV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    if (!floors)
                    {
                        var wallPosition2nd = new Vector3(row, heightPos + 3f, bottomLeftV.z);
                        AddWallPositionToList(wallPosition2nd, possibleWallHorizontalSmallPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    }
                }
                for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos + 1.5f, topRightV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
                {
                    var wallPosition = new Vector3(bottomLeftV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    var wallPosition2nd = new Vector3(bottomLeftV.x, heightPos + 3f, col);
                    AddWallPositionToList(wallPosition2nd, possibleWallVerticalSmallPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
                {
                    var wallPosition = new Vector3(bottomRightV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    var wallPosition2nd = new Vector3(bottomRightV.x, heightPos + 3f, col);
                    AddWallPositionToList(wallPosition2nd, possibleWallVerticalSmallPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                break;
            // Bottom room is lower by 3
            case 6:
                for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, bottomLeftV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    if (!floors)
                    {
                        var wallPosition2nd = new Vector3(row, heightPos + 3, bottomLeftV.z);
                        AddWallPositionToList(wallPosition2nd, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    }
                }
                for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
                {
                    var wallPosition2nd = new Vector3(row, heightPos + 3, topRightV.z);
                    AddWallPositionToList(wallPosition2nd, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
                {
                    var wallPosition = new Vector3(bottomLeftV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    var wallPosition2nd = new Vector3(bottomLeftV.x, heightPos + 3, col);
                    AddWallPositionToList(wallPosition2nd, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
                {
                    var wallPosition = new Vector3(bottomRightV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    var wallPosition2nd = new Vector3(bottomRightV.x, heightPos + 3, col);
                    AddWallPositionToList(wallPosition2nd, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                break;
            // Bottom room is higher by 1.5
            case 7:
                for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos + 1.5f, bottomLeftV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, topRightV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    if (!floors)
                    {
                        var wallPosition2nd = new Vector3(row, heightPos + 3f, topRightV.z);
                        AddWallPositionToList(wallPosition2nd, possibleWallHorizontalSmallPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    }
                }
                for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
                {
                    var wallPosition = new Vector3(bottomLeftV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    var wallPosition2nd = new Vector3(bottomLeftV.x, heightPos + 3f, col);
                    AddWallPositionToList(wallPosition2nd, possibleWallVerticalSmallPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
                {
                    var wallPosition = new Vector3(bottomRightV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    var wallPosition2nd = new Vector3(bottomRightV.x, heightPos + 3f, col);
                    AddWallPositionToList(wallPosition2nd, possibleWallVerticalSmallPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                break;
            // Bottom room is higher by 3
            case 8:
                for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
                {
                    var wallPosition2nd = new Vector3(row, heightPos + 3f, bottomLeftV.z);
                    AddWallPositionToList(wallPosition2nd, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, topRightV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    if (!floors)
                    {
                        var wallPosition2nd = new Vector3(row, heightPos + 3f, topRightV.z);
                        AddWallPositionToList(wallPosition2nd, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    }
                }
                for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
                {
                    var wallPosition = new Vector3(bottomLeftV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    var wallPosition2nd = new Vector3(bottomLeftV.x, heightPos + 3f, col);
                    AddWallPositionToList(wallPosition2nd, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
                {
                    var wallPosition = new Vector3(bottomRightV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    var wallPosition2nd = new Vector3(bottomRightV.x, heightPos + 3f, col);
                    AddWallPositionToList(wallPosition2nd, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                break;
            // Regular rooms and same height level corridors
            default:
                for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, bottomLeftV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
                {
                    var wallPosition = new Vector3(row, heightPos, topRightV.z);
                    AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                }
                for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
                {
                    var wallPosition = new Vector3(bottomLeftV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
                {
                    var wallPosition = new Vector3(bottomRightV.x, heightPos, col);
                    AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                }
                if (height >= 2)
                {
                    for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
                    {
                        var wallPosition = new Vector3(row, heightPos + 3f, bottomLeftV.z);
                        AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    }
                    for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
                    {
                        var wallPosition = new Vector3(row, heightPos + 3f, topRightV.z);
                        AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    }
                    for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
                    {
                        var wallPosition = new Vector3(bottomLeftV.x, heightPos + 3f, col);
                        AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    }
                    for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
                    {
                        var wallPosition = new Vector3(bottomRightV.x, heightPos + 3f, col);
                        AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    }
                }
                if (height >= 3)
                {
                    for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
                    {
                        var wallPosition = new Vector3(row, heightPos + 6f, bottomLeftV.z);
                        AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    }
                    for (int row = (int)topLeftV.x; row < (int)topRightCorner.x; row++)
                    {
                        var wallPosition = new Vector3(row, heightPos + 6f, topRightV.z);
                        AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition, roomTypesForHorizontalDoors, roomType);
                    }
                    for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
                    {
                        var wallPosition = new Vector3(bottomLeftV.x, heightPos + 6f, col);
                        AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    }
                    for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
                    {
                        var wallPosition = new Vector3(bottomRightV.x, heightPos + 6f, col);
                        AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition, roomTypesForVerticalDoors, roomType);
                    }
                }
                break;
        }
    }

    private void CreateCeiling(Vector2 bottomLeftCorner, Vector2 topRightCorner, float heightPos)
    {
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, heightPos, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, heightPos, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, heightPos, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, heightPos, topRightCorner.y);

        Vector3 thickness = Vector3.up * 0.05f;

        Vector3[] vertices = new Vector3[] {
                // Top Face
                topLeftV, topRightV, bottomLeftV, bottomRightV,
                //Bottom Face
                topLeftV - thickness, topRightV - thickness, bottomLeftV - thickness, bottomRightV - thickness
            };

        // Define the faces (triangles) of the cube
        Face[] faces = new Face[] { new Face(new int[]
        {
            // Top face
            1, 2, 0, 1, 3, 2,
            // Bottom face
            6, 5, 4, 7, 5, 6,
            // Side faces
            0,4,1,1,4,5,
            1,5,3,3,5,7,
            3,7,2,2,7,6,
            2,6,0,0,6,4 }
        ) };

        // ProBuilder Mesh
        GameObject dungeonFloor = new GameObject("Ceiling: " + bottomLeftV + ", HeightPos: " + heightPos, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));

        // Create a cube
        ProBuilderMesh pbMesh = dungeonFloor.AddComponent<ProBuilderMesh>();

        // Set the modified vertices back to the ProBuilder mesh
        pbMesh.RebuildWithPositionsAndFaces(vertices, faces);

        // Rebuild the mesh
        pbMesh.ToMesh();
        dungeonFloor.GetComponent<MeshFilter>().mesh = pbMesh.gameObject.GetComponent<MeshFilter>().sharedMesh;
        dungeonFloor.GetComponent<MeshRenderer>().material = floorMaterial;
        dungeonFloor.GetComponent<MeshCollider>().sharedMesh = pbMesh.gameObject.GetComponent<MeshFilter>().sharedMesh;
        dungeonFloor.transform.parent = transform;
        dungeonFloor.layer = 20;

        pbMesh.Refresh();
    }

    private void FindPillarPosition(Vector3 bl, Vector3 tr)
    {
        float width = tr.x - bl.x;
        float length = tr.z - bl.z;

        Vector3 pillar1Pos;
        Vector3 pillar2Pos;

        int set = 0;
        int side = Random.Range(1,5);
        int chance = Random.Range(0, 99);
        if (chance > 0 && chance < 25)
        {
            set = 1;
        }
        else if (chance >= 25 && chance < 50)
        {
            set = 2;
        }
        // Long terrace
        if (length < width)
        {
            pillar1Pos = new Vector3(bl.x + width * 0.25f, bl.y - 3.11f, bl.z + length * 0.5f);
            pillar2Pos = new Vector3(bl.x + width * 0.75f, bl.y - 3.11f, bl.z + length * 0.5f);
            possiblePillarPosition.Add(pillar1Pos);
            possiblePillarPosition.Add(pillar2Pos);
            if (set == 1)
            {
                CreateBarrels(side, pillar1Pos);
            }
            else if (set == 2)
            {
                CreateBarrels(side, pillar2Pos);
            }
        }
        // Wide terrace
        else if (width < length)
        {
            pillar1Pos = new Vector3(bl.x + width * 0.5f, bl.y - 3.11f, bl.z + length * 0.25f);
            pillar2Pos = new Vector3(bl.x + width * 0.5f, bl.y - 3.11f, bl.z + length * 0.75f);
            possiblePillarPosition.Add(pillar1Pos);
            possiblePillarPosition.Add(pillar2Pos);
            if (set == 1)
            {
                CreateBarrels(side, pillar1Pos);
            }
            else if (set == 2)
            {
                CreateBarrels(side, pillar2Pos);
            }
        }
        else
        {
            pillar1Pos = new Vector3(bl.x + width * 0.5f, bl.y - 3.11f, bl.z + length * 0.5f);
            possiblePillarPosition.Add(pillar1Pos);
            if (set == 1 || set == 2)
            {
                CreateBarrels(side, pillar1Pos);
            }
        }        
    }

    private void CreateBarrels(int side, Vector3 pillarPos)
    {
        Vector3 barrel1;
        Vector3 barrel2;
        Vector3 barrel3;
        switch (side)
        {
            case 1:
                barrel1 = new Vector3(pillarPos.x, pillarPos.y, pillarPos.z + 0.9f);
                barrel2 = new Vector3(pillarPos.x + 0.9f, pillarPos.y, pillarPos.z);
                barrel3 = new Vector3(pillarPos.x + 0.9f, pillarPos.y, pillarPos.z + 0.9f);
                break;
            case 2:
                barrel1 = new Vector3(pillarPos.x + 0.9f, pillarPos.y, pillarPos.z);
                barrel2 = new Vector3(pillarPos.x, pillarPos.y, pillarPos.z - 0.9f);
                barrel3 = new Vector3(pillarPos.x + 0.9f, pillarPos.y, pillarPos.z - 0.9f);
                break;
            case 3:
                barrel1 = new Vector3(pillarPos.x, pillarPos.y, pillarPos.z - 0.9f);
                barrel2 = new Vector3(pillarPos.x - 0.9f, pillarPos.y, pillarPos.z);
                barrel3 = new Vector3(pillarPos.x - 0.9f, pillarPos.y, pillarPos.z - 0.9f);
                break;
            default:
                barrel1 = new Vector3(pillarPos.x - 0.9f, pillarPos.y, pillarPos.z);
                barrel2 = new Vector3(pillarPos.x, pillarPos.y, pillarPos.z + 0.9f);
                barrel3 = new Vector3(pillarPos.x - 0.9f, pillarPos.y, pillarPos.z + 0.9f);
                break;
        }
        Instantiate(barrel, barrel1, Quaternion.Euler(0, Random.Range(0, 360f), 0), transform);
        Instantiate(barrel, barrel2, Quaternion.Euler(0, Random.Range(0, 360f), 0), transform);
        Instantiate(barrel, barrel3, Quaternion.Euler(0, Random.Range(0, 360f), 0), transform);
    }

    private void CreateTerrace(Vector2 bottomLeftCorner, Vector2 topRightCorner, float heightPos, int height, RoomType roomType)
    {
        heightPos += 0.11f;
        float width = topRightCorner.x - bottomLeftCorner.x;
        float length = topRightCorner.y - bottomLeftCorner.y;

        int side = Random.Range(1, 3);

        Vector3 bottomLeftV;
        Vector3 bottomRightV;
        Vector3 topLeftV;
        Vector3 topRightV;

        Vector3 bottomLeftV2 = new Vector3();
        Vector3 bottomRightV2 = new Vector3();
        Vector3 topLeftV2 = new Vector3();
        Vector3 topRightV2 = new Vector3();

        // Long room
        if (length > width)
        {
            // Top
            if (side == 1)
            {
                float Y = Random.Range(topRightCorner.y - length * 0.25f, topRightCorner.y - length * 0.75f);
                bottomLeftV = new Vector3(bottomLeftCorner.x, heightPos, Y);
                bottomRightV = new Vector3(topRightCorner.x, heightPos, Y);
                topLeftV = new Vector3(bottomLeftCorner.x, heightPos, topRightCorner.y);
                topRightV = new Vector3(topRightCorner.x, heightPos, topRightCorner.y);
                Vector3 ladderPos = new Vector3(Random.Range(bottomLeftV.x + 1.66f, bottomRightV.x - 0.33f), heightPos - 3f, bottomLeftV.z - 0.03f);
                possibleLadderPosition.Add(ladderPos);
                possibleLadderRotation.Add(new Vector3(0, 0, 0));
                if (height > 2)
                {
                    float Y2 = Random.Range(Y, bottomLeftCorner.y + length * 0.75f);
                    bottomLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, bottomLeftCorner.y);
                    bottomRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, bottomLeftCorner.y);
                    topLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, Y2);
                    topRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, Y2);
                    Vector3 ladderPos2 = new Vector3(Random.Range(bottomLeftV2.x + 0.37f, bottomRightV2.x - 1.66f), heightPos, topLeftV2.z + 0.06f);
                    possibleLadderPosition.Add(ladderPos2);
                    possibleLadderRotation.Add(new Vector3(0, 180, 0));
                }
            }
            // Bottom
            else
            {
                float Y = Random.Range(bottomLeftCorner.y + length * 0.25f, bottomLeftCorner.y + length * 0.75f);
                bottomLeftV = new Vector3(bottomLeftCorner.x, heightPos, bottomLeftCorner.y);
                bottomRightV = new Vector3(topRightCorner.x, heightPos, bottomLeftCorner.y);
                topLeftV = new Vector3(bottomLeftCorner.x, heightPos, Y);
                topRightV = new Vector3(topRightCorner.x, heightPos, Y);
                Vector3 ladderPos = new Vector3(Random.Range(bottomLeftV.x + 0.37f, bottomRightV.x - 1.66f), heightPos - 3f, topLeftV.z + 0.06f);
                possibleLadderPosition.Add(ladderPos);
                possibleLadderRotation.Add(new Vector3(0, 180, 0));
                if (height > 2)
                {
                    float Y2 = Random.Range(Y, topRightCorner.y - length * 0.75f);
                    bottomLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, Y2);
                    bottomRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, Y2);
                    topLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, topRightCorner.y);
                    topRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, topRightCorner.y);
                    Vector3 ladderPos2 = new Vector3(Random.Range(bottomLeftV2.x + 1.66f, bottomRightV2.x - 0.33f), heightPos, bottomLeftV2.z - 0.03f);
                    possibleLadderPosition.Add(ladderPos2);
                    possibleLadderRotation.Add(new Vector3(0, 0, 0));
                }
            }            
        }
        // Wide room
        else if (length < width)
        {
            // Left
            if (side == 1)
            {
                float X = Random.Range(topRightCorner.x - width * 0.75f, topRightCorner.x - width * 0.25f);
                bottomLeftV = new Vector3(bottomLeftCorner.x, heightPos, bottomLeftCorner.y);
                bottomRightV = new Vector3(X, heightPos, bottomLeftCorner.y);
                topLeftV = new Vector3(bottomLeftCorner.x, heightPos, topRightCorner.y);
                topRightV = new Vector3(X, heightPos, topRightCorner.y);
                Vector3 ladderPos = new Vector3(bottomRightV.x + 0.03f, heightPos - 3f, Random.Range(bottomLeftV.z + 1.66f, topLeftV.z - 0.33f));
                possibleLadderPosition.Add(ladderPos);
                possibleLadderRotation.Add(new Vector3(0, -90, 0));
                if (height > 2)
                {
                    float X2 = Random.Range(topRightCorner.x - width * 0.75f, X);
                    bottomLeftV2 = new Vector3(X2, heightPos + 3f, bottomLeftCorner.y);
                    bottomRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, bottomLeftCorner.y);
                    topLeftV2 = new Vector3(X2, heightPos + 3f, topRightCorner.y);
                    topRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, topRightCorner.y);
                    Vector3 ladderPos2 = new Vector3(bottomLeftV2.x - 0.02f, heightPos, Random.Range(bottomLeftV2.z + 0.33f, topLeftV2.z - 1.66f));
                    possibleLadderPosition.Add(ladderPos2);
                    possibleLadderRotation.Add(new Vector3(0, 90, 0));
                }
            }
            // Right
            else
            {
                float X = Random.Range(topRightCorner.x - width * 0.75f, topRightCorner.x - width * 0.25f);
                bottomLeftV = new Vector3(X, heightPos, bottomLeftCorner.y);
                bottomRightV = new Vector3(topRightCorner.x, heightPos, bottomLeftCorner.y);
                topLeftV = new Vector3(X, heightPos, topRightCorner.y);
                topRightV = new Vector3(topRightCorner.x, heightPos, topRightCorner.y);
                Vector3 ladderPos = new Vector3(bottomLeftV.x - 0.02f, heightPos - 3f, Random.Range(bottomLeftV.z + 0.33f, topLeftV.z - 1.66f));
                possibleLadderPosition.Add(ladderPos);
                possibleLadderRotation.Add(new Vector3(0, 90, 0));
                if (height > 2)
                {
                    float X2 = Random.Range(topRightCorner.x - width * 0.25f, X);
                    bottomLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, bottomLeftCorner.y);
                    bottomRightV2 = new Vector3(X2, heightPos + 3f, bottomLeftCorner.y);
                    topLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, topRightCorner.y);
                    topRightV2 = new Vector3(X2, heightPos + 3f, topRightCorner.y);
                    Vector3 ladderPos2 = new Vector3(bottomRightV2.x + 0.03f, heightPos, Random.Range(bottomLeftV2.z + 1.66f, topLeftV2.z - 0.33f));
                    possibleLadderPosition.Add(ladderPos2);
                    possibleLadderRotation.Add(new Vector3(0, -90, 0));
                }
            }
        }
        // Square room
        else
        {
            // Long
            if (side == 1)
            {
                int side2 = Random.Range(1, 3);
                float Y = Random.Range(topRightCorner.y - length * 0.25f, topRightCorner.y - length * 0.75f);
                // Bottom side
                if (side2 == 1){
                    bottomLeftV = new Vector3(bottomLeftCorner.x, heightPos, bottomLeftCorner.y);
                    bottomRightV = new Vector3(topRightCorner.x, heightPos, bottomLeftCorner.y);
                    topLeftV = new Vector3(bottomLeftCorner.x, heightPos, Y);
                    topRightV = new Vector3(topRightCorner.x, heightPos, Y);
                    Vector3 ladderPos = new Vector3(Random.Range(bottomLeftV.x + 0.37f, bottomRightV.x - 1.66f), heightPos - 3f, topLeftV.z + 0.06f);
                    possibleLadderPosition.Add(ladderPos);
                    possibleLadderRotation.Add(new Vector3(0, 180, 0));
                    if (height > 2)
                    {
                        int side3 = Random.Range(1, 3);
                        float X = Random.Range(bottomLeftCorner.x + width * 0.25f, bottomLeftCorner.x + width * 0.75f);
                        // Left
                        if (side3 == 1)
                        {
                            bottomLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, bottomLeftCorner.y);
                            bottomRightV2 = new Vector3(X, heightPos + 3f, bottomLeftCorner.y);
                            topLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, Y);
                            topRightV2 = new Vector3(X, heightPos + 3f, Y);
                            Vector3 ladderPos2 = new Vector3(bottomRightV2.x + 0.03f, heightPos, Random.Range(bottomLeftV2.z + 1.66f, topLeftV2.z - 0.33f));
                            possibleLadderPosition.Add(ladderPos2);
                            possibleLadderRotation.Add(new Vector3(0, -90, 0));
                        }
                        // Right
                        else
                        {
                            bottomLeftV2 = new Vector3(X, heightPos + 3f, bottomLeftCorner.y);
                            bottomRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, bottomLeftCorner.y);
                            topLeftV2 = new Vector3(X, heightPos + 3f, Y);
                            topRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, Y);
                            Vector3 ladderPos2 = new Vector3(bottomLeftV2.x - 0.02f, heightPos, Random.Range(bottomLeftV2.z + 0.33f, topLeftV2.z - 1.66f));
                            possibleLadderPosition.Add(ladderPos2);
                            possibleLadderRotation.Add(new Vector3(0, 90, 0));
                        }
                    }
                }
                // Top side
                else
                {
                    bottomLeftV = new Vector3(bottomLeftCorner.x, heightPos, Y);
                    bottomRightV = new Vector3(topRightCorner.x, heightPos, Y);
                    topLeftV = new Vector3(bottomLeftCorner.x, heightPos, topRightCorner.y);
                    topRightV = new Vector3(topRightCorner.x, heightPos, topRightCorner.y);
                    Vector3 ladderPos = new Vector3(Random.Range(bottomLeftV.x + 1.66f, bottomRightV.x - 0.33f), heightPos - 3f, bottomLeftV.z - 0.03f);
                    possibleLadderPosition.Add(ladderPos);
                    possibleLadderRotation.Add(new Vector3(0, 0, 0));
                    if (height > 2)
                    {
                        int side3 = Random.Range(1, 3);
                        float X = Random.Range(bottomLeftCorner.x + width * 0.25f, bottomLeftCorner.x + width * 0.75f);
                        // Left
                        if (side3 == 1)
                        {
                            bottomLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, Y);
                            bottomRightV2 = new Vector3(X, heightPos + 3f, Y);
                            topLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, topRightCorner.y);
                            topRightV2 = new Vector3(X, heightPos + 3f, topRightCorner.y);
                            Vector3 ladderPos2 = new Vector3(bottomRightV2.x + 0.03f, heightPos, Random.Range(bottomLeftV2.z + 1.66f, topLeftV2.z - 0.33f));
                            possibleLadderPosition.Add(ladderPos2);
                            possibleLadderRotation.Add(new Vector3(0, -90, 0));
                        }
                        // Right
                        else
                        {
                            bottomLeftV2 = new Vector3(X, heightPos + 3f, Y);
                            bottomRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, Y);
                            topLeftV2 = new Vector3(X, heightPos + 3f, Y);
                            topRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, Y);
                            Vector3 ladderPos2 = new Vector3(bottomLeftV2.x - 0.02f, heightPos, Random.Range(bottomLeftV2.z + 0.33f, topLeftV2.z - 1.66f));
                            possibleLadderPosition.Add(ladderPos2);
                            possibleLadderRotation.Add(new Vector3(0, 90, 0));
                        }
                    }
                }                
            }
            // Wide
            else
            {
                int side2 = Random.Range(1, 3);
                float X = Random.Range(topRightCorner.x - width * 0.25f, topRightCorner.x - width * 0.75f);
                // Left side
                if (side2 == 1)
                {
                    bottomLeftV = new Vector3(bottomLeftCorner.x, heightPos, bottomLeftCorner.y);
                    bottomRightV = new Vector3(X, heightPos, bottomLeftCorner.y);
                    topLeftV = new Vector3(bottomLeftCorner.x, heightPos, topRightCorner.y);
                    topRightV = new Vector3(X, heightPos, topRightCorner.y);
                    Vector3 ladderPos = new Vector3(bottomRightV.x + 0.03f, heightPos - 3f, Random.Range(bottomLeftV.z + 1.66f, topLeftV.z - 0.33f));
                    possibleLadderPosition.Add(ladderPos);
                    possibleLadderRotation.Add(new Vector3(0, -90, 0));
                    if (height > 2)
                    {
                        int side3 = Random.Range(1, 3);
                        float Y = Random.Range(bottomLeftCorner.y + length * 0.25f, bottomLeftCorner.y + length * 0.75f);
                        // Top
                        if (side3 == 1)
                        {
                            bottomLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, Y);
                            bottomRightV2 = new Vector3(X, heightPos + 3f, Y);
                            topLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, topRightCorner.y);
                            topRightV2 = new Vector3(X, heightPos + 3f, topRightCorner.y);
                            Vector3 ladderPos2 = new Vector3(Random.Range(bottomLeftV2.x + 1.66f, bottomRightV2.x - 0.33f), heightPos, bottomLeftV2.z - 0.03f);
                            possibleLadderPosition.Add(ladderPos2);
                            possibleLadderRotation.Add(new Vector3(0, 0, 0));
                        }
                        // Bottom
                        else
                        {
                            bottomLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, bottomLeftCorner.y);
                            bottomRightV2 = new Vector3(X, heightPos + 3f, bottomLeftCorner.y);
                            topLeftV2 = new Vector3(bottomLeftCorner.x, heightPos + 3f, Y);
                            topRightV2 = new Vector3(X, heightPos + 3f, Y);
                            Vector3 ladderPos2 = new Vector3(Random.Range(bottomLeftV2.x + 0.37f, bottomRightV2.x - 1.66f), heightPos, topLeftV2.z + 0.06f);
                            possibleLadderPosition.Add(ladderPos2);
                            possibleLadderRotation.Add(new Vector3(0, 180, 0));
                        }
                    }
                }
                // Right side
                else
                {
                    bottomLeftV = new Vector3(X, heightPos, bottomLeftCorner.y);
                    bottomRightV = new Vector3(topRightCorner.x, heightPos, bottomLeftCorner.y);
                    topLeftV = new Vector3(X, heightPos, topRightCorner.y);
                    topRightV = new Vector3(topRightCorner.x, heightPos, topRightCorner.y);
                    Vector3 ladderPos = new Vector3(bottomLeftV.x - 0.02f, heightPos - 3f, Random.Range(bottomLeftV.z + 0.33f, topLeftV.z - 1.66f));
                    possibleLadderPosition.Add(ladderPos);
                    possibleLadderRotation.Add(new Vector3(0, 90, 0));
                    if (height > 2)
                    {
                        int side3 = Random.Range(1, 3);
                        float Y = Random.Range(bottomLeftCorner.y + length * 0.25f, bottomLeftCorner.y + length * 0.75f);
                        // Top
                        if (side3 == 1)
                        {
                            bottomLeftV2 = new Vector3(X, heightPos + 3f, Y);
                            bottomRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, Y);
                            topLeftV2 = new Vector3(X, heightPos + 3f, topRightCorner.y);
                            topRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, topRightCorner.y);
                            Vector3 ladderPos2 = new Vector3(Random.Range(bottomLeftV2.x + 1.66f, bottomRightV2.x - 0.33f), heightPos, bottomLeftV2.z - 0.03f);
                            possibleLadderPosition.Add(ladderPos2);
                            possibleLadderRotation.Add(new Vector3(0, 0, 0));
                        }
                        // Bottom
                        else
                        {
                            bottomLeftV2 = new Vector3(X, heightPos + 3f, bottomLeftCorner.y);
                            bottomRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, bottomLeftCorner.y);
                            topLeftV2 = new Vector3(X, heightPos + 3f, Y);
                            topRightV2 = new Vector3(topRightCorner.x, heightPos + 3f, Y);
                            Vector3 ladderPos2 = new Vector3(Random.Range(bottomLeftV2.x + 0.37f, bottomRightV2.x - 1.66f), heightPos, topLeftV2.z + 0.06f);
                            possibleLadderPosition.Add(ladderPos2);
                            possibleLadderRotation.Add(new Vector3(0, 180, 0));
                        }
                    }
                }
            }
            
        }

        FindPillarPosition(bottomLeftV, topRightV);

        Vector3 thickness = Vector3.up * 0.05f;

        Vector3[] vertices = new Vector3[] {
                // Top Face
                topLeftV, topRightV, bottomLeftV, bottomRightV,
                //Bottom Face
                topLeftV - thickness, topRightV - thickness, bottomLeftV - thickness, bottomRightV - thickness
            };

        // Define the faces (triangles) of the cube
        Face[] faces = new Face[] { new Face(new int[]
        {
            // Top face
            1, 2, 0, 1, 3, 2,
            // Bottom face
            6, 5, 4, 7, 5, 6,
            // Side faces
            0,4,1,1,4,5,
            1,5,3,3,5,7,
            3,7,2,2,7,6,
            2,6,0,0,6,4}
        ) };

        Vector3 temp = (bottomLeftV + topRightV) / 2;
        midPoints.Add(new Vector3(temp.x, heightPos, temp.z));

        // ProBuilder Mesh
        GameObject dungeonFloor = new GameObject("Terrace: " + temp + ", HeightPos: " + heightPos + ", Height: " + height, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));

        // Create a cube
        ProBuilderMesh pbMesh = dungeonFloor.AddComponent<ProBuilderMesh>();

        // Set the modified vertices back to the ProBuilder mesh
        pbMesh.RebuildWithPositionsAndFaces(vertices, faces);

        // Rebuild the mesh
        pbMesh.ToMesh();
        dungeonFloor.GetComponent<MeshFilter>().mesh = pbMesh.gameObject.GetComponent<MeshFilter>().sharedMesh;
        dungeonFloor.GetComponent<MeshRenderer>().material = floorMaterial;
        dungeonFloor.GetComponent<MeshCollider>().sharedMesh = pbMesh.gameObject.GetComponent<MeshFilter>().sharedMesh;
        dungeonFloor.transform.parent = transform;
        dungeonFloor.layer = 11;       

        pbMesh.Refresh();

        if (height > 2)
        {
            Vector3[] vertices2 = new Vector3[]
            {
                // Top Face
                topLeftV2, topRightV2, bottomLeftV2, bottomRightV2,
                // Bottom Face
                topLeftV2 - thickness, topRightV2 - thickness, bottomLeftV2 - thickness, bottomRightV2 - thickness
            };

            // ProBuilder Mesh
            GameObject dungeonFloor2 = new GameObject("Terrace: " + bottomLeftV2 + ", HeightPos: " + (heightPos + 3f) + ", Height: " + height, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));

            // Create a cube
            ProBuilderMesh pbMesh2 = dungeonFloor2.AddComponent<ProBuilderMesh>();

            // Set the modified vertices back to the ProBuilder mesh
            pbMesh2.RebuildWithPositionsAndFaces(vertices2, faces);

            // Rebuild the mesh
            pbMesh2.ToMesh();
            dungeonFloor2.GetComponent<MeshFilter>().mesh = pbMesh2.gameObject.GetComponent<MeshFilter>().sharedMesh;
            dungeonFloor2.GetComponent<MeshRenderer>().material = floorMaterial;
            dungeonFloor2.GetComponent<MeshCollider>().sharedMesh = pbMesh2.gameObject.GetComponent<MeshFilter>().sharedMesh;
            dungeonFloor2.transform.parent = transform;
            dungeonFloor2.layer = 11;

            pbMesh2.Refresh();

            Vector3 temp2 = (bottomLeftV2 + topRightV2) / 2;
            midPoints.Add(new Vector3(temp2.x, heightPos + 3f, temp2.z));

            if (roomType == RoomType.PlayerRoom || roomType == RoomType.EnemyRoom)
            {
                Vector3 pos = (bottomLeftV2 + topRightV2) / 2;
                SpawnEntities(pos, roomType);
            }
        }
        if (height == 2 && (roomType == RoomType.PlayerRoom || roomType == RoomType.EnemyRoom))
        {
            Vector3 pos = (bottomLeftV + topRightV) / 2;
            SpawnEntities(pos, roomType);
        }
    }

    private void AddWallPositionToList(Vector3 wallPosition, List<Vector3> wallList, List<Vector3> doorList, List<RoomType> roomTypes, RoomType roomType)
    {
        if (wallList.Contains(wallPosition)) 
        {
            doorList.Add(wallPosition);
            roomTypes.Add(roomType);
            wallList.Remove(wallPosition);
        }
        else
        {
            wallList.Add(wallPosition);
        }
    }

    private void DestroyAllChildren()
    {
        while(transform.childCount != 0)
        {
            foreach(Transform item in transform)
            {
                DestroyImmediate(item.gameObject);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
//Don't show the Warnings / Messages with the descriptions below, in the console. 
//CS0649 = "<value> is never assigned and will always have its default value of "blah"" Hidden because those objects that would have their default values, are set in the inspector.
//IDE0044 = "Make field readonly". Hidden because readonly hides the var in the inspector, meaning you can't assign a value, causing null ref errors.
//IDE0051 = "Private member <function / event name> is unused". Hidden becasue it is not true. They are called by Unity, Visual Studio just doesn't seem to know that.
#pragma warning disable CS0649, IDE0044, IDE0051 

public class GridController : MonoBehaviour
{
    [SerializeField] private bool generateTerrain = true;
    [Header("Objects")]
    [SerializeField] private GameObject worldObj;
    [SerializeField] private GameObject empty;
    [SerializeField] private GameObject landPiece;
    [SerializeField] private GameObject roadPiece;
    [SerializeField] private GameObject waterPiece;

    [SerializeField] private GameObject sedanSpawner;
    [SerializeField] private GameObject truckSpawner; 
    [SerializeField] private GameObject smLogSpawner;
    [SerializeField] private GameObject mdLogSpawner; 
    [SerializeField] private GameObject lgLogSpawner; 
    [SerializeField] private GameObject coin;
    [SerializeField] [Range(0,1)] private float spawnratePercent = 0.02f;
    [Header("Other Variables")]
    public int environmentWidth = 10;
    public int chunkHeight = 10;
    [SerializeField] private float spawnerXOffset = 20;

    [SerializeField] private GameObject[] prefabLibrary;

    public float gridSideLength = 1f;

    private Transform testObjTransform;
    private float zoffset = 0f;
    private Transform worldParent;

    private int chunkCounter;
    private int totalHeight = 4;

    private int lastMovingRowDirection = -1;
    //-1 = doesn't matter
    //0 = left to right
    //1 = right to left

    void Start()
    {
        worldParent = GameObject.Find("WorldParent").transform;
        testObjTransform = worldObj.GetComponent<Transform>();

        if (generateTerrain == true)
        {
            GenerateChunk(-1, true);
            GenerateChunk(0, true);
            GenerateChunk(1);
            GenerateChunk(2);
            GenerateChunk(3);
            GenerateChunk(4);
        }
    }
	void Update()
    {
        testObjTransform.position = SnapPositionToGrid(testObjTransform.position);
    }

    private void GenerateChunk(int zChunkOffset, bool startingChunk = false)
    {
        GameObject[,] tempArray;
        tempArray = new GameObject[environmentWidth, chunkHeight];

        //generate chunk object
        GameObject chunkEmptyParent = Instantiate(empty);
        chunkEmptyParent.transform.position = GetPositionOnGrid(0f, 0f, zChunkOffset * chunkHeight);
        chunkEmptyParent.transform.SetParent(worldParent);
        chunkEmptyParent.name = "chunk";

        //TODO, REPLACE THIS \/ make it so that grounds are generated per row, not per chunk, also factor in if it's a moving row

        // in chunk, z pos = 2*h - 0.5
        //10 rows, numbered 0 to 9

        

        int lastPos = 0;
        int movingRows = Random.Range(3, 7);//2-4 per chunk

        if (startingChunk)
        {
            movingRows = 0;
        }
        int[] movingRowPositions = new int[movingRows];
        int createdRows = 0;
        for (int i = 0; i < movingRows; i++)
        {
            movingRowPositions[i] = Random.Range(lastPos, Mathf.Min(9,lastPos+2)+1);
            if (movingRowPositions[i] >= lastPos && movingRowPositions[i]<10)
            {
                lastPos = movingRowPositions[i]+1;
                createdRows++;
            }
            else
            {
                break;
                //overboard
            }
            
        }


        movingRows = createdRows;
        for (int h = 0; h < chunkHeight; h++)//generate rows of platform objects to fill chunk
        {
            GameObject emptyParent = Instantiate(empty);
            empty.transform.position = GetPositionOnGrid(0f, 0f, h + zChunkOffset * chunkHeight);
            emptyParent.transform.SetParent(chunkEmptyParent.transform);
            
            bool flagForMovingRow = false;

            for(int i =0; i < movingRows; i++)
            {
                if (movingRowPositions[i] == h)
                {
                    flagForMovingRow = true;
                    break;
                }
            }
            if (flagForMovingRow)
            {//moving row
                emptyParent.name = "movingRow";

                //random value
                int typeInt = Random.Range(0, 5);

                //TODO: change ground based on typeInt
                GameObject groundToSpawn;

                if (typeInt < 3)
                {
                    groundToSpawn = waterPiece;
                }
                else
                {
                    groundToSpawn = roadPiece;
                }

                GameObject objectToSpawn = smLogSpawner;

                switch (typeInt)
                {
                    case 0://small log
                        objectToSpawn = smLogSpawner;
                        break;
                    case 1://medium log
                        objectToSpawn = mdLogSpawner;
                        break;
                    case 2://long log
                        objectToSpawn = lgLogSpawner;
                        break;
                    case 3://sedan
                        objectToSpawn = sedanSpawner;
                        break;
                    case 4://truck
                        objectToSpawn = truckSpawner;
                        break;
                }

                //JG edited here - changed the spawn X pos to a straight 5 so that the spawners spawn in the middle of the road/water piece, and so the spawning of the inital cars (on game start)
                //can be a simple offset from the spawners position. Since the spawn position of the objects not spawned on start is no longer based on the positon of the spawner, this will work fine. 
                GameObject spawner = Instantiate(objectToSpawn, new Vector3(5, 0, h), Quaternion.Euler(Vector3.zero), emptyParent.transform);
                spawner.transform.localPosition = new Vector3(0f, 0f, 1f);
                
                MovingObjectSpawner MCS = spawner.GetComponent<MovingObjectSpawner>();

                switch (lastMovingRowDirection)
                {
                    case -1:
                        MCS.objMoveRtoL = (Random.Range(0f, 1f) > 0.5f);
                        break;
                    case 0:
                        MCS.objMoveRtoL = true;
                        break;
                    case 1:
                        MCS.objMoveRtoL = false;
                        break;
                }

                if (MCS.objMoveRtoL)
                {
                    lastMovingRowDirection = 1;
                }
                else
                {
                    lastMovingRowDirection = 0;
                }


                if (typeInt < 3) {
                    MCS.spawnRotation = new Vector3(0, 0, 0);
                }
                else
                {
                    if (MCS.objMoveRtoL)
                    {
                        MCS.spawnRotation = new Vector3(0, -90, 0);
                    }
                    else
                    {
                        MCS.spawnRotation = new Vector3(0, 90, 0);
                    }
                }

                if (MCS.objMoveRtoL)
                {
                    spawner.transform.position = new Vector3(spawnerXOffset, spawner.transform.position.y, spawner.transform.position.z);
                }
                else
                {
                    spawner.transform.position = new Vector3(-spawnerXOffset, spawner.transform.position.y, spawner.transform.position.z);
                }

                


                GameObject ground = Instantiate(groundToSpawn);
                ground.transform.SetParent(chunkEmptyParent.transform);
                ground.transform.localPosition = new Vector3(5f, 0, h);
                ground.transform.localScale = new Vector3(32f, 1f, 1f);
                ground.transform.SetParent(emptyParent.transform);
            }
            else
            {//normal row

                lastMovingRowDirection = -1;
                //emptyParent.name = "row";

				//randomly add 3 blockers per row
				int obstacles = 0;

				int targetObstacles = Random.Range(1, 4);

				List<int> obstaclesList = new List<int>();

				while (obstacles != targetObstacles)
				{
                    int newObstacle;
                    if (startingChunk)
                    {// [][][][*][*][*][*][][][] astreiks indicate where obstacles cannot spawn in starting chunks
                        newObstacle = Random.Range(0, environmentWidth-4);
                        if (newObstacle > 2)
                        {
                            newObstacle += 4;
                        }
                    } else
                    {
                        newObstacle = Random.Range(0, environmentWidth);
                    }
					if (!obstaclesList.Contains(newObstacle))
					{
						obstaclesList.Add(newObstacle);
						obstacles++;
					}
				}

				for (int w = 0; w < environmentWidth; w++)//spawn blocker
				{
					if (obstaclesList.Contains(w))
					{
                        if (Random.Range(0f, 1f) < spawnratePercent)
                        {//spawn coin
                            tempArray[w, h] = Instantiate(coin);
                        }
                        else
                        {//spawn blocker
                            tempArray[w, h] = Instantiate(prefabLibrary[Random.Range(0, prefabLibrary.Length)]);
                        }

						tempArray[w, h].GetComponent<Transform>().position = GetPositionOnGrid(w, 0f, h + zChunkOffset * chunkHeight);
						tempArray[w, h].transform.SetParent(emptyParent.transform);
					}
				}

				GameObject ground = Instantiate(landPiece);
                ground.transform.SetParent(chunkEmptyParent.transform);
                ground.transform.localPosition = new Vector3(5f, 0, h);
                ground.transform.localScale = new Vector3(32f, 1f, 1f);
                ground.transform.SetParent(emptyParent.transform);

				spawnScenery(0);
				spawnScenery(1);
				spawnScenery(2);
				spawnScenery(3);
				spawnScenery(4);
			}

            //spawns bits of trees on the outer edge of the camera
            void spawnScenery(int pos)
            {
                GameObject obj = Instantiate(prefabLibrary[Random.Range(0, prefabLibrary.Length)]);
                obj.transform.position = GetPositionOnGrid(-1 -pos, 0f, h + zChunkOffset * chunkHeight);
                obj.transform.SetParent(emptyParent.transform);

                obj = Instantiate(prefabLibrary[Random.Range(0, prefabLibrary.Length)]);
                obj.transform.position = GetPositionOnGrid(environmentWidth + pos, 0f, h + zChunkOffset * chunkHeight);
                obj.transform.SetParent(emptyParent.transform);
            }
        }
    }

    private void CheckAndDeleteChunks()
    {
        foreach (Transform child in worldParent)
        {
            if (worldParent.localPosition.z < -1f * child.localPosition.z -20f )// the - determines the offset
            {
                Destroy(child.gameObject);
            }
        }
    }

    public Vector3 SnapPositionToGrid(Vector3 inputPos)
    { //snaps a transform's vector 3 to be on a grid, usefull for player after leaving moving object
        float x = Mathf.Round(inputPos.x / gridSideLength) * gridSideLength; 
        float y = inputPos.y;
        float z = Mathf.Round((inputPos.z- zoffset) / gridSideLength) * gridSideLength + zoffset;


        Vector3 output = new Vector3(x, y, z);
        return output;
    }

    public Vector3 GetPositionOnGrid(float x, float height, float z)
    { //returns a vector 3 calculated using coordinates on the grid as input 
        float new_x = x * gridSideLength;
        float new_z = z * gridSideLength + zoffset;
        return new Vector3(new_x, height, new_z);
    }

    public void ChangeZoffset(float relativeChange)
    {//recalculate offset to always be less than grid side length after moving it
        float pre = zoffset;
        zoffset += relativeChange;
        zoffset %= gridSideLength;

        
        float post = zoffset;
        if (post > pre)
        {
            chunkCounter++;
            if (chunkCounter == chunkHeight)
            {
                chunkCounter = 0;
                GenerateChunk(totalHeight);
                CheckAndDeleteChunks();
            }

        }
    }
}

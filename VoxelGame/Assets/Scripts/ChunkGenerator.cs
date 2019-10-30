using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    public BlockDataInternal[,,] blockDataArr = new BlockDataInternal[16, 64, 16];
    public bool[,,] blockExistsArr = new bool[16, 64, 16];
    public float[,] heightMap;
    public int[,] trees;

    public ChunkGenerator forwardChunk;
    public ChunkGenerator backwardChunk;
    public ChunkGenerator leftChunk;
    public ChunkGenerator rightChunk;

    public int xIndex;
    public int yIndex;

    public string getIndexKey()
    {
        string index = xIndex + "," + yIndex;
        return index;
    }

    public void InitiateChunk(float[,] perlinValues, int[,] treesMap)
    {
        heightMap = perlinValues;
        trees = treesMap;
        GenerateBlocks();
    }

    public void GenerateBlocks()
    {
        Vector3 beginning = transform.position;

        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 64; y++)
            {

                for (int z = 0; z < 16; z++)
                {
                    if (y < heightMap[x, z] || y == 0 || blockExistsArr[x,y,z])
                    {

                        if (trees[x, z] == 1 && y + 1 >= heightMap[x, z] && blockExistsArr[x, y, z] == false)
                        {
                            MakeTree(x, y, z);
                        }

                        if (!blockExistsArr[x, y, z])
                        {
                            blockDataArr[x, y, z] = new BlockDataInternal();
                            blockExistsArr[x, y, z] = true;
                            blockDataArr[x, y, z].blockType = 0; //natural
                            if (y > 5) blockDataArr[x, y, z].blockType = 2;
                        }

                        

                        //Now set all blocks neighbor booleans.
                        //DownCheck:
                        if (y > 0) //prevents oob
                        {
                            if (blockExistsArr[x, y - 1, z])
                            {
                                //set for current
                                blockDataArr[x, y, z].DownExists = true;
                                //set for below
                                blockDataArr[x, y - 1, z].UpExists = true;
                                if(y > 1 && blockExistsArr[x, y - 2, z] && blockDataArr[x,y, z].blockType == 0)
                                {
                                    blockDataArr[x, y - 2, z].blockType = 1; //dirt
                                }
                            }
                        }
                        else
                        {
                            blockDataArr[x, y, z].DownExists = true;
                        }
                        //left check
                        if (x > 0) //prevents oob
                        {
                            if (x == 15 && rightChunk == null) blockDataArr[x, y, z].RightExists = false;

                            if (blockExistsArr[x - 1, y, z])
                            {
                                //set for current
                                blockDataArr[x, y, z].LeftExists = true;
                                //set for left
                                blockDataArr[x - 1, y, z].RightExists = true;
                            }
                        }
                        //Try another chunk!
                        else
                        {
                            if (leftChunk != null)
                            {
                                if (leftChunk.blockExistsArr[15, y, z])
                                {
                                    blockDataArr[x, y, z].LeftExists = true;
                                    leftChunk.blockDataArr[15, y, z].RightExists = true;
                                    leftChunk.blockDataArr[15, y, z].DetermineVisibility();
                                }
                            }
                            else blockDataArr[x, y, z].LeftExists = false;
                        }
                        //backCheck
                        if (z > 0) //prevents oob
                        {
                            if (z == 15 && forwardChunk == null) blockDataArr[x, y, z].ForwardExists = false;
                            if (blockExistsArr[x, y, z - 1])
                            {
                                //set for current
                                blockDataArr[x, y, z].BackExists = true;
                                //set for back
                                blockDataArr[x, y, z - 1].ForwardExists = true;
                            }
                        }
                        //Check neighbor Chunk!
                        else
                        {
                            if (backwardChunk != null)
                            {
                                if (backwardChunk.blockExistsArr[x, y, 15])
                                {
                                    blockDataArr[x, y, z].BackExists = true;
                                    backwardChunk.blockDataArr[x, y, 15].ForwardExists = true;
                                    backwardChunk.blockDataArr[x, y, 15].DetermineVisibility();
                                }
                            }
                            else blockDataArr[x, y, z].BackExists = false;
                        }
                    }                    
                }
            }
        }
    }

    public void MakeTree(int x, int y, int z)
    {
        for (int t = 1; t < 6 && t < 58; t++)
        {
            blockDataArr[x, y + t, z] = new BlockDataInternal();
            blockExistsArr[x, y + t, z] = true;
            blockDataArr[x, y + t, z].blockType = 1;
        }
        blockDataArr[x, y + 6, z] = new BlockDataInternal();
        blockExistsArr[x, y + 6, z] = true;
        blockDataArr[x, y + 6, z].blockType = 3;

        int first = Random.Range(2, 5);
        int second = Random.Range(2, 5);
        int third = Random.Range(2, 5);
        int fourth = Random.Range(2, 5);

        if (x > 0)
        {
            blockDataArr[x - 1, y + first, z] = new BlockDataInternal();
            blockExistsArr[x - 1, y + first, z] = true;
            blockDataArr[x - 1, y + first, z].blockType = 3;

        }
        else if (leftChunk != null)
        {

            leftChunk.blockDataArr[15, y + first, z] = new BlockDataInternal();
            leftChunk.blockExistsArr[15, y + first, z] = true;
            leftChunk.blockDataArr[15, y + first, z].blockType = 3;
        }

        if (x < 15)
        {
            blockDataArr[x + 1, y + second, z] = new BlockDataInternal();
            blockExistsArr[x + 1, y + second, z] = true;
            blockDataArr[x + 1, y + second, z].blockType = 3;

        }
        else if (rightChunk != null)
        {

            rightChunk.blockDataArr[0, y + second, z] = new BlockDataInternal();
            rightChunk.blockExistsArr[0, y + second, z] = true;
            rightChunk.blockDataArr[0, y + second, z].blockType = 3;
        }
        if (z < 15)
        {
            blockDataArr[x, y + third, z + 1] = new BlockDataInternal();
            blockExistsArr[x, y + third, z + 1] = true;
            blockDataArr[x, y + third, z + 1].blockType = 3;
        }
        else if (forwardChunk != null)
        {

            forwardChunk.blockDataArr[x, y + third, 0] = new BlockDataInternal();
            forwardChunk.blockExistsArr[x, y + third, 0] = true;
            forwardChunk.blockDataArr[x, y + third, 0].blockType = 3;
        }
        if (z > 0)
        {
            blockDataArr[x, y + fourth, z - 1] = new BlockDataInternal();
            blockExistsArr[x, y + fourth, z - 1] = true;
            blockDataArr[x, y + fourth, z - 1].blockType = 3;
        }
        else if (backwardChunk != null)
        {
            forwardChunk.blockDataArr[x, y + fourth, 15] = new BlockDataInternal();
            forwardChunk.blockExistsArr[x, y + fourth, 15] = true;
            forwardChunk.blockDataArr[x, y + fourth, 15].blockType = 3;
        }

    }

    public void HideInvisible()
    {

        List<Vector3> vertices = new List<Vector3>();
        List<List<int>> triangles = new List<List<int>>();
        for(int i = 0; i < 4; i++)
        {
            List<int> blockType = new List<int>();
            triangles.Add(blockType);
        }

        int offset = 0;
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                for (int z = 0; z < 16; z++)
                {
                    if (blockExistsArr[x, y, z])
                    {
                        if (blockDataArr[x, y, z].DetermineVisibility())
                        {
                            vertices.AddRange(blockDataArr[x, y, z].GetVertices(x, y, z));
                            triangles[blockDataArr[x,y,z].blockType].AddRange(blockDataArr[x, y, z].calculateTriangles(offset));
                            offset += 24;
                        }
                    }

                }
            }
        }

        //Now combine them into one mesh:

        GenerateMesh(vertices, triangles);
    }
    public void GenerateMesh(List<Vector3> vertices, List<List<int>> triangles)
    {
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < uvs.Length; i += 4)
        {
            uvs[i] = new Vector2(0, 0);
            uvs[i + 1] = new Vector2(0, 1);
            uvs[i + 2] = new Vector2(1, 1);
            uvs[i + 3] = new Vector2(1, 0);

        }

        //Vector3 storedPosition = transform.position;
        //transform.position = Vector3.zero;

        MeshFilter filter = transform.GetComponent<MeshFilter>();

        filter.mesh = new Mesh();
        filter.mesh.subMeshCount = 4;
        filter.mesh.SetVertices(vertices);
        int type = 0;
        foreach(List<int> tring in triangles)
        {
            filter.mesh.SetTriangles(tring, type);
            type++;
        }
        filter.mesh.uv = uvs;
        transform.GetComponent<MeshCollider>().sharedMesh = filter.mesh;
        //transform.position = storedPosition;
    }
    public void DestroyBlock(RaycastHit hitInfo, Vector3 direction)
    {
        Vector3 hitInLocalSpace = transform.InverseTransformPoint(hitInfo.point + direction.normalized * .001f);
        int xCoord = (int)hitInLocalSpace.x;
        if (hitInLocalSpace.x % 1 > .5f) xCoord++;
        int yCoord = (int)hitInLocalSpace.y;
        if (hitInLocalSpace.y % 1 > .5f) yCoord++;
        int zCoord = (int)hitInLocalSpace.z;
        if (hitInLocalSpace.z % 1 > .5f) zCoord++;
        if (!blockExistsArr[xCoord, yCoord, zCoord])
        {
            Debug.Log("Garbage hit");
            return;
        }
        if (yCoord == 0) return;

        blockExistsArr[xCoord, yCoord, zCoord] = false;

        //Check to see if any block will be revealed:
        BlockDataInternal deletedBlock = blockDataArr[xCoord, yCoord, zCoord];
        blockDataArr[xCoord, yCoord, zCoord] = null;

        List<MeshFilter> addedMeshesList = new List<MeshFilter>();
        if (deletedBlock.DownExists)
        {
            blockDataArr[xCoord, yCoord - 1, zCoord].UpExists = false;
            blockDataArr[xCoord, yCoord - 1, zCoord].visible = true;
        }
        if (deletedBlock.UpExists && yCoord > 0)
        {
            blockDataArr[xCoord, yCoord + 1, zCoord].DownExists = false;
            blockDataArr[xCoord, yCoord + 1, zCoord].visible = true;
        }

        if (deletedBlock.LeftExists)
        {
            //within this chunk
            if (xCoord > 0)
            {
                blockDataArr[xCoord - 1, yCoord, zCoord].RightExists = false;
                blockDataArr[xCoord - 1, yCoord, zCoord].visible = true;
            }
            //outside this
            else
            {
                leftChunk.blockDataArr[15, yCoord, zCoord].RightExists = false;
                leftChunk.blockDataArr[15, yCoord, zCoord].visible = true;
                leftChunk.HideInvisible();
            }

        }
        if (deletedBlock.RightExists)
        {
            if (xCoord < 15)
            {
                blockDataArr[xCoord + 1, yCoord, zCoord].LeftExists = false;
                blockDataArr[xCoord + 1, yCoord, zCoord].visible = true;
            }
            else
            {
                rightChunk.blockDataArr[0, yCoord, zCoord].LeftExists = false;
                rightChunk.blockDataArr[0, yCoord, zCoord].visible = true;
                rightChunk.HideInvisible();
            }
        }
        if (deletedBlock.ForwardExists)
        {
            if (zCoord < 15)
            {
                blockDataArr[xCoord, yCoord, zCoord + 1].BackExists = false;
                blockDataArr[xCoord, yCoord, zCoord + 1].visible = true;
            }
            else
            {
                forwardChunk.blockDataArr[xCoord, yCoord, 0].BackExists = false;
                forwardChunk.blockDataArr[xCoord, yCoord, 0].visible = true;
                forwardChunk.HideInvisible();
            }
        }
        if (deletedBlock.BackExists)
        {
            if (zCoord > 0)
            {
                blockDataArr[xCoord, yCoord, zCoord - 1].ForwardExists = false;
                blockDataArr[xCoord, yCoord, zCoord - 1].visible = true;
            }
            else
            {
                backwardChunk.blockDataArr[xCoord, yCoord, 15].ForwardExists = false;
                backwardChunk.blockDataArr[xCoord, yCoord, 15].visible = true;
                backwardChunk.HideInvisible();
            }

        }

        //Update mesh with what we've learned.
        HideInvisible();
    }

    public void AddBlock(RaycastHit hitInfo, Vector3 direction, bool trueLocation, int blockType)
    {
        Vector3 hitInLocalSpace = transform.InverseTransformPoint(hitInfo.point - direction.normalized * .001f);
        int xCoord = (int)hitInLocalSpace.x;
        int yCoord = (int)hitInLocalSpace.y;
        int zCoord = (int)hitInLocalSpace.z;

        if (hitInLocalSpace.x % 1 > .5f) xCoord++;
        if (hitInLocalSpace.y % 1 > .5f) yCoord++;
        if (hitInLocalSpace.z % 1 > .5f) zCoord++;

       // Debug.Log("x: " + xCoord + ", Y: " + yCoord + "Z: " + zCoord);

        //check if added in next chunk.
        if (xCoord > 15 && !trueLocation)
        {
            rightChunk.AddBlock(hitInfo, direction, true, blockType);
            return;
        }
        if (hitInLocalSpace.x < 0 && !trueLocation)
        {
            leftChunk.AddBlock(hitInfo, direction, true, blockType);
            return;
        }
        if (zCoord > 15 && !trueLocation)
        {
            forwardChunk.AddBlock(hitInfo, direction, true, blockType);
            return;
        }
        if (hitInLocalSpace.z < 0 && !trueLocation)
        {
            backwardChunk.AddBlock(hitInfo, direction, true, blockType);
            return;
        }

        if (yCoord > 90) return;
        if (zCoord > 15) zCoord = 15;
        if (zCoord < 0) zCoord = 0;
        if (xCoord > 15) xCoord = 15;
        if (xCoord < 0) xCoord = 0;

        blockDataArr[xCoord, yCoord, zCoord] = new BlockDataInternal();
        blockDataArr[xCoord, yCoord, zCoord].blockType = blockType;
        blockExistsArr[xCoord, yCoord, zCoord] = true;

        //Check to see if any block will be hidden:
        BlockDataInternal addedBlock = blockDataArr[xCoord, yCoord, zCoord];
        addedBlock.visible = true;

        if (blockExistsArr[xCoord, yCoord - 1, zCoord]) //up
        {
            addedBlock.DownExists = true;
            blockDataArr[xCoord, yCoord - 1, zCoord].UpExists = true;
        }
        if (yCoord < 63 && blockExistsArr[xCoord, yCoord + 1, zCoord]) // down
        {
            addedBlock.UpExists = true;
            blockDataArr[xCoord, yCoord + 1, zCoord].DownExists = true;
        }

        if (xCoord > 0)
        {
            if (blockExistsArr[xCoord - 1, yCoord, zCoord]) // left
            {
                addedBlock.LeftExists = true;
                blockDataArr[xCoord - 1, yCoord, zCoord].RightExists = true;
            }
        }
        else
        {
            if (leftChunk.blockExistsArr[15, yCoord, zCoord])
            {
                addedBlock.LeftExists = true;
                leftChunk.blockDataArr[15, yCoord, zCoord].RightExists = true;
                leftChunk.HideInvisible();
            }
        }
        if (xCoord < 15) //right
        {
            if (blockExistsArr[xCoord + 1, yCoord, zCoord])
            {
                addedBlock.RightExists = true;
                blockDataArr[xCoord + 1, yCoord, zCoord].LeftExists = true;
            }
        }
        else
        {
            if (rightChunk.blockExistsArr[0, yCoord, zCoord])
            {
                addedBlock.RightExists = true;
                rightChunk.blockDataArr[0, yCoord, zCoord].LeftExists = true;
                rightChunk.HideInvisible();
            }
        }
        if (zCoord > 0)
        {
            if (blockExistsArr[xCoord, yCoord, zCoord - 1]) // backward
            {
                addedBlock.BackExists = true;
                blockDataArr[xCoord, yCoord, zCoord - 1].ForwardExists = true;
            }
        }
        else
        {
            if (backwardChunk.blockExistsArr[xCoord, yCoord, 15])
            {
                addedBlock.BackExists = true;
                backwardChunk.blockDataArr[xCoord, yCoord, 15].ForwardExists = true;
                backwardChunk.HideInvisible();
            }
        }

        if (zCoord < 15)
        {
            if (blockExistsArr[xCoord, yCoord, zCoord + 1]) // forward
            {
                addedBlock.ForwardExists = true;
                blockDataArr[xCoord, yCoord, zCoord + 1].BackExists = true;
            }
        }
        else
        {
            if (forwardChunk.blockExistsArr[xCoord, yCoord, 0])
            {
                addedBlock.ForwardExists = true;
                forwardChunk.blockDataArr[xCoord, yCoord, 0].BackExists = true;
                forwardChunk.HideInvisible();
            }
        }
        HideInvisible();
    }
}

public class BlockDataInternal
{
    public bool UpExists;
    public bool DownExists;
    public bool RightExists;
    public bool LeftExists;
    public bool ForwardExists;
    public bool BackExists;
    public bool visible;

    public int blockType;
    public bool DetermineVisibility()
    {
        //only invisible if all neighbors exist.
        visible = !(UpExists && DownExists && RightExists && LeftExists && ForwardExists && BackExists);
        return visible;
    }

    public Vector3[] GetVertices(int xOffset, int yOffset, int zOffset)
    {
        Vector3[] verticesArr = new Vector3[24];

        //Top
        verticesArr[0] = new Vector3(xOffset + 0.5f, yOffset + 0.5f, zOffset + 0.5f);
        verticesArr[1] = new Vector3(xOffset - 0.5f, yOffset + 0.5f, zOffset + 0.5f);
        verticesArr[2] = new Vector3(xOffset - 0.5f, yOffset + 0.5f, zOffset - 0.5f);
        verticesArr[3] = new Vector3(xOffset + 0.5f, yOffset + 0.5f, zOffset - 0.5f);

        //Bottom
        verticesArr[4] = new Vector3(xOffset + 0.5f, yOffset - 0.5f, zOffset + 0.5f);
        verticesArr[5] = new Vector3(xOffset - 0.5f, yOffset - 0.5f, zOffset + 0.5f);
        verticesArr[6] = new Vector3(xOffset - 0.5f, yOffset - 0.5f, zOffset - 0.5f);
        verticesArr[7] = new Vector3(xOffset + 0.5f, yOffset - 0.5f, zOffset - 0.5f);

        //Forward
        verticesArr[8] = new Vector3(xOffset + 0.5f, yOffset + 0.5f, zOffset + 0.5f);
        verticesArr[9] = new Vector3(xOffset + 0.5f, yOffset - 0.5f, zOffset + 0.5f);
        verticesArr[10] = new Vector3(xOffset - 0.5f, yOffset - 0.5f, zOffset + 0.5f);
        verticesArr[11] = new Vector3(xOffset - 0.5f, yOffset + 0.5f, zOffset + 0.5f);

        //Back
        verticesArr[12] = new Vector3(xOffset + 0.5f, yOffset + 0.5f, zOffset - 0.5f);
        verticesArr[13] = new Vector3(xOffset + 0.5f, yOffset - 0.5f, zOffset - 0.5f);
        verticesArr[14] = new Vector3(xOffset - 0.5f, yOffset - 0.5f, zOffset - 0.5f);
        verticesArr[15] = new Vector3(xOffset - 0.5f, yOffset + 0.5f, zOffset - 0.5f);

        //Right

        verticesArr[16] = new Vector3(xOffset + 0.5f, yOffset + 0.5f, zOffset + 0.5f);
        verticesArr[17] = new Vector3(xOffset + 0.5f, yOffset - 0.5f, zOffset + 0.5f);
        verticesArr[18] = new Vector3(xOffset + 0.5f, yOffset - 0.5f, zOffset - 0.5f);
        verticesArr[19] = new Vector3(xOffset + 0.5f, yOffset + 0.5f, zOffset - 0.5f);

        //Left


        verticesArr[20] = new Vector3(xOffset - 0.5f, yOffset + 0.5f, zOffset + 0.5f);
        verticesArr[21] = new Vector3(xOffset - 0.5f, yOffset - 0.5f, zOffset + 0.5f);
        verticesArr[22] = new Vector3(xOffset - 0.5f, yOffset - 0.5f, zOffset - 0.5f);
        verticesArr[23] = new Vector3(xOffset - 0.5f, yOffset + 0.5f, zOffset - 0.5f);

        return verticesArr;
    }
    public List<int> calculateTriangles(int indexOffset)
    {
        List<int> triangles = new List<int>();
        if (!UpExists)
        {
            triangles.Add(0 + indexOffset);
            triangles.Add(2 + indexOffset);
            triangles.Add(1 + indexOffset);

            triangles.Add(0 + indexOffset);
            triangles.Add(3 + indexOffset);
            triangles.Add(2 + indexOffset);

        }
        if (!DownExists)
        {
            triangles.Add(4 + indexOffset);
            triangles.Add(5 + indexOffset);
            triangles.Add(6 + indexOffset);

            triangles.Add(4 + indexOffset);
            triangles.Add(6 + indexOffset);
            triangles.Add(7 + indexOffset);
        }
        if (!LeftExists)
        {
            triangles.Add(20 + indexOffset);
            triangles.Add(22 + indexOffset);
            triangles.Add(21 + indexOffset);

            triangles.Add(22 + indexOffset);
            triangles.Add(20 + indexOffset);
            triangles.Add(23 + indexOffset);
        }
        if (!RightExists)
        {
            triangles.Add(16 + indexOffset);
            triangles.Add(17 + indexOffset);
            triangles.Add(18 + indexOffset);

            triangles.Add(18 + indexOffset);
            triangles.Add(19 + indexOffset);
            triangles.Add(16 + indexOffset);
        }
        if (!BackExists)
        {
            triangles.Add(12 + indexOffset);
            triangles.Add(13 + indexOffset);
            triangles.Add(14 + indexOffset);

            triangles.Add(12 + indexOffset);
            triangles.Add(14 + indexOffset);
            triangles.Add(15 + indexOffset);
        }
        if (!ForwardExists)
        {
            triangles.Add(8 + indexOffset);
            triangles.Add(10 + indexOffset);
            triangles.Add(9 + indexOffset);

            triangles.Add(10 + indexOffset);
            triangles.Add(8 + indexOffset);
            triangles.Add(11 + indexOffset);
        }

        return triangles;
    }

}


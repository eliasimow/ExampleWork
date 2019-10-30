using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    //Chunk Generation:
    private int ChunkNumber = 11;
    public GameObject ChunkPrefab;
    private GameObject[,] ChunkArr;
    //private ChunkGenerator[,] ChunkGeneratorsArr;

    //Perlin Generation:
    public static int seed = 1200;
    public float heightMultiplier = 5f;
    public int pixelLength;

    //infinite terrain
    public Transform player;
    public ChunkGenerator centerChunk;
    private int centerX = 5;
    private int centerY = 5;

    private bool coroutineGenerating = false;
    Dictionary<string, ChunkGenerator> chunkDictionary;
    public List<ChunkGenerator> presentChunkList;

    public void Start()
    {
        presentChunkList = new List<ChunkGenerator>();
        chunkDictionary = new Dictionary<string, ChunkGenerator>();
        seed *= 1000;
        for (int x = 0; x < ChunkNumber; x++)
        {
            for (int y = 0; y < ChunkNumber; y++)
            {
                CreateChunk(x, y);
            }
        }
        for(int x = 0; x < ChunkNumber; x++)
        {
            for(int y = 0; y < ChunkNumber; y++)
            {
                chunkDictionary[x + "," + y].HideInvisible();
            }
        }

        GetChunk(5, 5, centerChunk);
    }

    public void Update()
    {
        int xPlayerIndex = (int)(player.transform.position.x / 16);
        int yPlayerIndex = (int)(player.transform.position.z / 16);

        if ((Mathf.Abs(xPlayerIndex - centerX) > 1 || Mathf.Abs(yPlayerIndex - centerY) > 1 ) && !coroutineGenerating) //must reinitiate
        {
            coroutineGenerating = true;
            centerX = xPlayerIndex;
            centerY = yPlayerIndex;
            foreach(ChunkGenerator chunk in presentChunkList) //disable the old
            {
                if (Mathf.Abs(xPlayerIndex - chunk.xIndex) > 5 || Mathf.Abs(yPlayerIndex - chunk.yIndex) > 5)
                {
                    chunk.transform.parent.gameObject.SetActive(false);
                }
            }
            presentChunkList.Clear();
            StartCoroutine("RecastChunks");
        }
        
    }

    IEnumerator RecastChunks()
    {
        int x = 0;
        int y = 0;
        int dx = 0;
        int dy = -1;

        for(int i = 0; i < 121; i++) { 

                int xPlayerIndex = (int)(player.transform.position.x / 16);
                int yPlayerIndex = (int)(player.transform.position.z / 16);

                if ((Mathf.Abs(xPlayerIndex - (centerX + x)) > 5 || Mathf.Abs(yPlayerIndex - (centerY + y)) > 5)) yield return null;
        
                if (!GetChunk((centerX + x), (centerY + y), null)) //Must Create a new chunk
                {
                    CreateChunk((centerX + x), (centerY + y));
                    chunkDictionary[((centerX + x) + "," + (centerY + y))].HideInvisible();
                }
                else //reinitiate pre-existing chunk.
                {
                    chunkDictionary[((centerX + x) + "," + (centerY + y))].transform.parent.gameObject.SetActive(true);
                    presentChunkList.Add(chunkDictionary[((centerX + x) + "," + (centerY + y))]);
                }

                if ( ( x == y) || ((x < 0) && (x == -y)) || ((x>0) && (x == 1 - y)))
            {
                int storage = dx;
                dx = -dy;
                dy = storage;
            }
            x += dx;
            y += dy;

                yield return null;
                //yield return new WaitForSeconds(.01f);
                
        }
        coroutineGenerating = false;
    }


    public float[,] GeneratePerlin(int xChunkIndex, int yChunkIndex)
    {
        int perlinLength = 16;
        float[,] perlinValues = new float[perlinLength, perlinLength];

        Biome biomeHere = GetAverageBiome(xChunkIndex, yChunkIndex); //multiply by adjacents
        for (int i = 0; i < perlinLength; i++)
        {
            for (int j = 0; j < perlinLength; j++)
            {
                float value = Mathf.PerlinNoise(((float)i / 16 + seed + xChunkIndex), ((float)j / 16 + seed + yChunkIndex));
                value *= (Mathf.PerlinNoise(((float)i / 16 + seed*2 + xChunkIndex), ((float)j / 16 + seed*2 + yChunkIndex)) +1);
                value *= heightMultiplier * biomeHere.heightMultiplier;
                perlinValues[i, j] = Mathf.Pow(value, biomeHere.valleyMultiplier);
                perlinValues[i, j]++;
                if (perlinValues[i, j] > 100) perlinValues[i, j] = 100;
            }
        }
        return perlinValues;

    }

    private int[,] GenerateTrees(int xChunk, int yChunk) //-1 = no tree, 0 = undetermined, 1 = tree
    {
        int[,] isThereATree = new int[16, 16];

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                if (isThereATree[i, j] > -1)
                {
                    if (Mathf.PerlinNoise((float)i / 1.6f + xChunk - seed *4, (float)j / 1.6f + yChunk - seed * 4) > 0.95f)
                    {
                        isThereATree[i, j] = 1;
                        if (i > 0)
                        {
                            isThereATree[i - 1, j] = -1;
                            if (j > 0)
                                isThereATree[i - 1, j - 1] = -1;
                            if (j < 15)
                                isThereATree[i - 1, j + 1] = -1;
                        }
                        if (i < 15)
                        {
                            isThereATree[i + 1, j] = -1;
                            if (j < 15)
                                isThereATree[i + 1, j + 1] = -1;
                            if (j > 0)
                                isThereATree[i + 1, j - 1] = -1;

                        }
                        if (j > 0)
                            isThereATree[i, j - 1] = -1;
                        if (j < 15)
                            isThereATree[i, j + 1] = -1;

                    }
                
                }
            }
        }
        return isThereATree;
    }

    private Biome GetAverageBiome(int xIndex, int yIndex)
    {
        Biome leftBiome = getBiome(xIndex - 1, yIndex);
        Biome rightBiome = getBiome(xIndex + 1, yIndex);
        Biome topBiome = getBiome(xIndex, yIndex + 1);
        Biome bottomBiome = getBiome(xIndex, yIndex - 1);

        float averageHeight = leftBiome.heightMultiplier + rightBiome.heightMultiplier + topBiome.heightMultiplier + bottomBiome.heightMultiplier;
        averageHeight /= 4;


        float averageValley = leftBiome.valleyMultiplier + rightBiome.valleyMultiplier + topBiome.valleyMultiplier + bottomBiome.valleyMultiplier;
        averageValley /= 4;

        leftBiome = new Biome(-1, "combine", averageHeight, averageValley);
        return leftBiome;
    }

    private Biome getBiome (float xChunkIndex, float yChunkIndex)
    {
        float biomeIndex = Mathf.PerlinNoise(xChunkIndex/ 13f + seed * 3, yChunkIndex / 13f + seed * 3);
        biomeIndex *= 4;

        Biome BelowBiome = biomes[(int)biomeIndex];
        Biome AboveBiome = biomes[(int)biomeIndex + 1];
        biomeIndex %= 1;


        float newHeight = Mathf.Abs(BelowBiome.heightMultiplier - AboveBiome.heightMultiplier) * biomeIndex + Mathf.Min(BelowBiome.heightMultiplier, AboveBiome.heightMultiplier);
        float newValley = Mathf.Abs(BelowBiome.valleyMultiplier - AboveBiome.valleyMultiplier) * biomeIndex + Mathf.Min(BelowBiome.valleyMultiplier, AboveBiome.valleyMultiplier);

        Biome betweenBiome = new Biome(-1, "test", newHeight, newValley);

        return betweenBiome;
    }

    internal class Biome
    {
        public Biome(int i, string n, float h, float v)
        {
            index = i;
            name = n;
            heightMultiplier = h;
            valleyMultiplier = v;
        }
        public int index;
        public string name;
        public float heightMultiplier;
        public float valleyMultiplier;
    }

    Biome[] biomes = new Biome[5]
    {
        new Biome(0,"desert", .5f, 1.2f),
        new Biome(1,"meadows", 2f, .8f),
        new Biome(2,"hills", 5f, 1.4f),
        new Biome(3, "alpines", 7f, .8f),
        new Biome(2,"hills", 5f, 1.4f),
    };

    public int currentBiome;
    public void CreateChunk(int x, int y)
    {
        GameObject emptyHolder = new GameObject();
        emptyHolder.transform.position = new Vector3(x, 0, y);
        emptyHolder.transform.parent = transform;
        GameObject CurrentChunkObject = Instantiate(ChunkPrefab, emptyHolder.transform.position, transform.rotation, emptyHolder.transform);
        ChunkGenerator CurrentChunkScript = CurrentChunkObject.GetComponent<ChunkGenerator>();
        CurrentChunkScript.xIndex = x;
        CurrentChunkScript.yIndex = y;

        CurrentChunkObject.name = x + "," + y;
        emptyHolder.name = "Holder: " + CurrentChunkObject.name;

        presentChunkList.Add(CurrentChunkScript);
        chunkDictionary.Add(CurrentChunkScript.getIndexKey(), CurrentChunkScript);

        ChunkGenerator pointerchunk = null;
        if (GetChunk(x - 1, y, pointerchunk)) // left
        {
            pointerchunk = chunkDictionary[(x-1) + "," + y];
            pointerchunk.rightChunk = CurrentChunkScript;
            CurrentChunkScript.leftChunk = pointerchunk;
        }
        if (GetChunk(x + 1, y, pointerchunk)) // right
        {
            pointerchunk = chunkDictionary[(x + 1) + "," + y];
            pointerchunk.leftChunk = CurrentChunkScript;
            CurrentChunkScript.rightChunk = pointerchunk;
        }
        if (GetChunk(x, y - 1, pointerchunk)) // back
        {
            pointerchunk = chunkDictionary[x  + "," + (y-1)];
            pointerchunk.forwardChunk = CurrentChunkScript;
            CurrentChunkScript.backwardChunk = pointerchunk;
        }
        if (GetChunk(x, y + 1, pointerchunk)) // forward
        {
            pointerchunk = chunkDictionary[x + "," + (y+1)];
            pointerchunk.backwardChunk = CurrentChunkScript;
            CurrentChunkScript.forwardChunk = pointerchunk;
        }

        CurrentChunkScript.InitiateChunk(GeneratePerlin(x,y), GenerateTrees(x,y));
        CurrentChunkScript.transform.position = new Vector3(x * 16, 0, y * 16);
    }



    public bool GetChunk(int xIndex, int yIndex, ChunkGenerator temp)
    {
        string index = xIndex + "," + yIndex;
        return chunkDictionary.TryGetValue(index, out temp);
    }

}



    

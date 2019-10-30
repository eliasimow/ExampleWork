using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class InitiateWorld : MonoBehaviour
{
    public static int toBeSeed = -1;
    public static Text stringSeed;

    public void Begin()
    {
        if (toBeSeed == -1) toBeSeed = Random.Range(10, 1000);
        TerrainGenerator.seed = toBeSeed;
        SceneManager.LoadScene("TerrainMaker", LoadSceneMode.Single);
    }

    public void GrabSeed()
    {
        toBeSeed = int.Parse(stringSeed.text);
    }

}

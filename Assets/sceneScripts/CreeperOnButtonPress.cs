using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreeperOnButtonPress : MonoBehaviour
{

    public GameObject creeper;
    public Vector3 spawnLocation;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("p")) Instantiate(creeper, spawnLocation, Quaternion.identity);
    }
}

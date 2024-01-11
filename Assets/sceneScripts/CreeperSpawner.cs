using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreeperSpawner : MonoBehaviour
{
    [SerializeField] private GameObject creeper;
    [SerializeField] private Vector3 spawnLocation = default;

    // Start is called before the first frame update
    void Start()
    {

        InvokeRepeating("SpawnAgent", 10, 15);
    }

    // Update is called once per frame
    void Update()
    {
   
    }

    public void SpawnAgent()
    {
        Instantiate(creeper, spawnLocation, Quaternion.identity);
        
    }

    
        
}

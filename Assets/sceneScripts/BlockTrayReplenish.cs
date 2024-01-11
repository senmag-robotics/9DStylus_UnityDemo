using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTrayReplenish : MonoBehaviour
{
    public float spawnFrequency = 3f;
    public int dirtBlockLimit = 3;
    public int woodBlockLimit = 3;
    public int stoneBlockLimit = 3;
    public int obsidianBlockLimit = 2;

    public GameObject dirtPrefab;
    public GameObject woodPrefab;
    public GameObject stonePrefab;
    public GameObject obsidianPrefab;

    private int dirtC, stoneC, woodC, obsC;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnBlocks", 5f, spawnFrequency);
        
    }

    // Checks number of blocks in each tray and spawns new ones if necessary
    void SpawnBlocks()
    {
        dirtC = stoneC = woodC = obsC = 0;

        Vector3 detectionCube = new Vector3(0.3679833f, 0.24171415f, 0.22940545f);
        Vector3 trayPosition = GetComponent<Transform>().position;
        Collider[] DirtBlocks = Physics.OverlapBox(trayPosition + new Vector3(-0.401f,0.3f,-0.313f), detectionCube);
        Collider[] WoodBlocks = Physics.OverlapBox(trayPosition + new Vector3(0.406f, 0.3f, -0.313f), detectionCube);
        Collider[] StoneBlocks = Physics.OverlapBox(trayPosition + new Vector3(0.406f, 0.3f, 0.286f), detectionCube);
        Collider[] ObsidianBlocks = Physics.OverlapBox(trayPosition + new Vector3(-0.401f, 0.3f, 0.286f), detectionCube);

        foreach (var block in DirtBlocks)
        {
            if (block.gameObject.GetComponent<SnapToGrid>())
            {
                dirtC++;
            }
        }

        foreach (var block in WoodBlocks)
        {
            if (block.gameObject.GetComponent<SnapToGrid>())
            {
                woodC++;
            }
        }

        foreach (var block in StoneBlocks)
        {
            if (block.gameObject.GetComponent<SnapToGrid>())
            {
                stoneC++;
            }
        }

        foreach (var block in ObsidianBlocks)
        {
            if (block.gameObject.GetComponent<SnapToGrid>())
            {
                obsC++;
            }
        }

        if (dirtC < dirtBlockLimit) Instantiate(dirtPrefab, trayPosition + new Vector3(Random.Range(-0.201f, -0.601f), 0.8f, Random.Range(-0.213f, -0.413f)), Quaternion.identity);
        if (woodC < woodBlockLimit) Instantiate(woodPrefab, trayPosition + new Vector3(Random.Range(0.206f, 0.606f), 0.8f, Random.Range(-0.213f, -0.413f)), Quaternion.identity);
        if (stoneC < stoneBlockLimit) Instantiate(stonePrefab, trayPosition + new Vector3(Random.Range(0.206f, 0.606f), 0.8f, Random.Range(0.186f, 0.386f)), Quaternion.identity);
        if (obsC < obsidianBlockLimit) Instantiate(obsidianPrefab, trayPosition + new Vector3(Random.Range(-0.201f, -0.601f), 0.8f, Random.Range(0.186f, 0.386f)), Quaternion.identity);

    }
}

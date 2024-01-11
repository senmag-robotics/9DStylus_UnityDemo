using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class beatsaber : MonoBehaviour
{
	public GameObject cube;
	int counter;
	int next = 10;
	int lastRot = 0;
	
	// Start is called before the first frame update
	void Start()
    {
		counter = 0;

	}

    // Update is called once per frame
    void Update()
    {
		counter++;
		if(counter > next)
		{
			//Random rnd = new Random();
			next = Random.Range(15, 80);
			Quaternion rotation = new Quaternion(0, 0, 0, 0);
			int rot = Random.Range(0, 4);
			if (rot == lastRot) rot++;
			lastRot = rot;
			rotation = Quaternion.Euler(rot * 90, 270, 0);
			counter = 0;
			Instantiate(cube, transform.position, rotation);
		}
		
	}
}

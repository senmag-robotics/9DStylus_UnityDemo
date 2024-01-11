using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightFlasher : MonoBehaviour
{
	// Start is called before the first frame update
	public float rate = 0.02f;
	public float baseIntensity = 0.75f;
	public float magnitude = 0.25f;

	private float counter;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		counter += rate;
		if (counter > 360) counter -= 360;
		GetComponent<Light>().intensity = baseIntensity + Mathf.Sin(counter) * magnitude;

	}
}

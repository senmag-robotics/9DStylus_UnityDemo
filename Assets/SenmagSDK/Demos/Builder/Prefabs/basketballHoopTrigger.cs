using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basketballHoopTrigger : MonoBehaviour
{
    // Start is called before the first frame update

	public bool triggered = false;
	private int triggerCounter;
    void Start()
    {
		triggered = false;
		triggerCounter = 0;

	}

    // Update is called once per frame
    void Update()
    {
		if(triggerCounter > 0)
		{
			triggerCounter -= 1;
			if(triggerCounter == 0) triggered = false;
		}   
    }

	private void OnTriggerStay(Collider other)
	{
		triggered = true;
		triggerCounter = 5;
	}
}

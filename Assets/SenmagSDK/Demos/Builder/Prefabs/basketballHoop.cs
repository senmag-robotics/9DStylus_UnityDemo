using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SenmagHaptic;

public class basketballHoop : MonoBehaviour
{
    // Start is called before the first frame update
	public GameObject trigger;

	public GameObject explosion;

	private bool triggerLock;
	private int triggerCounter;
	void Start()
    {
		triggerLock = false;

	}

    // Update is called once per frame
    void Update()
    {
		
 
		if (trigger.GetComponent<basketballHoopTrigger>().triggered == true && triggerLock == false)
		{
			triggerLock = true;
			this.GetComponentInChildren<MeshCollider>().enabled = false;
			this.GetComponent<AudioSource>().Play();
			explosion.GetComponent<ParticleSystem>().Play();
			explosion.GetComponent<AudioSource>().Play();

			var hapticExplosions = GetComponents(typeof(Senmag_ThingyGoBoom));
			foreach (Senmag_ThingyGoBoom explosion in hapticExplosions) explosion.HapticBoom();
		}
		else if(trigger.GetComponent<basketballHoopTrigger>().triggered == false)
		{
			triggerLock = false;
			this.GetComponentInChildren<MeshCollider>().enabled = true;
		}
	}

	private void OnTriggerStay(Collider other)
	{
		triggerCounter = 5;
	}
}

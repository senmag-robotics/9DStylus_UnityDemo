using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Senmag_stylusTip : MonoBehaviour
{
    // Start is called before the first frame update
	private int enableCounter;
	public bool isColliding;

	void Start()
    {
		isColliding = false;
	}

    // Update is called once per frame
    void Update()
    {
		if(enableCounter > 0)		//if we are waiting to re-enable
		{
			enableCounter -= 1;
			if(enableCounter == 0)	//if we've not been triggering for a while, return to normal collider
			{
				if (gameObject.GetComponent<SphereCollider>() != null) gameObject.GetComponent<SphereCollider>().isTrigger = false;
				if (gameObject.GetComponent<BoxCollider>() != null) gameObject.GetComponent<BoxCollider>().isTrigger = false;
				if (gameObject.GetComponent<CapsuleCollider>() != null) gameObject.GetComponent<CapsuleCollider>().isTrigger = false;
				if (gameObject.GetComponent<MeshCollider>() != null) gameObject.GetComponent<MeshCollider>().isTrigger = false;
			}
		}

	}

	private void OnTriggerStay(Collider other)
	{
		//UnityEngine.Debug.Log("tip trigger stay");
		enableCounter = 10;
	}

	public void enableCollider()
	{
		//reenable, but keep as trigger
		enableCounter = 10;
		if (gameObject.GetComponent<SphereCollider>() != null) gameObject.GetComponent<SphereCollider>().enabled = true;
		if (gameObject.GetComponent<BoxCollider>() != null) gameObject.GetComponent<BoxCollider>().enabled = true;
		if (gameObject.GetComponent<CapsuleCollider>() != null) gameObject.GetComponent<CapsuleCollider>().enabled = true;
		if (gameObject.GetComponent<MeshCollider>() != null) gameObject.GetComponent<MeshCollider>().enabled = true;
	}

	public void disableCollider()
	{

		if (gameObject.GetComponent<SphereCollider>() != null){
			gameObject.GetComponent<SphereCollider>().enabled = false;
			gameObject.GetComponent<SphereCollider>().isTrigger = true;
		}
		if (gameObject.GetComponent<BoxCollider>() != null){
			gameObject.GetComponent<BoxCollider>().enabled = true;
			gameObject.GetComponent<BoxCollider>().isTrigger = true;
		}
		if (gameObject.GetComponent<CapsuleCollider>() != null){
			gameObject.GetComponent<CapsuleCollider>().enabled = true;
			gameObject.GetComponent<CapsuleCollider>().isTrigger = true;
		}
		if (gameObject.GetComponent<MeshCollider>() != null){
			gameObject.GetComponent<MeshCollider>().enabled = true;
			gameObject.GetComponent<MeshCollider>().isTrigger = true;
		}
	}

}

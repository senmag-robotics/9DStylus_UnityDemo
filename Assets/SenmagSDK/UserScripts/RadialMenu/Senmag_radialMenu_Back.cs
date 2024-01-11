using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SenmagHaptic;

public class Senmag_radialMenu_Back : MonoBehaviour
{
	// Start is called before the first frame update
	public Vector3 collisionPos;
	public Collision activeCollision;
	public bool collisionClear;

	int collisionCounter;
    void Start()
    {
		collisionClear = true;

	}

    // Update is called once per frame
    void Update()
    {
        if(collisionCounter > 0){
			collisionCounter -= 1;
			if(collisionCounter == 0){
				collisionClear = true;
				//activeCollision = null;
				//UnityEngine.Debug.Log("RMenu, collision clear");
			}
		}
    }

	public void OnCollisionEnter(Collision collision)
	{
		OnCollisionStay(collision);
	}

	public void OnCollisionStay(Collision collision)
	{
		if(collision.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null){
			//UnityEngine.Debug.Log("RMenu Collision was cursor");
			return;
		}
		activeCollision = collision;
		collisionPos = collision.contacts[0].point;
		collisionClear = false;
		collisionCounter = 5;
		//UnityEngine.Debug.Log("RMenu collisionStay");
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SenmagHaptic;

public class customMaterial : MonoBehaviour
{
	public float stiffness = 0;
	public float damping = 0;
	public float staticFriction = 0;
	public float dynamicFriction = 0;

	private int myForceIndex;
	public bool collisionActive;
	public Vector3 collisionPos1;
	public Vector3 collisionPos2;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnDrawGizmos()
	{
		if(collisionActive) Gizmos.DrawLine(collisionPos1, collisionPos2);
	}

	private void OnTriggerEnter(Collider collider)
	{
		//UnityEngine.Debug.Log("collide");
		if (isCursor(collider))
		{
			collisionActive = true;
			myForceIndex = collider.gameObject.transform.parent.GetComponent<Senmag_HapticCursor>().requestCustomForce(gameObject);
		}
		
	}
	private void OnTriggerStay(Collider collider)
	{
		UnityEngine.Debug.Log("collide");
		if (isCursor(collider))
		{
			if(myForceIndex == -1) myForceIndex = collider.gameObject.transform.parent.GetComponent<Senmag_HapticCursor>().requestCustomForce(gameObject);
			calculateForce(collider);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		//UnityEngine.Debug.Log("collide");
		if (isCursor(collider))
		{
			collisionActive = false;
			collider.gameObject.transform.parent.GetComponent<Senmag_HapticCursor>().releaseCustomForce(myForceIndex, gameObject);
			myForceIndex = -1;
		}
	}

	private bool isCursor(Collider collider)
	{
		if (collider.gameObject.transform.parent.GetComponent<Senmag_HapticCursor>())
		{
			UnityEngine.Debug.Log("was cursor");
			return true;
		}
		UnityEngine.Debug.Log("wasnt cursor");
		return false;
	}

	private void calculateForce(Collider collider)
	{
		collisionPos1 = collider.gameObject.transform.position;
		//collisionPos2 = GetComponent<BoxCollider>().ClosestPoint(collider.gameObject.transform.position);

		//collisionPos2 = GetComponent<BoxCollider>().ClosestPointOnBounds(collider.gameObject.transform.position);

		collisionPos2 = ClosetPointInBounds(collider.gameObject.transform.position, GetComponent<BoxCollider>().bounds);


		Vector3 force = new Vector3(0, 0, 0);

		collider.gameObject.transform.parent.GetComponent<Senmag_HapticCursor>().modifyCustomForce(myForceIndex, force, gameObject);
	}


	public Vector3 ClosetPointInBounds(Vector3 point, Bounds bounds)
	{
		Plane top = new Plane(Vector3.up, bounds.max);
		Plane bottom = new Plane(Vector3.down, bounds.min);

		Plane front = new Plane(Vector3.forward, bounds.max);
		Plane back = new Plane(Vector3.back, bounds.min);

		Plane right = new Plane(Vector3.right, bounds.max);
		Plane left = new Plane(Vector3.left, bounds.min);

		Vector3 topclose = top.ClosestPointOnPlane(point);
		Vector3 botclose = bottom.ClosestPointOnPlane(point);

		Vector3 frontclose = front.ClosestPointOnPlane(point);
		Vector3 backclose = back.ClosestPointOnPlane(point);

		Vector3 rightclose = right.ClosestPointOnPlane(point);
		Vector3 leftclose = left.ClosestPointOnPlane(point);

		Vector3 closest = point;
		float bestdist = float.MaxValue;
		foreach (Vector3 p in new Vector3[] {
			topclose, botclose, frontclose, backclose, leftclose, rightclose
		})
		{
			float dist = Vector3.Distance(p, point);
			if (dist < bestdist)
			{
				bestdist = dist;
				closest = p;
			}
		}

		return closest;
	}

}

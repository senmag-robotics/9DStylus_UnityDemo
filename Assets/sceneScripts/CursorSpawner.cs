using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SenmagHaptic
{
	public class CursorSpawner : MonoBehaviour
	{
		public GameObject objectToSpawn;
		// Start is called before the first frame update
		void Start()
		{
        
		}

		private void OnCollisionEnter(Collision collision)
		{
			//Debug.Log("collided...");
			if (collision.gameObject.GetComponent<collisionTrigger>())
			{
				//Debug.Log("collided with a cursor!");
				if(objectToSpawn == null)
				{
					collision.gameObject.gameObject.GetComponentInParent<Senmag_HapticCursor>().destroyCustomCursor();
				}
				else collision.gameObject.gameObject.GetComponentInParent<Senmag_HapticCursor>().createCustomCursor(objectToSpawn);
			}
		}
		// Update is called once per frame
		void Update()
		{
        
		}
	}
}
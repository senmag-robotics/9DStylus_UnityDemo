using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SenmagHaptic
{
	public class Senmag_ThingyGoBoom : MonoBehaviour
	{
		public float boomMagnitude;
		public float boomFrequency;
		public float boomDuration;
		public float distanceDeccay;
		public float timeDeccay;
		public float maxForce = 0.1f;

		private int boomProgress = 0;
		private int myCustomForceIndex = 0;
		private Vector3 myForce = new Vector3(0,0,0);
		
		

		private Vector3 boomPos;
		private Senmag_HapticCursor activeCursor;

		// Start is called before the first frame update
		void Start()
		{
			boomProgress = 0;
			//Invoke("HapticBoom", 9);

		}

		// Update is called once per frame
		void Update()
		{
			if (boomProgress != 0)
			{
				boomProgress++;
				if (boomProgress > boomDuration)
				{
					boomProgress = 0;
					activeCursor.modifyCustomForce(myCustomForceIndex, new Vector3(0, 0, 0), transform.gameObject); //cleanup
					activeCursor.releaseCustomForce(myCustomForceIndex, transform.gameObject);
					activeCursor = null;
				}
				else
				{
					myForce = activeCursor.currentPosition - boomPos;       //vector
					float distance = myForce.magnitude;
					myForce /= distance;
					if(distanceDeccay != 0 && timeDeccay != 0) myForce *= ((Mathf.Sin(boomProgress * boomFrequency / 100.0f) * boomMagnitude/1000.0f) / (distance * distance * distanceDeccay)) / (timeDeccay * boomProgress);
					else myForce *= (Mathf.Sin(boomProgress * boomFrequency / 100.0f) * boomMagnitude);
					if (myForce.magnitude > maxForce) myForce *= (maxForce / myForce.magnitude);
					activeCursor.modifyCustomForce(myCustomForceIndex, myForce, transform.gameObject);

					//UnityEngine.Debug.Log(myForce);
				}
			}
		}

		public void HapticBoom()		//trigger explosion effect when object is touched
		{
			activeCursor = GameObject.Find("SenmagWorkspace").GetComponent<Senmag_Workspace>().osenmagServer.deviceList[0].cursor.GetComponent<Senmag_HapticCursor>();
			
			if (boomProgress != 0)
			{
				activeCursor.modifyCustomForce(myCustomForceIndex, new Vector3(0, 0, 0), transform.gameObject);	//cleanup
			}
			boomProgress = 1;
			boomPos = transform.position;
			myCustomForceIndex = activeCursor.requestCustomForce(transform.gameObject); //get index of the next available custom force effect
			//Debug.Log("ThingyGoBoom");
		}
	}
}
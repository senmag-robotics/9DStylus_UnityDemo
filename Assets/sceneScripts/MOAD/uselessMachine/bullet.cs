using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SenmagHaptic {
	public class bullet : MonoBehaviour
	{
		// Start is called before the first frame update

		public GameObject bulletSource;

		public float bulletForce = 0.1f;
		public int forceLength = 20;
		private int forceCounter;
		private int customForceIndex;

		private Senmag_HapticCursor activeCursor;
		void Start()
		{
			customForceIndex = -1;
		}

		// Update is called once per frame
		void Update()
		{

		}

		private void FixedUpdate()
		{
			if (forceCounter > 0)
			{
				if (customForceIndex == -1) {
					customForceIndex = activeCursor.requestCustomForce(gameObject);
					//UnityEngine.Debug.Log("got new custom force");
					//customForce = GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().requestCustomForce(gameObject);
				}
				else
				{
					//customForceIndex = activeCursor.requestCustomForce(gameObject);

					Vector3 force = activeCursor.currentPosition - bulletSource.transform.position;

					//Vector3 force = GameObject.Find("cursor1").transform.GetChild(0).position - bulletSource.transform.position;
					force *= bulletForce / force.magnitude;
					activeCursor.modifyCustomForce(customForceIndex, force, gameObject);
					//UnityEngine.Debug.Log(force);
				}

				forceCounter -= 1;
			}
			else if (customForceIndex != -1)
			{
				//UnityEngine.Debug.Log("releasing custom force");
				activeCursor.releaseCustomForce(customForceIndex, gameObject);
				customForceIndex = -1;
				activeCursor = null;
			}
		}

		void OnParticleCollision(GameObject other)
		{
			//UnityEngine.Debug.Log("Hit!");
		
			if (other.GetComponentInParent<Senmag_HapticCursor>() != null)
			{
				if(other.GetComponentInParent<Senmag_HapticCursor>() != activeCursor)
				{
					if (customForceIndex != -1) activeCursor.releaseCustomForce(customForceIndex, gameObject);
				}
				
				activeCursor = other.GetComponentInParent<Senmag_HapticCursor>();
				forceCounter = forceLength;
				//UnityEngine.Debug.Log("Hit!");
			}
		}
	}
}

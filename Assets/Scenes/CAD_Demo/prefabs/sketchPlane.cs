using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SenmagHaptic
{

	public class sketchPlane : MonoBehaviour
	{
		// Start is called before the first frame update

		public float gridSize = 1f;
		public float gridForceGain = 1f;
		public float gridForcePeak = 0.2f;
		public float dampingGain = 0.1f;
		public float dampingFilter = 0.5f;

		public bool active;
		public Vector3 snapPos;



		private int customForceIndex = -1;
		Senmag_HapticCursor interactingCursor;

		

		private Vector3 dampingLast;
		private Vector3 positionLast;


		//List<GameObject> 

		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public void OnTriggerEnter(Collider collision)
		{
			if (collision.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
			{
				try
				{
					if (customForceIndex != -1)
					{
						interactingCursor.releaseCustomForce(customForceIndex, transform.gameObject);
						customForceIndex = -1;
					}
					interactingCursor = collision.gameObject.GetComponentInParent<Senmag_HapticCursor>();
					customForceIndex = interactingCursor.requestCustomForce(transform.gameObject);

					positionLast = transform.InverseTransformPoint(collision.gameObject.transform.position);
					active = true;
				}
				catch
				{
					UnityEngine.Debug.Log("Senmag_HapticCursor NotFound");
				}
			}
		}

		public void OnTriggerExit(Collider collision)
		{
			if (collision.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
			{
				//UnityEngine.Debug.Log("force assist finished");
				try
				{
					interactingCursor.releaseCustomForce(customForceIndex, transform.gameObject);
					customForceIndex = -1;
					active = false;

				}
				catch
				{
					UnityEngine.Debug.Log("Senmag_HapticCursorNotFound");
				}
			}
		}

		public void OnTriggerStay(Collider collision)
		{
			if (collision.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
			{
				Vector3 gridForce = new Vector3(0, 0, 0);
				Vector3 cursorPos = transform.InverseTransformPoint(collision.gameObject.transform.position);
				cursorPos.x *= transform.localScale.x;
				cursorPos.y *= transform.localScale.y;
				cursorPos.z *= transform.localScale.z;


				gridForce.y = -cursorPos.y * gridForceGain * 2;
				if (gridForce.y < gridForcePeak / 2f && gridForce.y > -gridForcePeak / 2f) gridForce.y = 0;
				if (gridForce.y > gridForcePeak / 2f) gridForce.y -= gridForcePeak / 2f;
				if (gridForce.y < -gridForcePeak / 2f) gridForce.y += gridForcePeak / 2f;

				

				if (cursorPos.x > 0) {
					gridForce.x = -((cursorPos.x % gridSize) - gridSize / 2.0f) * gridForceGain;
					snapPos.x = cursorPos.x - (cursorPos.x % gridSize) + gridSize/2f;
				}
				else {
					gridForce.x = ((-cursorPos.x % gridSize) - gridSize / 2.0f) * gridForceGain;
					snapPos.x = cursorPos.x + (-cursorPos.x % gridSize) - gridSize / 2f;

				}

				if (cursorPos.z > 0) {
					gridForce.z = -((cursorPos.z % gridSize) - gridSize / 2.0f) * gridForceGain;
					snapPos.z = cursorPos.z - (cursorPos.z % gridSize) + gridSize / 2f;
				}
				else {
					gridForce.z = ((-cursorPos.z % gridSize) - gridSize / 2.0f) * gridForceGain;
					snapPos.z = cursorPos.z + (-cursorPos.z % gridSize) - gridSize / 2f;
				}
				snapPos.y = 0;
				snapPos = transform.parent.TransformPoint(snapPos);

				if (gridForce.x < gridForcePeak / 2f && gridForce.x > -gridForcePeak / 2f) gridForce.x = 0;
				if (gridForce.x > gridForcePeak / 2f) gridForce.x -= gridForcePeak / 2f;
				if (gridForce.x < -gridForcePeak / 2f) gridForce.x += gridForcePeak / 2f;

				if (gridForce.z < gridForcePeak / 2f && gridForce.z > -gridForcePeak / 2f) gridForce.z = 0;
				if (gridForce.z > gridForcePeak / 2f) gridForce.z -= gridForcePeak / 2f;
				if (gridForce.z < -gridForcePeak / 2f) gridForce.z += gridForcePeak / 2f;

				if (gridForce.x > gridForcePeak) gridForce.x = gridForcePeak;
				if (gridForce.x < -gridForcePeak) gridForce.x = -gridForcePeak;
				if (gridForce.y > gridForcePeak * 4) gridForce.y = gridForcePeak*4;
				if (gridForce.y < -gridForcePeak * 4) gridForce.y = -gridForcePeak * 4;
				if (gridForce.z > gridForcePeak) gridForce.z = gridForcePeak;
				if (gridForce.z < -gridForcePeak) gridForce.z = -gridForcePeak;

				//float normaliseGain = gridForce.magnitude / gridForcePeak;
				//if (normaliseGain > 1) gridForce /= normaliseGain;

				UnityEngine.Debug.Log(gridForce);

				Vector3 damping = (cursorPos - positionLast) * dampingGain;
				positionLast = cursorPos;
				damping = ((1.0f - dampingFilter) * damping) + dampingFilter * dampingLast;
				dampingLast = damping;


				gridForce = transform.gameObject.transform.rotation * gridForce - damping;
				interactingCursor.modifyCustomForce(customForceIndex, gridForce, transform.gameObject);
			}
		}

	}
}

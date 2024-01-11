using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SenmagHaptic
{

	public class Senmag_button_forcefield : MonoBehaviour
	{
		public float strength;
		private int customForceIndex;
		public float depthGain = 2;
		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

		public void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.name == "cursor")
			{
				//UnityEngine.Debug.Log("force assist triggered");
				try
				{
					customForceIndex = other.gameObject.GetComponentInParent<Senmag_HapticCursor>().requestCustomForce(transform.gameObject);
				}
				catch
				{
					UnityEngine.Debug.Log("Senmag_HapticCursor NotFound");
				}
			}
		}
		public void OnTriggerStay(Collider other)
		{
			if (other.gameObject.name == "cursor")
			{
				if (customForceIndex >= 0)
				{
					//UnityEngine.Debug.Log("force assist triggered");
					try
					{
						Vector3 force = new Vector3(0, 0, 0);
						Vector3 cursorPos = transform.InverseTransformPoint(other.gameObject.transform.position);
						float zgain = (cursorPos.z + 1) * depthGain;
						if (zgain > 1) zgain = 1;
						float distance = Mathf.Sqrt((cursorPos.x * cursorPos.x) + (cursorPos.y * cursorPos.y));
						if (distance > 0.5f) force = cursorPos * 0;
						else if (distance > 0.25f) force = -cursorPos * (0.5f - distance) * strength * zgain;
						else force = -cursorPos * (distance) * strength * zgain;
						//else force = cursorPos * 0;

						//force.z = force.y;
						//force.y = 0;
						force.z = 0;

						//UnityEngine.Debug.Log("force pre-rotation" + force * 100f);

						force = transform.gameObject.transform.rotation * force;
						//UnityEngine.Debug.Log("force post-rotation" + force * 100f);
						



						//UnityEngine.Debug.Log(transform.InverseTransformPoint(other.gameObject.transform.position));
						//UnityEngine.Debug.Log(zgain);

						other.gameObject.GetComponentInParent<Senmag_HapticCursor>().modifyCustomForce(customForceIndex, force, transform.gameObject);
					}
					catch
					{

					}
				}
			}
		}
		public void OnTriggerExit(Collider other)
		{
			if (other.gameObject.name == "cursor")
			{
				//UnityEngine.Debug.Log("force assist finished");
				try
				{
					other.gameObject.GetComponentInParent<Senmag_HapticCursor>().releaseCustomForce(customForceIndex, transform.gameObject);
				}
				catch
				{
					UnityEngine.Debug.Log("Senmag_HapticCursorNotFound");
				}
			}
		}
	}
}

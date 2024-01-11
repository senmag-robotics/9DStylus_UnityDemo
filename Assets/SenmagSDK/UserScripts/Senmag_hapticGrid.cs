using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SenmagHaptic
{

	/*public class LPFilter
	{
		public float a = 0;
		private float yprev = 0;
		public void init(float aval)
		{
			a = aval;
			yprev = 0;
		}
		public float update(float value)
		{
			yprev = (1 - a) * yprev + value * a;
			return yprev;
		}
	}*/

	public class Senmag_hapticGridSettings
	{
		public float spacing = 0.1f;
		public float maxForce = 0.001f;		//
		public float gain = 0.1f;          //
		public float forceExponment = 3;
		public Vector3 offset = new Vector3(0, 0, 0);
	};


	public class Senmag_hapticGrid : MonoBehaviour
	{
		public float testGain = 1;
		private bool enabled = false;
		private int customForceIndex = -1;

		private LPFilter[] positionFilters = new LPFilter[3] {new LPFilter(), new LPFilter(), new LPFilter() };
		Senmag_hapticGridSettings gridSettings = new Senmag_hapticGridSettings();

		// Start is called before the first frame update
		void Start()
		{
			positionFilters[0].init(0.1f);
			positionFilters[1].init(0.1f);
			positionFilters[2].init(0.1f);
		}

		// Update is called once per frame
		void Update()
		{
			
		}

		void FixedUpdate()
		{
			if (enabled)
			{
				//GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().requestCustomForce(transform.gameObject);
				Vector3 force = new Vector3(0, 0, 0);
				Vector3 cursorPos = GameObject.Find("cursor1").transform.GetChild(0).position;//  .gameObject.transform.position + gridSettings.offset;
																							  //UnityEngine.Debug.Log("pos" + cursorPos);
				cursorPos.x = positionFilters[0].update(cursorPos.x);
				cursorPos.y = positionFilters[1].update(cursorPos.y);
				cursorPos.z = positionFilters[2].update(cursorPos.z);
				force.x = ((Mathf.Abs(cursorPos.x) % gridSettings.spacing) - (gridSettings.spacing / 2)) / gridSettings.spacing;
				force.y = ((Mathf.Abs(cursorPos.y) % gridSettings.spacing) - (gridSettings.spacing / 2)) / gridSettings.spacing;
				force.z = ((Mathf.Abs(cursorPos.z) % gridSettings.spacing) - (gridSettings.spacing / 2)) / gridSettings.spacing;

				force.x = -Mathf.Pow(force.x, gridSettings.forceExponment) * Mathf.Pow(2, gridSettings.forceExponment);
				force.y = -Mathf.Pow(force.y, gridSettings.forceExponment) * Mathf.Pow(2, gridSettings.forceExponment);
				force.z = Mathf.Pow(force.z, gridSettings.forceExponment) * Mathf.Pow(2, gridSettings.forceExponment);

				if (cursorPos.x < 0) force.x *= -1;
				if (cursorPos.y < 0) force.y *= -1;
				//if (cursorPos.z > 0) force.z *= -1;

				force *= 0.1f;

				//UnityEngine.Debug.Log(force * 100);

				//force = new Vector3(0, 0, 0);
				force = force * gridSettings.gain;
				//if (force.magnitude > gridSettings.maxForce)
				//{
				//	force = force * (gridSettings.maxForce / force.magnitude);
				//}
				//force = force * testGain;
				force.x *= 2f;
				if (GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().modifyCustomForce(customForceIndex, force, transform.gameObject) == false) enableGrid(gridSettings);


			}
		}

		public void enableGrid(Senmag_hapticGridSettings settings)
		{
			GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().releaseCustomForce(customForceIndex, transform.gameObject);
			customForceIndex = GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().requestCustomForce(transform.gameObject);
			if(customForceIndex >= 0)		//if we successfulyl got a custom force vector to use
			{
				gridSettings = settings;
				if (gridSettings.gain == 0) disableGrid();
				else enabled = true;
			}
		}

		public void disableGrid()
		{
			if (customForceIndex >= 0)      //if we successfulyl got a custom force vector to use
			{
				GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().releaseCustomForce(customForceIndex, transform.gameObject);
			}
			enabled = false;
		}
	}
}

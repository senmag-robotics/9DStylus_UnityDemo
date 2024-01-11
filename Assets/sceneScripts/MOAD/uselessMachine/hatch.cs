using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uselessBox
{
	public class hatch : MonoBehaviour
	{
		public float hatchTolerance;
		public float hatchSmoothing;
		public float hatchTargetAngle = 0;
		public bool targetAchieved;
		// Start is called before the first frame update
		void Start()
		{
			hatchTargetAngle = 0;
		}

		// Update is called once per frame
		void Update()
		{
			Quaternion target = Quaternion.Euler(0, hatchTargetAngle - 180, 180);
			transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * hatchSmoothing);
			if (Mathf.Abs(transform.localRotation.eulerAngles.y - 180) > (Mathf.Abs(hatchTargetAngle) - hatchTolerance) && (Mathf.Abs(transform.localRotation.eulerAngles.y - 180) < (Mathf.Abs(hatchTargetAngle) + hatchTolerance))) targetAchieved = true;
			else targetAchieved = false;
		}

		public void setHatchTarget(float target)
		{
			hatchTargetAngle = target;
			targetAchieved = false;
		}
	}
}
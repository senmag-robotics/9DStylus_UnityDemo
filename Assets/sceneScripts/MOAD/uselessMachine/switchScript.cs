using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uselessBox
{
	public class switchScript : MonoBehaviour
	{
		public bool switchState = false;
		public float switchAngle = 8f;
		public float hysterisis = 1f;

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			if (switchState == false)
			{
				if (GetComponent<HingeJoint>().angle > hysterisis)
				{
					JointSpring tmp = GetComponent<HingeJoint>().spring;
					tmp.targetPosition = switchAngle;
					GetComponent<HingeJoint>().spring = tmp;
					switchState = true;
				}
			}
			if (switchState == true)
			{
				if (GetComponent<HingeJoint>().angle < -hysterisis)
				{
					JointSpring tmp = GetComponent<HingeJoint>().spring;
					tmp.targetPosition = -switchAngle;
					GetComponent<HingeJoint>().spring = tmp;
					switchState = false;
				}
			}
		}
	}
}
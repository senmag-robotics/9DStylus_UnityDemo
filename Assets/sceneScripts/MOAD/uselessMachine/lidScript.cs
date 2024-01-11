using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uselessBox
{
	public class lidScript : MonoBehaviour
	{
		public enum LidState
		{
			closed,
			opening,
			open,
			deployingLever,
			retractingLever,
			closing,
		}

		public bool isOpen;
		public float lidOpenAngle;
		public GameObject lever;

		public float lidSmoothing = 5f;
		private float targetPosition;

		public Vector2 leverDeployedAngles;
		public Vector2 leverRetractedAngles;

		public LidState lidState;
		private float jointTolerance = 3f;
		// Start is called before the first frame update
		void Start()
		{
			lidState = LidState.closed;

			JointSpring tmp = lever.GetComponent<HingeJoint>().spring;
			tmp.targetPosition = leverRetractedAngles[0];
			lever.GetComponent<HingeJoint>().spring = tmp;
			tmp.targetPosition = leverRetractedAngles[1];
			lever.transform.GetChild(0).GetComponent<HingeJoint>().spring = tmp;
		}

		// Update is called once per frame
		void Update()
		{
			Quaternion target = Quaternion.Euler(targetPosition - 180, 0, 0);
			transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * lidSmoothing);
			if (transform.localRotation.eulerAngles.x > lidOpenAngle * 0.9) isOpen = true;
			else isOpen = false;

			if (lidState == LidState.opening)
			{
				if (isOpen == true)
				{
					lidState = LidState.deployingLever;

				}
			}
			if (lidState == LidState.deployingLever)
			{
				JointSpring tmp = lever.GetComponent<HingeJoint>().spring;
				tmp.targetPosition = leverDeployedAngles[0];
				lever.GetComponent<HingeJoint>().spring = tmp;
				tmp.targetPosition = leverDeployedAngles[1];
				lever.transform.GetChild(0).GetComponent<HingeJoint>().spring = tmp;

				if (lever.GetComponent<HingeJoint>().angle < (leverDeployedAngles[0] + jointTolerance) && lever.GetComponent<HingeJoint>().angle > (leverDeployedAngles[0] - jointTolerance))
				{
					if (lever.transform.GetChild(0).GetComponent<HingeJoint>().angle < (leverDeployedAngles[1] + jointTolerance) && lever.transform.GetChild(0).GetComponent<HingeJoint>().angle > (leverDeployedAngles[1] - jointTolerance))
					{
						lidState = LidState.retractingLever;
					}
				}
				//UnityEngine.Debug.Log(lever.GetComponent<HingeJoint>().angle + " " + lever.transform.GetChild(0).GetComponent<HingeJoint>().angle);
			}
			if (lidState == LidState.retractingLever)
			{
				JointSpring tmp = lever.GetComponent<HingeJoint>().spring;
				tmp.targetPosition = leverRetractedAngles[0];
				lever.GetComponent<HingeJoint>().spring = tmp;
				tmp.targetPosition = leverRetractedAngles[1];
				lever.transform.GetChild(0).GetComponent<HingeJoint>().spring = tmp;

				if (lever.GetComponent<HingeJoint>().angle < (leverRetractedAngles[0] + jointTolerance) && lever.GetComponent<HingeJoint>().angle > (leverRetractedAngles[0] - jointTolerance))
				{
					if (lever.transform.GetChild(0).GetComponent<HingeJoint>().angle < (leverRetractedAngles[1] + jointTolerance) && lever.transform.GetChild(0).GetComponent<HingeJoint>().angle > (leverRetractedAngles[1] - jointTolerance))
					{
						lidState = LidState.closing;
					}
				}
			}
			if (lidState == LidState.closing)
			{
				closeLid();
				if (isOpen == false)
				{
					lidState = LidState.closed;
				}
			}
		}


		public void openLid()
		{
			targetPosition = -lidOpenAngle;
		}
		public void closeLid()
		{
			targetPosition = 0;
		}

		public void pressSwitch()
		{
			openLid();
			if (isOpen == false)
			{
				lidState = LidState.opening;
			}
			else
			{
				lidState = LidState.deployingLever;
			}
		}
	}
}
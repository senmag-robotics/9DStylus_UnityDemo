using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class leg : MonoBehaviour
{

	
	public enum LegState
	{
		retracted,
		raisingHatch,
		extendingLeg,
		deployed,
		raisingLeg,
		retractingLeg,
		destroyed,
	}

	public enum LegDeployState
	{
		raiseHatch,
		extend,
		rotate,
	}
	public bool tooLow;
	public bool reachedTarget;
	public Vector3 angleTargets;

	public GameObject hatch;
	public float hatchRaisedPos; 
	public LegState legState;
	public Vector3 retractedAngles;
	public float retractedDistance = -0;

	private Vector3 deployedPosition;

	private GameObject leg_base;
	private GameObject leg_mid;
	private GameObject leg_tip;

	private float legExtendSmooth = 5f;
	private float hatchTolerance = 2f;
	private float legAngleTolerance = 2f;
	private float legExtendTolerance = 5f;

	private float hatchTargetAngle = 0;

	private Vector3 retractedPosition;

	// Start is called before the first frame update
	void Start()
    {
		leg_base = gameObject;
		leg_mid = transform.GetChild(0).gameObject;
		leg_tip = leg_mid.transform.GetChild(0).gameObject;

		setAngles(retractedAngles);

		leg_base.GetComponent<Rigidbody>().isKinematic = true;
		//leg_mid.GetComponent<Rigidbody>().isKinematic = true;
		//leg_tip.GetComponent<Rigidbody>().isKinematic = true;

		deployedPosition = leg_base.transform.localPosition;
		legState = LegState.retracted;

		retractedPosition = deployedPosition;
		retractedPosition.x -= retractedDistance;
		leg_base.transform.localPosition = retractedPosition;


	}

    // Update is called once per frame
    void Update()
    {
		tooLow = transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<legTip>().tipCollided;


		if (legState == LegState.raisingHatch)
		{
			if((hatch.GetComponent<HingeJoint>().angle > (hatchRaisedPos - hatchTolerance)) && (hatch.GetComponent<HingeJoint>().angle < (hatchRaisedPos + hatchTolerance)))
			{
				legState = LegState.extendingLeg;
			}
		}
		else if(legState == LegState.extendingLeg)
		{
			leg_base.transform.localPosition = Vector3.Slerp(leg_base.transform.localPosition, deployedPosition, Time.deltaTime * legExtendSmooth);
			if((leg_base.transform.localPosition - deployedPosition).magnitude < legExtendTolerance)
			{
				legState = LegState.deployed;
				leg_base.GetComponent<Rigidbody>().isKinematic = false;
				setAngles(new Vector3(0, 0, 0));
			}
		}

		else if(legState == LegState.raisingLeg)
		{
			setAngles(retractedAngles);
			if((leg_base.GetComponent<HingeJoint>().angle > (retractedAngles[0] - legAngleTolerance)) && (leg_base.GetComponent<HingeJoint>().angle < (retractedAngles[0] + legAngleTolerance))
				&& (leg_mid.GetComponent<HingeJoint>().angle > (retractedAngles[1] - legAngleTolerance)) && (leg_mid.GetComponent<HingeJoint>().angle < (retractedAngles[1] + legAngleTolerance))
				&& (leg_tip.GetComponent<HingeJoint>().angle > (retractedAngles[2] - legAngleTolerance)) && (leg_tip.GetComponent<HingeJoint>().angle < (retractedAngles[2] + legAngleTolerance)))
			{
				legState = LegState.retractingLeg;
				leg_base.GetComponent<Rigidbody>().isKinematic = true;
			}
		}
		else if(legState == LegState.retractingLeg)
		{
			leg_base.transform.localPosition = Vector3.Slerp(leg_base.transform.localPosition, retractedPosition, Time.deltaTime * legExtendSmooth);
			if ((leg_base.transform.localPosition - retractedPosition).magnitude < legExtendTolerance)
			{
				legState = LegState.retracted;
				JointSpring tmp = hatch.GetComponent<HingeJoint>().spring;
				tmp.targetPosition = 0;
				hatch.GetComponent<HingeJoint>().spring = tmp;
			}
		}
    }

	public void deployLeg()
	{
		if(legState == LegState.retracted)
		{
			UnityEngine.Debug.Log("deploying leg...");
			legState = LegState.raisingHatch;
			JointSpring tmp = hatch.GetComponent<HingeJoint>().spring;
			tmp.targetPosition = hatchRaisedPos;
			hatch.GetComponent<HingeJoint>().spring = tmp;
			hatchTargetAngle = hatchRaisedPos;
		}
	}

	public void retractLeg()
	{
		//if(legState == LegState.deployed)
		//{
			legState = LegState.raisingLeg;
		//}
	}

	void setAngles(Vector3 angles)
	{
		JointSpring tmp = leg_base.GetComponent<HingeJoint>().spring;
		tmp.targetPosition = angles[0];
		leg_base.GetComponent<HingeJoint>().spring = tmp;
		tmp.targetPosition = angles[1];
		leg_mid.GetComponent<HingeJoint>().spring = tmp;
		tmp.targetPosition = angles[2];
		leg_tip.GetComponent<HingeJoint>().spring = tmp;
	}

	public void updateStep(float leftRight, float forwardBack, float stepPhase)
	{
		while (stepPhase > 360) stepPhase -= 360;
		while (stepPhase < 0) stepPhase += 360;
		if (legState == LegState.deployed)
		{
			Vector3 lastAngles;
			Vector3 nextAngles;
			//raise, reset, lower, step
			if (stepPhase < 40) //raise
			{
				lastAngles = new Vector3(-15 * forwardBack, -25, -15 * leftRight + 10);
				nextAngles = new Vector3(-15 * forwardBack, 0, -15 * leftRight + 10);
				setAngles(Vector3.Lerp(lastAngles, nextAngles, stepPhase / 40f));
			}
			else if (stepPhase < 80)   //reset
			{
				lastAngles = new Vector3(-15 * forwardBack, 0, -15 * leftRight + 10);
				nextAngles = new Vector3(15 * forwardBack, 0, 15 * leftRight + 10);
				setAngles(Vector3.Lerp(lastAngles, nextAngles, (stepPhase - 40) / 40f));
			}
			else if (stepPhase < 120)   //lower
			{
				lastAngles = new Vector3(15 * forwardBack, 0, 15 * leftRight + 10);
				nextAngles = new Vector3(15 * forwardBack, -25, 15 * leftRight + 10);
				setAngles(Vector3.Lerp(lastAngles, nextAngles, (stepPhase - 80) / 40f));
			}
			else if (stepPhase < 240)   //step start to mid
			{
				lastAngles = new Vector3(15 * forwardBack, -25, 15 * leftRight + 10);
				nextAngles = new Vector3(0, -25 + Mathf.Abs(leftRight) * 5, 0);
				setAngles(Vector3.Lerp(lastAngles, nextAngles, (stepPhase - 120) / 120));
			}
			else if (stepPhase < 360)   //step mit to end
			{
				lastAngles = new Vector3(0, -25 + Mathf.Abs(leftRight) * 5, 0);
				nextAngles = new Vector3(-15 * forwardBack, -25, -15 * leftRight + 10);
				setAngles(Vector3.Lerp(lastAngles, nextAngles, (stepPhase - 240) / 120));
			}
		}
	}
}

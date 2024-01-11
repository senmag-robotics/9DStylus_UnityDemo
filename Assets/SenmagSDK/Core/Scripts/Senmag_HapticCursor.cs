using SenmagHaptic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using SenmagTypes;
using System;
using System.Collections.Specialized;
using System.Diagnostics;

namespace SenmagHaptic
{

	public class CustomForce
	{
		public bool allocated = false;
		public Vector3 force = new Vector3(0,0,0);
		public int updateCounter = 0;
		public GameObject owner;
	};

	public class collisionTrigger : MonoBehaviour		//can trigger cursor updates ahead of the main physics loop
	{
		private void OnCollisionEnter(Collision collision)
		{
			gameObject.GetComponentInParent<Senmag_HapticCursor>().triggerForceUpdate();
			//Debug.Log("collision!");
		}
		private void OnCollisionStay(Collision collision)
		{
			gameObject.GetComponentInParent<Senmag_HapticCursor>().triggerForceUpdate();
		}
		private void OnCollisionExit(Collision collision)
		{
			gameObject.GetComponentInParent<Senmag_HapticCursor>().triggerForceUpdate();
			//	Debug.Log("done colliding!");
		}
	}

	public class Senmag_HapticCursor : MonoBehaviour
	{
		public bool allowCursorOrientation = false;

		public int					collisionStatus;
		bool						droppingCursor;

		public Vector3				accelerationCompensation = new Vector3(2.5f, 1.5f, 2f);
		private Vector3				posLast;
		private Vector3				speedLast;
		private List<LPFilter>		accelerationFilter = new List<LPFilter>();

		GameObject					cursor;
		GameObject					cursorTarget;
		GameObject					cursorBaseModel;
		ConfigurableJoint			spring;

		ConfigurableJoint			objectSpring;
		private bool				objectPickedUp;

		public Senmag_stylusControl	stylusControl;

		GameObject					temporaryCursor;
		GameObject					heldCursor;
		public GameObject			rightClickMenu;
		bool						cursorHold;
		ConfigurableJoint			customCursorJoint;

		private Vector3				localPositionOffset = new Vector3(0, 0, 0);
		private Quaternion			localRotationOffset = new Quaternion(0, 0, 0, 1);
		private Vector3				lastForce;
		public float				maxForceVelocity = .4f;

		private Vector3				positionLast = new Vector3(0, 0, 0);
		private float				maxPositionVelocity = 1f;

		int							safeStart;
		//const int					safeStartDuration = 100;        //whenever the safe start is activated, the cursor will require this many '0' force samples before haptic rendering is enabled
		const int					safeStartDuration = 2;      //whenever the safe start is activated, the cursor will require this many '0' force samples before haptic rendering is enabled
		const int					maxCustomForces = 10;
		private List<CustomForce>	customForces = new List<CustomForce>(0);

		private List<LPFilter>		positionFilter = new List<LPFilter>();

		public float				safeStartThreshold = 0.01f;
		public float				cursorTeleportThreshold = 1f;

		int currentCustomForce = 0;
		float positionFilterDefault = 0.5f;

		float accelerationFIlterDefault = 0.25f;

		public Vector3 currentPosition = new Vector3(0, 0, 0);

		GameObject			rotationDummy;

		private bool rightClickLatch;


		// Start is called before the first frame update
		void Start()
		{
			rightClickLatch = false;
			setSafeStart();
			while (positionFilter.Count < 3){
				positionFilter.Add(new LPFilter());
				positionFilter[positionFilter.Count-1].init(positionFilterDefault);

				

			}

			while (accelerationFilter.Count < 3)
			{
				accelerationFilter.Add(new LPFilter());
				accelerationFilter[accelerationFilter.Count - 1].init(accelerationFIlterDefault);
			}

			UnityEngine.Debug.Log("acceleration Filter has " + accelerationFilter.Count + " items");
			
			//heldCursor = new GameObject();
			cursorHold = false;
			collisionStatus = 0;
			customForces.Clear();
			for (int x = 0; x < maxCustomForces; x++) customForces.Add(new CustomForce());		//generate list of custom forces
		}

		public void setPositionFilterStrength(float strength)
		{
			while(positionFilter.Count < 3) positionFilter.Add(new LPFilter());
			
			positionFilter[0].init(strength);
			positionFilter[1].init(strength);
			positionFilter[2].init(strength);
		}

		public void setSafeStart()
		{
			safeStart = safeStartDuration;
		}

		public int requestCustomForce(GameObject newOwner)
		{
			for(int x = 0; x < maxCustomForces; x++)
			{
				if(customForces[x].allocated == false)
				{
					customForces[x].allocated = true;
					customForces[x].force = new Vector3(0, 0, 0);
					customForces[x].updateCounter = 0;
					customForces[x].owner = newOwner;
					return x;
				}
			}
			UnityEngine.Debug.Log("All custom forces are assigned - are you releasing them when you finish?");
			return -1;
		}
		public bool releaseCustomForce(int index, GameObject owner)
		{
			if (index < 0) return false;    //index safety
			if (index >= maxCustomForces) return false;
			if (customForces[index].owner != owner) return false;

			customForces[index].allocated = false;
			customForces[index].force = new Vector3(0, 0, 0);
			return true;
		}

		public bool modifyCustomForce(int index, Vector3 force, GameObject owner)
		{
			if (index < 0) return false;	//index safety
			if (index >= maxCustomForces) return false;

			if (customForces[index].allocated == true)
			{
				if (customForces[index].owner == owner)
				{
					customForces[index].force = force;
					customForces[index].updateCounter = 0;
					return true;
				}
				else
				{
					UnityEngine.Debug.Log("You do not own this force index - it may have expired and been re-allocated.");
					return false;
				}
			}
			else
			{
				UnityEngine.Debug.Log("You do not own this force index - it may have expired and been re-allocated.");
				return false;
			}
		}

		void OnDrawGizmos()
		{
			
		}

		// Update is called once per frame
		void Update()
		{

            if (Input.GetKeyDown(KeyCode.F5))
            {
				resetQuaternion();

            }
			



			if (rightClickMenu != null)
			{
				if (rightClickMenu.GetComponentInChildren<Senmag_radialMenu>().wasClicked && rightClickMenu.GetComponentInChildren<Senmag_radialMenu>().currentScale == 0 && rightClickMenu.GetComponentInChildren<Senmag_radialMenu>().instantiatedObject.gameObject == null)
				{
					Destroy(rightClickMenu);
				}
			}

			if (stylusControl != null)
			{
				if (stylusControl.isColliding == false)
				{
					if (stylusControl.Input_isHeld(Stylus_Action.rightClick))
					{
						if(rightClickLatch == false){
							rightClickLatch = true;
							if (rightClickMenu != null)
							{
								Destroy(rightClickMenu);
								UnityEngine.Debug.Log("destroying old menu");
							}
							else if (stylusControl.isColliding == false)
							{
								rightClickMenu = Instantiate(gameObject.GetComponentInParent<Senmag_Workspace>().defaultRightClickMenu);
								rightClickMenu.transform.position = currentPosition;
							}
						}
					}
					else rightClickLatch = false;
				}
				//	else rightClickLatch = false;
			

				/*if (rightClickMenu != null)
				{
					if (stylusControl.Input_wasClicked(Stylus_Action.rightClick)){
						Destroy(rightClickMenu);
						UnityEngine.Debug.Log("destroying old menu");
					}
				}

				else if (stylusControl.isColliding == false)
				{
					if (stylusControl.Input_wasClicked(Stylus_Action.rightClick))
					{
						if (rightClickMenu != null)
						{
							Destroy(rightClickMenu);
							UnityEngine.Debug.Log("destroying old menu");
						}
						else if (stylusControl.isColliding == false)
						{
							rightClickMenu = Instantiate(gameObject.GetComponentInParent<Senmag_Workspace>().defaultRightClickMenu);
							rightClickMenu.transform.position = currentPosition;
						}
					}

				}*/




				
			}

			if (droppingCursor == true)
			{
				GameObject droppedCursor = Instantiate(temporaryCursor);
				if (droppedCursor.GetComponent<SnapToGrid>()) droppedCursor.GetComponent<SnapToGrid>().Snap();

				destroyCustomCursor();
				droppingCursor = false;
			}
		}

		void FixedUpdate()
		{
			
		}

		public void generateCursor(GameObject parent, GameObject cursorModel, string cursorName, DK1DeviceState deviceState, float cursorsize, float cursorFrictionDynamic, float cursorFrictionStatic)
		{
			//Start();
			setSafeStart();

			localPositionOffset = Vector3.zero;
			localRotationOffset = Quaternion.identity;

			//GameObject cursor;
			//GameObject cursorTarget;
			//GameObject cursorBaseModel;
			//ConfigurableJoint spring;

			
			cursor = this.gameObject;
			cursor.transform.parent = parent.transform;
			cursor.name = "SenmagCursor_" + cursorName;
			UnityEngine.Debug.Log("Creating new cursor with name: " + cursor.name);
			cursor.transform.localPosition = new Vector3(0,0,0);

			cursorTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			cursorTarget.name = "Device Location";
			cursorTarget.transform.parent = cursor.transform;
			cursorTarget.GetComponent<SphereCollider>().enabled = false;
			cursorTarget.GetComponent<MeshRenderer>().enabled = false;
			cursorTarget.transform.localScale = new Vector3(cursorsize*0.1f, cursorsize * 0.1f, cursorsize * 0.1f);
			cursorTarget.transform.localPosition = new Vector3(0, 0, 0);
			cursorTarget.AddComponent<Rigidbody>();
			cursorTarget.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
			//cursorTarget.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;// | RigidbodyConstraints.FreezeRotation;

			cursorBaseModel = Instantiate(cursorModel);
			cursorBaseModel.name = "Default cursor";
			cursorBaseModel.transform.parent = cursor.transform;
			cursorBaseModel.transform.localScale = new Vector3(cursorsize, cursorsize, cursorsize);
			cursorBaseModel.transform.localPosition = new Vector3(0, 0, 0);
			if (cursorBaseModel.GetComponent<Rigidbody>() == null) cursorBaseModel.AddComponent<Rigidbody>();
			//cursorBaseModel.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
			cursorBaseModel.GetComponent<Rigidbody>().mass = 0.001f;
			cursorBaseModel.GetComponent<Rigidbody>().drag = 0.0f;
			cursorBaseModel.GetComponent<Rigidbody>().useGravity = false;
			cursorBaseModel.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;

			cursorBaseModel.GetComponent<Senmag_stylusControl>().hideStylusBody();

			foreach (Transform child in cursor.transform)
			{
				GameObject current = child.gameObject;
				if (current.GetComponent<MeshCollider>() != null)
				{
					current.GetComponent<MeshCollider>().material.dynamicFriction = cursorFrictionDynamic;
					current.GetComponent<MeshCollider>().material.staticFriction = cursorFrictionStatic;
					current.GetComponent<MeshCollider>().material.frictionCombine = PhysicMaterialCombine.Average;
				}
				if (current.GetComponent<MeshCollider>() != null)
				{
					current.GetComponent<MeshCollider>().material.dynamicFriction = cursorFrictionDynamic;
					current.GetComponent<MeshCollider>().material.staticFriction = cursorFrictionStatic;
					current.GetComponent<MeshCollider>().material.frictionCombine = PhysicMaterialCombine.Average;
				}
				if (current.GetComponent<BoxCollider>() != null)
				{
					current.GetComponent<BoxCollider>().material.dynamicFriction = cursorFrictionDynamic;
					current.GetComponent<BoxCollider>().material.staticFriction = cursorFrictionStatic;
					current.GetComponent<BoxCollider>().material.frictionCombine = PhysicMaterialCombine.Average;
				}
				if (current.GetComponent<CapsuleCollider>() != null)
				{
					current.GetComponent<CapsuleCollider>().material.dynamicFriction = cursorFrictionDynamic;
					current.GetComponent<CapsuleCollider>().material.staticFriction = cursorFrictionStatic;
					current.GetComponent<CapsuleCollider>().material.frictionCombine = PhysicMaterialCombine.Average;
				}
			}

			//cursorBaseModel.GetComponent<SphereCollider>().material.dynamicFriction = cursorFrictionDynamic;
			//cursorBaseModel.GetComponent<SphereCollider>().material.staticFriction = cursorFrictionStatic;
			//cursorBaseModel.GetComponent<SphereCollider>().material.frictionCombine = PhysicMaterialCombine.Average;

			spring = cursorTarget.AddComponent<ConfigurableJoint>();
			spring.connectedBody = cursorBaseModel.GetComponent<Rigidbody>();
			spring.GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Locked; //.set(locked);
			spring.GetComponent<ConfigurableJoint>().yMotion = ConfigurableJointMotion.Locked; //.set(locked);
			spring.GetComponent<ConfigurableJoint>().zMotion = ConfigurableJointMotion.Locked; //.set(locked);

			spring.GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Locked; //.set(locked);
			spring.GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Locked; //.set(locked);
			spring.GetComponent<ConfigurableJoint>().angularZMotion = ConfigurableJointMotion.Locked; //.set(locked);

			spring.autoConfigureConnectedAnchor = false;
			spring.anchor = new Vector3(0, 0, 0);
			spring.connectedAnchor = new Vector3(0, 0, 0);



			stylusControl = null;
			foreach (Transform child in transform)			//search for a stylusControl object in the cursor model
			{
				if(child.GetComponent<Senmag_stylusControl>() != null)
				{
					stylusControl = child.GetComponent<Senmag_stylusControl>();
				}
			}


			/*rotationDummy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			rotationDummy.GetComponent<SphereCollider>().enabled = false;
			rotationDummy.transform.parent = cursor.transform;
			rotationDummy.transform.localScale = new Vector3(.1f, .1f, .1f);*/

		}



		float FILTER_COEFFICIENT = 0.98f;
		float RAD_TO_DEG = 57.2957795131f;
        float complementaryFilter(float newAngle, float newRate, float dt, float previousAngle)
        {
            float complementaryAngle;
            complementaryAngle = FILTER_COEFFICIENT * (previousAngle + newRate * dt) + (1 - FILTER_COEFFICIENT) * newAngle;
            return complementaryAngle;
        }




		float SAMPLE_FREQ = 100.0f;    // Sample frequency in Hz
		float BETA = 0.75f;             // Filter gain beta


        float q0 = 1.0f, q1 = 0.0f, q2 = 0.0f, q3 = 0.0f;

		void resetQuaternion()
		{
			q0 = 1.0f;
			q1 = 0.0f;
			q2 = 0.0f;
			q3 = 0.0f;
        }

        void normalizeQuaternion()
        {
            float norm = (float)Math.Sqrt(q0 * q0 + q1 * q1 + q2 * q2 + q3 * q3);
            q0 /= norm;
            q1 /= norm;
            q2 /= norm;
            q3 /= norm;
        }

        void calculateOrientation(float accelX, float accelY, float accelZ, float gyroX, float gyroY, float gyroZ, float dt)
        {
            // Convert acceleration values to m/s^2
            float ax = accelX;
            float ay = accelY;
            float az = accelZ;

            // Convert gyroscope values to rad/s
            float gx = gyroX;
            float gy = gyroY;
            float gz = gyroZ;

            // Normalize accelerometer measurements
            float accelNorm = (float)Math.Sqrt(ax * ax + ay * ay + az * az);
            ax /= accelNorm;
            ay /= accelNorm;
            az /= accelNorm;

			// Estimated direction of gravity
			float[] v1 = { 0.0f, 0.0f, 0.0f };
            float[] v2 = { 0.0f, 0.0f, 0.0f };

            v1[0] = 2.0f * (q1 * q3 - q0 * q2);
            v1[1] = 2.0f * (q0 * q1 + q2 * q3);
            v1[2] = q0 * q0 - q1 * q1 - q2 * q2 + q3 * q3;

            v2[0] = 2.0f * (q1 * q2 + q0 * q3);
            v2[1] = q0 * q0 - q1 * q1 + q2 * q2 - q3 * q3;
            v2[2] = 2.0f * (q2 * q3 - q0 * q1);

            // Error is cross product between estimated and measured direction of gravity
            
            float[] error = { 0.0f, 0.0f, 0.0f };
            error[0] = accelY * v2[2] - accelZ * v2[1];
            error[1] = accelZ * v2[0] - accelX * v2[2];
            error[2] = accelX * v2[1] - accelY * v2[0];

            // Compute gyroscope measurement drift
            float[] gyroDrift = { 0.0f, 0.0f, 0.0f };
            gyroDrift[0] = 2.0f * BETA * error[0];
            gyroDrift[1] = 2.0f * BETA * error[1];
            gyroDrift[2] = 2.0f * BETA * error[2];

            // Update gyro measurements
            gx += gyroDrift[0];
            gy += gyroDrift[1];
            gz += gyroDrift[2];

            // Integrate gyroscope data
            float qDot0 = 0.5f * (-q1 * gx - q2 * gy - q3 * gz);
            float qDot1 = 0.5f * (q0 * gx + q2 * gz - q3 * gy);
            float qDot2 = 0.5f * (q0 * gy - q1 * gz + q3 * gx);
            float qDot3 = 0.5f * (q0 * gz + q1 * gy - q2 * gx);

            // Integrate rate of change of quaternion
            q0 += qDot0 * dt;
            q1 += qDot1 * dt;
            q2 += qDot2 * dt;
            q3 += qDot3 * dt;

            // Normalize quaternion
            normalizeQuaternion();

            // Calculate roll, pitch, and yaw angles from quaternion
            float roll = (float)Math.Atan2(2.0f * (q0 * q1 + q2 * q3), 1.0f - 2.0f * (q1 * q1 + q2 * q2));
            float pitch = (float)Math.Asin(2.0f * (q0 * q2 - q3 * q1));
            float yaw = (float)Math.Atan2(2.0f * (q0 * q3 + q1 * q2), 1.0f - 2.0f * (q2 * q2 + q3 * q3));

            // Convert angles to degrees
            roll *= 180.0f / (float)Math.PI;
            pitch *= 180.0f / (float)Math.PI;
            yaw *= 180.0f / (float)Math.PI;

            // Print the final roll, pitch, and yaw angles
            //printf("Roll: %.2f degrees\n", roll);
            //printf("Pitch: %.2f degrees\n", pitch);
            //printf("Yaw: %.2f degrees\n", yaw);

            //cursorTarget.transform.localRotation = Quaternion.Euler(roll, pitch, yaw);
            //cursorTarget.transform.localRotation = Quaternion.Euler(roll, yaw, pitch);
            //cursorTarget.transform.localRotation = Quaternion.Euler(pitch, roll, yaw);
            //cursorTarget.transform.localRotation = Quaternion.Euler(pitch, yaw, roll);
           // cursorTarget.transform.localRotation = Quaternion.Euler(yaw, -pitch, roll);
			/*Quaternion rotation = new Quaternion();
			rotation.w = q0;
            rotation.x = q1;
            rotation.y = q2;
            rotation.z = q3;*/


            //cursorTarget.transform.localRotation = rotation;
            //cursorTarget.transform.localRotation = Quaternion.Euler(yaw, roll, pitch);
            //cursorTarget.transform.localRotation = Quaternion.Euler(yaw, roll, pitch);


            cursorTarget.transform.localRotation = Quaternion.Euler(yaw, -pitch, roll);
        }

        void updateQuaternion9A(float ax, float ay, float az, float gx, float gy, float gz, float mx, float my, float mz)
        {
			float sampleFreq = 100f;
			float betaDef = 01f;
            float recipNorm;
            float s0, s1, s2, s3;
            float qDot1, qDot2, qDot3, qDot4;
            float hx, hy, bx, bz;
            float halfvx, halfvy, halfvz, halfwx, halfwy, halfwz;

            // Normalize accelerometer and magnetometer measurements
            recipNorm = 1.0f / (float)Math.Sqrt(ax * ax + ay * ay + az * az);
            ax *= recipNorm;
            ay *= recipNorm;
            az *= recipNorm;
            recipNorm = 1.0f / (float)Math.Sqrt(mx * mx + my * my + mz * mz);
            mx *= recipNorm;
            my *= recipNorm;
            mz *= recipNorm;

            hx = mx * q0 - mz * q2;
            hy = mx * q1 + my * q3;
            bx = (float)Math.Sqrt(hx * hx + hy * hy);
            bz = mz * q0 + mx * q2;

            // Estimated direction of gravity and magnetic field
            s0 = q2 * bx - q3 * bz;
            s1 = q2 * bz + q3 * bx;
            s2 = q0 * bz + q1 * bx;
            s3 = q0 * bx - q1 * bz;

            // Compute quaternion rate of change
            qDot1 = 0.5f * (-q1 * s0 - q2 * s1 - q3 * s2);
            qDot2 = 0.5f * (q0 * s0 + q2 * s2 - q3 * s3);
            qDot3 = 0.5f * (q0 * s1 - q1 * s2 + q3 * s0);
            qDot4 = 0.5f * (q0 * s2 + q1 * s1 - q2 * s0);

            // Integrate to update quaternion
            q0 += qDot1 * (1.0f / sampleFreq);
            q1 += qDot2 * (1.0f / sampleFreq);
            q2 += qDot3 * (1.0f / sampleFreq);
            q3 += qDot4 * (1.0f / sampleFreq);

            // Normalize quaternion
            recipNorm = 1.0f / (float)Math.Sqrt(q0 * q0 + q1 * q1 + q2 * q2 + q3 * q3);
            q0 *= recipNorm;
            q1 *= recipNorm;
            q2 *= recipNorm;
            q3 *= recipNorm;

            cursorTarget.transform.localRotation = new Quaternion(q0, q1, q2, q3);
        }



		public float sampleFreq = 8000.0f;  // Sample frequency in Hz
        public float twoKpDef = 1.0f;     // 2 * proportional gain
        public float twoKiDef = 1.0f;     // 2 * integral gain

        //float q0 = 1.0f, q1 = 0.0f, q2 = 0.0f, q3 = 0.0f; // Quaternion elements representing orientation
        public float integralFBx = 0.0f, integralFBy = 0.0f, integralFBz = 0.0f; // Integral feedback terms

        void updateMahony(float ax, float ay, float az, float gx, float gy, float gz, float mx, float my, float mz)
        {
            float recipNorm;
            float halfvx, halfvy, halfvz;
            float halfex, halfey, halfez;
            float qa, qb, qc;

            // Normalize accelerometer and magnetometer measurements
            recipNorm = 1.0f / (float)Math.Sqrt(ax * ax + ay * ay + az * az);
            ax *= recipNorm;
            ay *= recipNorm;
            az *= recipNorm;
            recipNorm = 1.0f / (float)Math.Sqrt(mx * mx + my * my + mz * mz);
            mx *= recipNorm;
            my *= recipNorm;
            mz *= recipNorm;

            // Compute reference direction of Earth's magnetic field
            halfvx = q1 * q3 - q0 * q2;
            halfvy = q0 * q1 + q2 * q3;
            halfvz = q0 * q0 - 0.5f + q3 * q3;

            // Compute estimated direction of gravity and magnetic field
            halfex = ay * halfvz - az * halfvy;
            halfey = az * halfvx - ax * halfvz;
            halfez = ax * halfvy - ay * halfvx;

            // Apply feedback terms
            if (twoKiDef > 0.0f)
            {
                integralFBx += twoKiDef * halfex * (1.0f / sampleFreq);
                integralFBy += twoKiDef * halfey * (1.0f / sampleFreq);
                integralFBz += twoKiDef * halfez * (1.0f / sampleFreq);
                gx += integralFBx;  // Apply integral feedback
                gy += integralFBy;
                gz += integralFBz;
            }

            // Apply proportional feedback
            gx += twoKpDef * halfex;
            gy += twoKpDef * halfey;
            gz += twoKpDef * halfez;

            // Integrate rate of change of quaternion
            gx *= (0.5f * (1.0f / sampleFreq)); // Pre-multiply common factors
            gy *= (0.5f * (1.0f / sampleFreq));
            gz *= (0.5f * (1.0f / sampleFreq));
            qa = q0;
            qb = q1;
            qc = q2;

            UnityEngine.Debug.Log(" " + qa + " " + qb + " " + qc);
            q0 += (-qb * gx - qc * gy - q3 * gz);
            q1 += (qa * gx + qc * gz - q3 * gy);
            q2 += (qa * gy - qb * gz + q3 * gx);
            q3 += (qa * gz + qb * gy - qc * gx);

            // Normalize quaternion
            recipNorm = 1.0f / (float)Math.Sqrt(q0 * q0 + q1 * q1 + q2 * q2 + q3 * q3);
            q0 *= recipNorm;
            q1 *= recipNorm;
            q2 *= recipNorm;
            q3 *= recipNorm;

            cursorTarget.transform.localRotation = new Quaternion(q0, q1, q2, q3);
        }

		SensorFusion sensorFusion = new SensorFusion();
		KalmanFilter kalmanFilter = new KalmanFilter();

        static int count = 0;
		public void setState(DK1DeviceState state)
		{
			if (float.IsNaN(state.currentPosition[0]) == false && float.IsNaN(state.currentPosition[1]) == false && float.IsNaN(state.currentPosition[2]) == false)
			{
				cursorTarget.transform.localPosition = new Vector3(positionFilter[0].update(state.currentPosition[0] - localPositionOffset.x), positionFilter[1].update(state.currentPosition[1] - localPositionOffset.y), -positionFilter[2].update(state.currentPosition[2] - localPositionOffset.z));

				
				if(float.IsNaN(state.currentRotation[0]) == false && float.IsNaN(state.currentRotation[1]) == false && float.IsNaN(state.currentRotation[2]) == false && float.IsNaN(state.currentRotation[3]) == false){
					//UnityEngine.Debug.Log(" " + state.currentRotation[0] + ", " + state.currentRotation[1] + ", " + state.currentRotation[2] + ", " +  state.currentRotation[3]);

					byte[] buf = new byte[4];
                    Int16[] accel = new Int16[3];
                    Int16[] gyro = new Int16[3];
                    Int16[] magnetometer = new Int16[3];

                    buf = BitConverter.GetBytes(state.currentRotation[0]);
                    gyro[1] = (Int16)(buf[0] | buf[1] << 8);
                    gyro[0] = (Int16)(buf[2] | buf[3] << 8);
					//UnityEngine.Debug.Log(" " + buf[0] + " " + buf[1] + " " + buf[2] + " " + buf[3]);
                    //gyro[0] = BitConverter.ToInt16(buf, 0);


                    buf = BitConverter.GetBytes(state.currentRotation[1]);
                    accel[0] = (Int16)(buf[0] | buf[1] << 8);
                    gyro[2] = (Int16)(buf[2] | buf[3] << 8);
                    //UnityEngine.Debug.Log(" " + buf[0] + " " + buf[1] + " " + buf[2] + " " + buf[3]);

                    buf = BitConverter.GetBytes(state.currentRotation[2]);
                    accel[2] = (Int16)(buf[0] | buf[1] << 8);
                    accel[1] = (Int16)(buf[2] | buf[3] << 8);
                    //UnityEngine.Debug.Log(" " + buf[0] + " " + buf[1] + " " + buf[2] + " " + buf[3]);

                    buf = BitConverter.GetBytes(state.currentRotation[3]);
                    magnetometer[1] = (Int16)(buf[0] | buf[1] << 8);
                    magnetometer[0] = (Int16)(buf[2] | buf[3] << 8);
					magnetometer[2] = (Int16)state.framerate;
                    //UnityEngine.Debug.Log(" " + accel[0] + ", " + accel[1] + ", " + accel[2]);
                    //UnityEngine.Debug.Log(" " + magnetometer[0] + ", " + magnetometer[1] + ", " + magnetometer[2]);

                    double[] accelScaled = new double[3];
                    double[] gyroScaled = new double[3];
                    double[] magScaled = new double[3];
                    float accelScale = 32768f / (2f * 9.81f);

					accelScaled[0] = accel[0];// / accelScale;
					accelScaled[1] = accel[1];/// accelScale;
					accelScaled[2] = accel[2];// / accelScale;

					float gyroScale = 32768f / 500;
					gyroScaled[0] = gyro[0] / gyroScale;
                    gyroScaled[1] = gyro[1] / gyroScale;
                    gyroScaled[2] = gyro[2] / gyroScale;

					magScaled[0] = magnetometer[0];
                    magScaled[1] = magnetometer[1];
                    magScaled[2] = magnetometer[2];

                    //                    UnityEngine.Debug.Log(" " + accelScaled[0] + ", " + accelScaled[1] + ", " + accelScaled[2] + ", " + gyroScaled[0] + ", " + gyroScaled[1] + ", " + gyroScaled[2]);

                    //calculateOrientation(accelScaled[0], accelScaled[1], accelScaled[2], gyroScaled[0], gyroScaled[1], gyroScaled[2], 0.00006f);

                    //calculateOrientation(accelScaled[0], accelScaled[1], accelScaled[2], gyroScaled[0], gyroScaled[1], gyroScaled[2], 0.00006f);
                    //calculateOrientation(accelScaled[0], accelScaled[1], accelScaled[2], gyroScaled[0], gyroScaled[1], gyroScaled[2], 0.0002f);

                    //updateQuaternion9A(accelScaled[0], accelScaled[1], accelScaled[2], gyroScaled[0], gyroScaled[1], gyroScaled[2], magnetometer[0], magnetometer[1], magnetometer[2]);
  
                    //sensor_fusion(accela, maga, gyroa, 0.001);
                    double[] orientation = kalmanFilter.Update(accelScaled, gyroScaled, magScaled, 0.0001);

                    //double[] orientation = sensorFusion.Update(accelScaled, gyroScaled, magScaled, 0.0001);
					Quaternion quart = new Quaternion();
					quart.w = (float)orientation[0];
                    quart.x = (float)orientation[1];
                    quart.y = (float)orientation[2];
                    quart.z = (float)orientation[3];
                    //cursorTarget.transform.localRotation = quart;
                    if (allowCursorOrientation == true) updateMahony((float)accelScaled[0], (float)accelScaled[2], (float)accelScaled[1], (float)gyroScaled[0], (float)gyroScaled[2], (float)gyroScaled[1], (float)magnetometer[0], (float)magnetometer[2], (float)magnetometer[1]);
                }

				if(stylusControl != null)
				{
					if(Input.GetKey(KeyCode.Space)) state.stylusState ^= 1;
					stylusControl.processStylusByte(state.stylusState);
				}
			}
			
		}


		public Vector3 getCurrentForce()
		{
			Vector3 displacement = new Vector3();

			if(temporaryCursor){
				displacement = temporaryCursor.transform.position - cursorTarget.transform.position;
				currentPosition = temporaryCursor.transform.position;
			}
			else{
				displacement = cursorBaseModel.transform.position - cursorTarget.transform.position;
				currentPosition = cursorBaseModel.transform.position;
			}

			if (displacement.magnitude > cursorTeleportThreshold)
			{
				UnityEngine.Debug.Log("Teleporting cursor...");

				if (temporaryCursor) temporaryCursor.transform.position = cursorTarget.transform.localPosition;
				cursorBaseModel.transform.position = cursorTarget.transform.localPosition;
				setSafeStart();
			}
			
			for (int x = 0; x < maxCustomForces; x++)
			{
				if (customForces[x].allocated == true)
				{
					displacement += (customForces[x].force * (gameObject.GetComponentInParent<Senmag_Workspace>().spatialMultiplier / 10f)) * (gameObject.GetComponentInParent<Senmag_Workspace>().hapticStiffness / 10f);
					customForces[x].updateCounter += 1;
					if (customForces[x].updateCounter > 10000) releaseCustomForce(x, customForces[x].owner);          //if it hasn't been updated for a while, auto-release it
				}
			}


			//accelerationCompensation = new Vector3(0.1f, 0, 0);
			Vector3 speed = currentPosition - posLast;
			posLast = currentPosition;

			/*Vector3 acceleration = new Vector3(0,0,0);
			//UnityEngine.Debug.Log(accelerationFilter.Count);
			acceleration.x = accelerationFilter[0].update(speed.x - speedLast.x) * accelerationCompensation.x;
			acceleration.y = accelerationFilter[1].update(speed.y - speedLast.y) * accelerationCompensation.y;
			acceleration.z = accelerationFilter[2].update(speed.z - speedLast.z) * accelerationCompensation.z;*/
			//speedLast = speed;

			//displacement -= acceleration;

			//accelerationFilter = new List<LPFilter>();



			safeStart = 0;
			if (safeStart > 0){
				//UnityEngine.Debug.Log("SafeStart");
				if (temporaryCursor)
				{
					if (Math.Abs(displacement.magnitude) < safeStartThreshold) safeStart -= 1;
				}
				else if (Math.Abs(displacement.magnitude) < safeStartThreshold) safeStart -= 1;

				displacement.x = 0;
				displacement.y = 0;
				displacement.z = 0;
			}

			if ((displacement - lastForce).magnitude > maxForceVelocity)
			{
				UnityEngine.Debug.Log("Max force velocity exceeded...");
				setSafeStart();
				displacement = new Vector3(0, 0, 0);
			}

			//UnityEngine.Debug.Log(displacement);
			lastForce = displacement;
			return displacement;
		}

		public void createCustomCursor(GameObject newCursor, int overideSafeStart = 0)
		{
			if(safeStart == 0 || overideSafeStart == 1)																	//require zero contact between cursor changes
			{
				if(temporaryCursor){																					//if a cursor already exists, delete it
				//	Debug.Log("custom cursor already...");
					Destroy(temporaryCursor.gameObject);
				}
					
				cursor.GetComponent<SphereCollider>().enabled = false;											//create a new cursor from the new object
				cursor.GetComponent<MeshRenderer>().enabled = false;

				setSafeStart();

				temporaryCursor = Instantiate(newCursor, cursorTarget.transform.position, newCursor.transform.rotation);
				//customCursor.transform.parent = transform;
				if (temporaryCursor.GetComponent<Rigidbody>() == false) {
					temporaryCursor.AddComponent<Rigidbody>();

					if(temporaryCursor.GetComponent<MeshCollider>() == true) temporaryCursor.GetComponent<MeshCollider>().convex = true;
				}
				temporaryCursor.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
				temporaryCursor.GetComponent<Rigidbody>().isKinematic = false;

				if (temporaryCursor.GetComponent<MeshCollider>() == true) temporaryCursor.GetComponent<MeshCollider>().material.dynamicFriction = 0;
				if (temporaryCursor.GetComponent<MeshCollider>() == true) temporaryCursor.GetComponent<MeshCollider>().material.staticFriction = 0;

				Quaternion rot = transform.rotation;
				temporaryCursor.transform.parent = transform;
			
				customCursorJoint = temporaryCursor.AddComponent(typeof(ConfigurableJoint)) as ConfigurableJoint;
				customCursorJoint.connectedBody = cursorTarget.GetComponent<Rigidbody>();
				customCursorJoint.xMotion = ConfigurableJointMotion.Locked; //.set(locked);
				customCursorJoint.yMotion = ConfigurableJointMotion.Locked; //.set(locked);
				customCursorJoint.zMotion = ConfigurableJointMotion.Locked; //.set(locked);
				customCursorJoint.angularXMotion = ConfigurableJointMotion.Locked;
				customCursorJoint.angularYMotion = ConfigurableJointMotion.Locked;
				customCursorJoint.angularZMotion = ConfigurableJointMotion.Locked;
				customCursorJoint.gameObject.layer = 2;

				customCursorJoint.autoConfigureConnectedAnchor = true;
				customCursorJoint.anchor = new Vector3( 0, 0, 0 );
				customCursorJoint.connectedAnchor = new Vector3( 0, 0, 0 );

				temporaryCursor.AddComponent<collisionTrigger>();
				temporaryCursor.GetComponent<Collider>().enabled = true;

				if (temporaryCursor.GetComponent<MeshCollider>())
				{
					temporaryCursor.GetComponent<MeshCollider>().convex = true;
				}
				//customCursor.GetComponent<Renderer>().enabled = true;
			}
			//customCursor.transform.rotation = transform.rotation;
		}
		public void destroyCustomCursor()
		{
			setSafeStart();
			Destroy(temporaryCursor.gameObject);
			cursor.GetComponent<SphereCollider>().enabled = true;
			cursor.GetComponent<MeshRenderer>().enabled = true;
			if(cursorHold == true)
			{
				UnityEngine.Debug.Log("regenerating old cursor...");
				createCustomCursor(heldCursor.gameObject, 1);
				Destroy(heldCursor.gameObject);
				cursorHold = false;
			}
			
		}
		public void holdCursor()
		{
			heldCursor = Instantiate(temporaryCursor);
			//heldCursor = customCursor;
			heldCursor.GetComponent<Collider>().enabled = false;
			heldCursor.GetComponent<Renderer>().enabled = false;
			cursorHold = true;
		}
		public void dropCustomCursor()
		{

			setSafeStart();
			cursor.GetComponent<SphereCollider>().enabled = false;	
			temporaryCursor.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
			temporaryCursor.GetComponent<Rigidbody>().isKinematic = true;
			temporaryCursor.GetComponent<MeshCollider>().convex = false;
			
			Destroy(temporaryCursor.GetComponent<ConfigurableJoint>());
			Destroy(temporaryCursor.GetComponent<collisionTrigger>());
			droppingCursor = true;
		}

		public void dropCustomCursorOnGrid()
		{
			setSafeStart();
			cursor.GetComponent<SphereCollider>().enabled = false;
			temporaryCursor.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
			temporaryCursor.GetComponent<Rigidbody>().isKinematic = true;
			temporaryCursor.GetComponent<MeshCollider>().convex = false;

			Destroy(temporaryCursor.GetComponent<ConfigurableJoint>());
			Destroy(temporaryCursor.GetComponent<collisionTrigger>());
			droppingCursor = true;

		}

		//if the cursor collides with anything, send the forces before waiing for the next update
		public void triggerForceUpdate()
		{
			gameObject.GetComponentInParent<Senmag_Workspace>().updateCursorForces(0);
		}
		bool grabbedObjectKinematic = false;
		public void pickUpObject(GameObject targetObject, bool hideMainCursor)
		{
			UnityEngine.Debug.Log("picking up " + targetObject.name);

			if(objectSpring == null) objectSpring = cursorTarget.AddComponent<ConfigurableJoint>();
			//if (objectSpring == null) objectSpring = cursorBaseModel.AddComponent<ConfigurableJoint>();
			
			
			if (targetObject.GetComponent<Rigidbody>() == null){
				UnityEngine.Debug.Log(targetObject.name + " doesn't have a RigidBody");
				return;
			}

			objectSpring.enableCollision = false;
			objectSpring.autoConfigureConnectedAnchor = true;
			objectSpring.anchor = new Vector3(0, 0, 0);
			//objectSpring.connectedAnchor = new Vector3(0, 0, 0);
			if(targetObject.GetComponent<Rigidbody>().isKinematic){
				grabbedObjectKinematic = true;
				targetObject.GetComponent<Rigidbody>().isKinematic = false;
			}
			else grabbedObjectKinematic = false;

			targetObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

			objectSpring.connectedBody = targetObject.GetComponent<Rigidbody>();
			

			//use a free joint with high gain - releases objects more cleanly
			objectSpring.xMotion = ConfigurableJointMotion.Free; //.set(locked);
			objectSpring.yMotion = ConfigurableJointMotion.Free; //.set(locked);
			objectSpring.zMotion = ConfigurableJointMotion.Free; //.set(locked);

			objectSpring.angularXMotion = ConfigurableJointMotion.Locked; //.set(locked);
			objectSpring.angularYMotion = ConfigurableJointMotion.Locked; //.set(locked);
			objectSpring.angularZMotion = ConfigurableJointMotion.Locked; //.set(locked);

			JointDrive springDrive = new JointDrive();
			springDrive.positionSpring = 1000000f;
			springDrive.maximumForce = 1000000;
			springDrive.positionDamper = 3000;
			objectSpring.xDrive = springDrive;
			objectSpring.yDrive = springDrive;
			objectSpring.zDrive = springDrive;


			if (stylusControl != null) stylusControl.disableCollider();
		}
		public void dropObject(GameObject targetObject)
		{
			UnityEngine.Debug.Log("dropping " + targetObject.name);
			if (objectSpring.connectedBody == targetObject.GetComponent<Rigidbody>())
			{
				if(grabbedObjectKinematic == true) targetObject.GetComponent<Rigidbody>().isKinematic = true;
				//Vector3 velocity = targetObject.GetComponent<Rigidbody>().velocity;
				objectSpring.connectedBody = null;
				Destroy(objectSpring);//.connectedBody = null;
				//targetObject.GetComponent<Rigidbody>().velocity = velocity;
			}

			if (stylusControl != null) stylusControl.enableCollider();
		}


		public void attachCursorToObject(GameObject targetObject, bool offsetPosition)
		{
			cursor.GetComponent<SphereCollider>().enabled = false;
			cursor.GetComponent<MeshRenderer>().enabled = false;
			temporaryCursor = targetObject;

			


			spring.autoConfigureConnectedAnchor = false;
			spring.anchor = new Vector3(0, 0, 0);
			spring.connectedAnchor = new Vector3(0, 0, 0);

			if (offsetPosition == false)
			{
				localPositionOffset = new Vector3(0, 0, 0);
				localRotationOffset = new Quaternion(0, 0, 0, 1);
			}
			else
			{
				localPositionOffset = cursorTarget.transform.position - targetObject.transform.position;
				localRotationOffset = Quaternion.FromToRotation(cursorTarget.transform.forward, targetObject.transform.forward * -1);

				cursorTarget.transform.rotation *= localRotationOffset;
				cursorTarget.transform.position -= localPositionOffset;
			}

			spring.connectedBody = targetObject.GetComponent<Rigidbody>();
			spring.GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Locked; //.set(locked);
			spring.GetComponent<ConfigurableJoint>().yMotion = ConfigurableJointMotion.Locked; //.set(locked);
			spring.GetComponent<ConfigurableJoint>().zMotion = ConfigurableJointMotion.Locked; //.set(locked);
			spring.GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Locked;
			spring.GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Locked;
			spring.GetComponent<ConfigurableJoint>().angularZMotion = ConfigurableJointMotion.Locked;

			setSafeStart();
		}
		
		public void releaseCursor()
		{
			cursor.GetComponent<SphereCollider>().enabled = true;
			cursor.GetComponent<MeshRenderer>().enabled = true;
			spring.connectedBody = cursor.GetComponent<Rigidbody>();
			cursorTarget.transform.position += localPositionOffset;

			
			localRotationOffset = new Quaternion(0, 0, 0, 1);
			//localRotationOffset = new Vector3(0, 0, 0);
			localPositionOffset = new Vector3(0, 0, 0);
			spring.GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Locked; //.set(locked);
			spring.GetComponent<ConfigurableJoint>().yMotion = ConfigurableJointMotion.Locked; //.set(locked);
			spring.GetComponent<ConfigurableJoint>().zMotion = ConfigurableJointMotion.Locked; //.set(locked);
			spring.GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Free;
			spring.GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Free;
			spring.GetComponent<ConfigurableJoint>().angularZMotion = ConfigurableJointMotion.Free;
			spring.autoConfigureConnectedAnchor = false;
			spring.anchor = new Vector3(0, 0, 0);
			spring.connectedAnchor = new Vector3(0, 0, 0);
			temporaryCursor = null;
			setSafeStart();
		}


        public class KalmanFilter
        {
            private double[] qEst;  // Estimated Quaternion [w, x, y, z]
            private double[] gyroBias = { 0, 0, 0 }; // Gyroscope bias
            private double[] q = { 0, 0, 0, 0 };  // Predicted Quaternion
            private double[] P;  // Error Covariance Matrix
            private double beta = 0.02; // Filter gain
            private double dt;

            public KalmanFilter()
            {
                qEst = new double[] { 1.0, 0.0, 0.0, 0.0 };
                //P = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
                P = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            }

            public double[] Update(double[] accelerometer, double[] gyroscope, double[] magnetometer, double dt)
            {
                this.dt = dt;

                // Normalize accelerometer and magnetometer readings
                NormalizeVector(accelerometer);
                NormalizeVector(magnetometer);

                // Predict the orientation using the gyroscope
                PredictOrientation(gyroscope);

                // Update the orientation using accelerometer and magnetometer data
                UpdateOrientation(accelerometer, magnetometer);

                return qEst;
            }

            private void PredictOrientation(double[] gyro)
            {
                double[] omega = new double[] { 0.0, gyro[0], gyro[1], gyro[2] };
                QuaternionMultiply(qEst, omega, q);

                // Apply bias correction
                for (int i = 1; i < 4; i++)
                {
                    q[i] -= gyroBias[i - 1] * dt;
                }

                // Normalize predicted quaternion
                NormalizeQuaternion(q);
            }

            private void UpdateOrientation(double[] accelerometer, double[] magnetometer)
            {
                // Compute the gravitational reference direction
                double v = 2.0 * q[1] * q[3] - 2.0 * q[0] * q[2];
                double[] reference = new double[]
                {
        v,
        2.0 * q[0] * q[1] + 2.0 * q[2] * q[3],
        2.0 * (q[0] * q[0] + q[3] * q[3]) - 1.0
                };
                NormalizeVector(reference);

                // Compute the error between the estimated and measured direction
                double[] error = new double[3];
                CrossProduct(accelerometer, reference, error);
                for (int i = 0; i < 3; i++)
                {
                    error[i] += magnetometer[i];
                }

                // Kalman Gain calculation
                double[] S = new double[3];
                MatrixMultiply(P, error, S);
                double[] K = new double[3];

                // Limit the gain to prevent NaN values
                double sumS = S[0] + S[1] + S[2] + 1e-4; // 1e-4 to prevent division by zero
                if (sumS > 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        K[i] = S[i] / sumS;
                    }
                }
                else
                {
                    // Handle the case where sumS is too small, e.g., by not updating K.
                    return;
                }

                // Update quaternion estimate
                for (int i = 0; i < 3; i++)
                {
                    qEst[i + 1] += K[i] * error[i];
                }

                // Normalize quaternion estimate
                NormalizeQuaternion(qEst);

                // Update error covariance matrix P
                for (int i = 0; i < 3; i++)
                {
                    P[i * 4 + 0] -= K[i] * S[0];
                    P[i * 4 + 1] -= K[i] * S[1];
                    P[i * 4 + 2] -= K[i] * S[2];
                    P[i * 4 + 3] -= K[i] * S[0];
                }
            }

            private void MatrixMultiply(double[] matrix, double[] vector, double[] result)
            {
                if (matrix.Length == 16 && vector.Length == 3 && result.Length == 3)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        result[i] = 0;
                        for (int j = 0; j < 3; j++)
                        {
                            result[i] += matrix[i * 4 + j] * vector[j];
                        }
                    }
                }
                else
                {
                    // Handle error, e.g., by throwing an exception or logging a message
                    // based on your application's requirements.
                    throw new ArgumentException("Matrix and vector dimensions are incorrect.");
                }
            }
            private void NormalizeVector(double[] vector)
            {
                double norm = 0;
                for (int i = 0; i < vector.Length; i++)
                {
                    norm += vector[i] * vector[i];
                }
                norm = Math.Sqrt(norm);
                for (int i = 0; i < vector.Length; i++)
                {
                    vector[i] /= norm;
                }
            }

            private void CrossProduct(double[] a, double[] b, double[] result)
            {
                result[0] = a[1] * b[2] - a[2] * b[1];
                result[1] = a[2] * b[0] - a[0] * b[2];
                result[2] = a[0] * b[1] - a[1] * b[0];
            }

            private void NormalizeQuaternion(double[] quaternion)
            {
                double norm = 0;
                for (int i = 0; i < quaternion.Length; i++)
                {
                    norm += quaternion[i] * quaternion[i];
                }
                norm = Math.Sqrt(norm);
                for (int i = 0; i < quaternion.Length; i++)
                {
                    quaternion[i] /= norm;
                }
            }

            private void QuaternionMultiply(double[] a, double[] b, double[] result)
            {
                result[0] = a[0] * b[0] - a[1] * b[1] - a[2] * b[2] - a[3] * b[3];
                result[1] = a[0] * b[1] + a[1] * b[0] + a[2] * b[3] - a[3] * b[2];
                result[2] = a[0] * b[2] - a[1] * b[3] + a[2] * b[0] + a[3] * b[1];
                result[3] = a[0] * b[3] + a[1] * b[2] - a[2] * b[1] + a[3] * b[0];
            }

           

            // Other utility functions (NormalizeVector, QuaternionMultiply, MatrixMultiply, etc.) remain the same as in the previous response.
        }


        public class SensorFusion
        {
            private double[] q;  // Quaternion [w, x, y, z]
            private double beta = 0.02; // Filter gain
            private double[] gyroBias = { 0, 0, 0 }; // Gyroscope bias to compensate for drift

            public SensorFusion()
            {
                q = new double[] { 1.0, 0.0, 0.0, 0.0 };
            }

            public double[] Update(double[] accelerometer, double[] gyroscope, double[] magnetometer, double dt)
            {
                // Normalize accelerometer and magnetometer readings
                NormalizeVector(accelerometer);
                NormalizeVector(magnetometer);

                // Compute the quaternion rate of change from gyroscope data
                double[] omega = new double[] { 0.0, gyroscope[0], gyroscope[1], gyroscope[2] };
                double[] qDot = new double[4];

                QuaternionMultiply(q, omega, qDot);
                for (int i = 0; i < 4; i++)
                {
                    qDot[i] *= 0.5;
                }

                // Compute the gravitational reference direction
                double v = 2.0 * q[1] * q[3] - 2.0 * q[0] * q[2];
                double[] reference = new double[]
                {
            v,
            2.0 * q[0] * q[1] + 2.0 * q[2] * q[3],
            2.0 * (q[0] * q[0] + q[3] * q[3]) - 1.0
                };
                NormalizeVector(reference);

                // Compute the error between the estimated and measured direction
                double[] error = new double[3];
                CrossProduct(accelerometer, reference, error);
                for (int i = 0; i < 3; i++)
                {
                    error[i] += magnetometer[i];
                }

                // Compute and apply the feedback term
                for (int i = 0; i < 3; i++)
                {
                    qDot[i + 1] -= 2.0 * beta * error[i];
                }

                // Integrate to get the new quaternion
                for (int i = 0; i < 4; i++)
                {
                    q[i] += qDot[i] * dt;
                }
                NormalizeQuaternion(q);

                // Compensate for gyroscope drift by adjusting the quaternion
                double[] correction = new double[4];
                QuaternionMultiply(q, new double[] { 0.0, gyroBias[0], gyroBias[1], gyroBias[2] }, correction);

                // Apply the correction to the quaternion
                for (int i = 1; i < 4; i++)
                {
                    q[i] -= correction[i] * dt;
                }
                NormalizeQuaternion(q);

                return q;
            }

            private static void NormalizeVector(double[] vector)
            {
                double norm = 0;
                for (int i = 0; i < vector.Length; i++)
                {
                    norm += vector[i] * vector[i];
                }
                norm = Math.Sqrt(norm);
                for (int i = 0; i < vector.Length; i++)
                {
                    vector[i] /= norm;
                }
            }

            private static void CrossProduct(double[] a, double[] b, double[] result)
            {
                result[0] = a[1] * b[2] - a[2] * b[1];
                result[1] = a[2] * b[0] - a[0] * b[2];
                result[2] = a[0] * b[1] - a[1] * b[0];
            }

            private static void NormalizeQuaternion(double[] quaternion)
            {
                double norm = 0;
                for (int i = 0; i < quaternion.Length; i++)
                {
                    norm += quaternion[i] * quaternion[i];
                }
                norm = Math.Sqrt(norm);
                for (int i = 0; i < quaternion.Length; i++)
                {
                    quaternion[i] /= norm;
                }
            }

            private static void QuaternionMultiply(double[] a, double[] b, double[] result)
            {
                result[0] = a[0] * b[0] - a[1] * b[1] - a[2] * b[2] - a[3] * b[3];
                result[1] = a[0] * b[1] + a[1] * b[0] + a[2] * b[3] - a[3] * b[2];
                result[2] = a[0] * b[2] - a[1] * b[3] + a[2] * b[0] + a[3] * b[1];
                result[3] = a[0] * b[3] + a[1] * b[2] - a[2] * b[1] + a[3] * b[0];
            }
        }

    }
}
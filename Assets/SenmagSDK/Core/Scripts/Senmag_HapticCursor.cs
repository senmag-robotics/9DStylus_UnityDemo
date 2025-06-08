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
		public bool allowCursorOrientation = true;

		public int					collisionStatus;
		bool						droppingCursor;

		public Vector3				accelerationCompensation = new Vector3(2.5f, 1.5f, 2f);
		private Vector3				posLast;
		private Vector3				speedLast;
		private List<LPFilter>		accelerationFilter = new List<LPFilter>();

		GameObject					cursor;
		public GameObject			cursorTarget;
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

        public Vector3				cursorPositionOffset = new Vector3(0, 0, 0);

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
		private bool cursorHidden = false;


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

			//UnityEngine.Debug.Log("acceleration Filter has " + accelerationFilter.Count + " items");
			
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
								if (gameObject.GetComponentInParent<Senmag_Workspace>().defaultRightClickMenu != null)
								{
									rightClickMenu = Instantiate(gameObject.GetComponentInParent<Senmag_Workspace>().defaultRightClickMenu);
									rightClickMenu.transform.position = currentPosition;
								}
							}
						}
					}
					else rightClickLatch = false;
				}		
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

		public void destroyCursor()
		{
			Destroy(gameObject);
		}


        public void generateCursor(GameObject parent, GameObject cursorModel, string cursorName, SenmagDeviceStatus deviceState, float cursorsize, float cursorFrictionDynamic, float cursorFrictionStatic)
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
			cursorBaseModel.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
			//cursorBaseModel.GetComponent<Rigidbody>().mass = 0.001f;
			cursorBaseModel.GetComponent<Rigidbody>().mass = gameObject.GetComponentInParent<Senmag_Workspace>().cursorMass;

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

		public void showFullCursor()
		{
            cursorBaseModel.GetComponent<Senmag_stylusControl>().showStylusBody();
        }
		public void showBasicCursor()
		{
            cursorBaseModel.GetComponent<Senmag_stylusControl>().hideStylusBody();
        }

        static int count = 0;
		public void setState(SenmagDeviceStatus state, float positionGain)
		{
			if (float.IsNaN(state.currentPosition[0]) == false && float.IsNaN(state.currentPosition[1]) == false && float.IsNaN(state.currentPosition[2]) == false)
			{
				cursorTarget.transform.localPosition = new Vector3(positionFilter[0].update((state.currentPosition[0] * positionGain) - localPositionOffset.x), positionFilter[1].update((state.currentPosition[1] * positionGain) - localPositionOffset.y), -positionFilter[2].update((state.currentPosition[2] * positionGain) - localPositionOffset.z));
		
				
				cursorTarget.transform.position += cursorPositionOffset;



                if (float.IsNaN(state.currentOrientation[0]) == false && float.IsNaN(state.currentOrientation[1]) == false && float.IsNaN(state.currentOrientation[2]) == false && float.IsNaN(state.currentOrientation[3]) == false){
                    //UnityEngine.Debug.Log(" " + state.currentRotation[0] + ", " + state.currentRotation[1] + ", " + state.currentRotation[2] + ", " +  state.currentRotation[3]);

                    if (allowCursorOrientation == true)
					{
                        //cursorTarget.transform.localRotation = new Quaternion(state.currentOrientation[0], -state.currentOrientation[1], -state.currentOrientation[3], state.currentOrientation[2]);
                        //cursorBaseModel.transform.localRotation = new Quaternion(state.currentOrientation[0], -state.currentOrientation[1], -state.currentOrientation[3], state.currentOrientation[2]);

                        //cursorTarget.transform.localRotation = new Quaternion(-state.currentOrientation[2], -state.currentOrientation[3], state.currentOrientation[1], state.currentOrientation[0]);
                        //cursorBaseModel.transform.localRotation = new Quaternion(-state.currentOrientation[2], -state.currentOrientation[3], state.currentOrientation[1], state.currentOrientation[0]);

                        cursorTarget.transform.localRotation = new Quaternion(state.currentOrientation[0], state.currentOrientation[1], state.currentOrientation[2], state.currentOrientation[3]);
                        cursorBaseModel.transform.localRotation = new Quaternion(state.currentOrientation[0], state.currentOrientation[1], state.currentOrientation[2], state.currentOrientation[3]);
                    }
                }

				if(stylusControl != null)
				{
					if(Input.GetKey(KeyCode.Space)) state.stylusButtons ^= 1;
					stylusControl.processStylusByte(state.stylusButtons);
				}
			}
			
		}

		public void setCursorOffset(Vector3 offset)
		{
            cursorTarget.transform.position += (cursorPositionOffset - offset);
            cursorPositionOffset = offset;
        }

		public Vector3 getPosition()
		{
			return cursorTarget.transform.position;
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
					//displacement += (customForces[x].force / (gameObject.GetComponentInParent<Senmag_Workspace>().spatialMultiplier / 10f)) * (gameObject.GetComponentInParent<Senmag_Workspace>().hapticStiffness / 10f);
					displacement += (customForces[x].force / (10f* gameObject.GetComponentInParent<Senmag_Workspace>().hapticStiffness));/// (gameObject.GetComponentInParent<Senmag_Workspace>().spatialMultiplier / 10f)) * (gameObject.GetComponentInParent<Senmag_Workspace>().hapticStiffness / 10f);
                    customForces[x].updateCounter += 1;
					if (customForces[x].updateCounter > 10000) releaseCustomForce(x, customForces[x].owner);          //if it hasn't been updated for a while, auto-release it
				}
			}

			Vector3 speed = currentPosition - posLast;
			posLast = currentPosition;

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

			if (stylusControl != null)
			{
				stylusControl.enableCollider();
				if (cursorHidden == true) stylusControl.showStylusTip();
			}


		}


        public void linkObjectToPosition(GameObject targetObject, bool hideMainCursor)		//links an object to the cursor position
        {
            UnityEngine.Debug.Log("linking object " + targetObject.name);

            if (objectSpring == null) objectSpring = cursorTarget.AddComponent<ConfigurableJoint>();
            //if (objectSpring == null) objectSpring = cursorBaseModel.AddComponent<ConfigurableJoint>();


            if (targetObject.GetComponent<Rigidbody>() == null)
            {
                UnityEngine.Debug.Log(targetObject.name + " doesn't have a RigidBody");
                return;
            }

            objectSpring.enableCollision = false;
            //objectSpring.autoConfigureConnectedAnchor = true;
            objectSpring.autoConfigureConnectedAnchor = false;
            objectSpring.anchor = new Vector3(0, 0, 0);
            //objectSpring.connectedAnchor = new Vector3(0, 0, 0);
            if (targetObject.GetComponent<Rigidbody>().isKinematic)
            {
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

			if(hideMainCursor == true)
			{


				if (stylusControl != null)
				{
					cursorHidden = true;

                    stylusControl.hideStylusTip();
                    stylusControl.disableCollider();

				}
            }
        }
        public void attachCursorToObject(GameObject targetObject, bool offsetPosition)
		{
			//cursor.GetComponent<SphereCollider>().enabled = false;
			//cursor.GetComponent<MeshRenderer>().enabled = false;
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
			//cursor.GetComponent<SphereCollider>().enabled = true;
			//cursor.GetComponent<MeshRenderer>().enabled = true;
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


    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SenmagHaptic;

namespace uselessBox
{
	public class turret : MonoBehaviour
	{
		public enum TurretType
		{
			cannon,
			minigun,
			missile,
		}
		public enum TurretState
		{
			retracted,
			raisingHatch,
			deploying,
			deployed,
			disengage,
			retracting,
			loweringHatch,
		}

		public TurretType turretType;
		public bool isActive;
		public GameObject hatch;
		public TurretState turretState;
		public float hatchOpenAngle = 90f;
		public float hatchSmoothing = 5f;
		public float turretSmoothing = 10f;

		public float turretRange = 0.1f;
		public float turretReload = 1f;

		public GameObject post;
		public bool hatchOpen;
		public bool turretRaised;

		public float retractPosition;

		private float hatchTargetAngle;
		private float hatchTolerance = 2f;
		private float heightTolerance = 0.1f;
		private float turretAngleTolerance = 1f;
		private Quaternion defaultRotation;

		public AudioClip fireAudio;

		private Vector3 raisedPosition;
		private float targetHeight;

		private int soundCounter = 0;
		public float gunSoundFreq = 1;

		public float lastFireTime;
		public float reloadTime;
		private bool lastGrappleState;

		// Start is called before the first frame update
		void Start()
		{
			hatchOpen = false;
			

			turretState = TurretState.retracted;
			isActive = false;
			
			gameObject.GetComponent<Rigidbody>().isKinematic = true;

			hatchTargetAngle = 0;
			raisedPosition = post.transform.localPosition;
			defaultRotation = post.transform.localRotation;

			

			targetHeight = raisedPosition.z + retractPosition;
			if (turretType == TurretType.minigun)
			{
				transform.GetChild(0).GetComponent<ParticleSystem>().enableEmission = false;
				transform.GetChild(1).GetComponent<ParticleSystem>().enableEmission = false;
				GetComponent<MeshRenderer>().enabled = false;
				post.GetComponent<MeshRenderer>().enabled = false;
			}
			
			
		}

		// Update is called once per frame
		void Update()
		{

			if (turretType == TurretType.missile)
			{
				
				//Quaternion target = Quaternion.Euler(0, hatchTargetAngle - 180, 180);
				//hatch.transform.localRotation = Quaternion.Slerp(hatch.transform.localRotation, target, Time.deltaTime * hatchSmoothing);
				//if (Mathf.Abs(hatch.transform.localRotation.eulerAngles.y - 180) > (Mathf.Abs(hatchOpenAngle) - hatchTolerance) && (Mathf.Abs(hatch.transform.localRotation.eulerAngles.y - 180) < (Mathf.Abs(hatchOpenAngle) + hatchTolerance))) hatchOpen = true;
				//else hatchOpen = false;
				
				//UnityEngine.Debug.Log(hatch.transform.localRotation.eulerAngles.y-180 + "  " + hatchTargetAngle);

				Vector3 targetPos = new Vector3(raisedPosition.x, raisedPosition.y, targetHeight);
				post.transform.localPosition = Vector3.Slerp(post.transform.localPosition, targetPos, Time.deltaTime * hatchSmoothing);
				if ((Mathf.Abs(post.transform.localPosition.z) > (Mathf.Abs(targetHeight) - heightTolerance)) && (Mathf.Abs(post.transform.localPosition.z) < (Mathf.Abs(targetHeight) + heightTolerance))) turretRaised = true;
				else turretRaised = false;


				if (turretState == TurretState.raisingHatch && hatch.GetComponent<hatch>().targetAchieved == true)
				{
					turretState = TurretState.deploying;
					targetHeight = raisedPosition.z;
				}

				else if (turretState == TurretState.deploying)
				{
					if (turretRaised == true)
					{
						turretState = TurretState.deployed;
						post.GetComponent<MeshCollider>().enabled = true;
					}
				}

				else if (turretState == TurretState.deployed)
				{
					hatch.GetComponent<hatch>().setHatchTarget(hatchOpenAngle);
					if(GetComponent<missile>().missileState != missile.MissileState.ready)
					{
						turretState = TurretState.disengage;
					}
				}
				else if (turretState == TurretState.disengage)
				{
					post.GetComponent<MeshCollider>().enabled = false;
					Quaternion lookOnLook = defaultRotation;

					turretState = TurretState.retracting;
					targetHeight = raisedPosition.z + retractPosition;
					//gameObject.GetComponent<Rigidbody>().isKinematic = true;
					

				}
				else if (turretState == TurretState.retracting)
				{
					if (turretRaised == true)
					{
						turretState = TurretState.retracted;
						post.GetComponent<MeshRenderer>().enabled = false;
						hatch.GetComponent<hatch>().setHatchTarget(0);
					}
				}

			}


			if (turretType == TurretType.minigun)
			{
				//Quaternion target = Quaternion.Euler(0, hatchTargetAngle - 180, 180);
				//hatch.transform.localRotation = Quaternion.Slerp(hatch.transform.localRotation, target, Time.deltaTime * hatchSmoothing);
				//if (Mathf.Abs(hatch.transform.localRotation.eulerAngles.y - 180) > (Mathf.Abs(hatchOpenAngle) - hatchTolerance) && (Mathf.Abs(hatch.transform.localRotation.eulerAngles.y - 180) < (Mathf.Abs(hatchOpenAngle) + hatchTolerance))) hatchOpen = true;
				//else hatchOpen = false;
				//UnityEngine.Debug.Log(hatch.transform.localRotation.eulerAngles.y-180 + "  " + hatchTargetAngle);

				Vector3 targetPos = new Vector3(raisedPosition.x, raisedPosition.y, targetHeight);
				post.transform.localPosition = Vector3.Slerp(post.transform.localPosition, targetPos, Time.deltaTime * hatchSmoothing);
				if ((Mathf.Abs(post.transform.localPosition.z) > (Mathf.Abs(targetHeight) - heightTolerance)) && (Mathf.Abs(post.transform.localPosition.z) < (Mathf.Abs(targetHeight) + heightTolerance))) turretRaised = true;
				else turretRaised = false;


				if (turretState == TurretState.raisingHatch && hatch.GetComponent<hatch>().targetAchieved == true)
				{
					turretState = TurretState.deploying;
					targetHeight = raisedPosition.z;
				}

				else if (turretState == TurretState.deploying)
				{
					if (turretRaised == true)
					{
						gameObject.GetComponent<Rigidbody>().isKinematic = false;
						turretState = TurretState.deployed;
						post.GetComponent<MeshCollider>().enabled = true;
						GetComponent<MeshCollider>().enabled = true;
					}
				}

				else if (turretState == TurretState.deployed)
				{
					hatch.GetComponent<hatch>().setHatchTarget(hatchOpenAngle);

					Vector3 cursorPos = GameObject.Find("SenmagWorkspace").GetComponent<Senmag_Workspace>().osenmagServer.deviceList[0].cursor.GetComponent<Senmag_HapticCursor>().currentPosition;


					Quaternion lookOnLook = Quaternion.LookRotation(transform.position - cursorPos);
					transform.rotation = Quaternion.Lerp(transform.rotation, lookOnLook, Time.deltaTime * turretSmoothing);

					if ((transform.position - cursorPos).magnitude < turretRange)
					{
						soundCounter += 1;
						if (soundCounter >  (1 / Time.deltaTime) / gunSoundFreq)
						{
							transform.GetComponent<AudioSource>().Play();
							soundCounter = 0;
						}
						
						transform.GetChild(0).GetComponent<ParticleSystem>().enableEmission = true;
						transform.GetChild(1).GetComponent<ParticleSystem>().enableEmission = true;
						/*RaycastHit hit;
						Vector3 direction = transform.TransformDirection(Vector3.back) * turretRange;
						Debug.DrawRay(transform.position, direction);
						if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.back), out hit, turretRange))
						{
							UnityEngine.Debug.Log("hit!");
							if (hit.collider.gameObject.name == "cursor")
							{
								UnityEngine.Debug.Log("hit cursor!");
							}
						}*/
					}
					else
					{
						
						transform.GetChild(0).GetComponent<ParticleSystem>().enableEmission = false;
						transform.GetChild(1).GetComponent<ParticleSystem>().enableEmission = false;
					}
					




				}
				else if (turretState == TurretState.disengage)
				{
					post.GetComponent<MeshCollider>().enabled = false;
					GetComponent<MeshCollider>().enabled = false;
					transform.GetChild(0).GetComponent<ParticleSystem>().enableEmission = false;
					transform.GetChild(1).GetComponent<ParticleSystem>().enableEmission = false;
					Quaternion lookOnLook = defaultRotation;
					transform.localRotation = Quaternion.Slerp(transform.localRotation, lookOnLook, Time.deltaTime * turretSmoothing);
					if(Quaternion.Angle(transform.localRotation, defaultRotation) <= turretAngleTolerance)
					{
						turretState = TurretState.retracting;
						targetHeight = raisedPosition.z + retractPosition;
						gameObject.GetComponent<Rigidbody>().isKinematic = true;
					}

				}
				else if(turretState == TurretState.retracting)
				{
					if(turretRaised == true)
					{
						turretState = TurretState.retracted;
						post.GetComponent<MeshRenderer>().enabled = false;
						GetComponent<MeshRenderer>().enabled = false;

						hatch.GetComponent<hatch>().setHatchTarget(0);
						//closeHatch();
					}
				}

			}

			

			if (turretType == TurretType.cannon)
			{
				JointSpring tmp = hatch.GetComponent<HingeJoint>().spring;
				tmp.targetPosition = hatchTargetAngle;
				hatch.GetComponent<HingeJoint>().spring = tmp;

				if (Mathf.Abs(hatch.GetComponent<HingeJoint>().angle) > (Mathf.Abs(hatchOpenAngle) - hatchTolerance) && (Mathf.Abs(hatch.GetComponent<HingeJoint>().angle) < (Mathf.Abs(hatchOpenAngle) + hatchTolerance))) hatchOpen = true;
				else hatchOpen = false;
				//UnityEngine.Debug.Log(hatch.transform.localRotation.eulerAngles.y-180 + "  " + hatchTargetAngle);

				Vector3 targetPos = new Vector3(raisedPosition.x, raisedPosition.y, targetHeight);
				post.transform.localPosition = Vector3.Slerp(post.transform.localPosition, targetPos, Time.deltaTime * hatchSmoothing);
				if ((Mathf.Abs(post.transform.localPosition.z) > (Mathf.Abs(targetHeight) - heightTolerance)) && (Mathf.Abs(post.transform.localPosition.z) < (Mathf.Abs(targetHeight) + heightTolerance))) turretRaised = true;
				else turretRaised = false;


				if (turretState == TurretState.raisingHatch && hatchOpen == true)
				{
					turretState = TurretState.deploying;
					targetHeight = raisedPosition.z;
				}

				else if (turretState == TurretState.deploying)
				{
					if (turretRaised == true)
					{
						lastFireTime = 100000000;
						gameObject.GetComponent<Rigidbody>().isKinematic = false;
						turretState = TurretState.deployed;
						post.GetComponent<MeshCollider>().enabled = true;
						//GetComponent<MeshCollider>().enabled = true;
					}
				}

				else if (turretState == TurretState.deployed)
				{
					Vector3 cursorPos = GameObject.Find("SenmagWorkspace").GetComponent<Senmag_Workspace>().osenmagServer.deviceList[0].cursor.GetComponent<Senmag_HapticCursor>().currentPosition;
					Quaternion lookOnLook = Quaternion.LookRotation(cursorPos - transform.position);
					transform.rotation = Quaternion.Slerp(transform.rotation, lookOnLook, Time.deltaTime * turretSmoothing);


					if (transform.GetChild(0).GetComponent<grapple>().isGrappling == true)
					{
						lastGrappleState = true;
					}
					else
					{
						if(lastGrappleState == true)
						{
							lastGrappleState = false;
							lastFireTime = 0;
						}
						lastFireTime += 1;
					}


					if ((transform.position - cursorPos).magnitude < turretRange)
					{
						if (lastFireTime > (1 / Time.deltaTime) * reloadTime)
						{

							if (transform.GetChild(0).GetComponent<ParticleSystem>().isPlaying == false && transform.GetChild(0).GetComponent<grapple>().isGrappling == false)
							{
								transform.GetChild(0).GetComponent<ParticleSystem>().Play();
								transform.GetComponent<AudioSource>().Play();
								lastFireTime = 0;

							}
						}
						

					}
					else
					{

					}

				}
				else if (turretState == TurretState.disengage)
				{
					post.GetComponent<MeshCollider>().enabled = false;
					//GetComponent<MeshCollider>().enabled = false;
					transform.GetChild(0).GetComponent<grapple>().isGrappling = false;

					Quaternion lookOnLook = defaultRotation;
					transform.localRotation = Quaternion.Slerp(transform.localRotation, lookOnLook, Time.deltaTime * turretSmoothing);
					//UnityEngine.Debug.Log(Quaternion.Angle(transform.localRotation, defaultRotation));
					if(Quaternion.Angle(transform.localRotation, defaultRotation) <= 2f)
					{
						turretState = TurretState.retracting;
						targetHeight = raisedPosition.z + retractPosition;
						gameObject.GetComponent<Rigidbody>().isKinematic = true;
					}

				}
				else if(turretState == TurretState.retracting)
				{
					if(turretRaised == true)
					{
						closeHatch();
						turretState = TurretState.retracted;
						post.GetComponent<MeshRenderer>().enabled = false;
						//GetComponent<MeshRenderer>().enabled = false;
						
						
					}
				}

			}

			



			/*if (turretState == TurretState.deployed)
			{
				if (turretType == TurretType.cannon) transform.LookAt(GameObject.Find("cursor1").transform.GetChild(0).position);
				else if (turretType == TurretType.minigun) transform.LookAt(transform.position - (GameObject.Find("cursor1").transform.GetChild(0).position - transform.position));
			}
			else
			{

			}*/
		}

		

		public void deployTurret()
		{
			if (turretState == TurretState.retracted)
			{
				
				if(turretType == TurretType.minigun || (turretType == TurretType.missile && GetComponent<missile>().missileState == missile.MissileState.ready))
				{
					hatch.GetComponent<hatch>().setHatchTarget(hatchOpenAngle);
				}
				else
				{
					openHatch();
				}




				turretState = TurretState.raisingHatch;

				post.GetComponent<MeshRenderer>().enabled = true;
				if (turretType == TurretType.minigun)
				{
					GetComponent<MeshRenderer>().enabled = true;
				}
			}
		}

		public void setReloadSpeed(float speed, float projectileSpeed)
		{
			if (turretType == TurretType.minigun)
			{
				if (speed < 1) speed = 1;
				transform.GetChild(0).GetComponent<ParticleSystem>().startSpeed = projectileSpeed;

				var em = transform.GetChild(0).GetComponent<ParticleSystem>().emission;
				em.rateOverTime = speed;
				em = transform.GetChild(1).GetComponent<ParticleSystem>().emission;
				em.rateOverTime = speed;

				gunSoundFreq = speed;

				//transform.GetChild(1).GetComponent<ParticleSystem>().emission = em;
				//transform.GetChild(1).GetComponent<ParticleSystem>().emission = tmp;

			}
			if(turretType == TurretType.cannon)
			{
				UnityEngine.Debug.Log("grapple reload set to " + speed);
				reloadTime = speed;
				transform.GetChild(0).GetComponent<ParticleSystem>().startSpeed = projectileSpeed;
			}
		}

		public void retractTurret()
		{
			turretState = TurretState.disengage;
		}

		public void openHatch()
		{
			hatchTargetAngle = hatchOpenAngle;
		}

		public void closeHatch()
		{
			hatchTargetAngle = 0;
		}
	}
}

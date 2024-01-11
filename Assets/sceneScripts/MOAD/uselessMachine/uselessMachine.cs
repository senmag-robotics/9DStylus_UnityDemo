using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SenmagHaptic;

namespace uselessBox
{
	[System.Serializable]
	public class BoxLevel
	{
		public int switchTogglesToComplete = 1;
		public int levelDisplayNumber;
		public string levelDisplayString;
		public float retaliationDelay;

		[Header("Gun1 Settings")]
		public bool gun1_enabled;
		public float gun1_aimspeed;
		public float gun1_reloadSpeed;
		public float gun1_bulletSpeed;

		[Header("Gun2 Settings")]
		public bool gun2_enabled;
		public float gun2_aimspeed;
		public float gun2_reloadSpeed;
		public float gun2_bulletSpeed;

		[Header("Cannon1 Settings")]
		public bool cannon1_enabled;
		public float cannon1_aimspeed;
		public float cannon1_reloadSpeed;
		public float cannon1_bulletSpeed;

		[Header("Cannon2 Settings")]
		public bool cannon2_enabled;
		public float cannon2_aimspeed;
		public float cannon2_reloadSpeed;
		public float cannon2_bulletSpeed;

		[Header("Box Motion Settings")]
		public bool boxMotion_enabled;
		public float boxMotion_speed;
		public float boxMotion_xspeed;
		public float boxMotion_xdistance;
		public float boxMotion_yspeed;
		public float boxMotion_ydistance;

		[Header("Missile1 Settings")]
		public bool missile1_enabled;
		public float missile1_reloadSpeed;

		[Header("Missile2 Settings")]
		public bool missile2_enabled;
		public float missile2_reloadSpeed;
	}

	public class uselessMachine : MonoBehaviour
	{
		public GameObject lid;
		public GameObject chassis;
		public GameObject turret_frontL;
		public GameObject turret_frontR;
		public GameObject turret_rearL;
		public GameObject turret_rearR;
		public GameObject leg1;
		public GameObject leg2;
		public GameObject leg3;
		public GameObject leg4;
		public GameObject boxSwitch;
		public GameObject lever;
		public GameObject missile_rearL;
		public GameObject missile_rearR;

		public GameObject messageBoard;




		public Vector2 lidOpenClosePositions;
		public float lidSpeed = 0.2f;
		private Vector3 lidRotationAxis = new Vector3(0, 115.2f, 29.7f);
		private int lidState;

		public KeyCode key_openLid;
		public KeyCode key_closeLid;
		public KeyCode key_pressSwitch;

		public KeyCode key_deployRL;
		public KeyCode key_deployRR;
		public KeyCode key_deployFL;
		public KeyCode key_deployFR;

		public KeyCode key_deployleg1;
		public KeyCode key_deployleg2;
		public KeyCode key_deployleg3;
		public KeyCode key_deployleg4;

		public KeyCode key_walkLeft;
		public KeyCode key_walkForward;
		public KeyCode key_walkRight;
		public KeyCode key_walkBack;

		public KeyCode key_deployMissiles;
		public KeyCode key_launchMissiles;

		public ParticleSystem boxDestroyedSmoke;
		public ParticleSystem boxDestroyedExplosion;

		private float legPhase = 0;
		public float legSpeed = 4;
		public float boxSpeed = 0.0005f;

		

		public int currentLevelIndex;
		private int numSwitchClicks;
		private bool switchStates;
		private float switchElapsedTime;

		public float minTimeBetweenLevels = 5;
		private int timeSinceLastLevel;
		public List<BoxLevel> levelSettings = new List<BoxLevel>();

		private BoxLevel currentLevel;

		private Vector3 startPos;

		public bool boxDestroyed;

		private float boxMotion;

		private float lastMissileLaunch;
		private float minTimeBetweenMissiles = 2f;

		private bool forceLoad = true;

		// Start is called before the first frame update
		void Start() {
			boxDestroyedSmoke.enableEmission = false;
			boxDestroyedExplosion.enableEmission = false;
			currentLevelIndex = 0;
			currentLevel = levelSettings[currentLevelIndex];
			startPos = transform.position;
			switchStates = false;
			numSwitchClicks = 0;
			boxDestroyed = false;

			loadLevel(levelSettings[0]);
		}

		// Update is called once per frame
		void Update()
		{
			if (boxDestroyed)
			{
				boxDestroyedSmoke.enableEmission = true;
				return;
			}

			if (forceLoad)
			{
				forceLoad = false;
				//loadLevel(levelSettings[0]);
			}

			if(currentLevel.boxMotion_enabled == true)
			{
				if (leg1.GetComponent<leg>().legState == leg.LegState.deployed
					&& leg2.GetComponent<leg>().legState == leg.LegState.deployed
					&& leg3.GetComponent<leg>().legState == leg.LegState.deployed
					&& leg4.GetComponent<leg>().legState == leg.LegState.deployed)
				{
					float leftRight = 0;
					float forwardBack = 0;

					boxMotion += currentLevel.boxMotion_speed;
					if (boxMotion > 360) boxMotion -= 360;
					leftRight = Mathf.Sin(boxMotion * currentLevel.boxMotion_xspeed) * currentLevel.boxMotion_speed * currentLevel.boxMotion_xdistance;
					forwardBack = Mathf.Sin((boxMotion+90) * currentLevel.boxMotion_yspeed) * currentLevel.boxMotion_speed * currentLevel.boxMotion_ydistance;

					legPhase += legSpeed;
					if (legPhase > 360) legPhase -= 360;
					leg1.GetComponent<leg>().updateStep(-leftRight, -forwardBack, legPhase);
					leg2.GetComponent<leg>().updateStep(-leftRight, -forwardBack, legPhase + 180);
					leg3.GetComponent<leg>().updateStep(leftRight, forwardBack, legPhase + 90);
					leg4.GetComponent<leg>().updateStep(leftRight, forwardBack, legPhase + 270);

					Vector3 position = transform.localPosition;
					position.x -= leftRight * boxSpeed * legSpeed * 10;
					position.z += forwardBack * boxSpeed * legSpeed * 10;
					transform.localPosition = position;
				}
			}

			lastMissileLaunch += 1;
			if (currentLevel.missile2_enabled)
			{
				if (missile_rearL.GetComponent<missile>().missileState == missile.MissileState.ready)
				{
					if (missile_rearL.GetComponent<turret>().turretState == turret.TurretState.deployed || turret_rearL.GetComponent<turret>().turretState == turret.TurretState.deployed)
					{
						if (lastMissileLaunch > (1 / Time.deltaTime) * minTimeBetweenMissiles)
						{
							missile_rearL.GetComponent<missile>().launch();
							lastMissileLaunch = 0;
						}
					}
					else missile_rearL.GetComponent<turret>().deployTurret();
				}
				
			}
			else if(missile_rearL.GetComponent<turret>().turretState != turret.TurretState.retracted) missile_rearL.GetComponent<turret>().retractTurret();

			if (currentLevel.missile1_enabled)
			{
				if (missile_rearR.GetComponent<missile>().missileState == missile.MissileState.ready)
				{
					if (missile_rearR.GetComponent<turret>().turretState == turret.TurretState.deployed || turret_rearR.GetComponent<turret>().turretState == turret.TurretState.deployed)
					{
						if (lastMissileLaunch > (1 / Time.deltaTime) * minTimeBetweenMissiles)
						{
							missile_rearR.GetComponent<missile>().launch();
							lastMissileLaunch = 0;
						}
					}
					else missile_rearR.GetComponent<turret>().deployTurret();
				}
			}
			else if (missile_rearR.GetComponent<turret>().turretState != turret.TurretState.retracted) missile_rearR.GetComponent<turret>().retractTurret();

			checkBoxHeight();
			manualControls();

			timeSinceLastLevel += 1;
			if (boxSwitch.GetComponent<switchScript>().switchState == false)
			{
				if (switchStates == true)
				{
					switchStates = false;
					if (timeSinceLastLevel > (1 / Time.deltaTime) * minTimeBetweenLevels)
					{
						numSwitchClicks += 1;
					}
				}

				if (lid.GetComponent<lidScript>().lidState == lidScript.LidState.closed)
				{
					lid.GetComponent<lidScript>().pressSwitch();
				}
			}
			else switchStates = true;


			if(numSwitchClicks >= currentLevel.switchTogglesToComplete)
			{
				timeSinceLastLevel = 0;
				numSwitchClicks = 0;
				currentLevelIndex += 1;
				if (currentLevelIndex < levelSettings.Count)
				//if (currentLevelIndex < 0)
				{
					loadLevel(levelSettings[currentLevelIndex]);
				}
				else
				{
					destroyBox();
					UnityEngine.Debug.Log("play explosion");
					boxDestroyedExplosion.enableEmission = true;
					boxDestroyedExplosion.Play();
				}
			}
		}

		public void destroyBox()
		{
			
			boxDestroyed = true;

			

			Component[] hapticExplosions;
			hapticExplosions = GetComponents(typeof(Senmag_ThingyGoBoom));
			foreach (Senmag_ThingyGoBoom explosion in hapticExplosions) explosion.HapticBoom();

			transform.GetComponent<AudioSource>().Play();

			UnityEngine.Debug.Log("destroy box");
			List<GameObject> children = new List<GameObject>(0);
			GetDescendants(transform, children);
			foreach (GameObject obj in children)
			{
				if (obj.GetComponent<Rigidbody>())
				{
					obj.GetComponent<Rigidbody>().isKinematic = false;
					obj.GetComponent<Rigidbody>().useGravity = true;
					obj.GetComponent<Rigidbody>().drag = 0.1f;
					obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
					obj.GetComponent<Rigidbody>().AddForce(obj.transform.localPosition / 30, ForceMode.Impulse);
				}
				if (obj.GetComponent<HingeJoint>()) Destroy(obj.GetComponent<HingeJoint>());
				if (obj.GetComponent<ConfigurableJoint>()) Destroy(obj.GetComponent<ConfigurableJoint>());

				if (obj.GetComponent<MeshRenderer>()) obj.GetComponent<MeshRenderer>().enabled = true;

				if (obj.GetComponent<turret>()) Destroy(obj.GetComponent<turret>());

				if (obj.GetComponent<missile>()) Destroy(obj.GetComponent<missile>());

				if (obj.GetComponent<leg>()) Destroy(obj.GetComponent<leg>());

				if (obj.GetComponent<legTip>()) Destroy(obj.GetComponent<legTip>());

				if (obj.GetComponent<lidScript>()) Destroy(obj.GetComponent<lidScript>());

				if (obj.GetComponent<switchScript>()) Destroy(obj.GetComponent<switchScript>());

				if (obj.GetComponent<ParticleSystem>()) obj.GetComponent<ParticleSystem>().enableEmission = false;

			}

		}
		void OpenLid()
		{
			lidState = 1;
		}

		void CloseLid()
		{
			lidState = 2;
		}

		void checkBoxHeight()
		{
			if (boxDestroyed) return;
			if (leg1.GetComponent<leg>().tooLow == true)
			{
				Vector3 tmp = transform.position;
				tmp.y += 0.001f;
				transform.position = tmp;
			}
			else if (leg2.GetComponent<leg>().tooLow == true)
			{
				Vector3 tmp = transform.position;
				tmp.y += 0.001f;
				transform.position = tmp;
			}
			else if (leg3.GetComponent<leg>().tooLow == true)
			{
				Vector3 tmp = transform.position;
				tmp.y += 0.001f;
				transform.position = tmp;
			}
			else if (leg4.GetComponent<leg>().tooLow == true)
			{
				Vector3 tmp = transform.position;
				tmp.y += 0.001f;
				transform.position = tmp;
			}
			else
			{
				Vector3 tmp = transform.position;
				tmp.y -= 0.001f;
				//if(tmp.y > startPos.y) transform.position = tmp;
			}
		}

		public void GetDescendants(Transform parent, List<GameObject> list)
		{
			foreach (Transform child in parent)
			{
				list.Add(child.gameObject);
				GetDescendants(child, list);
			}
		}

		public void loadLevel(BoxLevel level)
		{
			timeSinceLastLevel = 0;
			currentLevel = level;
			UnityEngine.Debug.Log("level load");
			//string text = string.Format("{0}\r\n\r\nLevel: {1}", currentLevel.levelDisplayString, currentLevel.levelDisplayNumber);
			string text = string.Format("{0}", currentLevel.levelDisplayString);
			UnityEngine.Debug.Log(text);
			if(messageBoard != null) messageBoard.GetComponent<MessageBanner>().setText(text, true, 5);

			if (level.boxMotion_enabled == true)
			{
				if(leg1.GetComponent<leg>().legState != leg.LegState.deployed) leg1.GetComponent<leg>().deployLeg();
				if(leg2.GetComponent<leg>().legState != leg.LegState.deployed) leg2.GetComponent<leg>().deployLeg();
				if(leg3.GetComponent<leg>().legState != leg.LegState.deployed) leg3.GetComponent<leg>().deployLeg();
				if(leg4.GetComponent<leg>().legState != leg.LegState.deployed) leg4.GetComponent<leg>().deployLeg();
			}
			else
			{
				if (leg1.GetComponent<leg>().legState != leg.LegState.retracted) leg1.GetComponent<leg>().retractLeg();
				if (leg2.GetComponent<leg>().legState != leg.LegState.retracted) leg2.GetComponent<leg>().retractLeg();
				if (leg3.GetComponent<leg>().legState != leg.LegState.retracted) leg3.GetComponent<leg>().retractLeg();
				if (leg4.GetComponent<leg>().legState != leg.LegState.retracted) leg4.GetComponent<leg>().retractLeg();
			}

			if(level.gun1_enabled == true)
			{
				if (turret_rearR.GetComponent<turret>().turretState != turret.TurretState.deployed) turret_rearR.GetComponent<turret>().deployTurret();
				turret_rearR.GetComponent<turret>().turretSmoothing = level.gun1_aimspeed;
				turret_rearR.GetComponent<turret>().setReloadSpeed(level.gun1_reloadSpeed, level.gun1_bulletSpeed);
			}
			else if (turret_rearR.GetComponent<turret>().turretState != turret.TurretState.retracted) turret_rearR.GetComponent<turret>().retractTurret();

			if (level.gun2_enabled == true)
			{
				if (turret_rearL.GetComponent<turret>().turretState != turret.TurretState.deployed) turret_rearL.GetComponent<turret>().deployTurret();
				turret_rearL.GetComponent<turret>().turretSmoothing = level.gun2_aimspeed;
				turret_rearL.GetComponent<turret>().setReloadSpeed(level.gun2_reloadSpeed, level.gun2_bulletSpeed);
			}
			else if (turret_rearL.GetComponent<turret>().turretState != turret.TurretState.retracted) turret_rearL.GetComponent<turret>().retractTurret();

			if (level.cannon1_enabled == true)
			{
				if (turret_frontR.GetComponent<turret>().turretState != turret.TurretState.deployed) turret_frontR.GetComponent<turret>().deployTurret();
				turret_frontR.GetComponent<turret>().turretSmoothing = level.cannon1_aimspeed;
				turret_frontR.GetComponent<turret>().setReloadSpeed(level.cannon1_reloadSpeed, level.cannon1_bulletSpeed);
			}
			else if (turret_frontR.GetComponent<turret>().turretState != turret.TurretState.retracted) turret_frontR.GetComponent<turret>().retractTurret();

			if (level.cannon2_enabled == true)
			{
				if (turret_frontL.GetComponent<turret>().turretState != turret.TurretState.deployed) turret_frontL.GetComponent<turret>().deployTurret();
				turret_frontL.GetComponent<turret>().turretSmoothing = level.cannon2_aimspeed;
				turret_frontL.GetComponent<turret>().setReloadSpeed(level.cannon2_reloadSpeed, level.cannon2_bulletSpeed);
			}
			else if (turret_frontL.GetComponent<turret>().turretState != turret.TurretState.retracted) turret_frontL.GetComponent<turret>().retractTurret();

			if (level.missile1_enabled == true)
			{
				if (missile_rearR.GetComponent<turret>().turretState != turret.TurretState.deployed) missile_rearR.GetComponent<turret>().deployTurret();
			}
			else if (missile_rearR.GetComponent<turret>().turretState != turret.TurretState.retracted) missile_rearR.GetComponent<turret>().retractTurret();

			if (level.missile2_enabled == true)
			{
				if (missile_rearL.GetComponent<turret>().turretState != turret.TurretState.deployed) missile_rearL.GetComponent<turret>().deployTurret();
			}
			else if (missile_rearL.GetComponent<turret>().turretState != turret.TurretState.retracted) turret_frontR.GetComponent<turret>().retractTurret();

			

		}

		public void manualControls()
		{
			float leftRight = 0;
			float forwardBack = 0;
			if (Input.GetKey(key_walkLeft)) leftRight -= 1;
			if (Input.GetKey(key_walkRight)) leftRight += 1;
			if (Input.GetKey(key_walkForward)) forwardBack -= 1;
			if (Input.GetKey(key_walkBack)) forwardBack += 1;
			if (leftRight != 0 || forwardBack != 0)
			{
				legPhase += legSpeed;

				if (legPhase > 360) legPhase -= 360;
				leg1.GetComponent<leg>().updateStep(-leftRight, -forwardBack, legPhase);
				leg2.GetComponent<leg>().updateStep(-leftRight, -forwardBack, legPhase + 180);
				leg3.GetComponent<leg>().updateStep(leftRight, forwardBack, legPhase + 90);
				leg4.GetComponent<leg>().updateStep(leftRight, forwardBack, legPhase + 270);

				Vector3 position = transform.position;
				position.x -= leftRight * boxSpeed * legSpeed;
				position.z += forwardBack * boxSpeed * legSpeed;
				transform.position = position;
			}
			checkBoxHeight();

			if (Input.GetKey(key_deployleg1)) leg1.GetComponent<leg>().deployLeg();
			if (Input.GetKey(key_deployleg2)) leg2.GetComponent<leg>().deployLeg();
			if (Input.GetKey(key_deployleg3)) leg3.GetComponent<leg>().deployLeg();
			if (Input.GetKey(key_deployleg4)) leg4.GetComponent<leg>().deployLeg();




			if (Input.GetKeyDown(key_deployMissiles))
			{
				if (missile_rearL.GetComponent<turret>().turretState == turret.TurretState.retracted)
				{
					missile_rearL.GetComponent<turret>().deployTurret();
					missile_rearR.GetComponent<turret>().deployTurret();
				}
				else
				{
					missile_rearL.GetComponent<turret>().retractTurret();
					missile_rearR.GetComponent<turret>().retractTurret();
				}
			}

			if (Input.GetKeyDown(key_launchMissiles))
			{
				missile_rearL.GetComponent<missile>().launch();
				missile_rearR.GetComponent<missile>().launch();
			}

			if (Input.GetKey(key_openLid))
			{
				lid.GetComponent<lidScript>().openLid();
				turret_rearL.GetComponent<turret>().openHatch();
				turret_rearR.GetComponent<turret>().openHatch();
			}
			if (Input.GetKey(key_closeLid))
			{
				lid.GetComponent<lidScript>().closeLid();
				turret_rearL.GetComponent<turret>().retractTurret();
				turret_rearR.GetComponent<turret>().retractTurret();
				turret_frontL.GetComponent<turret>().retractTurret();
				turret_frontR.GetComponent<turret>().retractTurret();
				leg1.GetComponent<leg>().retractLeg();
				leg2.GetComponent<leg>().retractLeg();
				leg3.GetComponent<leg>().retractLeg();
				leg4.GetComponent<leg>().retractLeg();
			}
			if (Input.GetKey(key_deployRL)) turret_rearL.GetComponent<turret>().deployTurret();

			if (Input.GetKey(key_deployRR)) turret_rearR.GetComponent<turret>().deployTurret();

			if (Input.GetKey(key_deployFL)) turret_frontL.GetComponent<turret>().deployTurret();

			if (Input.GetKey(key_deployFR)) turret_frontR.GetComponent<turret>().deployTurret();


			if (Input.GetKey(key_pressSwitch)) lid.GetComponent<lidScript>().pressSwitch();
		}
	}
}
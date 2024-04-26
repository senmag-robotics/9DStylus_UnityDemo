using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SenmagHaptic {
	public class TelerobotDemo : MonoBehaviour
	{
		bool keyState;
		bool gripperKeyState;
		bool gripperState = false;

		bool lockState = false;

		public GameObject senmagWorkspace;

        public Stylus_Action robotAttachButton = Stylus_Action.none;
        public Stylus_Action robotGripperButton = Stylus_Action.none;

		bool robotAttachButtonLatch = false;
        bool robotGripperButtonLatch = false;
	

        //public KeyCode robotGripKey;
        //public KeyCode robotGripperKey;
        public KeyCode sceneResetKey;

        public GameObject robotTip;
		public GameObject robotParameterController;

		public GameObject Button_GridSizeLarge;
		public GameObject Button_GridSizeMed;
		public GameObject Button_GridSizeSmall;

		public GameObject Button_GridStrengthHigh;
		public GameObject Button_GridStrengthMed;
		public GameObject Button_GridStrengthLow;

		public GameObject Button_SpatialMultiplierHigh;
		public GameObject Button_SpatialMultiplierMed;
		public GameObject Button_SpatialMultiplierLow;


		public GameObject Button_ResetObjects;
		public List<GameObject> ResetableSceneObjects;
		private List<Vector3> SceneObjectStartPositions = new List<Vector3>();
		private List<Quaternion> SceneObjectStartRotations = new List<Quaternion>();


		public Vector3 GridScales = new Vector3(0, 0, 0);
		public Vector3 GridStrengths = new Vector3(0, 0, 0);
		public Vector3 SpatialMultipliers = new Vector3(10, 5, 1);

		private Senmag_hapticGrid hapticGrid;
		private int spatialMultiplyerSetting = 0;
		private float globalSpatialMultiplyerSetting;
		private int gridStrengthSetting = 2;

		private bool cursorTableCollisionsEnabled = false;
		private float cursorTableHeightThreshold = 0.05f;

        Quaternion rotationOffset;

		Senmag_hapticGridSettings gridSettings = new Senmag_hapticGridSettings();

		// Start is called before the first frame update
		void Start()
		{
			robotTip.GetComponent<Rigidbody>().isKinematic = true;
			hapticGrid = transform.gameObject.AddComponent<Senmag_hapticGrid>();

			gridSettings.gain = GridStrengths[2];
			gridSettings.spacing = GridScales[2];
			gridSettings.maxForce = GridStrengths[2] / 10f;
			gridSettings.forceExponment = 3;
			gridSettings.offset = new Vector3(0, 0, 0);
			//gridSettings.maxForce = 1;
			//hapticGrid.enableGrid(gridSettings);
			Button_GridSizeSmall.GetComponent<Senmag_button>().isHighlighted = true;
			Button_GridStrengthLow.GetComponent<Senmag_button>().isHighlighted = true;
			Button_SpatialMultiplierHigh.GetComponent<Senmag_button>().isHighlighted = true;


			for(int x = 0; x < ResetableSceneObjects.Count; x++)
			{
				SceneObjectStartPositions.Add(ResetableSceneObjects[x].transform.position);
				SceneObjectStartRotations.Add(ResetableSceneObjects[x].transform.rotation);
			}
			UnityEngine.Debug.Log("tracking " + ResetableSceneObjects.Count + " objects...");
		}

		void resetSceneObjects()
		{
			for (int x = 0; x < ResetableSceneObjects.Count; x++)
			{
				ResetableSceneObjects[x].transform.position = SceneObjectStartPositions[x];
				ResetableSceneObjects[x].transform.rotation = SceneObjectStartRotations[x];
				ResetableSceneObjects[x].GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
				ResetableSceneObjects[x].GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
			}
		}

		// Update is called once per frame
		void Update()
		{

			/*if(GameObject.Find("cursor1").transform.GetChild(1).transform.position.y < cursorTableHeightThreshold && cursorTableCollisionsEnabled == true)
			{
				cursorTableCollisionsEnabled = false;
				GameObject.Find("cursor1").transform.GetChild(1).GetComponent<SphereCollider>().enabled = false;
				GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().setSafeStart();
			}
			if (GameObject.Find("cursor1").transform.GetChild(1).transform.position.y > cursorTableHeightThreshold && cursorTableCollisionsEnabled == false)
			{
				cursorTableCollisionsEnabled = true;
				GameObject.Find("cursor1").transform.GetChild(1).GetComponent<SphereCollider>().enabled = true;
				GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().setSafeStart();
				//Physics.Collis (GameObject.Find("cursor1").transform.GetChild(1).GetComponent<SphereCollider>(), TableObject.GetComponent<BoxCollider>());
			}*/

			/*if (Button_ResetObjects.GetComponent<Senmag_button>().wasClicked())
			{
				resetSceneObjects();
			}*/

			/*if (Input.GetKey(robotGripKey) && keyState == false)
			{
				keyState = true;
				UnityEngine.Debug.Log("robot attached");
				GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().attachCursorToObject(robotTip, true);
				robotTip.GetComponent<Rigidbody>().isKinematic = false;
			}
			if (!Input.GetKey(robotGripKey) && keyState == true)
			{
				keyState = false;
				UnityEngine.Debug.Log("robot released");
				robotTip.GetComponent<Rigidbody>().isKinematic = true;
				GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().releaseObject();
			}*/


			//public Stylus_Action robotAttachButton = Stylus_Action.none;
			//public Stylus_Action robotGripperButton = Stylus_Action.none;

			//bool robotAttachButtonLatch = false;
			//bool robotGripperButtonLatch = false;






            if (Input.GetKey(sceneResetKey)) { 
				Application.LoadLevel(Application.loadedLevel);
			}

            //if (senmagWorkspace.GetComponentInChildren<Senmag_stylusControl>() != null)
			//{
			//	UnityEngine.Debug.Log("found stylusControl");
			//}

                //if (Input.GetKey(robotGripKey))
			if (senmagWorkspace.GetComponentInChildren<Senmag_stylusControl>().Input_isHeld(robotAttachButton))
            {
				if (keyState == false)
				{
					if (lockState == false) {
						lockState = true;
						keyState = true;

						rotationOffset = senmagWorkspace.GetComponentInChildren<Senmag_HapticCursor>().cursorTarget.transform.rotation * Quaternion.Inverse(robotTip.transform.rotation);

                        UnityEngine.Debug.Log("robot attached");
                        //globalSpatialMultiplyerSetting = GameObject.Find("Workspace").GetComponent<Senmag_Workspace>().spatialMultiplier;

                        //hapticGrid.enableGrid(gridSettings);

                        //senmagWorkspace.GetComponentInChildren<Senmag_HapticCursor>().gameObject.

                        //GameObject.Find("Workspace").GetComponent<Senmag_Workspace>().spatialMultiplier = SpatialMultipliers[spatialMultiplyerSetting];


                        //GameObject.Find("cursor1").transform.GetChild(0).transform.position /= (globalSpatialMultiplyerSetting / SpatialMultipliers[spatialMultiplyerSetting]);
                        //GameObject.Find("cursor1").transform.GetChild(0).transform.position /= (globalSpatialMultiplyerSetting / SpatialMultipliers[spatialMultiplyerSetting]);




                        //GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().attachCursorToObject(robotTip, true);

                        senmagWorkspace.GetComponentInChildren<Senmag_HapticCursor>().setCursorOffset(robotTip.transform.position - senmagWorkspace.GetComponentInChildren<Senmag_HapticCursor>().cursorTarget.transform.position);

                        senmagWorkspace.GetComponentInChildren<Senmag_HapticCursor>().linkObjectToPosition(robotTip, true);
						robotTip.GetComponent<Rigidbody>().isKinematic = false;
					}
					else
					{
						lockState = false;
						keyState = true;
						UnityEngine.Debug.Log("robot released");

                        senmagWorkspace.GetComponentInChildren<Senmag_HapticCursor>().setCursorOffset(new Vector3(0, 0, 0));

                        robotTip.GetComponent<Rigidbody>().isKinematic = true;
                        senmagWorkspace.GetComponentInChildren<Senmag_HapticCursor>().dropObject(robotTip);
						//GameObject.Find("Workspace").GetComponent<Senmag_Workspace>().spatialMultiplier = globalSpatialMultiplyerSetting;
					}
				}
			}
			else keyState = false;





            //if (Input.GetKey(robotGripperKey))
            if (senmagWorkspace.GetComponentInChildren<Senmag_stylusControl>().Input_isHeld(robotGripperButton))
            {
				if (gripperKeyState == false)
				{
					if (gripperState == false)
					{
						gripperState = true;
						gripperKeyState = true;


						robotParameterController.GetComponent<telerobotParamControl>().closeGripper();

					}
					else
					{
						gripperState = false;
						gripperKeyState = true;

						robotParameterController.GetComponent<telerobotParamControl>().openGripper();

					}
				}
			}
			else gripperKeyState = false;

/*
			if (Button_SpatialMultiplierHigh.GetComponent<Senmag_button>().wasClicked())
			{
				spatialMultiplyerSetting = 0;
				Button_SpatialMultiplierHigh.GetComponent<Senmag_button>().isHighlighted = true;
				Button_SpatialMultiplierMed.GetComponent<Senmag_button>().isHighlighted = false;
				Button_SpatialMultiplierLow.GetComponent<Senmag_button>().isHighlighted = false;
			}
			if (Button_SpatialMultiplierMed.GetComponent<Senmag_button>().wasClicked())
			{
				spatialMultiplyerSetting = 1;
				Button_SpatialMultiplierHigh.GetComponent<Senmag_button>().isHighlighted = false;
				Button_SpatialMultiplierMed.GetComponent<Senmag_button>().isHighlighted = true;
				Button_SpatialMultiplierLow.GetComponent<Senmag_button>().isHighlighted = false;
			}
			if (Button_SpatialMultiplierLow.GetComponent<Senmag_button>().wasClicked())
			{
				spatialMultiplyerSetting = 2;
				Button_SpatialMultiplierHigh.GetComponent<Senmag_button>().isHighlighted = false;
				Button_SpatialMultiplierMed.GetComponent<Senmag_button>().isHighlighted = false;
				Button_SpatialMultiplierLow.GetComponent<Senmag_button>().isHighlighted = true;
			}

			if (Button_GridSizeLarge.GetComponent<Senmag_button>().wasClicked())
			{
				gridSettings.spacing = GridScales[0];
				hapticGrid.enableGrid(gridSettings);
				Button_GridSizeLarge.GetComponent<Senmag_button>().isHighlighted = true;
				Button_GridSizeMed.GetComponent<Senmag_button>().isHighlighted = false;
				Button_GridSizeSmall.GetComponent<Senmag_button>().isHighlighted = false;
			}
			if (Button_GridSizeMed.GetComponent<Senmag_button>().wasClicked())
			{
				gridSettings.spacing = GridScales[1];
				hapticGrid.enableGrid(gridSettings);
				Button_GridSizeLarge.GetComponent<Senmag_button>().isHighlighted = false;
				Button_GridSizeMed.GetComponent<Senmag_button>().isHighlighted = true;
				Button_GridSizeSmall.GetComponent<Senmag_button>().isHighlighted = false;
			}
			if (Button_GridSizeSmall.GetComponent<Senmag_button>().wasClicked())
			{
				gridSettings.spacing = GridScales[2];
				hapticGrid.enableGrid(gridSettings);
				Button_GridSizeLarge.GetComponent<Senmag_button>().isHighlighted = false;
				Button_GridSizeMed.GetComponent<Senmag_button>().isHighlighted = false;
				Button_GridSizeSmall.GetComponent<Senmag_button>().isHighlighted = true;
			}


			if (Button_GridStrengthHigh.GetComponent<Senmag_button>().wasClicked())
			{
				gridSettings.gain = GridStrengths[0];
				gridSettings.maxForce = GridStrengths[0] * 10f;
				hapticGrid.enableGrid(gridSettings);
				Button_GridStrengthHigh.GetComponent<Senmag_button>().isHighlighted = true;
				Button_GridStrengthMed.GetComponent<Senmag_button>().isHighlighted = false;
				Button_GridStrengthLow.GetComponent<Senmag_button>().isHighlighted = false;
			}
			if (Button_GridStrengthMed.GetComponent<Senmag_button>().wasClicked())
			{
				gridSettings.gain = GridStrengths[1];
				gridSettings.maxForce = GridStrengths[1] * 10f;
				hapticGrid.enableGrid(gridSettings);
				Button_GridStrengthHigh.GetComponent<Senmag_button>().isHighlighted = false;
				Button_GridStrengthMed.GetComponent<Senmag_button>().isHighlighted = true;
				Button_GridStrengthLow.GetComponent<Senmag_button>().isHighlighted = false;
			}
			if (Button_GridStrengthLow.GetComponent<Senmag_button>().wasClicked())
			{
				gridSettings.gain = GridStrengths[2];
				gridSettings.maxForce = GridStrengths[2] * 10f;
				hapticGrid.enableGrid(gridSettings);
				Button_GridStrengthHigh.GetComponent<Senmag_button>().isHighlighted = false;
				Button_GridStrengthMed.GetComponent<Senmag_button>().isHighlighted = false;
				Button_GridStrengthLow.GetComponent<Senmag_button>().isHighlighted = true;
			}*/
		}
		void FixedUpdate()
		{
            if(lockState == true)
			{
				//UnityEngine
				//robotTip.transform.rotation = senmagWorkspace.GetComponentInChildren<Senmag_HapticCursor>().cursorTarget.transform.rotation * rotationOffset;
            }
        }
		void generateGrid()
		{

		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace SenmagHaptic
{
	public class Senmag_button : MonoBehaviour
	{
		private bool isHidden = false;
		public bool isEnabled = true;
		private bool ispressed = false;
		private bool isReleased = false;
		private bool isClicked = false;
		private GameObject buttonFace;
		private GameObject buttonBase;
		private GameObject buttonSides;

		private Canvas FaceCanvas;
		private GameObject FaceBackground;
		private GameObject FaceText;

		private ConfigurableJoint spring;
		JointDrive springDrive;
		private GameObject forcefield;
		private float faceThickness = 0.1f;
		private AudioSource audioSource;


		//[HideInInspector]
		public bool isHighlighted;
		private bool highlightStatus = false;

		public bool isFlashing;
		private float flashStatus = 0f;
		public float flashRate = 0.02f;
		public float flashIntensity = 0.1f;


		[Header("Button Size")]
		[Space(20)]
		public Vector3 Scale;
		public Material BaseMaterial;
		public Color BaseColor;

		[Header("Button Sounds")]
		[Space(20)]
		public AudioClip clickSound;
		public AudioClip releaseSound;
		public float volume;

		[Header("Face Prefab")]
		[Space(20)]
		public GameObject FaceCanvasPrefab;

		[Header("Text Options")]
		[Space(20)]

		public TMP_FontAsset Font;
		public FontStyles Style;
		public float FontScale;
		[MultilineAttribute(2)]

		public string ButtonText;
		public Color TextColor;
		public Color BorderColor;
		public float BorderThickness;
		public Color BackgroundColor;
		public Color BackgroundColorHighlighted;
		public int BackgroundResolution = 500;




		[Header("Button Spring")]
		[Space(20)]
		public float SpringStrength;
		public float ClickStrength;


		[Header("Target Assistance")]
		[Space(20)]
		public float TargetAssistStrength;
		public Vector3 TargetAssistRange;
		//[Space(10)]



		// Start is called before the first frame update
		void Start()
		{
			if (clickSound || releaseSound)
			{
				audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.playOnAwake = false;
				audioSource.volume = volume;
			}

			//UnityEngine.Debug.Log(TextColor);
			buttonFace = GameObject.CreatePrimitive(PrimitiveType.Cube);
			buttonBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
			forcefield = GameObject.CreatePrimitive(PrimitiveType.Cube);
			buttonSides = GameObject.CreatePrimitive(PrimitiveType.Cube);

			if (FaceCanvasPrefab == null)
			{
				FaceCanvasPrefab = new GameObject();
				FaceCanvas = FaceCanvasPrefab.AddComponent<Canvas>();

				buttonFace.AddComponent<Canvas>();
				FaceCanvas.transform.parent = buttonFace.transform;
				FaceCanvas.transform.localPosition = new Vector3(0, 0, 0);
				FaceCanvas.transform.rotation = new Quaternion(0, 0, 0, 0);
				FaceCanvas.transform.localScale = new Vector3(1, 1, 1);


				FaceBackground = new GameObject();

				FaceBackground.AddComponent<Image>();

				FaceBackground.transform.parent = FaceCanvas.transform;
				FaceBackground.name = "Background";
				FaceBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
				FaceBackground.GetComponent<Image>().type = Image.Type.Sliced;
				FaceBackground.GetComponent<Image>().pixelsPerUnitMultiplier = BackgroundResolution;
				FaceBackground.GetComponent<Image>().sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
				FaceBackground.GetComponent<Image>().color = BackgroundColor;

				FaceText = new GameObject();
				FaceText.name = "Text";
				FaceText.transform.localScale = new Vector3(1, Scale.x / Scale.y, 1);
				FaceText.transform.parent = FaceCanvas.transform;
				FaceText.AddComponent<TextMeshProUGUI>();

				FaceText.GetComponent<TextMeshProUGUI>().text = ButtonText;
				FaceText.GetComponent<TextMeshProUGUI>().fontSize = FontScale;
				FaceText.GetComponent<TextMeshProUGUI>().color = TextColor;
				FaceText.GetComponent<TextMeshProUGUI>().font = Resources.Load("Fonts/ARIAL", typeof(TMP_FontAsset)) as TMP_FontAsset;
				FaceText.GetComponent<TextMeshProUGUI>().font = Font;
				FaceText.GetComponent<TextMeshProUGUI>().fontStyle = Style;
				FaceText.GetComponent<TextMeshProUGUI>().outlineWidth = BorderThickness;
				FaceText.GetComponent<TextMeshProUGUI>().outlineColor = BorderColor;
				FaceText.GetComponent<TextMeshProUGUI>().horizontalAlignment = HorizontalAlignmentOptions.Center;
				FaceText.GetComponent<TextMeshProUGUI>().verticalAlignment = VerticalAlignmentOptions.Middle;
				FaceBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
				FaceBackground.GetComponent<RectTransform>().localScale = new Vector2(1, 1);
			}
			else
			{
				FaceCanvasPrefab = Instantiate(FaceCanvasPrefab);
				FaceBackground = FaceCanvasPrefab.transform.GetChild(0).gameObject;
				BackgroundColor = FaceBackground.GetComponent<Image>().color;
			}
			FaceCanvasPrefab.name = "FaceCanvas";
			FaceCanvas = FaceCanvasPrefab.GetComponent<Canvas>();
			FaceCanvasPrefab.transform.parent = buttonFace.transform;
			FaceCanvasPrefab.transform.rotation = new Quaternion(0, 0, 0, 0);
			FaceCanvasPrefab.transform.localPosition = new Vector3(0, 0, -.5f);

			buttonBase.name = "Base";
			buttonFace.name = "Face";
			forcefield.name = "Forcefield";
			buttonSides.name = "sides";

			//set parent objects
			buttonBase.transform.parent = transform;
			buttonFace.transform.parent = transform;
			forcefield.transform.parent = transform;
			buttonSides.transform.parent = buttonFace.transform;

			buttonSides.transform.localScale = new Vector3(1, 1, 5);
			buttonSides.transform.localPosition = new Vector3(0, 0, 2.5f);
			buttonSides.GetComponent<MeshRenderer>().enabled = false;

			//buttonSides.GetComponent<Rigidbody>().isKinematic = true;
			Physics.IgnoreCollision(buttonSides.GetComponent<BoxCollider>(), buttonFace.GetComponent<BoxCollider>());
			Physics.IgnoreCollision(buttonSides.GetComponent<BoxCollider>(), buttonBase.GetComponent<BoxCollider>());

			if (BaseMaterial != null) buttonBase.GetComponent<Renderer>().material = BaseMaterial;
			else buttonBase.GetComponent<Renderer>().material.color = BaseColor;

			//set rotations
			buttonFace.transform.rotation = new Quaternion(0, 0, 0, 0);
			buttonBase.transform.rotation = new Quaternion(0, 0, 0, 0);
			forcefield.transform.rotation = new Quaternion(0, 0, 0, 0);

			//set initial positions
			buttonBase.transform.localPosition = new Vector3(0, 0, 0);
			buttonFace.transform.localPosition = new Vector3(0, 0, -Scale.z / 2f);
			forcefield.transform.localPosition = new Vector3(0, 0, -(TargetAssistRange.z / 2f) * (Scale.z / 2f) - (Scale.z * faceThickness / 2f));

			//set scales
			buttonBase.transform.localScale = new Vector3(Scale.x, Scale.y, Scale.z * faceThickness);
			buttonFace.transform.localScale = new Vector3(Scale.x, Scale.y, Scale.z * faceThickness);
			forcefield.transform.localScale = new Vector3(Scale.x * TargetAssistRange.x, Scale.y * TargetAssistRange.y, Scale.z * TargetAssistRange.z / 2f);
			forcefield.AddComponent<Senmag_button_forcefield>();
			forcefield.GetComponent<Senmag_button_forcefield>().strength = TargetAssistStrength;
			//configure properties
			buttonBase.AddComponent<Rigidbody>();
			buttonBase.GetComponent<Rigidbody>().isKinematic = true;

			buttonFace.AddComponent<Rigidbody>();
			buttonFace.GetComponent<Rigidbody>().isKinematic = false;
			buttonFace.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
			buttonFace.GetComponent<Rigidbody>().useGravity = false;
			buttonFace.GetComponent<MeshRenderer>().enabled = false;


			forcefield.AddComponent<Rigidbody>();
			forcefield.GetComponent<Rigidbody>().isKinematic = true;
			forcefield.GetComponent<BoxCollider>().isTrigger = true;
			forcefield.GetComponent<MeshRenderer>().enabled = false;


			spring = buttonBase.AddComponent<ConfigurableJoint>();
			spring.connectedBody = buttonFace.GetComponent<Rigidbody>();
			spring.xMotion = ConfigurableJointMotion.Locked;
			spring.yMotion = ConfigurableJointMotion.Locked;
			spring.zMotion = ConfigurableJointMotion.Free;
			spring.anchor = new Vector3(0, 0, faceThickness + (-1f / faceThickness) / 2f);
			spring.autoConfigureConnectedAnchor = false;
			spring.connectedAnchor = new Vector3(0, 0, 0);
			spring.enableCollision = true;

			springDrive = new JointDrive();
			springDrive.positionSpring = 100000f;
			springDrive.maximumForce = ClickStrength;
			springDrive.positionDamper = 150;
			spring.zDrive = springDrive;
			spring.autoConfigureConnectedAnchor = true;

			buttonBase.transform.localScale = new Vector3(Scale.x, Scale.y, Scale.z * faceThickness * 2);
		}

		// Update is called once per frame
		void Update()
		{
			if (isFlashing == true)
			{
				flashStatus += flashRate;
				if (flashStatus > 360) flashStatus -= 360;
				Color tmpColor;
				if (isHighlighted) tmpColor = BackgroundColorHighlighted;
				else tmpColor = BackgroundColor;


				tmpColor.r += Mathf.Sin(flashStatus) * flashIntensity;
				tmpColor.g += Mathf.Sin(flashStatus) * flashIntensity;
				tmpColor.b += Mathf.Sin(flashStatus) * flashIntensity;
				FaceBackground.GetComponent<Image>().color = tmpColor;
			}
			else
			{
				if (isHighlighted) FaceBackground.GetComponent<Image>().color = BackgroundColorHighlighted;
				else FaceBackground.GetComponent<Image>().color = BackgroundColor;
			}
		}
		void FixedUpdate()
		{
			if (isEnabled == true && isHidden == false)
			{
				float position = buttonFace.transform.localPosition.z / (-Scale.z / 2f);
				float sideHeight = 5 * position;
				if (sideHeight > 4.8f) sideHeight = 4.8f;
				buttonSides.transform.localScale = new Vector3(1, 1, sideHeight);
				buttonSides.transform.localPosition = new Vector3(0, 0, 2.5f * position);

				//UnityEngine.Debug.Log(position);
				if (isClicked == false && position < 0.8f)
				{
					if (clickSound)
					{
						audioSource.clip = clickSound;
						audioSource.Play();
					}

					springDrive.maximumForce = SpringStrength;
					spring.zDrive = springDrive;
					isClicked = true;
				}
				if (position < 0.35f)
				{
					ispressed = true;
				}
				if (position < 0.1f || position > 2f)
				{
					UnityEngine.Debug.Log("resetting face");
					Vector3 tmp = buttonFace.transform.localPosition;
					tmp.x = 0;
					tmp.y = 0;
					tmp.z = -0.05f;
					buttonFace.transform.localPosition = tmp;
					spring.anchor = new Vector3(0, 0, faceThickness + (-1f / faceThickness) / 2f);
					isReleased = false;
				}
				if (position > 0.85f)
				{
					if (ispressed == true)
					{
						ispressed = false;
						isReleased = true;
						//isHighlighted = !isHighlighted;
					}
					if (isClicked == true)
					{
						springDrive.maximumForce = ClickStrength;
						spring.zDrive = springDrive;
						isClicked = false;
						if (releaseSound)
						{
							audioSource.clip = releaseSound;
							audioSource.Play();
						}
					}
				}
			}
		}

		public void resetButton()
		{
			Vector3 tmp;
			tmp.x = 0;
			tmp.y = 0;
			tmp.z = -0.05f;
			buttonFace.transform.localPosition = tmp;
			spring.anchor = new Vector3(0, 0, faceThickness + (-1f / faceThickness) / 2f);
			isReleased = false;
		}

		public bool wasClicked()
		{
			bool result = isReleased;
			isReleased = false;
			return result;
		}

		public void hide()
		{
			isHidden = true;
			buttonFace.GetComponent<MeshRenderer>().enabled = false;
			buttonBase.GetComponent<MeshRenderer>().enabled = false;
			FaceCanvas.GetComponent<Canvas>().enabled = false;
			disable();
		}
		public void show()
		{
			isHidden = false;
			//buttonFace.GetComponent<MeshRenderer>().enabled = true;
			buttonBase.GetComponent<MeshRenderer>().enabled = true;
			FaceCanvas.GetComponent<Canvas>().enabled = true;
			enable();
		}
		public void disable()
		{
			isEnabled = false;
			buttonFace.GetComponent<BoxCollider>().enabled = false;
			buttonBase.GetComponent<BoxCollider>().enabled = false;
			forcefield.GetComponent<BoxCollider>().enabled = false;
			buttonSides.GetComponent<BoxCollider>().enabled = false;
		}
		public void enable()
		{
			isEnabled = true;
			buttonFace.GetComponent<BoxCollider>().enabled = true;
			buttonBase.GetComponent<BoxCollider>().enabled = true;
			forcefield.GetComponent<BoxCollider>().enabled = true;
			buttonSides.GetComponent<BoxCollider>().enabled = true;
			Physics.IgnoreCollision(buttonSides.GetComponent<BoxCollider>(), buttonFace.GetComponent<BoxCollider>());
			Physics.IgnoreCollision(buttonSides.GetComponent<BoxCollider>(), buttonBase.GetComponent<BoxCollider>());
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Drawing;
using UnityEngine.UI;
using TMPro;

using SenmagHaptic;
using System;
using System.Collections.Specialized;
using System.Diagnostics;

[System.Serializable]
public class Senmag_RadialMenuLabel
{
	public Sprite icon;
	public string text;
	public GameObject objectToInstantiateOnSelect;
}

public class Senmag_radialMenu : MonoBehaviour
{
	// Start is called before the first frame update
	public bool pemanant = false;

	[Header("Audio")]
	public AudioClip clickSound;
	public AudioClip releaseSound;
	public float volume = 1;

	[Header("Haptic click settings")]
	public float SpringStrength;
	public float ClickStrength;

	[Header("Haptic hover settings")]
	public float forceGain = 15f;
	public float angleForceGain = .04f;

	[Header("Animation settings")]
	public float animationSpeed = 0.01f;

	[Header("Menu items")]
	public List<Senmag_RadialMenuLabel> segmentLabels = new List<Senmag_RadialMenuLabel>();

	[Header("Outputs")]
	public bool wasClicked = false;
	public int currentSelection = -1;
	public float currentScale = 0f;


	private bool ispressed = false;
	private int numSections = 1;

	private ConfigurableJoint spring;
	private JointDrive springDrive;
	private AudioSource audioSource;

	private GameObject face;
	private GameObject back;
	private Canvas canvas;
	private Canvas selectionCanvas;
	private GameObject backgroundPrefab;
	private GameObject dividerPrefab;
	private GameObject arcPrefab;
	private List<GameObject> dividers = new List<GameObject>();

	private List<GameObject> segmentIcons;// = new List<GameObject>();
	private List<GameObject> segmentText;// = new List<GameObject>();


	private bool closing;
	private bool opening;
	private Vector3 fullScale;
	private bool cursorInteracting;
	private int myCustomForceIndex;

	public Senmag_HapticCursor activeCursor;
	public GameObject instantiatedObject;

	public GameObject instantiatedFromObject;
	public Senmag_radialMenu(List<Senmag_RadialMenuLabel> labels)
	{
		segmentLabels = labels;
		numSections = labels.Count;
	}
	void Start()
    {
		cursorInteracting = false;
		currentSelection = -1;
		myCustomForceIndex = -1;

		if (clickSound == null) clickSound = Resources.Load("Sounds/Senmag_default_buttonClick") as AudioClip;
		if (releaseSound == null) releaseSound = Resources.Load("Sounds/Senmag_default_buttonRelease") as AudioClip;

		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.volume = volume;

		if (this.GetComponent<SphereCollider>() != null) this.GetComponent<SphereCollider>().enabled = false;
		if (this.GetComponent<CapsuleCollider>() != null) this.GetComponent<CapsuleCollider>().enabled = false;
		if (this.GetComponent<MeshCollider>() != null) this.GetComponent<MeshCollider>().enabled = false;

		if (this.GetComponent<BoxCollider>() == null) this.gameObject.AddComponent<BoxCollider>();
		this.gameObject.GetComponent<BoxCollider>().isTrigger = true;
		this.gameObject.GetComponent<BoxCollider>().enabled = true;

		numSections = segmentLabels.Count;
		segmentIcons = new List<GameObject>();
		segmentText = new List<GameObject>();
		UnityEngine.Debug.Log("RMenu loading...");

		face = GameObject.CreatePrimitive(PrimitiveType.Cube);
		face.name = "Face";
		//face.transform.parent = this.gameObject.transform;
		face.transform.SetParent(this.gameObject.transform);
		face.transform.rotation = new Quaternion(0, 0, 0, 0);
		face.transform.localPosition = new Vector3(0, 0, 0f);
		face.transform.localScale = new Vector3(1, 1, 0.1f);
		face.AddComponent<Rigidbody>();
		face.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
		//face.GetComponent<Rigidbody>().isKinematic = true;
		face.GetComponent<MeshRenderer>().enabled = false;

		back = GameObject.CreatePrimitive(PrimitiveType.Cube);
		back.name = "Back";
		//back.transform.parent = this.gameObject.transform;
		back.transform.SetParent(this.gameObject.transform);
		back.transform.rotation = new Quaternion(0, 0, 0, 0);
		back.transform.localPosition = new Vector3(0, 0, 0.8f);
		back.transform.localScale = new Vector3(1, 1, 0.1f);
		back.AddComponent<Rigidbody>();
//		back.GetComponent<Rigidbody>().isKinematic = false;
		back.GetComponent<Rigidbody>().isKinematic = true;
//		back.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
		//back.GetComponent<Rigidbody>().mass = 0;

		back.GetComponent<MeshRenderer>().enabled = false;
		back.AddComponent<Senmag_radialMenu_Back>();

		Physics.IgnoreCollision(face.GetComponent<BoxCollider>(), back.GetComponent<BoxCollider>());

		spring = this.gameObject.AddComponent<ConfigurableJoint>();
		spring.connectedBody = face.GetComponent<Rigidbody>();
		spring.xMotion = ConfigurableJointMotion.Locked;
		spring.yMotion = ConfigurableJointMotion.Locked;
		spring.zMotion = ConfigurableJointMotion.Free;
		spring.anchor = new Vector3(0, 0, 0);
		spring.autoConfigureConnectedAnchor = false;
		spring.connectedAnchor = new Vector3(0, 0, 0);
		spring.enableCollision = true;

		springDrive = new JointDrive();
		springDrive.positionSpring = 10000000f;
		springDrive.maximumForce = ClickStrength;
		springDrive.positionDamper = 200;
		spring.zDrive = springDrive;
		spring.autoConfigureConnectedAnchor = true;

		backgroundPrefab = Resources.Load("RMenu_BackgroundPrefab") as GameObject;
		dividerPrefab = Resources.Load("RMenu_DividerPrefab") as GameObject;
		arcPrefab = Resources.Load("RMenu_arcPrefab") as GameObject;

		backgroundPrefab = Instantiate(backgroundPrefab);
		//backgroundPrefab.transform.parent = this.gameObject.transform;
		backgroundPrefab.transform.SetParent(this.gameObject.transform);
		backgroundPrefab.transform.localPosition = new Vector3(0,0, 0.0f);
		backgroundPrefab.transform.localScale = new Vector3(1,1,1);
		backgroundPrefab.transform.localEulerAngles = new Vector3(0,0,0);

		for (int x = 0; x < numSections; x++)
		{
			GameObject newDivider = Instantiate(dividerPrefab);
			//newDivider.transform.parent = this.gameObject.transform;
			newDivider.transform.SetParent(this.gameObject.transform);
			newDivider.transform.localPosition = new Vector3(0, 0, 0.0f);
			newDivider.transform.localScale = new Vector3(1, 1, 1);
			newDivider.transform.localEulerAngles = new Vector3(0,0,x * (360.0f / numSections) - (180f / numSections));
		}

		arcPrefab = Instantiate(arcPrefab);
		//arcPrefab.transform.parent = face.gameObject.transform;
		arcPrefab.transform.SetParent(face.gameObject.transform);
		arcPrefab.transform.localPosition = new Vector3(0, 0, 0.0f);
		arcPrefab.transform.localScale = new Vector3(1, 1, 1);
		arcPrefab.transform.GetChild(3).transform.localEulerAngles = new Vector3(0,0, -(360.0f / numSections));

		for(int x = 0; x < segmentLabels.Count; x++)
		{
			GameObject newCanvas = new GameObject();
			newCanvas.name = "segment_" + x + "_label";

			newCanvas.AddComponent<Canvas>();
			newCanvas.AddComponent<CanvasScaler>();
			newCanvas.GetComponent<Canvas>().sortingOrder = 1;
			RectTransform rt = newCanvas.GetComponent(typeof(RectTransform)) as RectTransform;
			rt.sizeDelta = new Vector2(1, 1);

			float radius = 0.35f;
			float offset = 0.03f;
			float scale = 0.2f;
			//newCanvas.transform.parent = this.gameObject.transform;
			newCanvas.transform.SetParent(this.gameObject.transform);
			newCanvas.transform.localPosition = new Vector3(Mathf.Sin(Mathf.Deg2Rad * x * (360.0f / numSections)) * radius, Mathf.Cos(Mathf.Deg2Rad * x * (360.0f / numSections)) * radius + offset, 0.0f);
			newCanvas.transform.localScale = new Vector3(scale, scale, scale);
			newCanvas.transform.localEulerAngles = new Vector3(0, 0, 0);

			if (segmentLabels[x].icon != null)
			{
				newCanvas.AddComponent<Image>();
				newCanvas.GetComponent<Image>().sprite = segmentLabels[x].icon;
				newCanvas.GetComponent<Image>().preserveAspect = true;

				segmentIcons.Add(newCanvas);
			}

			if(segmentLabels[x].text != null){
				GameObject newText = new GameObject();
				newText.name = "Text";
				
				//newText.transform.parent = newCanvas.transform;
				newText.transform.SetParent(newCanvas.transform);
				newText.AddComponent<TextMeshProUGUI>();

				newText.GetComponent<TextMeshProUGUI>().text = segmentLabels[x].text;
				newText.GetComponent<TextMeshProUGUI>().fontSize = 0.3f;
				newText.GetComponent<TextMeshProUGUI>().color = UnityEngine.Color.black;
				newText.GetComponent<TextMeshProUGUI>().font = Resources.Load("Fonts/ARIAL", typeof(TMP_FontAsset)) as TMP_FontAsset;
				newText.GetComponent<TextMeshProUGUI>().horizontalAlignment = HorizontalAlignmentOptions.Center;
				newText.GetComponent<TextMeshProUGUI>().verticalAlignment = VerticalAlignmentOptions.Middle;
				newText.transform.localScale = new Vector3(1, 1, 1);

				radius = 0.35f;
				offset = -0.7f;

				newText.GetComponent<TextMeshProUGUI>().transform.localPosition = new Vector3(0, offset, 0);
				newText.GetComponent<TextMeshProUGUI>().transform.localEulerAngles = new Vector3(0, 0, 0);

				segmentText.Add(newText);
			}
		}
		highlightSegment(-1);

		this.transform.LookAt(Camera.main.transform.position);
		transform.RotateAround(transform.position, transform.up, 180);
		fullScale = transform.localScale;
		this.transform.localScale = new Vector3(0,0,0);
		currentScale = 0;
		opening = true;


		back.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
		back.GetComponent<Rigidbody>().isKinematic = false;
		//back.GetComponent<Rigidbody>().mass = 0;
	}

	void configureFace()
	{
		face.transform.SetParent(this.gameObject.transform);
		face.transform.rotation = new Quaternion(0, 0, 0, 0);
		face.transform.localPosition = new Vector3(0, 0, 0f);
		face.transform.localScale = new Vector3(1, 1, 0.1f);
		face.AddComponent<Rigidbody>();
		face.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
		//face.GetComponent<Rigidbody>().isKinematic = true;
		face.GetComponent<MeshRenderer>().enabled = false;

		back.transform.SetParent(this.gameObject.transform);
		back.transform.rotation = new Quaternion(0, 0, 0, 0);
		back.transform.localPosition = new Vector3(0, 0, 0.8f);
		back.transform.localScale = new Vector3(1, 1, 0.1f);
		back.GetComponent<Rigidbody>().isKinematic = true;
		back.GetComponent<MeshRenderer>().enabled = false;

		Physics.IgnoreCollision(face.GetComponent<BoxCollider>(), back.GetComponent<BoxCollider>());

		spring.connectedBody = face.GetComponent<Rigidbody>();
		spring.xMotion = ConfigurableJointMotion.Locked;
		spring.yMotion = ConfigurableJointMotion.Locked;
		spring.zMotion = ConfigurableJointMotion.Free;
		spring.anchor = new Vector3(0, 0, 0);
		spring.autoConfigureConnectedAnchor = false;
		spring.connectedAnchor = new Vector3(0, 0, 0);
		spring.enableCollision = true;

		springDrive.positionSpring = 10000000f;
		springDrive.maximumForce = ClickStrength;
		springDrive.positionDamper = 200;
		spring.zDrive = springDrive;
		spring.autoConfigureConnectedAnchor = true;
	}



	// Update is called once per frame
	bool latch = false;
	bool startedCollisionFree = false;
    void Update()
    {
		if(startedCollisionFree == false){
			if(back.GetComponent<Senmag_radialMenu_Back>().collisionClear == false)
			{
				if(back.GetComponent<Senmag_radialMenu_Back>().activeCollision != null){
					//Vector3 collisionPos = this.transform.InverseTransformPoint(back.GetComponent<Senmag_radialMenu_Back>().activeCollision.GetContact(0).point);
					Vector3 collisionPos = this.transform.InverseTransformPoint(back.GetComponent<Senmag_radialMenu_Back>().collisionPos);
					//collisionPos = (gameObject.transform.rotation) * collisionPos;
					transform.localPosition -= collisionPos * 0.03f;

				}
				else
				{
					UnityEngine.Debug.Log("collision was null");
				}
			}

			else if(currentScale == 1)
			{
				startedCollisionFree = true;
				back.GetComponent<Rigidbody>().mass = 1;
				configureFace();
			}
		}

		if (opening == true)
		{	
			currentScale += (1 - currentScale) * animationSpeed;
			if(currentScale >= 0.98f){
				
				currentScale = 1;
				opening = false;
			}
			this.transform.localScale = fullScale * currentScale;
		}
		else if(currentScale == 0 && instantiatedObject.gameObject == null)
		{
			UnityEngine.Debug.Log("Self destroy...");
			Destroy(transform.parent.gameObject);
		}
		if (closing == true)
		{
			currentScale -= (currentScale) * animationSpeed;
			if (currentScale <= 0.02f)
			{
				currentScale = 0;
				closing = false;
			}
			this.transform.localScale = fullScale * currentScale;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
		{
			cursorInteracting = true;
			activeCursor = other.gameObject.GetComponentInParent<Senmag_HapticCursor>();
			activeCursor.GetComponentInChildren<Senmag_stylusControl>().Input_wasClicked(Stylus_Action.leftClick);
			myCustomForceIndex = activeCursor.requestCustomForce(this.gameObject);
			//UnityEngine.Debug.Log("cursor enter with forceIndex: " + myCustomForceIndex);
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
		{
			activeCursor.releaseCustomForce(myCustomForceIndex, this.gameObject);
			cursorInteracting = false;
			activeCursor = null;
			//UnityEngine.Debug.Log("cursor exit");
		}
	}

	public void OnCollisionStay(Collision collision)
	{
		UnityEngine.Debug.Log("RMenu collisionStay");
	}

	void highlightSegment(int segment)
	{
		if(segment < 0)
		{
			//arcPrefab.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
			arcPrefab.SetActive(false);
			return;
		}
		if(segment < numSections)
		{
			arcPrefab.SetActive(true);
			//arcPrefab.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
			arcPrefab.transform.localEulerAngles = new Vector3(0, 0, -(segment * (360.0f / numSections) - (180f / numSections)));
			for(int x = 0; x < segmentIcons.Count; x++){
				//if(x == segment) segmentIcons[x].transform.parent = face.gameObject.transform;
				//else segmentIcons[x].transform.parent = gameObject.transform;

				if(x == segment) segmentIcons[x].transform.SetParent(face.gameObject.transform);
				else segmentIcons[x].transform.SetParent(gameObject.transform);

				Vector3 pos = segmentIcons[x].transform.localPosition;
				pos.z = 0;
				segmentIcons[x].transform.localPosition = pos;
			}


			for (int x = 0; x < segmentText.Count; x++)
			{
			//	if (x == segment) segmentText[x].transform.parent = face.gameObject.transform;
			//	else segmentText[x].transform.parent = gameObject.transform;
			}
		}
	}


	void FixedUpdate()
	{
		


		if (currentScale == 1)
		{
			float position = face.transform.localPosition.z / (-this.transform.localScale.z / 2f);

			if (cursorInteracting)
			{

				Vector3 force = new Vector3(0, 0, 0);
				Vector3 localCursorPos = this.transform.InverseTransformPoint(activeCursor.currentGlobalPosition);

				float distanceToCenter = Mathf.Sqrt(localCursorPos.x * localCursorPos.x + localCursorPos.y * localCursorPos.y);
				int cursorSelection = -1;


				float centreForceRadius = 0.175f;
				float centreForceWidth = 0.04f;
				float angleThreshold = 10;

				if (distanceToCenter > centreForceRadius - centreForceWidth)
				{
					Vector3 localCursorPosUnity = localCursorPos * (1f / localCursorPos.magnitude);
					if (distanceToCenter < centreForceRadius)
					{

						force.x += (distanceToCenter - (centreForceRadius - centreForceWidth)) * localCursorPosUnity.x * -forceGain;
						force.y += (distanceToCenter - (centreForceRadius - centreForceWidth)) * localCursorPosUnity.y * -forceGain;
					}
					else if (distanceToCenter < centreForceRadius + centreForceWidth)
					{
						force.x += ((centreForceRadius + +centreForceWidth) - distanceToCenter) * localCursorPosUnity.x * forceGain;
						force.y += ((centreForceRadius + +centreForceWidth) - distanceToCenter) * localCursorPosUnity.y * forceGain;
					}

					float angleToObj = 180f + Mathf.Rad2Deg * Mathf.Atan2(-localCursorPos.x, -localCursorPos.y);
					float anglePerSection = (360f / numSections);

					float angleToSection = (angleToObj + (anglePerSection / 2.0f));
					if (angleToSection > 360) angleToSection -= 360;
					float cpos = (angleToSection / anglePerSection);
					cursorSelection = ((int)(cpos));

					float sectionForce = angleToSection - (cursorSelection * anglePerSection);
					if (sectionForce > (anglePerSection / 2)) sectionForce -= anglePerSection;

					//UnityEngine.Debug.Log(sectionForce);
					if (Mathf.Abs(sectionForce) < angleThreshold)
					{

						float forceMag = (angleThreshold - Mathf.Abs(sectionForce)) * angleForceGain;

						if (sectionForce < 0) forceMag *= -1;
						//UnityEngine.Debug.Log(forceMag);
						force.x += Mathf.Sin(Mathf.Deg2Rad * (angleToObj + 90)) * forceMag;
						force.y += Mathf.Cos(Mathf.Deg2Rad * (angleToObj + 90)) * forceMag;
					}
				}

				if (cursorSelection != currentSelection)
				{
					currentSelection = cursorSelection;
					highlightSegment(currentSelection);
				}

				//force = Quaternion.Inverse(gameObject.transform.rotation) * force;
				force = (gameObject.transform.rotation) * force;
				activeCursor.modifyCustomForce(myCustomForceIndex, force, this.gameObject);

				//UnityEngine.Debug.Log(cursorSelection);



				//			UnityEngine.Debug.Log(position);
				if (ispressed == false && position < -1f || (activeCursor.GetComponentInChildren<Senmag_stylusControl>() != null && activeCursor.GetComponentInChildren<Senmag_stylusControl>().Input_wasClicked(Stylus_Action.leftClick)))
				{
					//UnityEngine.Debug.Log("RMenu completed...");
					if (clickSound)
					{
						audioSource.clip = clickSound;
						audioSource.Play();
					}

					springDrive.maximumForce = SpringStrength;
					spring.zDrive = springDrive;

					if (pemanant == false)
					{
						ispressed = true;
						wasClicked = true;
						closing = true;

						if (currentSelection != -1)
						{
							if (segmentLabels[currentSelection].objectToInstantiateOnSelect != null)
							{
								//UnityEngine.Debug.Log("Spawning object...");
								instantiatedObject = Instantiate(segmentLabels[currentSelection].objectToInstantiateOnSelect);

								instantiatedObject.transform.parent = this.transform.parent.gameObject.transform;
								instantiatedObject.transform.localPosition = new Vector3(0, 0, 0.2f);
								//instantiatedObject.transform.position = this.gameObject.transform.position;
							}
							else
							{
								//UnityEngine.Debug.Log("RMenu: Object to instantiate was null...");
							}
						}
						return;
					}
				}

				if (position < -1f)
				{
					ispressed = true;
				}
			}
			else
			{
				currentSelection = -1;

                highlightSegment(-1);
			}
            //UnityEngine.Debug.Log(face.transform.localPosition.z);
            if (face.transform.localPosition.z < -back.transform.localPosition.z - .1f)
			{
				UnityEngine.Debug.Log("resetting face");
				Vector3 tmp = face.transform.localPosition;
				tmp.x = 0;
				tmp.y = 0;
				tmp.z = -back.transform.localPosition.z - .1f;
				face.transform.localPosition = tmp;
			}
			if (face.transform.localPosition.z > back.transform.localPosition.z + .1f)
			{
				UnityEngine.Debug.Log("resetting face");
				Vector3 tmp = face.transform.localPosition;
				tmp.x = 0;
				tmp.y = 0;
				tmp.z = back.transform.localPosition.z + .1f;
				face.transform.localPosition = tmp;
			}
			if (position > -0.75f)
			{
				if (ispressed == true)
				{
					ispressed = false;
		
					springDrive.maximumForce = ClickStrength;
					spring.zDrive = springDrive;

					
						wasClicked = true;
						if (pemanant == false) closing = true;
						if (releaseSound)
						{
							UnityEngine.Debug.Log("release");
							audioSource.clip = releaseSound;
							audioSource.Play();
						}
						else UnityEngine.Debug.Log("noRelease");
					
				}
			}
		}
	}


	private void OnDestroy()
	{
		if (cursorInteracting)
		{
			activeCursor.releaseCustomForce(myCustomForceIndex, this.gameObject);
			cursorInteracting = false;
			activeCursor = null;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Stylus_Buttons
{
	none = 0,
	all = 1,
	button1 = 2,
	button2 = 3,
	button3 = 4,
	navSwitch = 5,
}
public enum Stylus_ButtonState
{
	released = 0,
	pressed = 1,
}
public enum Stylus_NavswitchState
{
	released = 0,
	pressed = 1,
	halfForward = 2,
	fullforward = 3,
	halfBackward = 4,
	fullBackward = 5,
}

public enum Stylus_Action
{
	none = 0,
	leftClick = 1,
	rightClick = 2,
	middleClick = 3,
	auxClick = 4,
	scrollUpShort = 5,
	scrollUpLong = 6,
	scrollDownShort = 7,
	scrollDownLong = 8,
}

public struct Stylus_inputState
{
	public bool clicked;
	public bool held;
}

public class Senmag_StylusState
{
	public Stylus_ButtonState button1State;
	public Stylus_ButtonState button2State;
	public Stylus_ButtonState button3State;
	public Stylus_NavswitchState navswitchState;
}

public class Senmag_stylusControl : MonoBehaviour
{
	public Senmag_StylusState stylusState = new Senmag_StylusState();
	[Header("Physical buttons")]
	public GameObject button1;
	public GameObject button2;
	public GameObject button3;
	public GameObject navSwitch;
	public GameObject stylusBase;
	public GameObject defaultTool;

	[Header("Button Bindings")]
	public Stylus_Action button1Binding = Stylus_Action.leftClick;
	public Stylus_Action button2Binding = Stylus_Action.rightClick;
	public Stylus_Action button3Binding = Stylus_Action.auxClick;
	public Stylus_Action scrollClickBinding = Stylus_Action.middleClick;
	public Stylus_Action scrollFwdShortBinding = Stylus_Action.scrollUpShort;
	public Stylus_Action scrollFwdLongBinding = Stylus_Action.scrollUpLong;
	public Stylus_Action scrollBackShortBinding = Stylus_Action.scrollDownShort;
	public Stylus_Action scrollBackLongBinding = Stylus_Action.scrollDownLong;

	public Stylus_inputState	leftClickState;
	public Stylus_inputState	rightClickState;
	public Stylus_inputState	auxClickState;
	public Stylus_inputState	middleClickState;
	public Stylus_inputState	scrollUpShortState;
	public Stylus_inputState	scrollUpLongState;
	public Stylus_inputState	scrollDownShortState;
	public Stylus_inputState	scrollDownLongState;

	[Header("Tool prefabs")]
	public GameObject testTool1;
	public GameObject testTool2;
	public GameObject testTool3;
	public GameObject testTool4;


	public GameObject currentToolTip;

	public Material baseColour;
	public Material highlightColour;

	public bool stylusVisible = true;


	public byte stateByte;
	public byte stateByteLast;

	private float buttonPressDistance = 0.012f;

	public bool button1Highlighted;
	public bool button2Highlighted;
	public bool button3Highlighted;
	public bool navSwitchHighlighted;

	public bool anyHighlighted;
	private bool highlightDirection;
	private float highlightPhase;
	private float highlightSpeed = 0.1f;

	private Stylus_Action lastAction;
	private bool	newActions;
	public bool isColliding;
	private int colliderCounter = 0;

	// Start is called before the first frame update
	void Start()
    {
		lastAction = Stylus_Action.none;
		setTool_default();
		stylusState.button1State = Stylus_ButtonState.released;
		stylusState.button2State = Stylus_ButtonState.released;
		stylusState.button3State = Stylus_ButtonState.released;
		stylusState.navswitchState = Stylus_NavswitchState.released;
		moveButtons();
		anyHighlighted = false;
	}

	public Stylus_Action getAction()
	{
		Stylus_Action action = lastAction;
		lastAction = Stylus_Action.none;
		return action;
	}

	public void OnCollisionStay(Collision collision)
	{
		isColliding = true;
		colliderCounter = 2;
	}


	void Update()
	{
		if(colliderCounter > 0)
		{
			colliderCounter -= 1;
			if(colliderCounter == 0) isColliding  = false;
		}

		if (Input.GetKeyDown(KeyCode.Keypad1)) setTool_custom(testTool1, 1.0f);
		if (Input.GetKeyDown(KeyCode.Keypad2)) setTool_custom(testTool2, 1.0f);
		if (Input.GetKeyDown(KeyCode.Keypad3)) setTool_custom(testTool3, 1.0f);
		if (Input.GetKeyDown(KeyCode.Keypad4)) setTool_custom(testTool4, 1.0f);
		if (Input.GetKeyDown(KeyCode.Keypad5)) setToolScale(new Vector3(0.25f, 0.25f, 0.25f));
		if (Input.GetKeyDown(KeyCode.Keypad6)) setToolScale(new Vector3(0.5f, 0.5f, 0.5f));
		if (Input.GetKeyDown(KeyCode.Keypad7)) setToolScale(new Vector3(1f, 1f, 1f));
		if (Input.GetKeyDown(KeyCode.Keypad8)) hideStylusBody();
		if (Input.GetKeyDown(KeyCode.Keypad9)) showStylusBody();


		//if (anyHighlighted == true)
		//{
			Color fadeColor = Color.Lerp(baseColour.color, highlightColour.color, highlightPhase);

			if (button1Highlighted == true)
			{
				if (button1.transform.GetChild(1).GetComponent<ParticleSystem>().isPlaying == false) button1.transform.GetChild(1).GetComponent<ParticleSystem>().Play();

				button1.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = fadeColor;
				button1.transform.GetChild(1).GetComponent<ParticleSystem>().startColor = fadeColor;
			}
			else if (button1.transform.GetChild(1).GetComponent<ParticleSystem>().isPlaying == true)
			{
				button1.transform.GetChild(1).GetComponent<ParticleSystem>().Stop();
				button1.transform.GetChild(0).GetComponent<MeshRenderer>().material = baseColour;
			}

			if (button2Highlighted == true)
			{
				if (button2.transform.GetChild(1).GetComponent<ParticleSystem>().isPlaying == false) button2.transform.GetChild(1).GetComponent<ParticleSystem>().Play();

				button2.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = fadeColor;
				button2.transform.GetChild(1).GetComponent<ParticleSystem>().startColor = fadeColor;
			}
			else if (button2.transform.GetChild(1).GetComponent<ParticleSystem>().isPlaying == true)
			{
				button2.transform.GetChild(1).GetComponent<ParticleSystem>().Stop();
				button2.transform.GetChild(0).GetComponent<MeshRenderer>().material = baseColour;
			}

			if (button3Highlighted == true)
			{
				if (button3.transform.GetChild(1).GetComponent<ParticleSystem>().isPlaying == false) button3.transform.GetChild(1).GetComponent<ParticleSystem>().Play();

				button3.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = fadeColor;
				button3.transform.GetChild(1).GetComponent<ParticleSystem>().startColor = fadeColor;
			}
			else if (button3.transform.GetChild(1).GetComponent<ParticleSystem>().isPlaying == true)
			{
				button3.transform.GetChild(1).GetComponent<ParticleSystem>().Stop();
				button3.transform.GetChild(0).GetComponent<MeshRenderer>().material = baseColour;
			}

			if (navSwitchHighlighted == true)
			{
				if (navSwitch.transform.GetChild(1).GetComponent<ParticleSystem>().isPlaying == false) navSwitch.transform.GetChild(1).GetComponent<ParticleSystem>().Play();

				navSwitch.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = fadeColor;
				navSwitch.transform.GetChild(1).GetComponent<ParticleSystem>().startColor = fadeColor;
			}
			else if (navSwitch.transform.GetChild(1).GetComponent<ParticleSystem>().isPlaying == true)
			{
				navSwitch.transform.GetChild(1).GetComponent<ParticleSystem>().Stop();
				navSwitch.transform.GetChild(0).GetComponent<MeshRenderer>().material = baseColour;
			}

		//}

		if (stateByteLast != stateByte)
		{
			stateByteLast = stateByte;
			processStylusByte(stateByte);
		}
	}

	public void setToolScale(Vector3 scale)
	{
		currentToolTip.transform.localScale = scale;
	}

	public void setTool_default()
	{
		
		setTool_custom(defaultTool, 1.0f);
	}

	public void setTool_custom(GameObject newTool, float scale)
	{
		Destroy(currentToolTip);
		UnityEngine.Debug.Log("new tool!");
		currentToolTip = Instantiate(newTool);
		UnityEngine.Debug.Log("new tool!: " + newTool.name);
		currentToolTip.transform.parent = transform;
		currentToolTip.transform.localScale = new Vector3(scale, scale, scale);
		currentToolTip.transform.localPosition = new Vector3(0, 0, 0);
	}

	public GameObject getcurrentTool()
	{
		return currentToolTip;
	}

	public void highlightButton(Stylus_Buttons button, bool highlight)
	{
		if(button == Stylus_Buttons.all)
		{
			if(highlight == true)
			{
				button1Highlighted = true;
				button2Highlighted = true;
				button3Highlighted = true;
				navSwitchHighlighted = true;
			}
			else
			{
				button1Highlighted = false;
				button2Highlighted = false;
				button3Highlighted = false;
				navSwitchHighlighted = false;
			}
		}
		if(button == Stylus_Buttons.button1)
		{
			if (highlight == true) button1Highlighted = true;
			else button1Highlighted = false;
		}
		if (button == Stylus_Buttons.button2)
		{
			if (highlight == true) button2Highlighted = true;
			else button2Highlighted = false;
		}
		if (button == Stylus_Buttons.button3)
		{
			if (highlight == true) button3Highlighted = true;
			else button3Highlighted = false;
		}
		if (button == Stylus_Buttons.navSwitch)
		{
			if (highlight == true) navSwitchHighlighted = true;
			else navSwitchHighlighted = false;
		}
		if(button1Highlighted || button2Highlighted || button3Highlighted || navSwitchHighlighted)
		{
			anyHighlighted = true;
			InvokeRepeating("updateFadeVal", 0, 0.1f);
		}
		else
		{
			anyHighlighted = false;
			CancelInvoke("updateFadeVal");
        }
	}

    public void hideStylusTip()
	{
        Renderer[] rs = currentToolTip.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rs) r.enabled = false;
    }

    public void showStylusTip()
    {
        Renderer[] rs = currentToolTip.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rs) r.enabled = true;
    }

    public void hideStylusBody()
	{
		button1.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
		button2.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
		button3.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
		navSwitch.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
		stylusBase.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
	}
	public void showStylusBody()
	{
		button1.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
		button2.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
		button3.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
		navSwitch.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
		stylusBase.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
	}

	public bool Input_wasClicked(Stylus_Action action)
	{
		if(action == Stylus_Action.leftClick)
		{
			bool clicked = leftClickState.clicked;
			leftClickState.clicked = false;
			return clicked;
		}
		if (action == Stylus_Action.rightClick)
		{
			bool clicked = rightClickState.clicked;
			rightClickState.clicked = false;
			return clicked;
		}
		if (action == Stylus_Action.auxClick)
		{
			bool clicked = auxClickState.clicked;
			auxClickState.clicked = false;
			return clicked;
		}
		if (action == Stylus_Action.middleClick)
		{
			bool clicked = middleClickState.clicked;
			middleClickState.clicked = false;
			return clicked;
		}
		if (action == Stylus_Action.scrollUpLong)
		{
			bool clicked = scrollUpLongState.clicked;
			scrollUpLongState.clicked = false;
			return clicked;
		}
		if (action == Stylus_Action.scrollUpShort)
		{
			bool clicked = scrollUpShortState.clicked;
			scrollUpShortState.clicked = false;
			return clicked;
		}
		if (action == Stylus_Action.scrollDownLong)
		{
			bool clicked = scrollDownLongState.clicked;
			scrollDownLongState.clicked = false;
			return clicked;
		}
		if (action == Stylus_Action.scrollDownShort)
		{
			bool clicked = scrollDownShortState.clicked;
			scrollDownShortState.clicked = false;
			return clicked;
		}
		return false;
	}
	public bool Input_isHeld(Stylus_Action action)
	{
		if (action == Stylus_Action.leftClick) return leftClickState.held;
		if (action == Stylus_Action.rightClick) return rightClickState.held;
		if (action == Stylus_Action.auxClick) return auxClickState.held;
		if (action == Stylus_Action.middleClick) return middleClickState.held;
		if (action == Stylus_Action.scrollUpLong) return scrollUpLongState.held;
		if (action == Stylus_Action.scrollUpShort) return scrollUpShortState.held;
		if (action == Stylus_Action.scrollDownLong) return scrollDownLongState.held;
		if (action == Stylus_Action.scrollDownShort) return scrollDownShortState.held;
		return false;
	}

	public bool Input_newActions()
	{
		if(newActions == true)
		{
			newActions = false;
			return true;
		}
		return false;
	}

	void processButtonPress(Stylus_Action action)
	{
		newActions = true;
		if (action == Stylus_Action.leftClick)
		{
			leftClickState.clicked = true;
			leftClickState.held = true;
		}
		if (action == Stylus_Action.rightClick)
		{
			rightClickState.clicked = true;
			rightClickState.held = true;
		}
		if (action == Stylus_Action.auxClick)
		{
			auxClickState.clicked = true;
			auxClickState.held = true;
		}
		if (action == Stylus_Action.middleClick)
		{
			middleClickState.clicked = true;
			middleClickState.held = true;
		}
		if (action == Stylus_Action.scrollUpShort)
		{
			scrollUpShortState.clicked = true;
			scrollUpShortState.held = true;
		}
		if (action == Stylus_Action.scrollUpLong)
		{
			scrollUpLongState.clicked = true;
			scrollUpLongState.held = true;
		}
		if (action == Stylus_Action.scrollDownShort)
		{
			scrollDownShortState.clicked = true;
			scrollDownShortState.held = true;
		}
		if (action == Stylus_Action.scrollDownLong)
		{
			scrollDownLongState.clicked = true;
			scrollDownLongState.held = true;
		}
	}

	void processButtonRelease(Stylus_Action action)
	{
		newActions = true;
		if (action == Stylus_Action.leftClick)
		{
			leftClickState.held = false;
		}
		if (action == Stylus_Action.rightClick)
		{
			rightClickState.held = false;
		}
		if (action == Stylus_Action.auxClick)
		{
			auxClickState.held = false;
		}
		if (action == Stylus_Action.middleClick)
		{
			middleClickState.held = false;
		}
		if (action == Stylus_Action.scrollUpShort)
		{
			scrollUpShortState.held = false;
		}
		if (action == Stylus_Action.scrollUpLong)
		{
			scrollUpLongState.held = false;
		}
		if (action == Stylus_Action.scrollDownShort)
		{
			scrollDownShortState.held = false;
		}
		if (action == Stylus_Action.scrollDownLong)
		{
			scrollDownLongState.held = false;
		}
	}

	public void processStylusByte(byte state)
	{
		if ((state & (1 << 0)) != 0)
		{
			//UnityEngine.Debug.Log("Stylus::processStylusByte1");
			if(stylusState.button1State != Stylus_ButtonState.pressed) processButtonPress(button1Binding);
			stylusState.button1State = Stylus_ButtonState.pressed;

		}
		else{
			stylusState.button1State = Stylus_ButtonState.released;
			processButtonRelease(button1Binding);
		}


		if ((state & (1 << 1)) != 0){
			if (stylusState.button2State != Stylus_ButtonState.pressed) processButtonPress(button2Binding);
			stylusState.button2State = Stylus_ButtonState.pressed;
		}
		else{
			stylusState.button2State = Stylus_ButtonState.released;
			processButtonRelease(button2Binding);
		}
		

		if ((state & (1 << 7)) != 0){
			if (stylusState.button3State != Stylus_ButtonState.pressed) processButtonPress(button3Binding);
			stylusState.button3State = Stylus_ButtonState.pressed;
		}
		else{
			stylusState.button3State = Stylus_ButtonState.released;
			processButtonRelease(button3Binding);
		}

		stylusState.navswitchState = Stylus_NavswitchState.released;

		if ((state & (1 << 4)) != 0){
			if (stylusState.navswitchState != Stylus_NavswitchState.pressed) processButtonPress(scrollClickBinding);
			stylusState.navswitchState = Stylus_NavswitchState.pressed;
		}
		else
		{
			processButtonRelease(scrollClickBinding);
		}

		if ((state & (1 << 5)) != 0){
			if (stylusState.navswitchState != Stylus_NavswitchState.fullforward) processButtonPress(scrollFwdLongBinding);
			stylusState.navswitchState = Stylus_NavswitchState.fullforward;
		}
		else
		{
			processButtonRelease(scrollFwdLongBinding);
		}

		if ((state & (1 << 6)) != 0){
			if (stylusState.navswitchState != Stylus_NavswitchState.halfForward) processButtonPress(scrollFwdShortBinding);
			stylusState.navswitchState = Stylus_NavswitchState.halfForward;
		}
		else
		{
			processButtonRelease(scrollFwdShortBinding);
		}

		if ((state & (1 << 3)) != 0){
			if (stylusState.navswitchState != Stylus_NavswitchState.fullBackward) processButtonPress(scrollBackLongBinding);
			stylusState.navswitchState = Stylus_NavswitchState.fullBackward;
		}
		else
		{
			processButtonRelease(scrollBackLongBinding);
		}

		if ((state & (1 << 2)) != 0){
			if (stylusState.navswitchState != Stylus_NavswitchState.halfBackward) processButtonPress(scrollBackShortBinding);
			stylusState.navswitchState = Stylus_NavswitchState.halfBackward;
		}
		else {
			processButtonRelease(scrollBackShortBinding);
		}
        moveButtons();
    }

	public void enableCollider()
	{
		isColliding = false;
		if (currentToolTip.GetComponentInChildren<Senmag_stylusTip>() != null)
		{
			currentToolTip.GetComponentInChildren<Senmag_stylusTip>().enableCollider();
		}

		/*if (currentToolTip.GetComponent<SphereCollider>() != null) currentToolTip.GetComponent<SphereCollider>().enabled = true;
		if (currentToolTip.GetComponent<BoxCollider>() != null) currentToolTip.GetComponent<BoxCollider>().enabled = true;
		if (currentToolTip.GetComponent<CapsuleCollider>() != null) currentToolTip.GetComponent<CapsuleCollider>().enabled = true;
		if (currentToolTip.GetComponent<MeshCollider>() != null) currentToolTip.GetComponent<MeshCollider>().enabled = true;

		if (currentToolTip.GetComponentInChildren<SphereCollider>() != null) currentToolTip.GetComponentInChildren<SphereCollider>().enabled = true;
		if (currentToolTip.GetComponentInChildren<BoxCollider>() != null) currentToolTip.GetComponentInChildren<BoxCollider>().enabled = true;
		if (currentToolTip.GetComponentInChildren<CapsuleCollider>() != null) currentToolTip.GetComponentInChildren<CapsuleCollider>().enabled = true;
		if (currentToolTip.GetComponentInChildren<MeshCollider>() != null) currentToolTip.GetComponentInChildren<MeshCollider>().enabled = true;*/
	}
	public void disableCollider()
	{
		if (currentToolTip.GetComponentInChildren<Senmag_stylusTip>() != null)
		{
			currentToolTip.GetComponentInChildren<Senmag_stylusTip>().disableCollider();
		}
		/*if(currentToolTip.GetComponent<SphereCollider>() != null) currentToolTip.GetComponent<SphereCollider>().enabled = false;
		if (currentToolTip.GetComponent<BoxCollider>() != null) currentToolTip.GetComponent<BoxCollider>().enabled = false;
		if (currentToolTip.GetComponent<CapsuleCollider>() != null) currentToolTip.GetComponent<CapsuleCollider>().enabled = false;
		if (currentToolTip.GetComponent<MeshCollider>() != null) currentToolTip.GetComponent<MeshCollider>().enabled = false;

		if (currentToolTip.GetComponentInChildren<SphereCollider>() != null) currentToolTip.GetComponentInChildren<SphereCollider>().enabled = false;
		if (currentToolTip.GetComponentInChildren<BoxCollider>() != null) currentToolTip.GetComponentInChildren<BoxCollider>().enabled = false;
		if (currentToolTip.GetComponentInChildren<CapsuleCollider>() != null) currentToolTip.GetComponentInChildren<CapsuleCollider>().enabled = false;
		if (currentToolTip.GetComponentInChildren<MeshCollider>() != null) currentToolTip.GetComponentInChildren<MeshCollider>().enabled = false;*/
	}

	private void updateFadeVal()
	{
		if (anyHighlighted == true)
		{
			if (highlightDirection == true)
			{
				highlightPhase += highlightSpeed;
				if (highlightPhase >= 1)
				{
					highlightDirection = false;
					highlightPhase -= highlightSpeed;
				}
			}
			else if (highlightDirection == false)
			{
				highlightPhase -= highlightSpeed;
				if (highlightPhase <= 0)
				{
					highlightDirection = true;
					highlightPhase += highlightSpeed;
				}
			}
		}
	}

	private void moveButtons()
	{
		//UnityEngine.Debug.Log("Stylus::moveButtons");
		if(stylusState.button1State == Stylus_ButtonState.pressed)
		{
			Vector3 tmp = button1.transform.GetChild(0).transform.localPosition;
			tmp.y = -buttonPressDistance;
			button1.transform.GetChild(0).transform.localPosition = tmp;
			button1.transform.GetChild(0).GetComponent<MeshRenderer>().material = highlightColour;
		}
		else
		{
			button1.transform.GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
			button1.transform.GetChild(0).GetComponent<MeshRenderer>().material = baseColour;
		}
		if (stylusState.button2State == Stylus_ButtonState.pressed)
		{
			Vector3 tmp = button2.transform.GetChild(0).transform.localPosition;
			tmp.y = -buttonPressDistance;
			button2.transform.GetChild(0).transform.localPosition = tmp;
			button2.transform.GetChild(0).GetComponent<MeshRenderer>().material = highlightColour;
		}
		else
		{
			button2.transform.GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
			button2.transform.GetChild(0).GetComponent<MeshRenderer>().material = baseColour;
		}
		if (stylusState.button3State == Stylus_ButtonState.pressed)
		{
			Vector3 tmp = button3.transform.GetChild(0).transform.localPosition;
			tmp.y = -buttonPressDistance;
			button3.transform.GetChild(0).transform.localPosition = tmp;
			button3.transform.GetChild(0).GetComponent<MeshRenderer>().material = highlightColour;
		}
		else
		{
			button3.transform.GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
			button3.transform.GetChild(0).GetComponent<MeshRenderer>().material = baseColour;
		}

		if (stylusState.navswitchState == Stylus_NavswitchState.pressed)
		{
			Vector3 tmp = new Vector3(90, 0, 0);
			navSwitch.transform.localRotation = Quaternion.Euler(tmp);
			navSwitch.transform.GetChild(0).transform.localPosition = new Vector3(0, 0.015f, 0);
			navSwitch.transform.GetChild(0).GetComponent<MeshRenderer>().material = highlightColour;
		}
		else
		{
			navSwitch.transform.GetChild(0).transform.localPosition = new Vector3(0, 0.03f, 0);


			if (stylusState.navswitchState == Stylus_NavswitchState.halfForward)
			{
				Vector3 tmp = new Vector3(90, 0, -12.5f);
				navSwitch.transform.localRotation = Quaternion.Euler(tmp);
				navSwitch.transform.GetChild(0).GetComponent<MeshRenderer>().material = highlightColour;
			}
			else if (stylusState.navswitchState == Stylus_NavswitchState.fullforward)
			{
				Vector3 tmp = new Vector3(90, 0, -25);
				navSwitch.transform.localRotation = Quaternion.Euler(tmp);
				navSwitch.transform.GetChild(0).GetComponent<MeshRenderer>().material = highlightColour;
			}
			else if (stylusState.navswitchState == Stylus_NavswitchState.halfBackward)
			{
				Vector3 tmp = new Vector3(90, 0, 12.5f);
				navSwitch.transform.localRotation = Quaternion.Euler(tmp);
				navSwitch.transform.GetChild(0).GetComponent<MeshRenderer>().material = highlightColour;
			}
			else if (stylusState.navswitchState == Stylus_NavswitchState.fullBackward)
			{
				Vector3 tmp = new Vector3(90, 0, 25);
				navSwitch.transform.localRotation = Quaternion.Euler(tmp);
				navSwitch.transform.GetChild(0).GetComponent<MeshRenderer>().material = highlightColour;
			}
			else
			{
				Vector3 tmp = new Vector3(90, 0, 0);
				navSwitch.transform.localRotation = Quaternion.Euler(tmp);
				navSwitch.transform.GetChild(0).GetComponent<MeshRenderer>().material = baseColour;
			}
		}
	}
    
}

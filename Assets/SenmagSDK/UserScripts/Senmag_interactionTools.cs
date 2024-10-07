using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


namespace SenmagHaptic 
{
	public enum Senmag_InteractionActionType
	{
		none = 0,
		pickup = 1,
		release = 2,
		pickupRelease = 3,
		openMenu = 4,
		destroy = 5,
	};

	public enum Senmag_StylusActionType
	{
		none = 0,
		click = 1,
		hold = 2,
		release = 3,
		clickRelease = 4,
	};


	public struct objectProperties
	{
		float	mass;
		float	frictionStatic;
		float	frictionDynamic;
		Color	colour;
	};


	[System.Serializable]
	public class Senmag_CursorInteraction
	{
		[Header("Triggers")]
		public Stylus_Action stylusKeybind = Stylus_Action.none;
		public Senmag_StylusActionType stylusKeyActionType = Senmag_StylusActionType.none;

		public bool triggeredByPhysicsTouch = false;
		public bool triggeredByPhysicsPress = false;
		public float pressThreshold = 0.5f;

		[Header("Actions")]
		public Senmag_InteractionActionType action = Senmag_InteractionActionType.none;
		public event EventHandler actionHandler;

		public bool buttonStateLatch;

		

	};


	public class Senmag_interactionTools : MonoBehaviour
	{
		public List<Senmag_CursorInteraction> cursorInteractions = new List<Senmag_CursorInteraction>();
		public bool	useCollisions = true;
		public bool useTriggers = true;

		public bool cursorInteracting;
		public Senmag_HapticCursor activeCursor;

		public GameObject defaultRightClickMenu;

		public float scaleSteps = 0;

		public bool pickedUp;
		public bool touched;

		private bool wasTouched = false;

		public int inhibitTouchAction = 0;

        public float extraGravity;
        private int myCustomForceIndex = -1;

        void Update()
		{
			if(myCustomForceIndex != -1)		//if a custom force is active...
			{
				if(pickedUp == true) activeCursor.modifyCustomForce(myCustomForceIndex, new Vector3(0, -extraGravity, 0), transform.gameObject);
                
            }

            

            if (inhibitTouchAction > 0) inhibitTouchAction--;
			if (inhibitTouchAction < 0) inhibitTouchAction++;
            touched = cursorInteracting;

            if ((cursorInteracting == true || pickedUp == true) && activeCursor != null)
			{
				float currentForce = activeCursor.getCurrentForce().magnitude;

				for (int x = 0; x < cursorInteractions.Count; x++)
				{
					if (cursorInteractions[x].triggeredByPhysicsTouch == true)
					{
						if (pickedUp == false && cursorInteractions[x].buttonStateLatch == false && inhibitTouchAction == 0)
						{
							//UnityEngine.Debug.Log("press pickup...");

							handleInteraction(cursorInteractions[x].action, Senmag_StylusActionType.none);
							cursorInteractions[x].buttonStateLatch = true;
						}
					}

					else
					{
						bool controlState = activeCursor.GetComponentInChildren<Senmag_stylusControl>().Input_isHeld(cursorInteractions[x].stylusKeybind);
						if (cursorInteractions[x].buttonStateLatch == false && controlState == true)        //button was clicked
						{
							cursorInteractions[x].buttonStateLatch = true;
							if (cursorInteractions[x].stylusKeyActionType == Senmag_StylusActionType.click || cursorInteractions[x].stylusKeyActionType == Senmag_StylusActionType.clickRelease)
							{
								handleInteraction(cursorInteractions[x].action, Senmag_StylusActionType.click);
							}
						}

						if (cursorInteractions[x].buttonStateLatch == true && controlState == true)        //button is held
						{
							if (cursorInteractions[x].stylusKeyActionType == Senmag_StylusActionType.hold)
							{
								handleInteraction(cursorInteractions[x].action, Senmag_StylusActionType.hold);
							}
						}

						if (cursorInteractions[x].buttonStateLatch == true && controlState == false)        //button was released
						{
							cursorInteractions[x].buttonStateLatch = false;
							if (cursorInteractions[x].stylusKeyActionType == Senmag_StylusActionType.release || cursorInteractions[x].stylusKeyActionType == Senmag_StylusActionType.clickRelease)
							{
								handleInteraction(cursorInteractions[x].action, Senmag_StylusActionType.release);
							}
						}
					}
				}
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if(useTriggers == false) return;

			if(other.gameObject.GetComponentInChildren<Senmag_interactionTools_override>() == null){
				if (other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
				{
					cursorInteracting = true;
					wasTouched = true;
                    activeCursor = other.gameObject.GetComponentInParent<Senmag_HapticCursor>();
					//myCustomForceIndex = activeCursor.requestCustomForce(this.gameObject);
				}
			}
		}
		private void OnTriggerExit(Collider other)
		{
			if (useTriggers == false) return;
			if (other.gameObject.GetComponentInChildren<Senmag_interactionTools_override>() == null)
			{
				if (other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
				{
					cursorInteracting = false;
				}
			}

			if (other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
			{
				for (int x = 0; x < cursorInteractions.Count; x++)
				{
					if (cursorInteractions[x].triggeredByPhysicsTouch == true && pickedUp == false)
					{
                        //UnityEngine.Debug.Log("press latch release");
                        cursorInteractions[x].buttonStateLatch = false;

					}
				}
			}
        }

		private void OnCollisionEnter(Collision other)
		{
			if (useCollisions == false) return;

			if (other.gameObject.GetComponentInChildren<Senmag_interactionTools_override>() == null)
			{
				if (other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
				{
					cursorInteracting = true;
                    wasTouched = true;
                    activeCursor = other.gameObject.GetComponentInParent<Senmag_HapticCursor>();
				}
			}
		}

		private void OnCollisionExit(Collision other)
		{
			if (useCollisions == false) return;
			if (other.gameObject.GetComponentInChildren<Senmag_interactionTools_override>() == null)
			{
				if (other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
				{
					cursorInteracting = false;
				}
			}
			if (other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
			{
				for (int x = 0; x < cursorInteractions.Count; x++)
				{
					if (cursorInteractions[x].triggeredByPhysicsTouch == true && pickedUp == false)
					{
						//UnityEngine.Debug.Log("press latch release");
						cursorInteractions[x].buttonStateLatch = false;
					}
				}
			}
        }

		public bool wasObjectTouched()
		{
			if(wasTouched == true)
			{
				wasTouched = false;
				return true;
			}
			return false;
		}

		

		public void handleInteraction(Senmag_InteractionActionType interaction, Senmag_StylusActionType stylusAction)
		{
			//UnityEngine.Debug.Log("Interaction tools: handling event");
			switch (interaction)
			{
				case (Senmag_InteractionActionType.pickup):
					if(cursorInteracting == true)
					{
                        if(extraGravity != 0) myCustomForceIndex = activeCursor.requestCustomForce(transform.gameObject); //get index of the next available custom force effect

                        activeCursor.pickUpObject(transform.gameObject, false);
						pickedUp = true;
					}
					break;
				case (Senmag_InteractionActionType.release):
					if (cursorInteracting == true)
					{
                        if (myCustomForceIndex != -1)
                        {
                            activeCursor.modifyCustomForce(myCustomForceIndex, new Vector3(0, 0, 0), transform.gameObject);
                            activeCursor.releaseCustomForce(myCustomForceIndex, transform.gameObject);
                            myCustomForceIndex = -1;
                        }
                        activeCursor.dropObject(transform.gameObject);
						pickedUp = false;
						activeCursor = null;
						inhibitTouchAction = 50;
                    }
					break;
				case (Senmag_InteractionActionType.pickupRelease):
					if(stylusAction == Senmag_StylusActionType.click)
					{
						activeCursor.pickUpObject(transform.gameObject, false);
						pickedUp = true;
                        if (extraGravity != 0) myCustomForceIndex = activeCursor.requestCustomForce(transform.gameObject); //get index of the next available custom force effect
                    }
					else if (stylusAction == Senmag_StylusActionType.release)
					{
                        if (myCustomForceIndex != -1)
                        {
                            activeCursor.modifyCustomForce(myCustomForceIndex, new Vector3(0, 0, 0), transform.gameObject);
                            activeCursor.releaseCustomForce(myCustomForceIndex, transform.gameObject);
                            myCustomForceIndex = -1;
                        }
                        activeCursor.dropObject(transform.gameObject);
						pickedUp = false;
						activeCursor = null;
                    }

					break;

				case (Senmag_InteractionActionType.openMenu):
					if(activeCursor.rightClickMenu != null)
					{
						Destroy(activeCursor.rightClickMenu);
					}
					else
					{
						activeCursor.rightClickMenu = Instantiate(defaultRightClickMenu);
						activeCursor.rightClickMenu.transform.position = transform.position + new Vector3(0, transform.localScale.y / 1.8f + activeCursor.rightClickMenu.transform.localScale.x / 3.5f, 0);
						if (activeCursor.rightClickMenu.GetComponent<Senmag_radialMenu>() != null) activeCursor.rightClickMenu.GetComponent<Senmag_radialMenu>().instantiatedFromObject = this.gameObject;
						else if (activeCursor.rightClickMenu.GetComponentInChildren<Senmag_radialMenu>() != null) activeCursor.rightClickMenu.GetComponentInChildren<Senmag_radialMenu>().instantiatedFromObject = this.gameObject;
					}
					break;
				case (Senmag_InteractionActionType.destroy):
					Destroy(this.gameObject);
					break;
			}
		}
	}
}
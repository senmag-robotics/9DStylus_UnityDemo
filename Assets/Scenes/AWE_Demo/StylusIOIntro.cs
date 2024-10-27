using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace SenmagHaptic
{
    public enum StylusTutorialState
    {
        intro,
        leftClick,
        pickUpObjects,
        dropObjects,
        rightClick,
        menu1,
        menu2,
        painting1,
        brushScale,
        painting2,
        normalCursor,
        end,
    }

    public class StylusIOIntro : MonoBehaviour
    {


        public StylusTutorialState tutorialState = StylusTutorialState.leftClick;
        public Stylus_Buttons buttonToHighlight;
        private Senmag_stylusControl stylusControl;

        public GameObject pickupObjectPrefab;
        public Vector3 pickupLocation;
        public Vector3 pickupScale;

        public GameObject putDownObjectPrefab;
        public Vector3 putDownLocation;
        public Vector3 putDownScale;

        public GameObject paintCanvasPrefab;
        public Vector3 paintCanvasLocation;
        public Vector3 paintCanvasRotation;
        public Vector3 paintCanvasScale;

        public GameObject defaultCursor;

        public GameObject rightClickMenuDefault;
        public GameObject rightClickMenuDemo;

        private GameObject pickupObject;
        private GameObject putDownObject;
        private GameObject paintCanvas;

        private MessageBanner gameObject_messageBanner;
        private MessageBanner gameObject_labelBanner;

        public float textSpeed = 5;
        public int taskComplete;
        DateTime sceneTimer;
        bool cursorMessageActive = false;
        string cursorMessage = "";
        float objectScale = 0;
        bool timerTriggered;

        private float objectScaleSpeed = 0.02f;
        // Start is called before the first frame update
        void Start()
        {

            // GameObject.Find("SenmagWorkspace").GetComponentInChildren<Senmag_HapticCursor>().getPosition();
            GameObject tmp = GameObject.Find("SenmagWorkspace");
            stylusControl = GameObject.Find("SenmagWorkspace").GetComponentInChildren<Senmag_stylusControl>();
            gameObject_messageBanner = GameObject.Find("MessageBanner").GetComponentInChildren<MessageBanner>();
            gameObject_labelBanner = GameObject.Find("CursorLabel").GetComponentInChildren<MessageBanner>();

            if (buttonToHighlight != Stylus_Buttons.none)
            {
                stylusControl.showStylusBody();
            }
            loadState(tutorialState);

        }

        // Update is called once per frame
        void Update()
        {
            if (cursorMessageActive == true)
            {
                if (gameObject_labelBanner.bannerState == BannerState.shown)
                {
                    gameObject_labelBanner.setText(cursorMessage, true, textSpeed);
                    cursorMessageActive = false;
                }
            }

            if (tutorialState == StylusTutorialState.intro)
            {
                if ((System.DateTime.Now - sceneTimer).TotalSeconds > 3)
                {
                    advanceState();
                }
            }

            if (tutorialState == StylusTutorialState.leftClick)
            {
                if (stylusControl.Input_wasClicked(Stylus_Action.leftClick))
                {
                    advanceState();
                }
            }
            if (tutorialState == StylusTutorialState.pickUpObjects)
            {
                if (objectScale < 1)
                {
                    objectScale += objectScaleSpeed;
                    if (objectScale >= 1)
                    {
                        objectScale = 1;
                        foreach (Rigidbody body in pickupObject.GetComponentsInChildren<Rigidbody>())
                        {
                            body.isKinematic = false;
                        }
                    }
                    pickupObject.transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), pickupScale, Mathf.SmoothStep(0, 1, objectScale));
                }
                else
                {

                    if (pickupObject.GetComponent<Senmag_interactionTools>().pickedUp == true) advanceState();
                }
            }
            if (tutorialState == StylusTutorialState.dropObjects)
            {
                if (objectScale < 1)
                {
                    objectScale += objectScaleSpeed;
                    if (objectScale >= 1) objectScale = 1;
                    putDownObject.transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), putDownScale, Mathf.SmoothStep(0, 1, objectScale));
                    putDownObject.GetComponentInChildren<basketballHoopTrigger>().triggered = false;
                }

                if (pickupObject.transform.position.y < -0.7 && pickupObject.GetComponent<Senmag_interactionTools>().pickedUp == false)
                {
                    pickupObject.transform.position = pickupLocation;
                    pickupObject.gameObject.GetComponentInChildren<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
                }

                if (putDownObject.GetComponentInChildren<basketballHoopTrigger>().triggered == true && pickupObject.GetComponent<Senmag_interactionTools>().pickedUp == false) advanceState();
            }
            if (tutorialState == StylusTutorialState.rightClick)
            {
                if (objectScale > 0)
                {
                    objectScale -= objectScaleSpeed;
                    if (objectScale <= 0)
                    {
                        objectScale = 0;
                        Destroy(pickupObject);
                        Destroy(putDownObject);
                    }
                    else
                    {
                        pickupObject.transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), pickupScale, Mathf.SmoothStep(0, 1, objectScale));
                        putDownObject.transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), putDownScale, Mathf.SmoothStep(0, 1, objectScale));
                    }
                }
                if (GameObject.Find(rightClickMenuDemo.name + "(Clone)")) advanceState();


            }
            if (tutorialState == StylusTutorialState.menu1)
            {
                if (GameObject.Find("Senmag_RMenu_Toolchanger(Clone)")) advanceState();
            }
            if (tutorialState == StylusTutorialState.menu2)
            {
                if (GameObject.Find("SenmagWorkspace").GetComponentInChildren<Tip_Painter>()) advanceState();
            }
            if (tutorialState == StylusTutorialState.painting1)
            {
                if (objectScale < 1)
                {
                    objectScale += objectScaleSpeed;
                    if (objectScale >= 1)
                    {
                        objectScale = 1;
                    }
                    paintCanvas.transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), paintCanvasScale, Mathf.SmoothStep(0, 1, objectScale));
                }

                if (timerTriggered == false)
                {
                    if (paintCanvas.GetComponentInChildren<Senmag_interactionTools>().wasObjectTouched())
                    {
                        timerTriggered = true;
                        sceneTimer = System.DateTime.Now;
                    }
                }
                else if ((System.DateTime.Now - sceneTimer).TotalSeconds > 3) advanceState();

            }
            if (tutorialState == StylusTutorialState.brushScale)
            {
                if (timerTriggered == false) {
                    if (stylusControl.Input_wasClicked(Stylus_Action.scrollDownLong)) timerTriggered = true;
                    if (stylusControl.Input_wasClicked(Stylus_Action.scrollDownShort)) timerTriggered = true;
                    if (stylusControl.Input_wasClicked(Stylus_Action.scrollUpLong)) timerTriggered = true;
                    if (stylusControl.Input_wasClicked(Stylus_Action.scrollUpShort)) timerTriggered = true;
                    sceneTimer = System.DateTime.Now;
                }
                else if ((System.DateTime.Now - sceneTimer).TotalSeconds > 3) advanceState();
            }
            if (tutorialState == StylusTutorialState.painting2)
            {
                if(paintCanvas.GetComponentInChildren<Senmag_interactionTools>() == null)
                {
                    Destroy(paintCanvas);
                    GameObject.Find("SenmagWorkspace").GetComponentInChildren<Senmag_HapticCursor>().stylusControl.setTool_custom(defaultCursor, 1f);
                    GameObject.Find("SenmagWorkspace").GetComponentInChildren<AWE_Demo>().advanceState();
                    advanceState();
                }
            }
        }

            void advanceState()
        {
            tutorialState++;
            loadState(tutorialState);
        }
        void loadState(StylusTutorialState state)
        {
            sceneTimer = System.DateTime.Now;
            if (state == StylusTutorialState.intro)
            {
                gameObject_messageBanner.setText("The stylus has buttons that can be used\n\rto interact with virtual objects!", true, 5f);
                setCursorMessage("");
            }

            if (state == StylusTutorialState.leftClick)
            {
                gameObject_messageBanner.setText("Locate the select button...", true, textSpeed);
                //setCursorMessage("<- Select Button");
                setCursorMessage("Press the 'select' button");
                stylusControl.highlightButton(Stylus_Buttons.button1, true);
            }

            if (state == StylusTutorialState.pickUpObjects)
            {
                stylusControl.hideStylusBody();
                gameObject_messageBanner.setText("The select button is used to select or grab objects.\n\r\n\rHold the select button and touch the ball to pick it up...", true, textSpeed);
                setCursorMessage("");
                stylusControl.highlightButton(Stylus_Buttons.button1, false);
                stylusControl.highlightButton(Stylus_Buttons.none, false);
                pickupObject = Instantiate(pickupObjectPrefab);
                objectScale = 0;
                pickupObject.transform.localScale = new Vector3(0, 0, 0);
                pickupObject.transform.position = pickupLocation;
                pickupObject.gameObject.GetComponentInChildren<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            }

            if (state == StylusTutorialState.dropObjects)
            {
                gameObject_messageBanner.setText("Release the button to drop the object,\n\r\n\rTry and drop the ball in the basket!", true, textSpeed);
                putDownObject = Instantiate(putDownObjectPrefab);
                objectScale = 0;
                putDownObject.transform.localScale = new Vector3(0, 0, 0);
                putDownObject.transform.position = putDownLocation;
            }

            if (state == StylusTutorialState.rightClick)
            {
                stylusControl.showStylusBody();
                setCursorMessage("Press the 'menu' button");
                gameObject_messageBanner.setText("Great! Now lets look at the next button...\n\r\n\rPress the menu button to open the menu...", true, textSpeed);
                stylusControl.highlightButton(Stylus_Buttons.button2, true);
                objectScale = 1;
                GameObject.Find("SenmagWorkspace").GetComponentInChildren<Senmag_Workspace>().defaultRightClickMenu = rightClickMenuDemo;

            }
            if (state == StylusTutorialState.menu1)
            {
                setCursorMessage("");
                stylusControl.highlightButton(Stylus_Buttons.button2, false);
                stylusControl.highlightButton(Stylus_Buttons.none, false);
                stylusControl.hideStylusBody();
                gameObject_messageBanner.setText("Highlight the 'tools' option by touching the cursor on it,\n\r\n\rPress the select button to open the tools...", true, textSpeed);
                objectScale = 1;
            }
            if (state == StylusTutorialState.menu2)
            {
                gameObject_messageBanner.setText("Great! Now select the brush tool...", true, textSpeed);
                objectScale = 1;
            }
            if (state == StylusTutorialState.painting1)
            {
                gameObject_messageBanner.setText("You can use the brush tool on the surface!\n\r\n\rThe brush reacts to pressure - press harder for\n\ra thicker line!\n\rTouch the pallete on the right to change colour!", true, textSpeed);
                paintCanvas = Instantiate(paintCanvasPrefab);
                objectScale = 0;
                paintCanvas.transform.localScale = new Vector3(0, 0, 0);
                paintCanvas.transform.eulerAngles = paintCanvasRotation;
                paintCanvas.transform.position = paintCanvasLocation;
                objectScale = 0;
            }
            if(state == StylusTutorialState.brushScale)
            {
                gameObject_messageBanner.setText("The scroll lever on the stylus can\n\radjust the size of the brush!", true, textSpeed);
                stylusControl.showStylusBody();
                stylusControl.highlightButton(Stylus_Buttons.navSwitch, true);
                timerTriggered = false;
            }
            if (state == StylusTutorialState.painting2)
            {
                gameObject_messageBanner.setText("Try out some of the other tools in the menu!\n\r\n\rWhen you're ready to continue, use the\n\rmenu button on the white surface and delete it...", true, textSpeed);
                stylusControl.hideStylusBody();
                stylusControl.highlightButton(Stylus_Buttons.navSwitch, false);
                timerTriggered = false;
            }

        }

        void setCursorMessage(string message)
        {
            if (message != "")
            {
                if (gameObject_labelBanner.bannerState != BannerState.shown) gameObject_labelBanner.showBanner();
                gameObject_labelBanner.setText("", false, textSpeed);
                cursorMessageActive = true;
                cursorMessage = message;
            }
            else
            {
                if (gameObject_labelBanner.bannerState != BannerState.hidden) gameObject_labelBanner.hideBanner();
                cursorMessageActive = false;
            }
        }
    }
}
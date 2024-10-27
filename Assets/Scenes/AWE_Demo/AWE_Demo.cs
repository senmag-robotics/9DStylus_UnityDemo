using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace SenmagHaptic
{

    public enum SceneAdvanceType
    {
        manual,
        timeDelay,
        timeDelayAfterTouch,
        timeDelayAfterRelease,
        button,
        motion,
    }

    public enum SceneObjectState
    {
        appearing,
        active,
        moving,
        disappearing,
        hidden,
    }
    [System.Serializable]
    public class SceneObject
    {
        public GameObject prefab;
        
        public SceneObjectState objectState;
        public List<Vector3> targetPositions = new List<Vector3>();
        public List<Vector3> targetScales = new List<Vector3>();
        public List<Vector3> targetOrientations = new List<Vector3>();

        public int destroyAfter;

        public float currentScale;
        public float currentMotion;

        
        public int lifetime = 0;
        public GameObject thisObject;
    }


    [System.Serializable]
    public class DemoState
    {
        [Header("UserLabel")]
        public string label;
        [Header("Configuration")]
        [Multiline]
        //[TextAreaAttribute(5, 5)]
        public string mainMessage;
        
        public string cursorMessage;

        public GameObject radialMenu;
        public GameObject cursorPrefab;

        public float globalStiffness = 0;

        public List<SceneObject> objectsToSpawn;
        public bool keepPreviousObjects;

        public float timeDelay;
        public float minimumTime;
        public SceneAdvanceType sceneAdvanceType = SceneAdvanceType.manual;
        public bool touchTrigger = false;
    }

    

    public class AWE_Demo : MonoBehaviour
    {



        [Header("Demo Items")]
        public KeyCode key_sceneNext;
        public KeyCode key_scenePrev;
        public KeyCode key_sceneReload;

        public GameObject gameObject_messageBanner;
        public GameObject gameObject_buttonNext;
        public GameObject gameObject_cursorLabel;

        public List<DemoState> DemoStates = new List<DemoState>();

        public float textSpeed = 0.1f;

        public int currentState = 0;
        public int startState = 0;
        DateTime stateStart;
        DateTime sceneTimer;
        bool textPending;
        bool timerSet;

        public float objectAppearSpeed = 0.02f;
        public float objectMotionSpeed = 0.02f;

        private bool cursorMessageActive;
        private List<SceneObject> sceneObjects = new List<SceneObject>();
        private bool waitingForTouch;
        private bool waitingForRelease;

        private List<bool> isKinematicTracker = new List<bool>();
        bool initialLoad;
        int stateCounter;

        Vector3 startPos;
        bool motionActivated;
        // Start is called before the first frame update
        void Start()
        {
            cursorMessageActive = false;
            currentState = startState;
            
            initialLoad = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (initialLoad)
            {
                initialLoad = false;
                loadState(DemoStates[startState]);
            }
            if (Input.GetKeyDown(key_sceneNext))
            {
                currentState += 1;
                //if (currentState >= DemoStates.Count) currentState = DemoStates.Count - 1;
                if (currentState >= DemoStates.Count) currentState = 0;
                loadState(DemoStates[currentState]);
            }
            if (Input.GetKeyDown(key_scenePrev))
            {
                currentState -= 1;
                if (currentState < 0) currentState = 0;
                loadState(DemoStates[currentState]);
            }

            if (textPending == true && gameObject_messageBanner.GetComponent<MessageBanner>().bannerState == BannerState.shown)
            {
                //gameObject_messageBanner.GetComponent<MessageBanner>().setText("The robot you are using is our prototype to support \n\rubiquitous interaction for 3D computer programs. \n\r", true, textSpeed );
                gameObject_messageBanner.GetComponent<MessageBanner>().setText(DemoStates[currentState].mainMessage, true, textSpeed);
                textPending = false;
            }

            if (gameObject_cursorLabel.GetComponent<MessageBanner>().bannerState == BannerState.shown || gameObject_cursorLabel.GetComponent<MessageBanner>().bannerState == BannerState.appearing)
            {
                Vector3 position = GameObject.Find("SenmagWorkspace").GetComponentInChildren<Senmag_HapticCursor>().getPosition();
                position.x += 0.9f;
                position.y += 0.15f;
                gameObject_cursorLabel.transform.position = position;
            }

            if (DemoStates[currentState].sceneAdvanceType == SceneAdvanceType.motion)
            {
                if ((startPos - GameObject.Find("SenmagWorkspace").GetComponentInChildren<Senmag_HapticCursor>().getPosition()).magnitude > DemoStates[currentState].timeDelay)
                {
                    advanceState();
                }
                    
            }

            if (cursorMessageActive == true)
            {
                if(gameObject_cursorLabel.GetComponent<MessageBanner>().bannerState == BannerState.shown)
                {
                    gameObject_cursorLabel.GetComponent<MessageBanner>().setText(DemoStates[currentState].cursorMessage, true, textSpeed);
                    cursorMessageActive = false;
                }

                Vector3 position = GameObject.Find("SenmagWorkspace").GetComponentInChildren<Senmag_HapticCursor>().getPosition();
                position.x += 0.9f;
                position.y += 0.15f;
                gameObject_cursorLabel.transform.position = position;
            }

            sceneSpecificActions();
            int count = 0;
            for(int x = 0; x < sceneObjects.Count; x++)
            {
                if (sceneObjects[x].objectState == SceneObjectState.appearing)
                {
                    sceneObjects[x].currentScale += objectAppearSpeed;
                    if (sceneObjects[x].currentScale >= 1)
                    {
                        sceneObjects[x].currentScale = 1;
                        foreach (Rigidbody body in sceneObjects[x].thisObject.GetComponentsInChildren<Rigidbody>())
                        {
                            body.isKinematic = isKinematicTracker[count];
                            count++;
                            //body.WakeUp();// = false;
                        }
                        sceneObjects[x].objectState = SceneObjectState.active;
                    }
                   // sceneObjects[x].thisObject.transform.localScale = sceneObjects[x].targetScales[sceneObjects[x].lifetime] * sceneObjects[x].currentScale;

                    sceneObjects[x].thisObject.transform.localScale = Vector3.Lerp(new Vector3(0,0,0), sceneObjects[x].targetScales[sceneObjects[x].lifetime], Mathf.SmoothStep(0, 1, sceneObjects[x].currentScale));
                }
                if (sceneObjects[x].objectState == SceneObjectState.moving)
                {
                    sceneObjects[x].currentMotion += objectMotionSpeed;
                    if (sceneObjects[x].currentMotion >= 1)
                    {
                        sceneObjects[x].currentMotion = 1;
                        sceneObjects[x].objectState = SceneObjectState.active;
                    }

                    sceneObjects[x].thisObject.transform.localPosition = Vector3.Lerp(sceneObjects[x].targetPositions[sceneObjects[x].lifetime-1], sceneObjects[x].targetPositions[sceneObjects[x].lifetime], Mathf.SmoothStep(0, 1, sceneObjects[x].currentMotion));
                    sceneObjects[x].thisObject.transform.localScale = Vector3.Lerp(sceneObjects[x].targetScales[sceneObjects[x].lifetime - 1], sceneObjects[x].targetScales[sceneObjects[x].lifetime], Mathf.SmoothStep(0, 1, sceneObjects[x].currentMotion));
                    sceneObjects[x].thisObject.transform.eulerAngles = Vector3.Lerp(sceneObjects[x].targetOrientations[sceneObjects[x].lifetime - 1], sceneObjects[x].targetOrientations[sceneObjects[x].lifetime], Mathf.SmoothStep(0, 1, sceneObjects[x].currentMotion));
                }
                if (sceneObjects[x].objectState == SceneObjectState.disappearing)
                {
                    sceneObjects[x].currentScale -= objectAppearSpeed;
                    if (sceneObjects[x].currentScale <= 0)
                    {
                        sceneObjects[x].currentScale = 0;
                        sceneObjects[x].objectState = SceneObjectState.hidden;
                        Destroy(sceneObjects[x].thisObject);
                        sceneObjects.RemoveAt(x);
                    }
                    // sceneObjects[x].thisObject.transform.localScale = sceneObjects[x].targetScales[sceneObjects[x].lifetime] * sceneObjects[x].currentScale;

                    else sceneObjects[x].thisObject.transform.localScale = Vector3.Lerp(new Vector3(0.001f, 0.001f, 0.001f), sceneObjects[x].targetScales[sceneObjects[x].targetScales.Count-1], Mathf.SmoothStep(0,1,sceneObjects[x].currentScale));
                }
            }

            if (DemoStates[currentState].sceneAdvanceType == SceneAdvanceType.button)
            {
                if((System.DateTime.Now - sceneTimer).TotalSeconds > DemoStates[currentState].timeDelay){
                    if(gameObject_buttonNext.GetComponent<Senmag_button>().isHidden == true)
                    {
                        gameObject_buttonNext.GetComponent<Senmag_button>().show();
                    }
                    if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) advanceState();

                }
            }

            if(waitingForTouch == true)
            {
                if (sceneObjects.Count > 0)
                {
                    if (sceneObjects[sceneObjects.Count - 1].thisObject.GetComponentInChildren<Senmag_interactionTools>() != null)
                    {
                        if (sceneObjects[sceneObjects.Count - 1].thisObject.GetComponentInChildren<Senmag_interactionTools>().wasObjectTouched())
                        {
                            waitingForTouch = false;
                            sceneTimer = System.DateTime.Now;
                        }
                    }
                }
            }
            
            //stateStart = System.DateTime.Now;
            if (DemoStates[currentState].sceneAdvanceType == SceneAdvanceType.timeDelay) {
                if ((System.DateTime.Now - stateStart).TotalSeconds > DemoStates[currentState].timeDelay) advanceState();
            }
            if(DemoStates[currentState].sceneAdvanceType == SceneAdvanceType.timeDelayAfterTouch)
            {
                if(waitingForTouch == false)
                {
                    if ((System.DateTime.Now - stateStart).TotalSeconds > DemoStates[currentState].minimumTime &&  ((System.DateTime.Now - sceneTimer).TotalSeconds > DemoStates[currentState].timeDelay)){
                        advanceState();
                    }
                }
            }
            if (DemoStates[currentState].sceneAdvanceType == SceneAdvanceType.timeDelayAfterRelease)
            {
                if (waitingForTouch == false)
                {
                    if (sceneObjects.Count > 0)
                    {
                        if (sceneObjects[sceneObjects.Count - 1].thisObject.GetComponentInChildren<Senmag_interactionTools>() != null)
                        {
                            if (sceneObjects[sceneObjects.Count - 1].thisObject.GetComponentInChildren<Senmag_interactionTools>().wasObjectTouched() || sceneObjects[sceneObjects.Count - 1].thisObject.GetComponentInChildren<Senmag_interactionTools>().touched)
                            {
                                sceneTimer = System.DateTime.Now;
                            }
                        }
                    }
                    if ((System.DateTime.Now - stateStart).TotalSeconds > DemoStates[currentState].minimumTime && ((System.DateTime.Now - sceneTimer).TotalSeconds > DemoStates[currentState].timeDelay))
                    {
                        advanceState();
                    }
                }
            }
        }

        public void sceneSpecificActions()
        {
            if (DemoStates[currentState].label == "intro force table")
            {
                if (GameObject.Find("Big table").GetComponentInChildren<Senmag_interactionTools>().wasObjectTouched())
                {
                    
                    stateCounter++;
                    if(stateCounter > 1) advanceState();
                }
            }
        }
        public void advanceState()
        {
            currentState += 1;
            loadState(DemoStates[currentState]);
        }

        void loadState(DemoState state)
        {
            UnityEngine.Debug.Log("Loading state: " + state.label);
            if (state.globalStiffness != 0) GameObject.Find("SenmagWorkspace").GetComponentInChildren<Senmag_Workspace>().hapticStiffness = state.globalStiffness;
            GameObject.Find("SenmagWorkspace").GetComponentInChildren<Senmag_Workspace>().defaultRightClickMenu = state.radialMenu;
            //GameObject.Find("SenmagWorkspace").GetComponentInChildren<Senmag_HapticCursor>().rightClickMenu = state.radialMenu;
             
            isKinematicTracker.Clear();
            stateCounter = 0;

            if (state.sceneAdvanceType == SceneAdvanceType.motion)
            {
                startPos = GameObject.Find("SenmagWorkspace").GetComponentInChildren<Senmag_HapticCursor>().getPosition();
            }

            if (state.sceneAdvanceType == SceneAdvanceType.button)
            {
                if(state.timeDelay == 0) gameObject_buttonNext.GetComponent<Senmag_button>().show();
                else gameObject_buttonNext.GetComponent<Senmag_button>().hide();
            }
            else gameObject_buttonNext.GetComponent<Senmag_button>().hide();



            for (int x = 0; x < sceneObjects.Count; x++)
            {
                sceneObjects[x].lifetime++;
                if (sceneObjects[x].lifetime >= sceneObjects[x].destroyAfter)
                {
                    sceneObjects[x].objectState = SceneObjectState.disappearing;
                    sceneObjects[x].currentScale = 1;
                }
                else {
                    if (sceneObjects[x].targetPositions.Count > sceneObjects[x].lifetime)
                    {
                        sceneObjects[x].objectState = SceneObjectState.moving;
                        sceneObjects[x].currentMotion = 0;
                    }
                }
            }

            waitingForTouch = true;
            waitingForRelease = true;

            stateStart = System.DateTime.Now;

            if (state.mainMessage == "" || state.mainMessage == " ")
            {
                UnityEngine.Debug.Log("State has no main text: ");
                gameObject_messageBanner.GetComponent<MessageBanner>().hideBanner();
            }
            else {
                if (gameObject_messageBanner.GetComponent<MessageBanner>().bannerState != BannerState.shown)
                {
                    gameObject_messageBanner.GetComponent<MessageBanner>().showBanner();
                    gameObject_cursorLabel.GetComponent<MessageBanner>().setText("", false, textSpeed);
                    textPending = true;
                }
                else gameObject_messageBanner.GetComponent<MessageBanner>().setText(state.mainMessage, true, textSpeed);

               // UnityEngine.Debug.Log("Setting text: " + state.mainMessage);
            }

            for(int x = 0; x < state.objectsToSpawn.Count; x++)
            {
                sceneObjects.Add(state.objectsToSpawn[x]);
                sceneObjects[sceneObjects.Count - 1].thisObject = Instantiate(sceneObjects[sceneObjects.Count - 1].prefab);
                sceneObjects[sceneObjects.Count - 1].thisObject.transform.localScale = new Vector3(0, 0, 0);
                sceneObjects[sceneObjects.Count - 1].thisObject.transform.localPosition = sceneObjects[sceneObjects.Count - 1].targetPositions[0];
                sceneObjects[sceneObjects.Count - 1].thisObject.transform.eulerAngles = sceneObjects[sceneObjects.Count - 1].targetOrientations[0];
                sceneObjects[sceneObjects.Count - 1].currentScale = 0;
                sceneObjects[sceneObjects.Count - 1].currentMotion = 0;
                sceneObjects[sceneObjects.Count - 1].lifetime = 0;
                sceneObjects[sceneObjects.Count - 1].objectState = SceneObjectState.appearing;


                foreach( Rigidbody body in sceneObjects[sceneObjects.Count - 1].thisObject.GetComponentsInChildren<Rigidbody>()){
                    if (body.isKinematic == false)
                    {
                        isKinematicTracker.Add(false);
                        body.isKinematic = true;
                    }
                    else isKinematicTracker.Add(true);
                    body.velocity = new Vector3(0, 0, 0);
                }
            }

            if (state.cursorMessage != "")
            {
                if (gameObject_cursorLabel.GetComponent<MessageBanner>().bannerState != BannerState.shown) gameObject_cursorLabel.GetComponent<MessageBanner>().showBanner();
                gameObject_cursorLabel.GetComponent<MessageBanner>().setText("", false, textSpeed);
                cursorMessageActive = true;
            }
            else
            {
                if (gameObject_cursorLabel.GetComponent<MessageBanner>().bannerState != BannerState.hidden) gameObject_cursorLabel.GetComponent<MessageBanner>().hideBanner();
                cursorMessageActive = false;
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MIGTorchControl : MonoBehaviour
{
    public GameObject weldWirePrefab;
    public Senmag_stylusControl stylusControl;
    public float wireSpeed = 1f;
    public float segmentLength = 10.0f;
    public bool feeding;
    public List<GameObject> weldWireSegments = new List<GameObject>();
    public int weldReady;

    private float wireExtrude = 0;
    // Start is called before the first frame update
    void Start()
    {
        stylusControl = GetComponentInParent<Senmag_stylusControl>();
    }

    // Update is called once per frame
    
    void Update()
    {

        if (weldReady >= 0) weldReady -= 1;

        if (stylusControl.Input_isHeld(Stylus_Action.leftClick))
        {
            feeding = true;
            wireExtrude += wireSpeed;
            foreach (GameObject segment in weldWireSegments)
            {
                if (segment.gameObject != null)
                {
                    Vector3 pos = segment.transform.localPosition;
                    pos.y += wireSpeed;
                    segment.transform.localPosition = pos;
                }
                //else weldWireSegments.Remove(segment);
            }
            if (wireExtrude >= segmentLength)
            {
                wireExtrude = 0;
                GameObject newWire = Instantiate(weldWirePrefab);
                newWire.transform.parent = transform;
                newWire.transform.localPosition = new Vector3(0, 0, 0);
                newWire.transform.localRotation = Quaternion.identity;
                weldWireSegments.Add(newWire);
            }
        }
        else feeding = false;

        foreach (GameObject segment in weldWireSegments)
        {
            if (segment.gameObject == null)
            {
                weldWireSegments.Remove(segment);
                break;
            }
        }

            }
}

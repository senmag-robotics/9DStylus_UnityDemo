using UnityEngine;
using SenmagHaptic;

public class CollisionPainter : MonoBehaviour{
    public Color paintColor;
    public bool isCursor = false;

    public float radiusConst = 0.01f;
	public float hardnessConst = 1;
	public float strengthConst = 1;

	public float pressureModRadius = 0;
	public float pressureModHardness = 0;
	public float pressureModStrength = 0;

	float radius = 0f;
	float strength = 0;
	float hardness = 0;

	private Senmag_HapticCursor cursor;
    void Awake()
    {
		radius = radiusConst;
		cursor = GetComponentInParent<Senmag_HapticCursor>();
	}
    void Update()
    {
		float pressure = cursor.getCurrentForce().magnitude;
		radius = radiusConst + pressureModRadius * pressure;
		hardness = hardnessConst + pressureModHardness * pressure;
		strength = strengthConst + pressureModStrength * pressure;
	}

	public void setRadius(float newRadius)
	{
		radiusConst = newRadius;
	}
    private void OnCollisionStay(Collision other) {
        
        Paintable p = other.collider.GetComponent<Paintable>();
        //UnityEngine.Debug.Log("Inside collision");
        if(p != null){
			//UnityEngine.Debug.Log("Collision paint");
			Vector3 pos = other.contacts[0].point;
            PaintManager.instance.paint(p, pos, radius, hardness, strength, paintColor);
        }
    }
}

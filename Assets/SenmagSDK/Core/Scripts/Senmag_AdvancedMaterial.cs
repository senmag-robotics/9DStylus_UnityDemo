using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace SenmagHaptic
{

    public enum TextureType{
        sine,
        sawtooth,
    }
    [System.Serializable]
    public class TextureSim
    {
        public TextureType pattern = TextureType.sine;
        public float amplitude = 0.2f;
        public float frequency = 0.01f;
    }

    public class Senmag_AdvancedMaterial : MonoBehaviour
    {
        public float stiffness = 5f;  // Spring stiffness (k)
        public float damping = 0.01f;  // Damping factor (c)
        public float dynamicFriction = 0.2f;  // Dynamic friction coefficient
        public List<TextureSim> textures = new List<TextureSim>();

        private int customForceIndex;
        private Vector3 sphereCenter;       // Center of the sphere (0,0,0 in this case)
        private Vector3 currentForce = Vector3.zero;  // Track the current force
        private Vector3 previousCursorPos = Vector3.zero;  // Track cursor position for velocity
        private float previousDistance;
        private Vector3 velocity;  // Track velocity for damping
        private Vector3 frictionForce = new Vector3(0, 0, 0);
        Senmag_HapticCursor interactingCursor;
        public float velocityFilter = 0.1f;

        public float posMag;
        void Start()
        {
            customForceIndex = -1;
            sphereCenter = transform.position;  // Initialize the sphere center
        }

        void Update()
        {
            // If the custom force is applied but the collider is disabled, release the custom force
            if (customForceIndex != -1 && gameObject.GetComponent<Collider>().enabled == false)
            {
                interactingCursor.releaseCustomForce(customForceIndex, transform.gameObject);
                customForceIndex = -1;
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
            {
                try
                {
                    if (customForceIndex != -1)
                    {
                        interactingCursor.releaseCustomForce(customForceIndex, transform.gameObject);
                        customForceIndex = -1;
                    }
                    interactingCursor = other.gameObject.GetComponentInParent<Senmag_HapticCursor>();
                    customForceIndex = interactingCursor.requestCustomForce(transform.gameObject);
                    previousCursorPos = other.gameObject.transform.position;
                }
                catch
                {
                    Debug.Log("Senmag_HapticCursor NotFound");
                }
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
            {
                if (customForceIndex >= 0)
                {
                    try
                    {
                        Vector3 cursorPos = other.gameObject.transform.position;
                        Vector3 directionToCursor = cursorPos - sphereCenter;

                        float distance = directionToCursor.magnitude;  // Distance from the sphere's center to the cursor
                        Vector3 normalizedDirection = directionToCursor.normalized;  // Normalized direction to cursor

                        Vector3 penetration;
                        float magnitude;
                        Physics.ComputePenetration(other, other.gameObject.transform.position, other.gameObject.transform.rotation, gameObject.GetComponent<Collider>(), gameObject.transform.position, gameObject.transform.rotation, out penetration, out magnitude);
                        penetration *= magnitude;
                        Debug.DrawLine(cursorPos, cursorPos+ penetration* magnitude, Color.magenta);

                        if (magnitude > 0)
                        {
                            velocity = ((cursorPos - previousCursorPos) / Time.fixedDeltaTime) * velocityFilter + (1.0f - velocityFilter)*velocity;
                            previousCursorPos = cursorPos;

                            

                            Vector3 springForce = penetration;
                            Vector3 dampingForce = -velocity * damping;

                            //Debug.DrawLine(cursorPos, cursorPos + springForce * 1f, Color.green);
                            //Vector3 normalVelocity = Vector3.Project(velocity, springForce);

                            Vector3 normalVelocity = (Quaternion.LookRotation(springForce, Vector3.up) * (-velocity));
                            normalVelocity.z = 0;


                            Vector3 frictionForce = Quaternion.Inverse(Quaternion.LookRotation(springForce, Vector3.up)) * normalVelocity;

                            Vector3 textureForce = new Vector3(0, 0, 0);
                                
                            foreach(TextureSim texture in textures)
                            {
                                posMag = (cursorPos.x + cursorPos.y + cursorPos.z) / 3f;
                                while(posMag > 1) posMag -= 1;
                                while (posMag < 0) posMag += 1;
                                if(texture.pattern == TextureType.sine) textureForce += (Mathf.Sin(((2f * Mathf.PI) / texture.frequency) * posMag) + 0.5f) * frictionForce * texture.amplitude;
                                if (texture.pattern == TextureType.sawtooth) textureForce += (posMag / texture.frequency) % 1 * frictionForce * texture.amplitude;


                            }
                            





                            Debug.DrawLine(cursorPos, cursorPos + frictionForce * .1f, Color.red);
                            frictionForce *= dynamicFriction;

                            currentForce = (springForce * stiffness) + dampingForce + (frictionForce * dynamicFriction * springForce.magnitude) + (textureForce * springForce.magnitude);

                            interactingCursor.modifyCustomForce(customForceIndex, currentForce, transform.gameObject);
                        }
                        else
                        {
                            // Outside the radius, stop applying force
                            interactingCursor.modifyCustomForce(customForceIndex, Vector3.zero, transform.gameObject);
                        }
                    }
                    catch
                    {
                        Debug.Log("Error modifying custom force");
                    }
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
            {
                try
                {
                    // Gradually release the force when the cursor exits
                    interactingCursor.releaseCustomForce(customForceIndex, transform.gameObject);
                    customForceIndex = -1;
                    currentForce = Vector3.zero;  // Reset the force when exiting
                }
                catch
                {
                    Debug.Log("Senmag_HapticCursor NotFound");
                }
            }
        }
    }
}

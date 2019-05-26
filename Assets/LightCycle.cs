using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

public class LightCycle : MonoBehaviour
{
    public Vector3 cameraOffset = Vector3.zero;

    public Transform cycle;
    public float mountDistance = 1.0f;
    public float cycleMaxSpeed = 20.0f;
    public float cycleAcceleration = 5.0f;
    public float brakeAcceleration = 7.0f;
    public float coastAcceleration = 2.0f;
    public float reverseSpeed = 5.0f;
    public float turnSpeed = 5.0f;

    public float deadzone = 0.1f;

    public MouseLook mouseLook;

    bool ridingCycle = false;
    bool cycleStillMoving = false;

    float currentSpeed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        mouseLook.Init(transform, Camera.main.transform);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            // Toggle control between walking and light cycle
            if(ridingCycle)
            {
                SwitchToWalking();
            }
            else
            {
                if ((new Vector3(transform.position.x,0,transform.position.z) - new Vector3(cycle.position.x,0,cycle.position.z)).magnitude < mountDistance)
                {
                    SwitchToCycle();
                }
            }
        }
        
        // Do update for cycle
        if (ridingCycle)
        {
            // Update mouse look
            mouseLook.LookRotation(transform, Camera.main.transform);
            mouseLook.UpdateCursorLock();

            // Get inputs
            //int turning = 0; // -1 for left, 1 for right
            float acceleration = CrossPlatformInputManager.GetAxis("Vertical");
            float turning = CrossPlatformInputManager.GetAxis("Horizontal");

            if (acceleration > deadzone)
            {
                // Accelerating
                currentSpeed += acceleration * cycleAcceleration * Time.deltaTime;
                if (currentSpeed > cycleMaxSpeed) currentSpeed = cycleMaxSpeed;
            }
            else if (acceleration < -deadzone)
            {
                // Braking or reversing
                if (currentSpeed <= 0)
                {
                    // Reversing
                    currentSpeed = -reverseSpeed;
                }
                else
                {
                    // Braking
                    currentSpeed += acceleration * brakeAcceleration * Time.deltaTime;
                    if (currentSpeed < 0) currentSpeed = 0;
                }
            }
            else
            {
                if (currentSpeed > 0)
                {
                    // Slowing
                    currentSpeed -= coastAcceleration * Time.deltaTime;
                }
                else
                {
                    // Stop reversing
                    currentSpeed = 0;
                }
            }

            float turnAtSpeedFactor = currentSpeed > 0 ? ((1.2f*cycleMaxSpeed) - currentSpeed) / cycleMaxSpeed : 1;

            //Vector3 currentTiltRot = cycle.GetChild(0).localRotation.eulerAngles;
            //float currentTilt = currentTiltRot.z;
            //float maxTilt = 20f;
            
            //float newTilt = Mathf.Lerp(currentTilt, maxTilt*turning, 0.5f);
            //Vector3 newTiltRot = new Vector3(currentTiltRot.x, currentTiltRot.y, newTilt);
            //cycle.GetChild(0).localRotation = Quaternion.Euler(newTiltRot);


            cycle.Translate(cycle.transform.forward * currentSpeed * Time.deltaTime, Space.World);
            cycle.Rotate(Vector3.up, currentSpeed * turnAtSpeedFactor * turning * turnSpeed * Time.deltaTime);
            
            
        }
    }

    void SwitchToWalking()
    {
        // Enable first person controller
        GetComponent<FirstPersonController>().enabled = true;
        transform.SetParent(null);
        GetComponent<FirstPersonController>().InitMouseLook();
        ridingCycle = false;
        currentSpeed = 0;
    }

    void SwitchToCycle()
    {
        // Disable first person controller
        GetComponent<FirstPersonController>().enabled = false;
        transform.parent = cycle.transform;
        transform.localPosition = cameraOffset;
        mouseLook.Init(transform, Camera.main.transform);
        ridingCycle = true;
    }
}

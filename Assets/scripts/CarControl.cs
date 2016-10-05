using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarControl : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // maximum steer angle the wheel can have
    public float maxBrakingTorque; //how fast should you brake
    public int antiRollValue; //prevents car from flipping
    public int carHealth = 100;
    public float controllerDeadzone = 0.15f;
    //public Vector3 newCom;

    [Header("Data References")]
    public DataManager data;
    public PlayerSwitching pSwitch;

    private Vector2 x_Input;
    private float accelerationForce = 0;
    private float brakingForce = 0;
    private Rigidbody rigid;
    private int mph;
    private int currentPlayer = 0;
    private Vector3 originCom;

    private void Start()
    {
		AkSoundEngine.PostEvent (4272623338, this.gameObject);
        rigid = GetComponent<Rigidbody>();
        originCom = rigid.centerOfMass;
    }

    private void Update()
    {
        //Gamestate stuff
        currentPlayer = pSwitch.currentPlayer;
        if (!pSwitch.DEBUG_MODE)
        {
            if (carHealth <= 0)
            {
                Debug.Log("KABOOM. Your car blew up! Player " + currentPlayer + " was eliminated!");
                pSwitch.RemovePlayer();
                carHealth = 100;
            }
        }

        //CONTROLS
        if (!pSwitch.playerWin)
        {
            brakingForce = -Input.GetAxis("Brake" + currentPlayer);
            accelerationForce = Mathf.Clamp(Input.GetAxis("Accelerate" + currentPlayer), 0.4f, 1.0f);
            print("currentPlayer: " + currentPlayer + ", accelerationForce: " + accelerationForce);
            x_Input = new Vector2(Input.GetAxis("Horizontal" + currentPlayer), Input.GetAxis("Vertical0"));
            //Hardcoded deadzone
            if (x_Input.magnitude < controllerDeadzone) x_Input = Vector2.zero;
            else x_Input = x_Input.normalized * ((x_Input.magnitude - controllerDeadzone) / (1 - controllerDeadzone));
        }
        else
        {
            rigid.constraints = RigidbodyConstraints.FreezeAll;
        }

    }

    private void FixedUpdate()
    {
        mph = (int)((rigid.velocity.magnitude * 10) / 2.5);
        data.CurrentMPH = mph;
        print(mph);
        float motor = maxMotorTorque * (accelerationForce * 3f);
        float steering = maxSteeringAngle * x_Input.x / ((150f - (mph * 0.75f)) / 150f);
        // ((1200f - (motor * 0.75f)) / 1200f) old motor calculation
        //print(steering);

		//Wwise engine sound! Sets RTPC "engine_pitch"
		AkSoundEngine.SetRTPCValue("engine_pitch", rigid.velocity.magnitude);

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            axleInfo.leftWheel.brakeTorque = brakingForce * maxBrakingTorque;
            axleInfo.rightWheel.brakeTorque = brakingForce * maxBrakingTorque;
            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
            if (!axleInfo.leftWheel.isGrounded && !axleInfo.rightWheel.isGrounded)
            {
                rigid.centerOfMass = newCom;
				AkSoundEngine.SetRTPCValue ("car_grounded", 70);
            }
            else
            {
                rigid.centerOfMass = originCom;
				AkSoundEngine.SetRTPCValue ("car_grounded", 60);
            }
            //Anti Roll Bar Code: DOESNT WORK
            //WheelHit hit = new WheelHit();
            //float travelL = 1f;
            //float travelR = 1f;
            //bool groundedL = axleInfo.leftWheel.GetGroundHit(out hit);
            //if (groundedL) travelL = (-axleInfo.leftWheel.transform.InverseTransformPoint(hit.point).y - axleInfo.leftWheel.radius) / axleInfo.leftWheel.suspensionDistance;
            //bool groundedR = axleInfo.rightWheel.GetGroundHit(out hit);
            //if (groundedR) travelR = (-axleInfo.rightWheel.transform.InverseTransformPoint(hit.point).y - axleInfo.rightWheel.radius) / axleInfo.rightWheel.suspensionDistance;
            //float antiRollForce = (travelL - travelR) * antiRollValue;
            //if (groundedL) rigid.AddForceAtPosition(axleInfo.leftWheel.transform.up * -antiRollForce, axleInfo.leftWheel.transform.position);
            //if (groundedR) rigid.AddForceAtPosition(axleInfo.rightWheel.transform.up * -antiRollForce, axleInfo.rightWheel.transform.position);
        }
        //Clamp that speed
        // rigid.velocity = new Vector3(Mathf.Clamp(rigid.velocity.x, -30, 30), Mathf.Clamp(rigid.velocity.y, -30, 30), Mathf.Clamp(rigid.velocity.z, -30, 30));
    }

    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (mph > 0 && mph < 50)
        {
			AkSoundEngine.PostEvent ("Play_LowSpeed_Impact", this.gameObject);
            Debug.Log("Boop. No Damage.");
        }
        else if (mph > 51 && mph < 80)
        {
			AkSoundEngine.PostEvent ("Play_temp_big_crash", this.gameObject);
            Debug.Log("Bonk. Minor Damage");
            carHealth -= 20;
        }
        else if (mph > 81 && mph < 110)
        {
			AkSoundEngine.PostEvent ("Play_temp_big_crash", this.gameObject);
            Debug.Log("CRRCH. Medium Damage");
            carHealth -= 40;
        }
        else if (mph > 111 && mph < 130)
        {
			AkSoundEngine.PostEvent ("Play_temp_big_crash", this.gameObject);
            Debug.Log("REEEEEEEEEEEEGFEGWFHE. High Damage");
            carHealth -= 60;
        }
        else
        {
            carHealth -= 100;
        }
    }
}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
}
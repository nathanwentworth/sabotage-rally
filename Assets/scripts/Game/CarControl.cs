﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;
using System;

public class CarControl : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // maximum steer angle the wheel can have
    public float maxBrakingTorque; //how fast should you brake
    public float controllerDeadzone = 0.15f;
    
    public GameObject shieldEffect;

    [Header("Data References")]
    private PlayerSwitching playerSwitch;
    private HUDManager hudManager;
    private AudioManager audioManager;

    private Vector2 x_Input;
    public int turningMultiplier;
    private float accelerationForce = 0;
    private float brakingForce = 0;
    private Rigidbody rigid;
    private int mph;

    public float MPH
    {
        get { return mph; }
    }


    private bool grounded;
    //Making this public for Powerup Reference
    [HideInInspector]
    public int currentIndex = 0;
    public bool shield;
    private bool invincible;
    private bool playing;
    private int trueCurrentIndex;

    [Header("Audio Bits")]
    public AudioSource carEngine;

    private Vector3 carOriginTrans;
    private bool currentlyCheckingIfCarIsStopped;

    private void Awake()
    {
        playerSwitch = GameObject.Find("GameSystem").GetComponent<PlayerSwitching>();
        hudManager = GameObject.Find("canvas-hud").GetComponent<HUDManager>();
        audioManager = GameObject.Find("AudioManagerObj [Level1]").GetComponent<AudioManager>();
    }

    private void Start()
    {
        turningMultiplier = 1;
        shield = false;
        currentlyCheckingIfCarIsStopped = false;
        rigid = GetComponent<Rigidbody>();
        carOriginTrans = transform.position;
    }

    private void Update()
    {
        shieldEffect.SetActive(shield);
        if (Time.timeScale == 1) { playing = true; } else { playing = false; }

        if (playing)
        {
            if (Input.GetKeyDown(KeyCode.Space) && mph < 2 && !playerSwitch.playerWin)
            {
                transform.rotation = Quaternion.identity;
                rigid.velocity = Vector3.zero;
                transform.position = carOriginTrans;
            }
            trueCurrentIndex = playerSwitch.currentIndex;
            if (DataManager.CurrentGameMode == DataManager.GameMode.Party) { currentIndex = trueCurrentIndex; }
            else { currentIndex = 0; }

            //CONTROLS
            if (!playerSwitch.playerWin)
            {
                InputDevice controller = null;
                if (playerSwitch.DEBUG_MODE) { controller = InputManager.ActiveDevice; }
                else { controller = DataManager.PlayerList[currentIndex].Controller; }
                brakingForce = controller.LeftTrigger.Value;
                accelerationForce = Mathf.Clamp(controller.RightTrigger.Value, 0.4f, 1.0f);
                x_Input = new Vector2(controller.Direction.X, controller.Direction.Y);
                //Hardcoded deadzone
                if (x_Input.magnitude < controllerDeadzone) x_Input = Vector2.zero;
                else x_Input = x_Input.normalized * turningMultiplier * ((x_Input.magnitude - controllerDeadzone) / (1 - controllerDeadzone));

                if (!grounded) {
                    Vector2 rotationalInput = new Vector2 (x_Input.y, x_Input.x); 
                    rigid.AddRelativeTorque(rotationalInput * 5000);
                }
            }
            else
            {
                rigid.constraints = RigidbodyConstraints.FreezeAll;
            }

        }


    }

    private void FixedUpdate()
    {
        mph = (int)((rigid.velocity.magnitude * 10) / 2.5);
        DataManager.CurrentMPH = mph;
        float motor = maxMotorTorque * (accelerationForce * 3f);
        float steering = maxSteeringAngle * x_Input.x / ((150f - (mph * 0.75f)) / 150f);

        if (mph < 1 && !currentlyCheckingIfCarIsStopped && !playerSwitch.passingController && !playerSwitch.startingGame && !hudManager.Paused && !playerSwitch.playerWin) {
            StartCoroutine(CheckIfCarIsStopped());
        }

        //Changes the pitch of the engine audioSource
        if (grounded) {
            carEngine.pitch = ((mph * 0.01f) - 0.3f);        
        } else {
            carEngine.pitch = ((mph * 0.01f));
        }

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
            IsGrounded(axleInfo.rightWheel, axleInfo.leftWheel);
        }
    }

    private void IsGrounded(WheelCollider right, WheelCollider left) {
        if (right.isGrounded && left.isGrounded) {
            grounded = true;
        } else {
            grounded = false;
        }
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
        if (!playerSwitch.DEBUG_MODE && !invincible)
        {
            if(mph > 65 && (other.gameObject.GetComponent<Rigidbody>() == null || other.gameObject.GetComponent<Rigidbody>().mass > 800))
            {
                if (!shield) {
                    int trueCurrentIndex = playerSwitch.currentIndex;
                    DataManager.PlayerList[trueCurrentIndex].Lives -= 1;
                    Debug.Log("Player " + DataManager.PlayerList[trueCurrentIndex].PlayerNumber.ToString() + " lost a life. They have " + DataManager.PlayerList[trueCurrentIndex].Lives + " lives remaining.");
                    // play high impact impact sound
                    StartCoroutine(audioManager.Impact(true));
                    hudManager.UpdateLivesDisplay();
                    StartCoroutine(DamageCooldown());
                    if(DataManager.PlayerList[trueCurrentIndex].Lives <= 0)
                    {
                        playerSwitch.RemovePlayer();
                    }
                } else {
                    shield = false;
                }
            } else {
                // play low speed impact sound
                StartCoroutine(audioManager.Impact(false));
            }
        }
    }

    IEnumerator DamageCooldown()
    {
        Debug.Log("Currently Invincible.");
        invincible = true;
        StartCoroutine("BlinkEffect");
        yield return new WaitForSeconds(1.5f);
        invincible = false;
        Debug.Log("No Longer Invincible.");
        StopCoroutine("BlinkEffect");
    }

    IEnumerator BlinkEffect()
    {
        bool isDisabled = false;
        Component[] renderers;
        renderers = GetComponentsInChildren<Renderer>();
        while (true)
        {
            if (!isDisabled)
            {
                foreach (Renderer child in renderers)
                {
                    child.enabled = false;
                    isDisabled = true;
                }
            }
            else
            {
                foreach (Renderer child in renderers)
                {
                    child.enabled = true;
                    isDisabled = false;
                }
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void ResetCarPosition() {
        float minD = 100000000;
        Transform closestSpawn = null;
        Transform carPos = transform;
        for (int i = 0; i < playerSwitch.spawnPoints.Length; i++) {
            float d = Vector3.Distance(playerSwitch.spawnPoints[i].transform.position, carPos.position);
            if (d < minD) {
                minD = d;
                closestSpawn = playerSwitch.spawnPoints[i].transform;
            }
        }
        transform.position = closestSpawn.transform.position;
        transform.rotation = closestSpawn.transform.rotation;
    }

    private IEnumerator CheckIfCarIsStopped() {
        currentlyCheckingIfCarIsStopped = true;
        Debug.Log("Checking to see if car is stopped");
        yield return new WaitForSeconds(1.5f);
        if (mph < 1) {
            Debug.Log("Car is stopped! Resetting position");
            ResetCarPosition();
        } else {
            Debug.Log("Car is not stopped, not resetting position");
        }
        currentlyCheckingIfCarIsStopped = false;
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
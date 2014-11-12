/*
 	Deriva
	An experimental/contemplative game

	Copyright 2014 Caio Hideki Matsumoto (hmcaio@hotmail.com)

	This file is part of Deriva.

		Deriva is free software: you can redistribute it and/or modify
		it under the terms of the GNU General Public License as published by
		the Free Software Foundation, either version 3 of the License, or
		(at your option) any later version.

		Deriva is distributed in the hope that it will be useful,
		but WITHOUT ANY WARRANTY; without even the implied warranty of
		MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
		GNU General Public License for more details.

		You should have received a copy of the GNU General Public License
		along with Deriva.  If not, see <http://www.gnu.org/licenses/>.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    #region FIELDS AND PROPERTIES

    public GameObject boostTrailPrefab;
    public enum PlayerState { IDLE, AUTO_PILOT, STRAIGHT, FREE, BOOST, SIGHTSEEING };
    public PlayerState state, oldState;
    public Vector3 force = new Vector3(0, 0, 10);
    public float mouseSensitivity = 5f;
    public float minX = -70;
    public float maxX = 70;
    public float turnSmoothing = 5f;
    public float minRot = -25, maxRot = 25;
    public float vDown, vUp;
    public float hDown, hUp;
    public float rotation = 1f;
    public float distance = 20f;
    public delegate void AutoPilotEvent();
    public event AutoPilotEvent OnAutoPilotOff;

    const int INPUT_MAX_DELTA = 100;
    const int ANGLE_MAX = 2;

    private float currentX, currentY;
    private Vector3 autoPilotDest;
    private Vector3 input;
    private Vector2 accelBase;

    private Touch t;
    private bool isTouching, prevIsTouching;
    private TouchType curTouch, prevTouch;
    private Vector2 touchDelta, startPos;
    private List<AreaTrigger> triggers;

    private delegate void Behaviour();
    private Behaviour behaviour;

    #endregion


    void Awake()
    {
        oldState = PlayerState.STRAIGHT;

        Application.targetFrameRate = 45;  //For debug purposes only
    }

    void OnEnable()
    {
        GameMaster.Instance.Player = gameObject;
    }

    void OnDestroy()//void OnDisable()
    {
        //if (GameMaster.Instance.player.Equals(gameObject))
        GameMaster.Instance.Player = null;
    }

    void Start()
    {
        triggers = new List<AreaTrigger>();
        OnAutoPilotOff += SetUpStraight;
        TurnOnAutoPilot(new Vector3(0, 3.977937f, 45));  //y = 3.977937 || 3.810657
    }

    void SetUpStraight()
    {
        state = PlayerState.STRAIGHT;

        OnAutoPilotOff -= SetUpStraight;
    }

    public void FinishedTutorial()
    {
        state = PlayerState.FREE;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE

        //For debug purposes only
        if (Input.GetKey(KeyCode.Space))
            force.z = 25;
        else
            force.z = 15;

        switch (state)
        {
            case PlayerState.IDLE:
                //Nothing for now

                break;
            case PlayerState.AUTO_PILOT:
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(autoPilotDest - transform.position), 1f);

                if (Vector3.Distance(transform.position, autoPilotDest) < 0.5f)
                {
                    state = PlayerState.IDLE;
                    if (OnAutoPilotOff != null)
                        OnAutoPilotOff();
                }

                break;
            case PlayerState.STRAIGHT:

                break;
            case PlayerState.FREE:
                currentX -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                currentY += Input.GetAxis("Mouse X") * mouseSensitivity;
                currentX = Mathf.Clamp(currentX, minX, maxX);
                transform.rotation = Quaternion.Euler(currentX, currentY, 0);

                break;
            case PlayerState.BOOST:

                break;
        }

#elif UNITY_WP8 || UNITY_ANDROID

        switch (state)
        {
            case PlayerState.IDLE:
                //Nothing

                break;
            case PlayerState.AUTO_PILOT:
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(autoPilotDest - transform.position), 0.5f);

                if (Vector3.Distance(transform.position, autoPilotDest) < 0.5f)
                {
                    //state = PlayerState.STRAIGHT;
                    state = PlayerState.IDLE;
                    if (OnAutoPilotOff != null)
                        OnAutoPilotOff();
                }

                break;
            case PlayerState.STRAIGHT:

                break;
            case PlayerState.FREE:
                isTouching = TouchInputManager.IsTouching;
                
                if (isTouching)
                {
                    if (!prevIsTouching)  //If it is the begining of the touch
                    {
                        //Save touch starting position
                        startPos = TouchInputManager.CurTouch.position;
                    }
                    else
                    {
                        //Calculate movement
                        touchDelta = TouchInputManager.CurTouch.position - startPos;  //Delta in screen units (pixels (?))
                        touchDelta.x = Mathf.Clamp(touchDelta.x, -INPUT_MAX_DELTA, INPUT_MAX_DELTA);
                        touchDelta.y = Mathf.Clamp(touchDelta.y, -INPUT_MAX_DELTA, INPUT_MAX_DELTA);

                        transform.Rotate(Vector3.up, touchDelta.x / 70, Space.Self);
                        transform.Rotate(Vector3.right, -touchDelta.y / 70, Space.Self);
                    }
                }
                else
                {
                    //Save accelerometer status for rotation reference
                    accelBase = new Vector2(Input.acceleration.x, Input.acceleration.y);
                    
                    state = PlayerState.SIGHTSEEING;
                }

                prevIsTouching = isTouching;

                break;
            case PlayerState.BOOST:

                break;
            case PlayerState.SIGHTSEEING:
                isTouching = TouchInputManager.IsTouching;
                
                if (!isTouching)
                {
                    input.x = CalcV(Input.acceleration.y);
                    input.y = CalcH(Input.acceleration.x);
                    gameObject.transform.Rotate(input * rotation * Time.deltaTime, Space.Self);
                }
                else
                {
                    //Save touch starting position
                    startPos = TouchInputManager.CurTouch.position;
                    
                    state = PlayerState.FREE;
                }
                
                prevIsTouching = isTouching;

                break;
        }
#endif
    }

    float CalcV(float input)
    {
        return Mathf.Clamp(
            ((maxRot - minRot) / (vUp + vDown)) * (input - accelBase.y - vUp) + maxRot, 
            minRot, maxRot);
    }

    float CalcH(float input)
    {
        return -Mathf.Clamp(
            ((maxRot - minRot) / (hUp + hDown)) * (input - accelBase.x - hUp) + maxRot, 
            minRot, maxRot);
    }

    void FixedUpdate()
    {
#if UNITY_EDITOR || UNITY_STANDALONE

        switch (state)
        {
            case PlayerState.IDLE:
                //Nothing

                break;
            case PlayerState.AUTO_PILOT:
                gameObject.rigidbody.AddRelativeForce(force, ForceMode.Acceleration);

                break;
            case PlayerState.STRAIGHT:
                gameObject.rigidbody.AddRelativeForce(force * Input.GetAxis("Vertical"), ForceMode.Acceleration);

                break;
            case PlayerState.FREE:
                gameObject.rigidbody.AddRelativeForce(force * Input.GetAxis("Vertical"), ForceMode.Acceleration);

                break;
            case PlayerState.BOOST:
                gameObject.rigidbody.AddRelativeForce(force * 25, ForceMode.Acceleration);

                break;
        }

#elif UNITY_WP8 || UNITY_ANDROID

        switch (state)
        {
            case PlayerState.IDLE:
                //Nothing

                break;
            case PlayerState.AUTO_PILOT:
                gameObject.rigidbody.AddRelativeForce(force, ForceMode.Acceleration);

                break;
            case PlayerState.STRAIGHT:
                if (TouchInputManager.IsTouching)
                    FixedMoveForward();

                break;
            case PlayerState.FREE:
                if (TouchInputManager.IsTouching)
                    FixedMoveForward();

                break;
            case PlayerState.BOOST:
                gameObject.rigidbody.AddRelativeForce(force * 20, ForceMode.Acceleration);

                break;
            case PlayerState.SIGHTSEEING:
                //Nothing
                
                break;
        }

#endif
    }

    public void FixedMoveForward()
    {
        gameObject.rigidbody.AddRelativeForce(force, ForceMode.Acceleration);
    }

//    public void MoveFree(bool isTouching)
//    {
//#if UNITY_EDITOR

//#elif UNITY_WP8 || UNITY_ANDROID
//        if (isTouching)
//        {
//            if (!prevIsTouching)  //If it is the begining of the touch
//            {
//                //Save touch starting position
//                startPos = TouchInputManager.CurTouch.position;
//            }
//            else
//            {
//                //Calculate movement
//                touchDelta = TouchInputManager.CurTouch.position - startPos;  //Delta in screen units (pixels (?))
//                touchDelta.x = Mathf.Clamp(touchDelta.x, -INPUT_MAX_DELTA, INPUT_MAX_DELTA);
//                touchDelta.y = Mathf.Clamp(touchDelta.y, -INPUT_MAX_DELTA, INPUT_MAX_DELTA);

//                transform.Rotate(Vector3.up, touchDelta.x / 70, Space.Self);
//                transform.Rotate(Vector3.right, -touchDelta.y / 70, Space.Self);
//            }
//        }

//        prevIsTouching = isTouching;
//#endif
//    }

    void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case Tags.ACTIVATOR:
                //After the player has reached the open area, destroy the track
                GameMaster.Instance.OnGameEvent(GameMaster.GameEvent.FREE_PLAYER);
                state = PlayerState.FREE;

                break;
            case Tags.ACTIVATOR2:
                //After the player has reached the open area, destroy the track
                GameMaster.Instance.OnGameEvent(GameMaster.GameEvent.ACTIVATOR2);

                break;
            case Tags.BOOST:
                //Create a new trail object and adjust some of its settings
                GameObject trail = (Instantiate(boostTrailPrefab) as GameObject);
                trail.transform.parent = gameObject.transform;
                trail.transform.localPosition = Vector3.zero;
                trail.transform.rotation = gameObject.transform.rotation;

                //Save state and change to BOOST state
                oldState = state;
                state = PlayerState.BOOST;

                //Start accelerating and showing trail
                StartCoroutine(BoostTimer(trail));

                break;
            case Tags.PORTAL_TRIGGER:
                TurnOnAutoPilot(other.transform.position);
                GameMaster.Instance.OnGameEvent(GameMaster.GameEvent.LEVEL_END);

                break;
        }
    }

    IEnumerator BoostTimer(GameObject trail)
    {
        //Acceleration
        for (float t = 0f; t < 0.2f; t += Time.deltaTime)
            yield return 0;  //yielding 0 => Wait for next frame

        //Go back to previous state
        state = oldState;

        //Waiting a little bit to slow down
        for (float t = 0f; t < 1.25f; t += Time.deltaTime)
            yield return 0;  //yielding 0 => Wait for next frame

        Destroy(trail);
    }

    public void TurnOnAutoPilot(Vector3 destPos)
    {
        autoPilotDest = destPos;
        oldState = state;
        state = PlayerState.AUTO_PILOT;
    }

    public void TurnOffAutoPilot()
    {
        if (state.Equals(PlayerState.AUTO_PILOT))
            state = oldState;
    }

    public void Add(AreaTrigger trigger)
    {
        if (triggers.Count == 0)
            TurnOffAutoPilot();
        triggers.Add(trigger);
    }

    public void Remove(AreaTrigger trigger)
    {
        triggers.Remove(trigger);
        if (triggers.Count == 0)
            TurnOnAutoPilot(trigger.transform.position);
    }
}

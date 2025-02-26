using System;
using KinematicCharacterController;
using UnityEngine;

public enum CharacterState
{
    Default,
    Drifting,
    InAir,
}
public struct PlayerInputs
{
    public float LeftWheelAxisForward;
    public float RightWheelAxisForward;

    public bool IsRightWheelMoving;
    public bool IsLeftWheelMoving;

    public bool IsLeftWheelHeld;
    public bool IsRightWheelHeld;
    
    public Quaternion CameraRotation;
}

public class CharacterController : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor _motor;

    [Header("Movement")]
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float maxStableMoveSpeed = 10f;
    [SerializeField] private float stableMoveSharpness = 15f; 
    private float _currentSpeed = 0f;
    
    [Header("Turning")]
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float orientationSharpness = 10f;
    [SerializeField] private float driftMultiplier = 1.5f;
    
    [Header("Drag")]
    [SerializeField] private Vector3 gravity = new Vector3(0, -30f, 0);
    [SerializeField] private float normalFriction = 0.98f;
    
    [Header("Drifting")]
    [SerializeField] private float driftSpeedThreshold = 10f;
    [SerializeField] private float driftFriction = 0.92f;
    private bool _isDrifting = false;

    [Header("Wheels")]
    [SerializeField] private Transform leftWheelPosition;
    [SerializeField] private Transform rightWheelPosition;
    private Vector3 _leftWheelPositionVector;
    private Vector3 _rightWheelPositionVector;

    [Header("Debugging")] 
    public bool DebugVector = false;

    private Vector3 _debugLW = Vector3.zero;
    private Vector3 _debugRW = Vector3.zero;
    
    private Vector3 _moveInputVector, _lookInputVector;

    public CharacterState CurrentCharacterState { get; private set; }

    void Start()
    {
        // Set initial state
        TransitionState(CharacterState.Default);
        
        // set up kinematic motor for this character controller
        _motor.CharacterController = this;

        if (leftWheelPosition != null && rightWheelPosition != null)
        {
            _leftWheelPositionVector = Vector3.ProjectOnPlane(leftWheelPosition.position - transform.position, _motor.CharacterUp);
            _rightWheelPositionVector = Vector3.ProjectOnPlane(rightWheelPosition.position - transform.position, _motor.CharacterUp);
        }
        else
        {
            throw new NullReferenceException("Add left wheel and right wheel position to model");
        }
    }

    void TransitionState(CharacterState newState)
    {
        CharacterState oldState = CurrentCharacterState;
        OnExitState(oldState, newState);
        CurrentCharacterState = newState;
        OnEnterState(newState, oldState);
    }

    void OnExitState(CharacterState state, CharacterState toState)
    {
        switch (state)
        {
            case CharacterState.Default:
            {
                break;
            }
            case CharacterState.Drifting:
            {
                break;
            }
            case CharacterState.InAir:
            {
                break;
            }
            
        }
    }

    void OnEnterState(CharacterState state, CharacterState fromState)
    {
        switch (state)
        {
            case CharacterState.Default:
            {
                break;
            }
            case CharacterState.Drifting:
            {
                break;
            }
            case CharacterState.InAir:
            {
                break;
            }
        }
    }

    public void SetInputs(ref PlayerInputs inputs)
    {
        // TODO: Drifting
        // There are some different ways to transition into drifting.
        // 1. One wheel is held
        //  I may want to see which direction the held to initialize
        //  the slip angle
        // 2. Two wheel held
        //  Should have more control of the slip angle.
        //  Also Do I want the player to have to hold both wheels and
        //  then apply force to decide which slip angle they want to
        //  take.
        //  In VR when moving the wheels there should always be a hold
        //  if even for a fraction of a second. This means that I need
        //  to find a specific condition for initializing the drift.
        //      I'm guessing that at a certain speed limit when you have
        //      opposite forces on the wheels greater than a specific
        //      magnitude should initialize the drift.
        //      Hold is there to continue the drift.
        
        // TODO: Drift Speed
        // Need to have a certain speed to initialize drifting.
        if (_currentSpeed > driftSpeedThreshold
            && ((inputs.IsLeftWheelHeld && inputs.RightWheelAxisForward != 0)
                || (inputs.IsRightWheelHeld && inputs.LeftWheelAxisForward != 0)))
        {
            _isDrifting = true;
        }
        
        // TODO: Drifting Exit
        // Need a way to transition out of drifting
        
        // Change state based on context of player
        // input and position
        if (!_motor.GroundingStatus.IsStableOnGround)
            TransitionState(CharacterState.InAir);
        else if (_isDrifting)
            TransitionState(CharacterState.Drifting);
        else
            TransitionState(CharacterState.Default);
        
        switch (CurrentCharacterState)
        {
            case CharacterState.Default:
            {
                SetInputsDefault(ref inputs);
                break;
            }
            case CharacterState.Drifting:
            {
                SetInputsDrifting(ref inputs);
                break;
            }
            case CharacterState.InAir:
            {
                SetInputsInAir(ref inputs);
                break;
            }
        }
        

    }

    private void SetInputsDefault(ref PlayerInputs inputs)
    {
        // Assign the wheel movement input to a vector based on the input magnitude
        // if the magnitude is zero then this is the zero vector
        Vector3 leftWheelMoveInput = inputs.LeftWheelAxisForward * leftWheelPosition.forward;
        Vector3 rightWheelMoveInput = inputs.RightWheelAxisForward * rightWheelPosition.forward;
        
        // if there is movement then orient that movement to the motor's plane and find
        // the difference between the wheels position and the characters position to get
        // vectors that point towards the characters forward and -forward vector extending
        // from the wheels.
        if (leftWheelMoveInput != Vector3.zero)
            leftWheelMoveInput = (leftWheelMoveInput - _leftWheelPositionVector);
        if (rightWheelMoveInput != Vector3.zero)
            rightWheelMoveInput = (rightWheelMoveInput - _rightWheelPositionVector);

        // stores the vectors to show them in the gizmos for debuging
        if (DebugVector)
        {
            _debugLW = leftWheelMoveInput;
            _debugRW = rightWheelMoveInput;
        }
        
        Vector3 moveInputVector = Vector3.ProjectOnPlane(leftWheelMoveInput + rightWheelMoveInput, _motor.CharacterUp).normalized;
        Vector3 wheelTurnDirection = Vector3.zero;

        // TODO
        // still working on turning
        /*if (Vector3.Dot(moveInputVector, Vector3.ProjectOnPlane(transform.forward, _motor.CharacterUp)) > 0)
        {
            wheelTurnDirection = moveInputVector.normalized - Vector3.ProjectOnPlane(transform.forward, _motor.CharacterUp);
        }
        else
        {
            wheelTurnDirection = moveInputVector.normalized - Vector3.ProjectOnPlane(-transform.forward, _motor.CharacterUp);
        }*/
        
        //Quaternion wheelRotation = Quaternion.LookRotation(wheelTurnDirection, _motor.CharacterUp);
        
        _moveInputVector = moveInputVector;
        _lookInputVector = wheelTurnDirection;
        
    }

    public void OnDrawGizmos()
    {
        if (DebugVector)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, _moveInputVector);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(leftWheelPosition.position, _debugLW);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rightWheelPosition.position, _debugRW);
        }
    }

    private void SetInputsDrifting(ref PlayerInputs inputs)
    {
        // TODO:
        // delete code when writing script
        _isDrifting = false;
    }

    private void SetInputsInAir(ref PlayerInputs inputs)
    {
        
    }
    
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        // TODO
        // This rotation is based on the character not the camera. Which is good
        // it is what I want anyway. The character is currently snapping to the
        // camera right now though.
        // 
        // I need to base the rotation on input.
        // 
        // The input also needs to be based on which characterState we are
        // currently in. If we are drifting the the rotation may not change alot.
        // When we are in the air I may be able to do some fancy rotations. Shouldn't
        // translate to VR though.
        if (_lookInputVector.sqrMagnitude > 0f && orientationSharpness > 0f)
        {
            // TODO
            // The look input vector is the vector the we are rotating too.
            // right now it is set up to follow the cameras look direction
            Vector3 smoothLookInputDirection = Vector3.Slerp(_motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-orientationSharpness * deltaTime)).normalized;
            
            currentRotation = Quaternion.LookRotation(smoothLookInputDirection, _motor.CharacterUp);
        }
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        _currentSpeed = currentVelocity.magnitude;
        // TODO: Future Planning
        // I do not think I should base velocity on the rotation. Eventually
        // The game will have problems in VR
        //
        // TODO:
        // I need to introduce momentum
        if (_motor.GroundingStatus.IsStableOnGround)
        {
            float currentVelocityMagnitude = currentVelocity.magnitude;
            Vector3 effectiveGroundNormal = _motor.GroundingStatus.GroundNormal;

            currentVelocity = _motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;
        
            Vector3 inputForward = Vector3.Cross(_moveInputVector, _motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputForward).normalized * _moveInputVector.magnitude;

            Vector3 targetMovementVelocity = reorientedInput * maxStableMoveSpeed;
        
            
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-stableMoveSharpness * deltaTime));
        }
        else
        {
            currentVelocity += gravity * deltaTime;
        }
        
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
        Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        
    }
}

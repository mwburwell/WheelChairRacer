using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Camera mainCamera;
    private PlayerInputHandler inputHandler;
    
    [Header("Movement")] 
    public float speed = 5f;
    public float topSpeed = 5f;
    public float turnSpeed = 10f;
    public float accelerationRate = 1f;
    
    public float lwActionTimer = 1f;
    private float _lwCounter = 0f;
    public float rwActionTimer = 1f;
    private float _rwCounter = 0f;
    
    public float decelerationRate = 1f;
    
    [Header("Forces")]
    private Vector3 _momentum = Vector3.zero;
    private Vector3 _centrifugalForce = Vector3.zero;

    private bool HoldingWheel = false;
    private bool _lW_HasAppliedForce = false;
    private bool _rW_HasAppliedForce = false;
    
    [Header("Debuging")]
    public bool DebugMode = false;
    public bool MovementOn = false;
    public bool TurningOn = false;
    public float DebugLogTimer = 2f;  // logger for every __ seconds
    private float countdown = 0f;
    /*private bool TimerActive = true;*/
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        inputHandler = PlayerInputHandler.Instance;
    }
    
    void Update()
    {
        if (inputHandler)
        {
            HandleMovement();
            // Movement_OldWay();
            HandleRotation();    
        }
        
    }

    void HandleMovement()
    {
        var changeInMomentum = Vector3.zero;
        
        if (inputHandler.LW_MoveValue > 0f || inputHandler.LW_MoveValue < 0f)
            _lW_HasAppliedForce = true;
        if (inputHandler.RW_MoveValue > 0f || inputHandler.RW_MoveValue < 0f)
            _rW_HasAppliedForce = true;


        // if 
        if (_lW_HasAppliedForce && lwActionTimer > _lwCounter)
        {
            Vector3 lwAcceleration = (inputHandler.LW_MoveValue * accelerationRate * transform.forward).normalized;
            changeInMomentum += _limitWheelTopSpeed(lwAcceleration);
            _lwCounter += Time.deltaTime;
        }
        if (_rW_HasAppliedForce && rwActionTimer > _rwCounter)
        {
            Vector3 rwAcceleration = (inputHandler.RW_MoveValue * accelerationRate * transform.forward).normalized;
            changeInMomentum += _limitWheelTopSpeed(rwAcceleration);
            _rwCounter += Time.deltaTime;
        }
        
        if (_momentum.magnitude > transform.forward.magnitude)
            changeInMomentum += _alignMomentumWithForwardVector();
        
        
        _momentum += changeInMomentum;
        
        
        // TODO
        // Add another main CENTRIFUGAL FORCE applied to movement aside from
        // momentum for sliding.
        
        
        // Debugging If Player can move then move the Player
        if (MovementOn)
        {
            characterController.Move(speed * Time.deltaTime * (_momentum + transform.forward + _centrifugalForce));
        }

        
        // reset applied Force Booleans and Wheel Action Timer Counters
        if (_lW_HasAppliedForce && inputHandler.LW_MoveValue == 0f)
        {
            _lW_HasAppliedForce = false;
            _lwCounter = 0f;
        }
        if (_rW_HasAppliedForce && inputHandler.RW_MoveValue == 0f)
        {
            _rW_HasAppliedForce = false;
            _rwCounter = 0f;
        }
    }

    private Vector3 _limitWheelTopSpeed(Vector3 force)
    {
        // If the current magnitude of momentum isn't at TopSpeed
        // the Player is allowed to apply force
        if ((_momentum + force).magnitude < topSpeed)
            return Time.deltaTime * force;
        
        // else no more force is applied
        return Vector3.zero;
    }

    private Vector3 _alignMomentumWithForwardVector()
    {
        if (Vector3.Dot(_momentum, transform.forward) > 0)
            return transform.forward.normalized - _momentum.normalized;
        
        return -transform.forward.normalized - _momentum.normalized;
    }

    void HandleRotation()
    {
        // TODO
        // Rotation notes:
        // MOMENTUM ROTATION RATE RELATIVITY
        // The rotation rate of the chair should be relative to the current momentum of the chair.
        // A larger momentum the harder to turn the vehicle.
        //
        // DISALLOW TURNING WHEN WHEEL IS HELD
        // If one of the wheels is being held should I even allow turning. If I remove the ability
        // to turn it would have a drifting effect, but the forward movement should change a little
        // into the direction of the turn.
        //
        // I will not disallow turning all together, but decouple the rig from the forward vector. This
        // will allow the vehicle to turn and only slightly affect the forward momentum vector.
        // There will be a maximum angle in which a vehicle can turn and that will be applied to the
        // momentum vector for drifting.
        //
        // DIRECTION OF MOMENTUM
        // The rig should not turn the forward vector the forward vector will always be the direction
        // of momentum.
        //
        // NEGATE FORWARD VECTORS
        // Forward vectors need to be negated to apply force in the opposite direction while turning
        // from their axis.
        //
        // NORMALIZE
        // do I need to explain further
       
        //if (Mathf.Abs(inputHandler.RW_MoveValue - inputHandler.LW_MoveValue) < 0.1f) return;
        
        // add the two input vectors together
        if (inputHandler.LW_HoldTriggered)
        {
            if (!HoldingWheel)
            {
                HoldingWheel = true;
                transform.Rotate(Vector3.up, -45 );
            }
            else
            {
                transform.Rotate(Vector3.up, -Vector3.Angle(transform.forward, _momentum) * _momentum.magnitude * Time.deltaTime);    
            }
            
        }
        else if (inputHandler.RW_HoldTriggered)
        {
            transform.Rotate(Vector3.up, inputHandler.LW_MoveValue * _momentum.magnitude * Time.deltaTime);
        }
        else
        {
            transform.Rotate(Vector3.up, -inputHandler.RW_MoveValue * turnSpeed * Time.deltaTime);
            transform.Rotate(Vector3.up, inputHandler.LW_MoveValue * turnSpeed * Time.deltaTime);
        }

        if (HoldingWheel && !inputHandler.RW_HoldTriggered && !inputHandler.LW_HoldTriggered)
        {
            HoldingWheel = false;
        }
        
        // forward vector drifts towards the momentum vector. This will be accomplished in
        // rotation handling.
        // This will show that the vehicle is trying to stay in line with momentum.
        /*float angleIntoMomentum = Vector3.SignedAngle(transform.forward, _momentum, Vector3.up);
        transform.Rotate(Vector3.up, angleIntoMomentum * turnSpeed * Time.deltaTime);
        
        if (DebugMode)
        {
            if (angleIntoMomentum > 0f)
            {
                Debug.Log($"Angle: {angleIntoMomentum}");
            }
            
        }*/
        //
        // This can be figured out later.
        // Something should be different if the momentum is pointed behind the vehicle.
        // If the vehicle forward is greater than 90 degrees from the momentum then it the
        // forward vector should try and point away from the momentum vector.
        // maybe a cap on 180 degrees. 

    }

    void Movement_OldWay()
    {
         // represents the push given by the wheels
        Vector3 acceleration = Vector3.zero;
        
        // First if Player is holding either wheel then we do slide mechanic
        if (inputHandler.RW_HoldTriggered || inputHandler.LW_HoldTriggered)
        {
            // If Player is holding a wheel and moving another wheel
            if (inputHandler.RW_MoveValue > 0 && !_rW_HasAppliedForce)
            {
                _rW_HasAppliedForce = true;

                acceleration = inputHandler.RW_MoveValue * accelerationRate * transform.forward.normalized;
            }
            else if (inputHandler.LW_MoveValue > 0 && !_lW_HasAppliedForce)
            {
                _lW_HasAppliedForce = true;
                
                acceleration =  inputHandler.LW_MoveValue * accelerationRate * transform.forward.normalized;
            }
        }
        // If Player is not holding wheel
        else
        {
            // If player is not holding a wheel then check to see if both wheels
            // are being applied the same input direction
            // Forward
            if (inputHandler.LW_MoveValue > 0 && inputHandler.RW_MoveValue > 0 && !_lW_HasAppliedForce &&
                !_rW_HasAppliedForce)
            {
                _lW_HasAppliedForce = true;
                _rW_HasAppliedForce = true;

                acceleration =  inputHandler.LW_MoveValue * accelerationRate * transform.forward.normalized;

            }
            // Backward
            else if (inputHandler.LW_MoveValue < 0 && inputHandler.RW_MoveValue < 0 && !_lW_HasAppliedForce &&
                     !_rW_HasAppliedForce)
            {
                _lW_HasAppliedForce = true;
                _rW_HasAppliedForce = true;

                acceleration =  inputHandler.RW_MoveValue * accelerationRate * transform.forward.normalized;
            }
        }
        
        // Apply the Forward and Backward force to the change in momentum if 
        // both wheels where applying the same directional force
        Vector3 changeInMomentum = ApplyForwardOrBackwardForce(acceleration);
        _momentum += changeInMomentum * Time.deltaTime;
        
        // Debugging If Player can move then move the Player
        if (MovementOn)
        {
            characterController.Move(speed * Time.deltaTime * _momentum);
        }

        
        // reset acceleration booleans
        if (inputHandler.LW_MoveValue == 0 && _lW_HasAppliedForce)
        {
            _lW_HasAppliedForce = false;
            
        }
        if (inputHandler.RW_MoveValue == 0 && _rW_HasAppliedForce)
        {
            _rW_HasAppliedForce = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (_momentum != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, _momentum);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward);
        }
    }

    // Allows the momentum vector to track and try to align with the Forward Vector
    // The dot product differentiates between the forward vector and the reverese vector.
    private Vector3 ApplyForwardOrBackwardForce(Vector3 force)
    {
        // if the dot product is positive then the momentum currently is in the forward
        // direction. If it is negative then it is in the backward direction
        // negate the forward.
        // The extra force between the forward and momentum vectors is also added to get
        // the momentum to point toward the forward vector.
        if (Vector3.Dot(_momentum, transform.forward) > 0)
            return transform.forward.normalized - _momentum.normalized + force;
        
        return -transform.forward.normalized - _momentum.normalized + force;
        
    }

    private static void DebugLogInput(string inputName, float value)
    {
        Debug.Log(inputName + " : " + value);
    }
    private static void DebugLogVector(string vectorName, Vector3 value)
    {
        Debug.Log($"{vectorName}: ({value.x} - {value.y} - {value.z} magnitude: {value.magnitude})");
    }
}

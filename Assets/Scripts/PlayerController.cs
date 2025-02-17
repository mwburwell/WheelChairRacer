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
    private Vector3 _momentum = Vector3.zero;
    
    
    
    private bool _lW_HasAppliedForce = false;
    private bool _rW_HasAppliedForce = false;
    
    [Header("Debuging")]
    public bool DebugMode = false;
    // momentum is still being increased
    public bool MovementOn = false;
    public bool TurningOn = false;
    public float DebugLogTimer = 2f;  // logger for every __ seconds
    private float countdown = 0f;
    private bool TimerActive = true;
    
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
            HandleRotation();    
        }
        
    }

    void HandleMovement()
    {
        Vector3 acceleration = Vector3.zero;

        if (inputHandler.LW_MoveValue > 0 && inputHandler.RW_MoveValue > 0 && !_lW_HasAppliedForce && !_rW_HasAppliedForce)
        {
            _lW_HasAppliedForce = true;
            _rW_HasAppliedForce = true;
            
            acceleration = new Vector3(0f, 0f, inputHandler.LW_MoveValue * accelerationRate);
            
        }
        else if (inputHandler.LW_MoveValue < 0 && inputHandler.RW_MoveValue < 0 && !_lW_HasAppliedForce && !_rW_HasAppliedForce)
        {
            _lW_HasAppliedForce = true;
            _rW_HasAppliedForce = true;
            
            acceleration = new Vector3(0f, 0f, inputHandler.LW_MoveValue * accelerationRate);
        }
        
        // TODO
        // always the first forward acceleration applies more force than all others
        // that may be a bug with the DeltaTime value
        // TODO
        // Need to change this so it always applies the forward acceleration in the way
        // the player is facing or the transform.forward of this object. (once I have
        // the rotation fixed)
        Vector3 changeInMomentum = Vector3.zero;
        
        // forward vector and momentum vector need to try and come together
        // momentum vector needs to drift toward the forward vector
        
        
        if (acceleration != Vector3.zero)
        {
            if (Mathf.Abs(Vector3.Angle(_momentum, transform.forward)) > 1f)
                changeInMomentum = _momentum + (_momentum - transform.forward) + acceleration;
            else
            {
                changeInMomentum = _momentum + acceleration;
            }


            if ((_momentum + changeInMomentum).magnitude < topSpeed)
            {
                _momentum += changeInMomentum * Time.deltaTime;
            }
        }
        
        
        if (MovementOn)
        {
            characterController.Move((_momentum) * speed * Time.deltaTime);
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

        if (DebugMode)
        {
            if (DebugLogTimer < countdown)
            {
                countdown = 0f;
                DebugLogVector("Momentum Vector", _momentum);
                DebugLogVector("Forward Vector", transform.forward);
                DebugLogVector("Change Momentum Vector", changeInMomentum);
            }

            countdown += Time.deltaTime;
        }
            
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
       
        if (Mathf.Abs(inputHandler.RW_MoveValue - inputHandler.LW_MoveValue) < 0.1f) return;
        
        // add the two input vectors together
        transform.Rotate(Vector3.up, -inputHandler.RW_MoveValue * turnSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up, inputHandler.LW_MoveValue * turnSpeed * Time.deltaTime);
        
        // forward vector drifts towards the momentum vector. This will be accomplished in
        // rotation handling.
        // This will show that the vehicle is trying to stay in line with momentum.
        //
        // This can be figured out later.
        // Something should be different if the momentum is pointed behind the vehicle.
        // If the vehicle forward is greater than 90 degrees from the momentum then it the
        // forward vector should try and point away from the momentum vector.
        // maybe a cap on 180 degrees. 

    }

    void OnDrawGizmosSelected()
    {
        if (_momentum != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, _momentum);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward);
        }
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

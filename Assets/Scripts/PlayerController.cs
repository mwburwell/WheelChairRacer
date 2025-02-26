using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public float decelerationRate = 1f;
    
    [Header("Forces")]
    private Vector3 _momentum = Vector3.zero;
    private bool HoldingWheel = false;
    private Vector3 _centrifugalForce = Vector3.zero;
    private GameObject _centrifugalPoint;
    
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
        if (!inputHandler) throw new UnityException("Input handler not found");
        
        HandleMovement();
        HandleRotation();
        
    }

    void HandleMovement()
    {
        var changeInMomentum = Vector3.zero;
        
        Vector3 lwAcceleration = inputHandler.LW_MoveValue != 0 
            ? inputHandler.LW_MoveValue * accelerationRate * transform.forward.normalized 
            : Vector3.zero;
        
        Vector3 rwAcceleration = inputHandler.RW_MoveValue != 0
            ? inputHandler.RW_MoveValue * accelerationRate * transform.forward.normalized
            : Vector3.zero;
        
        // Momentum
        changeInMomentum += _limitWheelTopSpeed(lwAcceleration + rwAcceleration);
        // TODO
        // Need to put some lag behind the alignment to the Forward vector for sliding
        // 
        // What I really need to do is to half the amount the momentum follows the forward
        // magnitude when the hold wheel drift is active
        //
        // Time.deltaTime gives a little lag for the momentum snapping to the forward vector
        // Exp is raising the constant e to the power of the momentum, greater momentum means
        // more push towards the forward vector
        if (_momentum.magnitude > 0f)
            changeInMomentum += Time.deltaTime * Mathf.Exp(_momentum.magnitude) * _alignMomentumWithForwardVector();

        // Centrifugal Force for drifting
        // Force points on the outward direction
        // F = m w^2 r
        // mass can be 1
        // I know radius
        // what is the angular velocity
        if (inputHandler.LW_HoldTriggered)
        {
            if (!_centrifugalPoint)
            {
                _centrifugalPoint = new GameObject("CentrifugalPoint");

                // TODO
                // the point is related to the centrifugal force equation.
                // do more research.
                _centrifugalPoint.transform.position = (_momentum.magnitude * 2 * Vector3.Cross(transform.forward, transform.up)) + transform.position; 
            }
            
            _centrifugalForce = (transform.position - _centrifugalPoint.transform.position) / 2;
            
        }
        else if (inputHandler.RW_HoldTriggered)
        {
            
        }
        
        _momentum += changeInMomentum;
        
        // Debugging If Player can move then move the Player
        if (MovementOn)
        {
            // characterController.Move(speed * Time.deltaTime * (_momentum ));
        }
        
        // clean up
        if (_centrifugalPoint && !inputHandler.LW_HoldTriggered && !inputHandler.RW_HoldTriggered)
        {
            Destroy(_centrifugalPoint.gameObject);
            _centrifugalForce = Vector3.zero;
        }
    }
    
    void HandleRotation()
    {
        // TODO
        // added a division by the momentum magnitude to simulate being harder to turn
        // at greater speeds MAY NEED TO CHANGE
        //
        // May also work weird when there is no magnitude... divide by zero you know
        
        if (!inputHandler.RW_HoldTriggered && !inputHandler.LW_HoldTriggered)
        {
            transform.Rotate(Vector3.up, inputHandler.LW_MoveValue * turnSpeed * Time.deltaTime);
            transform.Rotate(Vector3.up, -inputHandler.RW_MoveValue * turnSpeed * Time.deltaTime);
        }
        else
        {
            //transform.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(0, Vector3.SignedAngle(transform.forward, _centrifugalForce, transform.up), 0), turnSpeed);
            transform.Rotate(Vector3.up,  -Time.deltaTime * (Vector3.SignedAngle(_momentum.normalized, _centrifugalForce.normalized, transform.up)));
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, _momentum);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, _centrifugalForce);
        
        if (_centrifugalPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_centrifugalPoint.transform.position, 0.4f);
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

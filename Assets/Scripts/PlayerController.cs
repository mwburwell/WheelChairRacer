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
    public float decelerationRate = 1f;
    
    [Header("Forces")]
    private Vector3 _momentum = Vector3.zero;
    private Vector3 _centrifugalForce = Vector3.zero;
    private bool HoldingWheel = false;
    
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
            HandleRotation();    
        }
        
    }

    void HandleMovement()
    {
        var changeInMomentum = Vector3.zero;

        Vector3 lwAcceleration = (inputHandler.LW_MoveValue * accelerationRate * transform.forward).normalized;
        Vector3 rwAcceleration = (inputHandler.RW_MoveValue * accelerationRate * transform.forward).normalized;
        changeInMomentum += _limitWheelTopSpeed(lwAcceleration + rwAcceleration);
       
        // TODO
        // Need to put some lag behind the alignment to the Forward vector for sliding
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
        // added a division by the momentum magnitude to simulate being harder to turn
        // at greater speeds MAY NEED TO CHANGE
        //
        // May also work weird when there is no magnitude... divide by zero you know
        
        transform.Rotate(Vector3.up, inputHandler.LW_MoveValue * turnSpeed * Time.deltaTime);
        //transform.Rotate(Vector3.up, inputHandler.LW_MoveValue * (turnSpeed / (_momentum.magnitude > 1f ? _momentum.magnitude : 1f)) * Time.deltaTime);
    
        transform.Rotate(Vector3.up,-inputHandler.RW_MoveValue * turnSpeed  * Time.deltaTime);
        //transform.Rotate(Vector3.up,-inputHandler.RW_MoveValue * (turnSpeed / (_momentum.magnitude > 1f ? _momentum.magnitude : 1f)) * Time.deltaTime);
        
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, _momentum);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward);
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

using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float _defaultDistance = 6f,
        _minDistance = 3f,
        _maxDistance = 10f,
        
        _distanceMovementSpeed = 5f,
        _distanceMovementSharpness = 10f,
        _rotationSpeed = 10f,
        _rotationSharpness = 10000f,
        _followSharpness = 10000f,
        
        _defaultVerticalAngle = 20f,
        _minVerticalAngle = -90f,
        _maxVerticalAngle = 90f;

    private Transform _followTransform;
    private Vector3 _currentFollowPosition, _planarDirection;
    private float _targetVerticalAngle;
    
    private float _currentDistance, _targetDistance;

    private void Awake()
    {
        _currentDistance = _defaultDistance;
        _targetDistance = _currentDistance;
        _targetVerticalAngle = 0f;
        _planarDirection = Vector3.forward;
    }

    public void SetFollowTransform(Transform t)
    {
        _followTransform = t;
        _currentFollowPosition = t.position;
        _planarDirection = t.forward;
    }

    private void OnValidate()
    {
        _defaultDistance = Mathf.Clamp(_defaultDistance, _minDistance, _maxDistance);
        _defaultVerticalAngle = Mathf.Clamp(_defaultVerticalAngle, _minVerticalAngle, _maxVerticalAngle);
    }

    private void HandleRotationInput(float deltaTime, Vector3 rotationInput, out Quaternion targetRotation)
    {
        Quaternion rotationFromInput = Quaternion.Euler(_followTransform.up * (rotationInput.x * _rotationSpeed));
        _planarDirection = rotationFromInput * _planarDirection;
        Quaternion planarRotation = Quaternion.LookRotation(_planarDirection, _followTransform.up);

        _targetVerticalAngle -= (rotationInput.y * _rotationSpeed);
        _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, _minVerticalAngle, _maxVerticalAngle);
        Quaternion verticalRotation = Quaternion.Euler(_targetVerticalAngle, 0f, 0f);
        
        targetRotation = Quaternion.Slerp(transform.rotation, planarRotation * verticalRotation, _rotationSharpness * deltaTime);
        
        transform.rotation = targetRotation;
    }

    private void HandlePosition(float deltaTime, float zoomInput, Quaternion targetRotation)
    {
        _targetDistance += zoomInput * _distanceMovementSpeed;
        _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
        
        _currentFollowPosition = Vector3.Lerp(_currentFollowPosition, _followTransform.position, 1f - Mathf.Exp(-_followSharpness * deltaTime));
        Vector3 targetPosition = _currentFollowPosition - ((targetRotation * Vector3.forward) * _currentDistance);
        
        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, 1f - Mathf.Exp(-_distanceMovementSharpness * deltaTime));
        transform.position = targetPosition;
    }

    public void UpdateWithInput(float deltaTime, float zoomInput, Vector3 rotationInput)
    {
        if (_followTransform)
        {
            HandleRotationInput(deltaTime, rotationInput, out Quaternion targetRotation);
            HandlePosition(deltaTime, zoomInput, targetRotation);
        }
    }
}

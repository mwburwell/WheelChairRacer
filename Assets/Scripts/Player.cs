using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private PlayerCamera _playerCamera;
    [SerializeField]
    private Transform _cameraFollowPoint;
    [SerializeField]
    private CharacterController _characterController;
    
    private Vector3 _lookInputVector;

    private PlayerInputHandler _inputHandler;
    private void Start()
    {
        _playerCamera.SetFollowTransform(_cameraFollowPoint);
        _inputHandler = PlayerInputHandler.Instance;
    }
    
    private void Update()
    {
        HandleCharacterInputs();
    }

    private void HandleCharacterInputs()
    {
        PlayerInputs inputs = new PlayerInputs();
        inputs.LeftWheelAxisForward = _inputHandler.LW_MoveValue;
        inputs.RightWheelAxisForward = _inputHandler.RW_MoveValue;
        
        // TODO
        // To replicate the controls through VR controllers I should change
        // The Inputs to have to hold first then you can move the wheel.
        inputs.IsLeftWheelMoving = inputs.LeftWheelAxisForward != 0 ? true : false;
        inputs.IsRightWheelMoving = inputs.RightWheelAxisForward != 0 ? true : false;

        inputs.IsLeftWheelHeld = inputs.IsLeftWheelHeld;
        inputs.IsRightWheelHeld = inputs.IsRightWheelHeld;
        
        inputs.CameraRotation = _playerCamera.transform.rotation;
        
        _characterController.SetInputs(ref inputs);

    }

    private void HandleCameraInput()
    {
        float mouseUp = _inputHandler.MouseVerticalValue;
        float mouseRight = _inputHandler.MouseHorizontalValue;
        
        _lookInputVector = new Vector3(mouseRight, mouseUp, 0f);
        
        float scrollInput = _inputHandler.MouseScrollValue;
        _playerCamera.UpdateWithInput(Time.deltaTime, scrollInput, _lookInputVector);
    }

    private void LateUpdate()
    {
        HandleCameraInput();
    }
    
}

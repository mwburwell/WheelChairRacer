using System;

using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action Asset")] [SerializeField]
    private InputActionAsset PlayerControls;
    
    [Header("Action Map Name References")] [SerializeField]
    private string ActionMapName = "Player";

    [Header("Action Name References")] 
    [SerializeField] private string LW_Move = "MoveLW";
    [SerializeField] private string RW_Move = "MoveRW";
    [SerializeField] private string LW_Hold = "HoldLW";
    [SerializeField] private string RW_Hold = "HoldRW";

    private InputAction LW_MoveAction;
    private InputAction RW_MoveAction;
    private InputAction LW_HoldAction;
    private InputAction RW_HoldAction;
    
    public float LW_MoveValue {get; private set;}
    public float RW_MoveValue {get; private set;}
    public bool LW_HoldTriggered {get; private set;}
    public bool RW_HoldTriggered {get; private set;}
    
    public static PlayerInputHandler Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        LW_MoveAction = PlayerControls.FindActionMap(ActionMapName).FindAction(LW_Move);
        RW_MoveAction = PlayerControls.FindActionMap(ActionMapName).FindAction(RW_Move);
        LW_HoldAction = PlayerControls.FindActionMap(ActionMapName).FindAction(LW_Hold);
        RW_HoldAction = PlayerControls.FindActionMap(ActionMapName).FindAction(RW_Hold);
        
        RegisterInputActions();
    }

    void RegisterInputActions()
    {
        LW_MoveAction.performed += ctx => LW_MoveValue = ctx.ReadValue<float>();
        LW_MoveAction.canceled += ctx => LW_MoveValue = 0f;
        
        RW_MoveAction.performed += ctx => RW_MoveValue = ctx.ReadValue<float>();
        RW_MoveAction.canceled += ctx => RW_MoveValue = 0f;

        LW_HoldAction.started += ctx => LW_HoldTriggered = true;
        LW_HoldAction.canceled += ctx => LW_HoldTriggered = false;
        
        RW_HoldAction.started += ctx => RW_HoldTriggered = true;
        RW_HoldAction.canceled += ctx => RW_HoldTriggered = false;
    }

    private void OnEnable()
    {
        LW_MoveAction.Enable();
        RW_MoveAction.Enable();
        LW_HoldAction.Enable();
        RW_HoldAction.Enable();
    }

    private void OnDisable()
    {
        LW_MoveAction.Disable();
        RW_MoveAction.Disable();
        LW_HoldAction.Disable();
        RW_HoldAction.Disable();
    }
    
    
}

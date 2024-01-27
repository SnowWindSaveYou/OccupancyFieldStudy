using OccupancyFieldStudy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static OccupancyFieldStudy.OccEditManager;

public class RoiEditManager : MonoSingleton<RoiEditManager>
{
    #region [enums]
    internal enum ControlStyle
    {
        None,
        KeyboardMouse,
        VR
    }

    #endregion

    #region [ States ]
    private bool _isEditProcessing = false;
    public bool _blockEditOnMissingHit = true;

    private EditToolType _currentToolType = EditToolType.None;
    private IEditTool _currentEditTool;

    public bool IsEditProcessing
    {
        get => _isEditProcessing;
    }
    public EditToolType CurrentTool
    {
        get => _currentToolType;
    }
    public IEditTool CurrentEditTool
    {
        get => _currentEditTool;
    }

    public float BrushSize = 0.3f;

    #endregion [states]


    public PlayerInput playerInput;
    public OccEditManager occManager;
    public OccupancyFieldGraphObject OccEditObj;
    private Dictionary<EditToolType, IEditTool> _editToolDict = new Dictionary<EditToolType, IEditTool>();


    public void SetCurrentTool(EditToolType editToolType)
    {
        if (_isEditProcessing)
        {
            Debug.LogError("Current Tool is Processing, reject to change");
        }
        if (CurrentEditTool != null)
            CurrentEditTool.OnExit();
        _currentToolType = editToolType;
        _currentEditTool = _editToolDict[editToolType];
        _currentEditTool.OnEnter();
        EventsWatcher.Dispatch(MessageIds.OnEditToolChanged);
    }

    private bool IsEditableState()
    {
        if (_isEditProcessing
            ||IsPointerOverUI()
            || !IsPointerInsideScreen()
            )
            return false;
        if (_blockEditOnMissingHit)
            return false;
        return true;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    ////REVIEW: check this together with the focus PR; ideally, the code here should not be necessary
    private bool IsPointerInsideScreen()
    {
        var pointer = playerInput.GetDevice<Pointer>();
        if (pointer == null)
            return true;

        return Screen.safeArea.Contains(pointer.position.ReadValue());
    }



    #region [perform edit]
    private void handleOnEditBegin(InputAction.CallbackContext ctx)
    {
        if (!IsEditableState()) return;
        _isEditProcessing = true;
        if (CurrentEditTool != null) CurrentEditTool.OnEditBegin();
    }

    private void handleOnEditEnd(InputAction.CallbackContext ctx)
    {
        if (_isEditProcessing == false) return;
        _isEditProcessing = false;
        if (CurrentEditTool != null ) CurrentEditTool.OnEditEnd();
    }

    private void handleOnEditProcess()
    {
        if (_isEditProcessing && CurrentEditTool != null ) CurrentEditTool.OnEditProcessing();
    }
    #endregion

    #region [handle input actions]
    private void SetupInputActions()
    {
        playerInput.onControlsChanged+= handleOnControlerChanged;
        var editAction = playerInput.actions["EditInput"];
        editAction.started += handleOnEditBegin;
        editAction.canceled += handleOnEditEnd;
    }
    private void handleOnControlerChanged(PlayerInput input)
    {
        //TODO Desktop & VR
    }
    #endregion [handle input actions]

    #region [life cycle]

    private void Start()
    {
        SetupInputActions();
    }

    private void FixedUpdate()
    {

        handleOnEditProcess();

    }

    private void Update()
    {
        if (CurrentEditTool != null)
        {
            CurrentEditTool.OnUpdate();
        }
    }

    #endregion
}

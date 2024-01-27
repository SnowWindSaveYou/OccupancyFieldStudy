using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CursorGraphObject : MonoBehaviour
{


    public enum INPUT_TYPE
    {
        XRInteractor = 0,
        Mouse = 1,
    }
    public enum INTERACT_TYPE
    {
        AttachToScreenPlane,
        AttachToCurrVoi,
    }
    public enum SHAPE_TYPE
    {
        None,
        Sphere,
    }

    public class CursorSettingParam
    {
        public List<KeyValuePair<INPUT_TYPE, Func<bool>>> InteractPriorities = new List<KeyValuePair<INPUT_TYPE, Func<bool>>>();
        public List<KeyValuePair<INPUT_TYPE, Func<bool>>> InteractPrioritiesOnEditing = new List<KeyValuePair<INPUT_TYPE, Func<bool>>>();
        public SHAPE_TYPE Shape = SHAPE_TYPE.None;
        public bool BlockEditWhenMissAttaching = true;
    }
    private CursorSettingParam _cursorSettings;
    public CursorSettingParam CursorSettings
    {
        get => _cursorSettings;
        set { SetCursorSetting(value); }
    }
    #region [states]
    public INPUT_TYPE InputType;
    public Vector3 _currAttachNormal = Vector3.forward;
    public Vector3 _currAttachPosition = Vector3.zero;
    bool _missAttaching = false;
    float _preDist = 0;
    float _offsetZ = 0;
    RaycastHit _currentHitInfo;
    #endregion [states]

    public static CursorGraphObject Instance;

    public GameObject SphereNotifier;
    public Material BrushMaterial;
    public Renderer CursorRenderer;

    public void SetCursorSetting(CursorSettingParam cursorSettingParam)
    {
        _cursorSettings = cursorSettingParam;
        // notifier shape
        SphereNotifier.gameObject.SetActive(cursorSettingParam.Shape== SHAPE_TYPE.Sphere);
    }


    void handleUpdateCursorTransform()
    {
        if (_cursorSettings == null)
            return;
        var rem = RoiEditManager.Instance;
        List<KeyValuePair<INPUT_TYPE, Func<bool>>> prio;
        if (rem.IsEditProcessing){
            prio = _cursorSettings.InteractPrioritiesOnEditing.Where(s => s.Key == this.InputType).ToList();
        }else{
            prio = _cursorSettings.InteractPriorities.Where(s => s.Key == this.InputType).ToList();
        }
        if (prio.Count() == 0)
            Debug.LogError("Miss setting the action of cursor ");
        for (int i = 0; i < prio.Count(); i++)
        {
            _missAttaching = !prio[i].Value();
            if (_missAttaching == false)
                break;
        }

        if (_missAttaching)
        {
            rem._blockEditOnMissingHit = _cursorSettings.BlockEditWhenMissAttaching;
        }
        else
        {
            rem._blockEditOnMissingHit = false;
        }
        this.transform.position = _currAttachPosition;
        this.transform.forward = _currAttachNormal;

    }

    #region [attach actions]
    public bool MakeMouseAttachToScreenPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float currDist = _preDist;//开始时设置
        var mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (mouseScroll != 0)
        {
            currDist += mouseScroll / 10.0f;
        }
        _currAttachPosition = ray.GetPoint(currDist);
        _preDist = currDist;
        return true;
    }

    public bool MakeMouseAttachToCurrentVoi()
    {
        var voi = RoiManager.Instance.currentRoi;
        if (voi != null)
        {
            var voiCollider = voi.GetComponent<Collider>();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // try mouse interaction
            if (voiCollider.Raycast(ray, out _currentHitInfo, 10))
            {
                _currAttachPosition = _currentHitInfo.point + (_offsetZ) * ray.direction;
                _currAttachNormal = -_currentHitInfo.normal;
                _preDist = _currentHitInfo.distance;
                return true;
            }
        }
        return false;
    }
    #endregion [attach actions]

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        handleUpdateCursorTransform();
    }
}

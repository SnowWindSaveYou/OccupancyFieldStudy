using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CursorControler : MonoBehaviour
{
    public Collider targetCollider;

    public bool _mouseDraging = false;
    float _preDist = 7;
    //float _offsetZ = 0;
    bool _isEditing = false;
    Vector3 _preMousePos;
    public bool MissAttaching = true;

    Vector3 _currAttachNormal = Vector3.forward;


    public UnityEvent OnDragBegin;
    public UnityEvent OnDraging;
    public UnityEvent OnDragEnd;

    public UnityEvent<Vector3> OnTranslate;
    public UnityEvent<float> OnScale;

    bool MakeMouseAttachToCurrentVoi()
    {
        if (targetCollider != null)
        {
            RaycastHit hit;
            // try mouse interaction
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (targetCollider.Raycast(ray, out hit, 10))
            {
                this.transform.position = hit.point;
                _currAttachNormal = -hit.normal;
                _preDist = hit.distance;
                MissAttaching = false;
                return true;
            }
        }
        MissAttaching = true;
        return false;
    }

    void MakeMouseAttachToScreenPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float currDist = _preDist;//??????????
        var mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (mouseScroll != 0)
        {
            currDist += mouseScroll;
        }
        this.transform.position = ray.GetPoint(currDist);
        _preDist = currDist;
        MissAttaching = false;
    }

    void HandleDragBegin()
    {
        _mouseDraging = true;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        _preDist = (this.transform.position - ray.origin).magnitude;
        OnDragBegin.Invoke();
    }
    void HandleDragEnd()
    {
        _mouseDraging = false;
        // reset monitor state
        _preMousePos = Input.mousePosition;
        OnDragEnd.Invoke();

    }
    void HandleDraging()
    {
        OnDraging.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {
        OnTranslate?.Invoke(this.transform.position);
        OnScale?.Invoke(this.transform.lossyScale.x);
    }

    // Update is called once per frame
    void Update()
    {
        if (this.transform.hasChanged)
        {
            this.transform.hasChanged = false;
            OnTranslate?.Invoke(this.transform.position);
            OnScale?.Invoke(this.transform.lossyScale.x/2);
        }

        if (!_mouseDraging)
        {
            MakeMouseAttachToCurrentVoi();
        }
        else
        {
            MakeMouseAttachToScreenPlane();
            HandleDraging();
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            HandleDragBegin();
        }
        if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            HandleDragEnd();
        }
        // scale scene size
        var mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKey(KeyCode.LeftControl) &&mouseScroll != 0)
        {
            this.transform.localScale += Vector3.one * this.transform.localScale.x * 0.5f * mouseScroll;
        }


    }
}

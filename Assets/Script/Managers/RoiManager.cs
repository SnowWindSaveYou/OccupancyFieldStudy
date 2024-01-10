using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoiManager : MonoSingleton<RoiManager>
{
    
    public List<RoiGraphObject> RoiGraphObjectList;
    public RoiGraphObject currentRoi;

    public Action<RoiGraphObject> OnAddRoi;
    public Action<RoiGraphObject> OnRemoveRoi;


    public void CreateRoi(string roiName)
    {
        var go =GameObject.Instantiate( Resources.Load<GameObject>("Prefabs/RoiTemplate"));
        go.transform.SetParent(this.transform, true);
        var roi = go.GetComponent<RoiGraphObject>();
        roi.name = roiName;
        roi.Color = Color.white;
        OnAddRoi?.Invoke(roi);
    }

    public void RemoveRoi(RoiGraphObject roi)
    {
        if (currentRoi == roi) currentRoi = null;
        RoiGraphObjectList.Remove(roi);
        OnRemoveRoi.Invoke(roi);
        EventsWatcher.Dispatch(MessageIds.OnRemoveRoi,roi);
        Destroy(roi);
    }

    public void SetCurrentRoi(RoiGraphObject roi)
    {
        if (currentRoi == roi) return;
        if(currentRoi!=null) currentRoi.OnDisSelected();
        currentRoi = roi;
        if(roi!=null) roi.OnSelected();
        EventsWatcher.Dispatch(MessageIds.OnCurrentRoiChanged);
    }

    override public void Init()
    {
        if (RoiGraphObjectList == null) RoiGraphObjectList = new List<RoiGraphObject>();
    }
    private void Start()
    {
        var existRois = GameObject.FindObjectsOfType<RoiGraphObject>();
        foreach(var roi in existRois)
        {
            RoiGraphObjectList.Add(roi);
            OnAddRoi?.Invoke(roi);
        }
    }


}

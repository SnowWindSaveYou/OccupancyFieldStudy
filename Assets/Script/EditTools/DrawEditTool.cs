using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawEditTool : IEditTool
{
    CursorGraphObject.CursorSettingParam cursorSetting;

    public DrawEditTool()
    {
        var cursor = CursorGraphObject.Instance;
        cursorSetting = new CursorGraphObject.CursorSettingParam();
        cursorSetting.InteractPriorities.Add( new KeyValuePair<CursorGraphObject.INPUT_TYPE, Func<bool>>(CursorGraphObject.INPUT_TYPE.Mouse, cursor.MakeMouseAttachToCurrentVoi));
        cursorSetting.InteractPriorities.Add(new KeyValuePair<CursorGraphObject.INPUT_TYPE, Func<bool>>(CursorGraphObject.INPUT_TYPE.Mouse, cursor.MakeMouseAttachToScreenPlane));
        cursorSetting.InteractPrioritiesOnEditing.Add(new KeyValuePair<CursorGraphObject.INPUT_TYPE, Func<bool>>(CursorGraphObject.INPUT_TYPE.Mouse, cursor.MakeMouseAttachToScreenPlane));
        cursorSetting.Shape = CursorGraphObject.SHAPE_TYPE.Sphere;
    }
    public void OnEnter()
    {
        CursorGraphObject.Instance.CursorSettings = cursorSetting;
    }

    public void OnExit()
    {

    }

    public void OnEditBegin()
    {
        
    }

    public void OnEditEnd()
    {

    }

    public void OnEditProcessing()
    {

    }



    public void OnUpdate()
    {
  
    }
}

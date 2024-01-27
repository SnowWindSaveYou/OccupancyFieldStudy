using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEditTool 
{
    public void OnEnter();
    public void OnExit();
    public void OnUpdate();

    public void OnEditBegin();
    public void OnEditEnd();
    public void OnEditProcessing();
}

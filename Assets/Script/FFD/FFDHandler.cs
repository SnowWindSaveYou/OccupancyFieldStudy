using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FFDHandler : MonoBehaviour
{
    public struct FFDHandlerStruct
    {
        public Vector3 InitPos;
        public Vector3 CurrPos;
    }


    public Vector3 InitPos;
    public Action OnTransformChanged;
    // Update is called once per frame
    private void Start()
    {
        this.transform.hasChanged = false;//TODO
    }
    void Update()
    {
        if (this.transform.hasChanged)
        {
            HandleTransformChanged();
            this.transform.hasChanged = false;
        }
    }

    private void HandleTransformChanged()
    {
        OnTransformChanged?.Invoke();
    }

    public FFDHandlerStruct GetStruct()
    {
        FFDHandlerStruct a;

        a.InitPos = this.InitPos + 0.5f*Vector3.one;
        a.CurrPos = this.transform.localPosition + 0.5f * Vector3.one;

        return a;
    }


    //偏移量
    private Vector3 offset = Vector3.zero;
    private float z;
    private void OnMouseDown()
    {
        z = Vector3.Dot(transform.position - Camera.main.transform.position, Camera.main.transform.forward);
        //也可以是z= Camera.main.WorldToScreenPoint(transform.position).z;
        //debug一下就知道这两个z是一样的
        //保持鼠标的屏幕坐标的z值与被拖拽物体的屏幕坐标的z保持一致
        Vector3 v3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, z);
        //得到鼠标点击位置与物体中心位置的差值
        offset = transform.position - Camera.main.ScreenToWorldPoint(v3);
    }
    private void OnMouseDrag()
    {
        //时刻保持鼠标的屏幕坐标的z值与被拖拽物体的屏幕坐标保持一致
        Vector3 v3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, z);
        //鼠标的屏幕坐标转换为世界坐标+偏移量=物体的位置
        transform.position = Camera.main.ScreenToWorldPoint(v3) + offset;
    }
}

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


    //ƫ����
    private Vector3 offset = Vector3.zero;
    private float z;
    private void OnMouseDown()
    {
        z = Vector3.Dot(transform.position - Camera.main.transform.position, Camera.main.transform.forward);
        //Ҳ������z= Camera.main.WorldToScreenPoint(transform.position).z;
        //debugһ�¾�֪��������z��һ����
        //����������Ļ�����zֵ�뱻��ק�������Ļ�����z����һ��
        Vector3 v3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, z);
        //�õ������λ������������λ�õĲ�ֵ
        offset = transform.position - Camera.main.ScreenToWorldPoint(v3);
    }
    private void OnMouseDrag()
    {
        //ʱ�̱���������Ļ�����zֵ�뱻��ק�������Ļ���걣��һ��
        Vector3 v3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, z);
        //������Ļ����ת��Ϊ��������+ƫ����=�����λ��
        transform.position = Camera.main.ScreenToWorldPoint(v3) + offset;
    }
}

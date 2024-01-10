using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComputeShaderManager 
{
    static Dictionary<string, int> _property;
    public static int Prop(string key)
    {
        if (_property == null) _property = new Dictionary<string, int>();
        if(_property.ContainsKey(key))return _property[key];
        int id = Shader.PropertyToID(key);
        _property.Add(key, id);
        return id;
    }

    //public void Dispatch()

}

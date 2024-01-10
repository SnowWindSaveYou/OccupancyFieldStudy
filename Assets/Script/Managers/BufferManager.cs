using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferManager : MonoSingleton<BufferManager>
{
    private Dictionary<string,ComputeBuffer> _computeBuffers = new Dictionary<string, ComputeBuffer>();

    public ComputeBuffer GetBuffer(string key,int count, int stride, ComputeBufferType type = ComputeBufferType.Default)
    {
        if (_computeBuffers.ContainsKey(key))
        {
            var buffer = _computeBuffers[key];
            if (count > buffer.count)
            {
                buffer.Release();
                buffer = new ComputeBuffer(count, stride, type);
            }
            return buffer;
        }
        else
        {
            var buffer = new ComputeBuffer(count, stride, type);
            _computeBuffers.Add(key, buffer);
            return buffer;
        }
    }

    public void RemoveBuffer(string key)
    {
        if (_computeBuffers.ContainsKey(key))
        {
            _computeBuffers[key].Release();
            _computeBuffers.Remove(key);
        }
    }


    private void OnDestroy()
    {
        foreach(var kv in _computeBuffers)
        {
            kv.Value.Release();
        }
    }
}

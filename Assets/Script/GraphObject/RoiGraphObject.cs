using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OccupancyFieldStudy;


public class RoiGraphObject : MonoBehaviour
{
    private Color _color = Color.white;
    public Color Color
    {
        set { _color = value; }
        get => _color;
    }

    private bool _visible = true;
    public bool Visible
    {
        set { SetVisible( value); }
        get => _visible;
    }

    public SubVolumeEntry storedVolumeEntry;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    Vector3[] verticesList;
    Vector3[] normalsList;
    int[] indeciesList;


    public void SetVisible(bool isOn)
    {
        if (isOn == _visible) return;
        _visible = isOn;
        if (RoiManager.Instance.currentRoi == this)
        {

        }
        else
        {
            meshRenderer.enabled = isOn;
        }
    }
    public void SetColor(Color color)
    {
        if (color == _color) return;
        _color = color;
        meshRenderer.material.color = _color;
    }


    public void OnSelected()
    {

        VolumeHelper.Instance.ClearVolume(OccEditManager.Instance.targetOccObj.OccupanceTex);
        if (storedVolumeEntry == null) {
            storedVolumeEntry = new SubVolumeEntry();
            //RenderTexture.GetTemporary
        }
        else
        {
            // move stored data to edit space
            VolumeHelper.Instance.PasteVolume(OccEditManager.Instance.targetOccObj.OccupanceTex, storedVolumeEntry);
        }
        OccEditManager.Instance.targetOccObj.UpdateOccRenderer();
        this.meshRenderer.enabled = false;
    }
    public void OnDisSelected()
    {
        var occObj = OccEditManager.Instance.targetOccObj;
        //TODO store nonzero volume data
        VolumeHelper.Instance.CropVolume(occObj.OccupanceTex, storedVolumeEntry);
        var mesh = MarchingCubeFilter.Instance.GetMesh(occObj._mcOutput, Matrix4x4.identity, ref verticesList, ref normalsList, ref indeciesList);
        meshFilter.mesh = mesh;
        this.meshRenderer.enabled = Visible;
    }


    private void Start()
    {

    }

    private void OnDestroy()
    {
        if(storedVolumeEntry.volume!=null)
            ((RenderTexture)storedVolumeEntry.volume).Release();
        
    }
}

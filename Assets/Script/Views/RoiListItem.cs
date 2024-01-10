using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoiListItem : MonoBehaviour
{
    public Toggle _itemToggle;
    public Image _colorImage;
    public Text _nameText;
    public Toggle _visibleToggle;

    public RoiGraphObject _refRoi;

    public void SetRefRoi(RoiGraphObject roi)
    {
        _refRoi = roi;
        _nameText.text = roi.name;
        _colorImage.color = roi.Color;
        _visibleToggle.isOn = roi.Visible;
    }

    void handleSelected(bool isOn)
    {
        RoiManager.Instance.SetCurrentRoi(this._refRoi);
    }

    // Start is called before the first frame update
    void Start()
    {
        _itemToggle.onValueChanged.AddListener(handleSelected);
    }
    private void OnDestroy()
    {

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OccupancyFieldStudy;

public class EditToolBar : MonoBehaviour
{
    [Serializable]
    public class EditToolToggleSetting
    {
        public Toggle toggle;
        public OccEditManager.EditToolType toolType;
    }
    [SerializeField]
    public List<EditToolToggleSetting> editToolToggleSettings;

    private void Start()
    {
        foreach(var pair in editToolToggleSettings)
        {
            pair.toggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                    handleActive(pair.toolType);
            });
        }
    }

    private void handleActive(OccEditManager.EditToolType toolType)
    {
        OccEditManager.Instance.currentEditToolType = toolType;
    }
}

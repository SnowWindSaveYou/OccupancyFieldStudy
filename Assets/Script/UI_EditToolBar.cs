using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OccupancyFieldStudy
{
    public class UI_EditToolBar : MonoBehaviour
    {
        public OccEditManager occManager;
        public Button DrawBtn;
        public Button GrabBtn;
        public Button GrowthBtn;
        public Button ShinkBtn;
        public Button RaiseBtn;
        public Button TowardBtn;
        public Button FFDBtn;
        // Start is called before the first frame update
        void Start()
        {
            DrawBtn.onClick.AddListener(() => { occManager.currentEditToolType = OccEditManager.EditToolType.Draw; });
            GrabBtn.onClick.AddListener(() => { occManager.currentEditToolType = OccEditManager.EditToolType.Grab; });
            GrowthBtn.onClick.AddListener(() => { occManager.currentEditToolType = OccEditManager.EditToolType.Growth; });
            ShinkBtn.onClick.AddListener(() => { occManager.currentEditToolType = OccEditManager.EditToolType.Shink; });
            TowardBtn.onClick.AddListener(() => { occManager.currentEditToolType = OccEditManager.EditToolType.Toward; });
            FFDBtn.onClick.AddListener(() => { occManager.currentEditToolType = OccEditManager.EditToolType.Other; });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
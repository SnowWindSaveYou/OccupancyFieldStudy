using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EditToolBar : MonoBehaviour
{
    public OccEditManager occManager;
    public Button GrabBtn;
    public Button GrowthBtn;
    public Button ShinkBtn;
    public Button RaiseBtn;

    // Start is called before the first frame update
    void Start()
    {
        GrabBtn.onClick.AddListener(() => { occManager.currentEditToolType = OccEditManager.EditToolType.Grab; });
        GrowthBtn.onClick.AddListener(() => { occManager.currentEditToolType = OccEditManager.EditToolType.Growth; });
        ShinkBtn.onClick.AddListener(() => { occManager.currentEditToolType = OccEditManager.EditToolType.Shink; });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
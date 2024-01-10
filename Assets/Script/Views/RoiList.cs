using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoiList : MonoBehaviour
{

    public Transform Container;
    public RoiListItem template;
    public List<RoiListItem> roiListItems;


    RoiListItem getItem(RoiGraphObject roi)
    {
        return roiListItems.Find(i => i._refRoi == roi);
    }

    void handleCurrentRoiChanged()
    {
        var currRoi = RoiManager.Instance.currentRoi;
        var currItem = getItem( currRoi);
        if (currItem != null)
        {

        }
    }

    void handleAddNewRoi(RoiGraphObject roi)
    {
        var item = GameObject.Instantiate<RoiListItem>(template);
        item.SetRefRoi(roi);
        roiListItems.Add(item);
        item.transform.SetParent(Container, true);
        item.gameObject.SetActive(true);
    }
    void handleRemoveRoi(RoiGraphObject roi)
    {
        var item = getItem(roi);
        if (item != null)
        {
            roiListItems.Remove(item);
            Destroy(item);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        template.gameObject.SetActive(false);

        RoiManager.Instance.OnAddRoi += handleAddNewRoi;
        RoiManager.Instance.OnRemoveRoi += handleRemoveRoi;

        EventsWatcher.On(MessageIds.OnCurrentRoiChanged, handleCurrentRoiChanged);
    }
    private void OnDestroy()
    {
        if(RoiManager.Instance!=null) RoiManager.Instance.OnAddRoi -= handleAddNewRoi;
        if (RoiManager.Instance != null) RoiManager.Instance.OnRemoveRoi -= handleRemoveRoi;
        EventsWatcher.Off(MessageIds.OnCurrentRoiChanged, handleCurrentRoiChanged);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

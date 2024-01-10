using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoiWindow : MonoBehaviour
{
    public InputField RoiName;
    public Button button;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            RoiManager.Instance.CreateRoi(RoiName.text);
        });
    }


}

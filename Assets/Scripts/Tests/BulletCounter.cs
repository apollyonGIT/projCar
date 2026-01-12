using UnityEngine;
using World.Devices;
using TMPro;
using World.Devices.Device_AI;
using World.Devices.DeviceViews;

public class BulletCounter : MonoBehaviour
{
    private TextMeshPro m_text;
    public DeviceView_Spine dv;
    // Start is called before the first frame update
    void Start()
    {
        m_text = GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

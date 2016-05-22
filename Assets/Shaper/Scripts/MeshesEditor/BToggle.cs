using UnityEngine;
using UnityEngine.UI;

public class BToggle : MonoBehaviour
{
    Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();

        toggle.onValueChanged.AddListener(OnValueChanged);

        OnValueChanged(toggle.isOn);
    }

    void OnValueChanged(bool value)
    {
        toggle.targetGraphic.transform.localScale = value ? new Vector3(0, 0, 0) : new Vector3(1, 1, 1);
    }
}

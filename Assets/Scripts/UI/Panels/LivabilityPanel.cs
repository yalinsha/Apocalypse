using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LivabilityPanel : BasePanel<LivabilityPanel>
{
    public TMP_Text figure;
    public Image handle;
    int livability;
    void UpdateInfo()
    {
        livability = LivabilityManager.Instance.Livability;
        figure.text = livability.ToString();
        handle.rectTransform.eulerAngles = new Vector3(0, 0, - 6.5f * livability);
    }
    private void Start()
    {
        UpdateInfo();
        EventManager.Instance.onLivabilityChanged += () =>
        {
            UpdateInfo();
        };
    }
}

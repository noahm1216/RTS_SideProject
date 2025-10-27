using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_UnitInformation : MonoBehaviour
{

    public static UI_UnitInformation Instance { get; private set; }

    public RectTransform unitMultiple;
    public Image unitImageTemplate;

    public RectTransform unitSingle;
    public Image unitImage;
    public TextMeshProUGUI unitStatTextTemplate;
    private Unit[] unitsSelected;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        ToggleUnitUi(false, false);
    }

    public void ToggleUnitUi(bool _showMultiple, bool _showSingle)
    {
        if (unitMultiple) unitMultiple.gameObject.SetActive(_showMultiple);
        if (unitSingle) unitSingle.gameObject.SetActive(_showSingle);
    }

    public void SelectedUnits(Unit[] _unitsSelected)
    {
        if(_unitsSelected.Length == 0) { ToggleUnitUi(false, false); return; }

        unitsSelected = _unitsSelected;

        if (_unitsSelected.Length == 1)
        {
            for (int i = 0; i < unitStatTextTemplate.transform.parent.childCount; i++) // remove lingering instances
                if (unitStatTextTemplate.transform.parent.GetChild(i).gameObject.activeSelf == true)
                    Destroy(unitStatTextTemplate.transform.parent.GetChild(i).gameObject);

            unitImage.sprite = _unitsSelected[0].Data.icon; // icon

            TextMeshProUGUI textClone = Instantiate(unitStatTextTemplate, unitStatTextTemplate.transform.parent); // nickname
            textClone.text = _unitsSelected[0].NickName;
            textClone.gameObject.SetActive(true);

            textClone = Instantiate(unitStatTextTemplate, unitStatTextTemplate.transform.parent); // hp
            textClone.text = $"HP {_unitsSelected[0].CurrentHP}";
            textClone.gameObject.SetActive(true);

            ToggleUnitUi(false, true); // show UI
        }
        else
        {
            for (int i = 0; i < unitImageTemplate.transform.parent.childCount; i++) // remove lingering instances
                if (unitImageTemplate.transform.parent.GetChild(i).gameObject.activeSelf == true)
                    Destroy(unitImageTemplate.transform.parent.GetChild(i).gameObject);

            for(int j = 0; j < _unitsSelected.Length; j++) // repopulate the images
            {
                Image iconClone = Instantiate(unitImageTemplate, unitImageTemplate.transform.parent);
                iconClone.sprite = _unitsSelected[j].Data.icon; // icon
                iconClone.gameObject.SetActive(true);
            }

            ToggleUnitUi(true, false);  // show UI
        }

    }

}

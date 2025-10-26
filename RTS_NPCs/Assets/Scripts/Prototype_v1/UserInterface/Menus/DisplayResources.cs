using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DisplayResources : MonoBehaviour
{

    public TextMeshProUGUI woodText, foodText, stoneText, goldText;

    public void OnEnable()
    {        
        Manager_Objects.resourceChanged += UpdateUI;
    }

    public void OnDisable()
    {
        Manager_Objects.resourceChanged -= UpdateUI;
    }

    public void UpdateUI()
    {
        //print("THE UI IS READY TO CHANGE ");
        if (woodText)
            woodText.text = Manager_Objects.resourceWood.ToString();
        if (foodText)
            foodText.text = Manager_Objects.resourceFood.ToString();
        if (stoneText)
            stoneText.text = Manager_Objects.resourceStone.ToString();
        if (goldText)
            goldText.text = Manager_Objects.resourceGold.ToString();

    }
}

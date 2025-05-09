using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName ="BuildingData", menuName ="ScriptableObjects/BuildingData", order = 1)]
public class SO_Buildings : ScriptableObject
{
    public string buildingName;
    public Sprite buildingSprite;
    public bool isUnlocked;

    UnitExperienceHistory.ExperienceType experienceType;
    SimpleInteractor.BuildingType buildingType;
    SimpleInteractor.ResourceType resourceType;

    public Transform prefabToSpawn;
    public ResourceTypes resourceCosts;

}




[System.Serializable]
public class ResourceTypes
{
    public int resourceWood;
    public int resourceFood;
    public int resourceStone;
    public int resourceGold;

    public ResourceTypes(int _newWood, int _newFood, int _newStone, int _newGold)
    {
        resourceWood = _newWood;
        resourceFood = _newFood;
        resourceStone = _newStone;
        resourceGold = _newGold;
    }
}// end of ResourceTypes class

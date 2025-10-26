using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PlayerType {HUMAN, BOT}; // NATURE, etc...

public class ManagerUnits : MonoBehaviour
{
    PlayerType playerType;

    [SerializeField] private UnitData[] unitPresets;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) { print("Spawn Unit"); SpawnUnit(unitPresets[0], new Vector3(0,0,0)); }
    }

    public Unit SpawnUnit(UnitData data, Vector3 position)
    {
        GameObject newUnitObj = Instantiate(data.basePrefab, position, Quaternion.identity);
        Unit unit = newUnitObj.GetComponent<Unit>();
        unit.InitializeFromData(data);
        newUnitObj.transform.SetParent(transform);
        return unit;
    }

    public void SaveUnitCosmetics(Unit unit)
    {
        var cosmeticData = unit.GetCurrentCosmeticData();
        string json = JsonUtility.ToJson(cosmeticData);
        PlayerPrefs.SetString($"UnitCosmetic_{unit.NickName}", json);
    }

    public void LoadUnitCosmetics(Unit unit)
    {
        string key = $"UnitCosmetic_{unit.NickName}";
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            var data = JsonUtility.FromJson<UnitCosmeticData>(json);
            unit.ApplyCosmeticData(data);
        }
    }

    

   
}

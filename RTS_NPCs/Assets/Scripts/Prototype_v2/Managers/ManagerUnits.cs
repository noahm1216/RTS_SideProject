using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PlayerType {HUMAN, BOT}; // NATURE, etc...

public class ManagerUnits : MonoBehaviour
{
    public static ManagerUnits Instance { get; private set; }

    PlayerType playerType;

    [SerializeField] private UnitData[] unitPresets;
    public static List<Unit> unitsSpawned = new List<Unit>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) { print("Spawn Unit"); SpawnUnit(unitPresets[0], new Vector3(0,0,0), null); }

        RunUnits();
    }

    public Unit SpawnUnit(UnitData data, Vector3 position, string presetName = "")
    {
        int diceRoll = Random.Range(0, 100); // spawn one of the two models
        GameObject modelToSpawn = null;
        if (diceRoll > 50 && data.alternativePrefab) modelToSpawn = data.alternativePrefab;
        else modelToSpawn = data.basePrefab;

        GameObject newUnitObj = Instantiate(modelToSpawn, position, Quaternion.identity);
        Unit unit = newUnitObj.GetComponent<Unit>();
        if (data.possibleNames.Length> 0)
        {
            unit.NickName = data.possibleNames[Mathf.RoundToInt(diceRoll * data.possibleNames.Length / 100)];
            unit.transform.name = $"SpawnedUnit_{playerType}_{data.race}_{unit.NickName}_{unitsSpawned.Count}";
        }
        unitsSpawned.Add(unit);
        unit.InitializeFromData(data);
        newUnitObj.transform.SetParent(transform);

        if (!string.IsNullOrEmpty(presetName))
            UnitCosmeticSaveSystem.LoadCosmetics(unit, presetName);

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

    public void CommandUnit(UnitData.ActionTaking _newActionCommand, Vector3 pos) // NOTE: we should be passing other data
    {
        if (unitsSpawned.Count == 0) return; // no units
        for (int i = 0; i < unitsSpawned.Count; i++) // run and check each unit
            if (unitsSpawned[i].IsSelected)
            {
                unitsSpawned[i].SetAction(_newActionCommand);
                if (pos != Vector3.zero) unitsSpawned[i].SetWalkTarget(pos);
            }
    }


    private void RunUnits()
    {
        if (unitsSpawned.Count == 0) return; // no units

        for(int i=0; i < unitsSpawned.Count; i++) // run and check each unit
        {
            if (Time.time > unitsSpawned[i].LastActionTimeStamp + unitsSpawned[i].SpeedAction) // if the unit is ready to take a new action
            {
                switch (unitsSpawned[i].CurrentAction) // depending on the units action 
                {
                    case UnitData.ActionTaking.Attacking:
                        print("Unit Attacking");
                        break;
                    case UnitData.ActionTaking.Building:
                        print("Unit Building");
                        break;
                    case UnitData.ActionTaking.Gathering:
                        print("Unit Gathering");
                        break;
                    case UnitData.ActionTaking.Idle:
                        print("Unit Idle");
                        break;
                    case UnitData.ActionTaking.Relaxing:
                        print("Unit Relaxing");
                        break;
                    case UnitData.ActionTaking.Walking:
                        print("Unit Walking");
                        float dist = Vector3.Distance(unitsSpawned[i].targetMovePosition, unitsSpawned[i].transform.position);
                        if (dist > 0.15f)
                        {
                            float step = unitsSpawned[i].SpeedMove * Time.deltaTime;
                            unitsSpawned[i].transform.position = Vector3.MoveTowards(unitsSpawned[i].transform.position, unitsSpawned[i].targetMovePosition, step);
                        }
                        else unitsSpawned[i].SetAction( UnitData.ActionTaking.Idle);
                        break;
                    default:
                        Debug.LogWarning($"Unit Action: {unitsSpawned[i].CurrentAction} - Not Accounted For");
                        break;
                }

                unitsSpawned[i].RecordExperience(unitsSpawned[i].CurrentAction); // record the actions taken
            }
        }
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para> Hold Information for units, buildings, and resources </para>
/// </summary>
public class Manager_Objects : MonoBehaviour
{

    public static List<GameObject> npcObjects = new List<GameObject>();
    public static List<GameObject> collectableObjects  = new List<GameObject>();
    public static List<GameObject> buildingObjects = new List<GameObject>();

    public static Manager_Objects instance;

    // signal calling
    public delegate void ResourceChanged();
    public static event ResourceChanged resourceChanged;
    // resources
    public static int resourceWood { get; private set; }

    void Awake()
    {
        MakeSingleton();

        void MakeSingleton()
        {
            if (instance == null)
            {
                instance = this;
                //DontDestroyOnLoad(gameObject);
            }
            else if (instance != null)
            {
                Destroy(gameObject);
            }
        }

    }// end of Awake()

    private void OnEnable()
    {
        SendCommand.targetClick += SendUnitTargetSignals;
    }

    private void OnDisable()
    {
        SendCommand.targetClick -= SendUnitTargetSignals;
    }

    public void SendUnitTargetSignals(RaycastHit _sigTar) // sending the target though the manager is likely better than having EVERY single unit listening for the signal.
    {
        if (npcObjects.Count == 0)
            return;

        NPC_Behavior _npcBehavior = null;
        for(int npc = 0; npc < npcObjects.Count; npc++)
        {
            npcObjects[npc].TryGetComponent(out _npcBehavior);
            if(_npcBehavior != null) { _npcBehavior.SignalTarget(_sigTar); _npcBehavior = null; }
        }
    }


    public static bool CanChangeResources(SimpleInteractor.ResourceType _resType, int _changeBy)
    {
        switch (_resType)
        {
            case SimpleInteractor.ResourceType.Wood:
                if (resourceWood + _changeBy < 0) // if we dont have enough to change, then let the calling function know
                    return false;
                resourceWood += _changeBy;
                if (resourceWood <= 0)
                    resourceWood = 0;
                break;
            default:
                break;
        }
        //print("Invoking resource changed");
        resourceChanged?.Invoke();
        return true;
    }


    }// end of Manager_Objects class

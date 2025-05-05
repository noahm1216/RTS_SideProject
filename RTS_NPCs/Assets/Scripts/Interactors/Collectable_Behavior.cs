using UnityEngine;

public class Collectable_Behavior : MonoBehaviour
{
    public int resourceHealth { get; private set; } = 100;

    public GameObject resourceRootVisObj, resourceInProgressObj;    
    public bool resourceIsDepleted;

    private float aboveGroundY;

    private SimpleInteractor ref_SimpleInteractor;
    private SphereCollider resourceCollider;
    // health bars
    // dmg vfx


    private bool NoVitalMissingReferences()
    {
        string missingRefText = "";
        if (!Manager_Objects.instance) missingRefText += $"\nMissing: Manager_Objects.instance: {transform.name}";
        if (!resourceRootVisObj) missingRefText += $"\nMissing: resourceRootVisObj: {transform.name}";
        if (!resourceInProgressObj) missingRefText += $"\nMissing: resourceInProgressObj: {transform.name}";

        if (string.IsNullOrEmpty(missingRefText))
            return true;
        else
        { print("WARNING: Missing Content" + missingRefText); return false; }
    }

    private void MissingNonVitalReferences(string _msg)
    {
        print($"WARNING: Missing Non-Vital-Reference on {transform.name}\n" + _msg);
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if (!NoVitalMissingReferences())
            return;

        Manager_Objects.collectableObjects.Add(gameObject);

        // get refs
        if (!resourceCollider)
            TryGetComponent(out resourceCollider);
        if (!ref_SimpleInteractor)
            TryGetComponent(out ref_SimpleInteractor);

        // TODO: Right now we start trees at full, but we could create a system where they grow on their own over time and the health you get is whatever their HP is at the time
        //          IF we do this, it should be through the OBJ manager which could run through every tree every X seconds and call a "TimePassing" function which then handles either a "growth" or "decay" function depending on the tree's situation
        if (ref_SimpleInteractor) 
        {
            aboveGroundY = resourceRootVisObj.transform.localScale.y;

            if (ref_SimpleInteractor.myBuilding == SimpleInteractor.BuildingType.Resource)
            {
                resourceRootVisObj.transform.position = new Vector3(resourceRootVisObj.transform.position.x, aboveGroundY, resourceRootVisObj.transform.position.z);
                resourceInProgressObj.SetActive(resourceHealth < 100);
            }
            else
            {
                resourceRootVisObj.transform.position = new Vector3(resourceRootVisObj.transform.position.x, -aboveGroundY, resourceRootVisObj.transform.position.z);
                resourceInProgressObj.SetActive(resourceHealth <= 100);
            }
        }
        else
            MissingNonVitalReferences("ref_SimpleInteractor || Called by OnEnable()");

        resourceInProgressObj.SetActive(false);
        resourceIsDepleted = false;
       
       
    }// end of OnEnable()

    private void OnDisable()
    {
        if (!NoVitalMissingReferences())
            return;

        Manager_Objects.collectableObjects.Remove(gameObject);
    }// end of OnDisable()

    public void ResetResource()
    {
        if (ref_SimpleInteractor)
        {
            if (ref_SimpleInteractor.myBuilding == SimpleInteractor.BuildingType.Resource)
            {
                resourceHealth = 100;
                resourceRootVisObj.transform.position = new Vector3(resourceRootVisObj.transform.position.x, aboveGroundY, resourceRootVisObj.transform.position.z);
                resourceInProgressObj.SetActive(resourceHealth < 100);
            }
            else
            {
                resourceHealth = 0;
                resourceRootVisObj.transform.position = new Vector3(resourceRootVisObj.transform.position.x, -aboveGroundY, resourceRootVisObj.transform.position.z);
                resourceInProgressObj.SetActive(resourceHealth <= 100);
            }
        }
        else
            MissingNonVitalReferences("ref_SimpleInteractor || Called by ResetResource()");
    }

    public void TakeResource()
    {
        if (!NoVitalMissingReferences())
            return;

        print("Taking Resource");

        if (resourceIsDepleted)
        {
            print($"this resource has nothing to take: {transform.name}");
        }
        else // if the resource is still being built first time
        {
            resourceRootVisObj.transform.position -= new Vector3(0, 1, 0);
            resourceHealth -= 10;
            resourceInProgressObj.SetActive(true);

            if (!ResourceStillAvailable())
            {
                resourceHealth = 0;
                //resourceRootVisObj.transform.position = new Vector3(resourceRootVisObj.transform.position.x, belowGroundY_AboveGroundY.x, resourceRootVisObj.transform.position.z);
                resourceIsDepleted = true;               
                resourceInProgressObj.SetActive(false);
                resourceRootVisObj.SetActive(false);
                if (resourceCollider)
                    resourceCollider.enabled = false;
                else
                    MissingNonVitalReferences("resourceCollider || Called by TakeResource()");
                print("turning off the resource");
                gameObject.SetActive(false);
            }

        }

        //mySimpleInteractor.thisInteractorFinished = resourceIsDepleted;
    }// end of TakeResource()

    public bool ResourceStillAvailable()
    {
        return resourceHealth > 0;
    }


}// end of Collectable_Behavior class
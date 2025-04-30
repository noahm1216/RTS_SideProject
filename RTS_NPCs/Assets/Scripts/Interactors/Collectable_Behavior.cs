using UnityEngine;

public class Collectable_Behavior : MonoBehaviour
{
    public int resourceHealth { get; private set; } = 100;

    public GameObject ResourceRoot, ResourceProgress;
    public Vector2 belowGroundY_AboveGroundY;
    public bool resourceIsDepleted;

    private SphereCollider resourceCollider;
    private SimpleInteractor mySimpleInteractor;
    // health bars
    // dmg vfx


    // Start is called before the first frame update
    void OnEnable()
    {
        Manager_Objects.collectableObjects.Add(gameObject);
        ResourceRoot.transform.position = new Vector3(ResourceRoot.transform.position.x, belowGroundY_AboveGroundY.y, ResourceRoot.transform.position.z);
        ResourceProgress.SetActive(false);
        resourceIsDepleted = false;
        resourceCollider = transform.GetComponent<SphereCollider>();
        mySimpleInteractor = transform.GetComponent<SimpleInteractor>();
    }// end of OnEnable()

    private void OnDisable()
    {
        Manager_Objects.collectableObjects.Remove(gameObject);
    }// end of OnDisable()

   

    public void TakeResource()
    {
        print("Taking Resource");

        if (resourceIsDepleted)
        {
            print($"this resource has nothing to take: {transform.name}");
        }
        else // if the resource is still being built first time
        {
            ResourceRoot.transform.position -= new Vector3(0, 1, 0);
            resourceHealth -= 10;
            ResourceProgress.SetActive(true);

            if (!ResourceStillAvailable())
            {
                resourceHealth = 0;
                ResourceRoot.transform.position = new Vector3(ResourceRoot.transform.position.x, belowGroundY_AboveGroundY.x, ResourceRoot.transform.position.z);
                resourceIsDepleted = true;
                resourceCollider.enabled = false;
                ResourceProgress.SetActive(false);
                ResourceRoot.SetActive(false);
                print("turning off the resource");
                gameObject.SetActive(false);
            }

        }

        mySimpleInteractor.thisInteractorFinished = resourceIsDepleted;
    }// end of TakeResource()

    public bool ResourceStillAvailable()
    {
        return resourceHealth > 0;
    }


}// end of Collectable_Behavior class
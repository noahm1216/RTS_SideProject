using UnityEngine;
using UnityEngine.AI;

public class AgentNavigation : MonoBehaviour
{
    public NPC_Behavior ref_NPC_Behavior;
    public NavMeshAgent navMeshAgent;

    private float randomStopDistance;
    private float speedRotate = 2.5f;

    private float reducedUpdateCall;
    private float updateWaitTime = 1.5f;

    private void Start()
    {
        if (ref_NPC_Behavior)
        {
            if (ref_NPC_Behavior.randomizeDistTolerance)
                randomStopDistance = ref_NPC_Behavior.tarDistTolerance + Random.Range(0.01f, 0.5f);
            else
                randomStopDistance = ref_NPC_Behavior.tarDistTolerance;
        }
        else
            randomStopDistance = Random.Range(1f, 2.5f);
        navMeshAgent.stoppingDistance = randomStopDistance;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if (!ref_NPC_Behavior)
        {
            if (transform.GetComponent<NPC_Behavior>())
                ref_NPC_Behavior = transform.GetComponent<NPC_Behavior>();
            else
                ref_NPC_Behavior = gameObject.AddComponent<NPC_Behavior>();            
        }

        if (!navMeshAgent)
        {
            if (transform.GetComponent<NavMeshAgent>())
                navMeshAgent = transform.GetComponent<NavMeshAgent>();
            else
                navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!ref_NPC_Behavior)
            return;       

        // if we have a target
        if (ref_NPC_Behavior.mySigTar && Time.time > reducedUpdateCall + updateWaitTime)
        {
            // if we are in range
            if (IsInRangeToTarget(ref_NPC_Behavior.mySigTarPosition))
            {
                //print("reached target");                
                ref_NPC_Behavior.ChangeIsMoving(false);
                LookToSigTar(); // only fix rotation if we've already arrived at our target
            }
            else // else we are not in range
            {
                ref_NPC_Behavior.ChangeIsMoving(true);
                navMeshAgent.SetDestination(ref_NPC_Behavior.mySigTarPosition);
            }

            reducedUpdateCall = Time.time;
        }// end of have a signal target

        //// if we have a target AND 
        //if (ref_NPC_Behavior.mySigTar && ref_NPC_Behavior.isSelected)
        //{            
        //    navMeshAgent.SetDestination(ref_NPC_Behavior.mySigTarPosition);            
        //}

        //// if we have a target and the distance equals or is less than our stop-dist then we've reached the target
        //if (ref_NPC_Behavior.mySigTar && IsInRangeToTarget(ref_NPC_Behavior.mySigTarPosition) && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        //{
        //    ref_NPC_Behavior.reachedSigTar = true;
        //    ref_NPC_Behavior.isMoving = false;
        //    LookToSigTar(); // only fix rotation if we've already arrived at our target
        //}
        //else if (ref_NPC_Behavior.mySigTar && !IsInRangeToTarget(ref_NPC_Behavior.mySigTarPosition) && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        //{
        //    ref_NPC_Behavior.reachedSigTar = false;
        //    ref_NPC_Behavior.isMoving = true;
        //    navMeshAgent.SetDestination(ref_NPC_Behavior.mySigTarPosition);
        //}
        
    }// end of Update()

    // to check if we are in range of our new signal target
    private bool IsInRangeToTarget(Vector3 _OtherPos)
    {
        float dist = Vector3.Distance(_OtherPos, transform.position);
        if (dist > randomStopDistance*2)
            return false;
        else
            return true;
    }

    private void LookToSigTar()
    {
        if (!ref_NPC_Behavior.mySigTar)
            return;
        // Determine which direction to rotate towards
        Vector3 targetDirection = ref_NPC_Behavior.mySigTar.position - transform.position;
        // The step size is equal to speedRotate times frame time.
        float singleStep = speedRotate * Time.deltaTime;
        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
        // Draw a ray pointing at our target in
        Debug.DrawRay(transform.position, newDirection, Color.red);
        // Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(newDirection);
    }//end of LookToSigTar()


}// end of AgentNavigation class

using UnityEngine;
using UnityEngine.AI;

public class AgentNavigation : MonoBehaviour
{
    public NPC_Behavior script_NpcBehavior;
    public NavMeshAgent navMeshAgent;

    private float randomStopDistance;
    private float speedRotate = 2.5f;

    private float reducedUpdateCall;
    private float updateWaitTime = 1.5f;

    private void Start()
    {
        randomStopDistance = Random.Range(1f, 2.5f);
        navMeshAgent.stoppingDistance = randomStopDistance;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if (!script_NpcBehavior)
        {
            if (transform.GetComponent<NPC_Behavior>())
                script_NpcBehavior = transform.GetComponent<NPC_Behavior>();
            else
                script_NpcBehavior = gameObject.AddComponent<NPC_Behavior>();            
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
        if (!script_NpcBehavior)
            return;       

        // if we have a target
        if (script_NpcBehavior.mySigTar && Time.time > reducedUpdateCall + updateWaitTime)
        {
            // if we are in range
            if (IsInRangeToTarget(script_NpcBehavior.mySigTarPosition))
            {
                print("reached target");
                script_NpcBehavior.reachedSigTar = true;
                script_NpcBehavior.isMoving = false;
                LookToSigTar(); // only fix rotation if we've already arrived at our target
            }
            else // else we are not in range
            {
                script_NpcBehavior.reachedSigTar = false;
                script_NpcBehavior.isMoving = true;
                navMeshAgent.SetDestination(script_NpcBehavior.mySigTarPosition);
            }

            reducedUpdateCall = Time.time;
        }// end of have a signal target

        //// if we have a target AND 
        //if (script_NpcBehavior.mySigTar && script_NpcBehavior.isSelected)
        //{            
        //    navMeshAgent.SetDestination(script_NpcBehavior.mySigTarPosition);            
        //}

        //// if we have a target and the distance equals or is less than our stop-dist then we've reached the target
        //if (script_NpcBehavior.mySigTar && IsInRangeToTarget(script_NpcBehavior.mySigTarPosition) && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        //{
        //    script_NpcBehavior.reachedSigTar = true;
        //    script_NpcBehavior.isMoving = false;
        //    LookToSigTar(); // only fix rotation if we've already arrived at our target
        //}
        //else if (script_NpcBehavior.mySigTar && !IsInRangeToTarget(script_NpcBehavior.mySigTarPosition) && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        //{
        //    script_NpcBehavior.reachedSigTar = false;
        //    script_NpcBehavior.isMoving = true;
        //    navMeshAgent.SetDestination(script_NpcBehavior.mySigTarPosition);
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
        if (!script_NpcBehavior.mySigTar)
            return;
        // Determine which direction to rotate towards
        Vector3 targetDirection = script_NpcBehavior.mySigTar.position - transform.position;
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

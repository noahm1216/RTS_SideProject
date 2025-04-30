using UnityEngine;

public class AgentAnimation : MonoBehaviour
{
    public NPC_Behavior script_NpcBehavior;
    public Animator animController;


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
    }// end of OnEnable()

    // Update is called once per frame
    void Update()
    {
        if (!animController)
            return;

        animController.SetBool("isSelected", script_NpcBehavior.isSelected); // selected
        animController.SetBool("isMoving", script_NpcBehavior.isMoving); // moving
        animController.SetBool("takingAction", script_NpcBehavior.takingAction); // taking action
        //animController.SetTrigger("isDead", script_NpcBehavior.isMoving); // death

    }// end of update()


}// end of AgentAnimation class

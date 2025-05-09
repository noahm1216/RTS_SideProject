using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ClickMarker_Handler : MonoBehaviour
{

    public LineRenderer lineRenPathPrefab;
    private LineRenderer lineRenPathRuntime;
    private AgentNavigation recentAgentNavigation;

    public TextMeshPro[] textObjects;

    public UnityEvent onClickEvents;


    public void Start()
    {
        ChangeDisplayLineRenderer(false);
        ChangeDisplayText(false);
    }

    public void NewClickInformation()
    {
        DrawPath();
        onClickEvents?.Invoke();
    }

    public void DrawPath()
    {
        if (!lineRenPathRuntime && lineRenPathPrefab)
            lineRenPathRuntime = Instantiate(lineRenPathPrefab).GetComponent<LineRenderer>();

        if (lineRenPathRuntime)
        {
            recentAgentNavigation = GetUnitNavigationData();
            if (!recentAgentNavigation) return;

            lineRenPathRuntime.positionCount = 0;

            if (recentAgentNavigation.navMeshAgent.hasPath)
            {
                ChangeDisplayLineRenderer(true);
                ChangeDisplayText(true);


                lineRenPathRuntime.positionCount = recentAgentNavigation.navMeshAgent.path.corners.Length;
                lineRenPathRuntime.SetPosition(0, recentAgentNavigation.transform.position);

                if (lineRenPathRuntime.positionCount > 0)
                {
                    for (int c = 0; c < lineRenPathRuntime.positionCount; c++)
                        lineRenPathRuntime.SetPosition(c, recentAgentNavigation.navMeshAgent.path.corners[c]);
                }
                lineRenPathRuntime.SetPosition(lineRenPathRuntime.positionCount - 1, transform.position);                
            }
            else
                ChangeDisplayLineRenderer(false);
        }
    }

    public AgentNavigation GetUnitNavigationData()
    {
        NPC_Behavior _npcBehavior = null;
        AgentNavigation _npcAgentNav = null;

        for (int npc = 0; npc < Manager_Objects.npcObjects.Count; npc++)
        {
            Manager_Objects.npcObjects[npc].TryGetComponent(out _npcBehavior);
            Manager_Objects.npcObjects[npc].TryGetComponent(out _npcAgentNav);

            if (_npcBehavior && _npcBehavior.isSelected && _npcAgentNav)
                return _npcAgentNav;

            _npcBehavior = null;
            _npcAgentNav = null;
        }

        return _npcAgentNav;
    }

    public void ChangeDisplayLineRenderer(bool _show)
    {
        if (lineRenPathRuntime)
            lineRenPathRuntime.gameObject.SetActive(_show);
    }

    public void ChangeDisplayText(bool _show)
    {
        if (textObjects.Length > 0)
        {
            for (int i = 0; i < textObjects.Length; i++)
            {
                if (textObjects[i])
                    textObjects[i].gameObject.SetActive(_show);             
            }
        }
    }

    private void LateUpdate()
    {
        if(recentAgentNavigation && recentAgentNavigation.navMeshAgent.hasPath)
        {
            if (lineRenPathRuntime.positionCount > 0)
                lineRenPathRuntime.SetPosition(0, recentAgentNavigation.transform.position);
            if(recentAgentNavigation.navMeshAgent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathComplete)
            {
                ChangeDisplayLineRenderer(false);
                ChangeDisplayText(false);
                recentAgentNavigation = null;
            }

        }     
    }

}

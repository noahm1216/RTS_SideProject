using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// <para> A listener / sender of events and commands to units from the player </para>
/// </summary>
public class SendCommand : MonoBehaviour
{

    // _________________________________these variables are necessary testing or not testing
    [Tooltip("A top down orthographic camera... other cams work, but ortho works best")]
    // the camera from top down orthographic view  (can set up a 2nd seperate camera looking at 45 or whatever but for this script Ortho gives best results)
    public Camera myOrthoCam;
    // This variable will store the location of wherever we first click before dragging.
    private Vector3 initialRightClickPosition;

    // for visual effects
    private ClickMarker_Handler ref_ClickHandler;
    public GameObject hitpointVfxPrefab;
    private Transform hitpointVfx;

    // for signal sending
    public delegate void TargetClicked(RaycastHit _sigTar);
    public static event TargetClicked targetClick;

    

    void OnEnable()
    {
        if (!myOrthoCam && Camera.main)
            myOrthoCam = Camera.main;

        if (hitpointVfxPrefab && !hitpointVfx)
            hitpointVfx = Instantiate(hitpointVfxPrefab).transform;
        if (!ref_ClickHandler && hitpointVfx)
            hitpointVfx.TryGetComponent(out ref_ClickHandler);
    }// end of OnEnable()


    void Update()
    {
        CheckPlayerInput();
    }// end of Update()


    private void CheckPlayerInput()
    {
        // Click somewhere in the Game View. (right click)
        if (Input.GetMouseButtonUp(1))
        {
            //print("RIGHT CLICK RELEASE");
            RaycastHit hit; // Get the initial click position of the mouse. 
            Ray ray = myOrthoCam.ScreenPointToRay(Input.mousePosition);
            // if raycast hits a collider point
            if (Physics.Raycast(ray, out hit))
            {
                SelectionStart(ray, hit);
                PlaceVFX(hit);               
            }
        }      

    }// end of CheckPlayerInput()


    private void SelectionStart(Ray _ray, RaycastHit _hit)
    {
        print($"RIGHT CLICK HIT: {_hit.transform.name}\nTag: {_hit.transform.tag}");
        // mark he position
        initialRightClickPosition = _hit.point;
        // send the signal
        if (targetClick != null && _hit.point != null)
            targetClick(_hit);
    }// end of SelectionStart()
    

    public void PlaceVFX(RaycastHit _hit)
    {
        if (hitpointVfx)
            hitpointVfx.transform.position = _hit.point;
        if (ref_ClickHandler)
            ref_ClickHandler.NewClickInformation();
    }// end of PlaceVFX(RaycastHit _hit)
           

}// end of SendCommand class

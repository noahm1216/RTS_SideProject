using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para> Drag Select only worries about the camera and player mouse inputs </para>
/// </summary>
public class DragSelect : MonoBehaviour
{
    // _________________________________these variables are necessary testing or not testing
    [Tooltip("A top down orthographic camera... other cams work, but ortho works best")]
    // the camera from top down orthographic view  (can set up a 2nd seperate camera looking at 45 or whatever but for this script Ortho gives best results)
    public Camera myOrthoCam;
    // This variable will store the location of wherever we first click before dragging.
    private Vector3 initialLeftClickPosition;
    // the point our mouse is
    private Vector3 currentLeftClickPosition;
    // a check for if we shot raycast
    private bool isDraggingMouse;
    // a point for our raycast right click
    private Vector3 initialRightClickPosition;
    // the layers we are comfortable clicking
    public LayerMask leftClickableLayers;

    // the visuals for our selection square
    private Transform selectorCubeParent;
    private GameObject selectorCube;
    private MeshRenderer selectorCubeMesh;
    private BoxCollider selectorCollider;
    private Rigidbody selectorRB;
    private DragSelectCollider selectorScript_DSC;
    [Tooltip("if you want a specific material on the selection cube, put it here")]
    public Material selectionBoxMaterial;

    void OnEnable()
    {
        if (!myOrthoCam && Camera.main)
            myOrthoCam = Camera.main;

        if (!selectorCubeParent || !selectorCube)
            InitializeSelector();
    }// end of OnEnable()

    // create the cube / parent we'll be using
    private void InitializeSelector()
    {
        selectorCubeParent = new GameObject("SelectorParent").transform;
        selectorCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        selectorCube.transform.name = "SelectorCube";
        selectorCube.transform.SetParent(selectorCubeParent.transform);
        selectorCube.transform.position = new Vector3(-0.5f, 1, -0.5f);
        selectorCollider = selectorCube.AddComponent<BoxCollider>();
        selectorCollider.isTrigger = true;
        selectorRB = selectorCube.AddComponent<Rigidbody>();
        selectorRB.isKinematic = true;
        selectorScript_DSC = selectorCube.AddComponent<DragSelectCollider>();
        selectorCubeMesh = selectorCube.GetComponent<MeshRenderer>();
        if (selectionBoxMaterial)
            selectorCubeMesh.material = selectionBoxMaterial;

        SelectionEnded();
    }// end of InitializeSelector()    
    
    void Update()
    {
        CheckPlayerInput();
    }// end of Update()

    private void CheckPlayerInput()
    {
        // Click somewhere in the Game View. (left click)
        if (Input.GetMouseButtonDown(0))
        {            
            RaycastHit hit; // Get the initial click position of the mouse. 
            Ray ray = myOrthoCam.ScreenPointToRay(Input.mousePosition);
            // if raycast hits a collider point
            if (Physics.Raycast(ray, out hit))
                SelectionStart(ray, hit);
        }

        // While we are dragging.
        if (Input.GetMouseButton(0) && isDraggingMouse)
        {
            RaycastHit hit; // Get the initial click position of the mouse. 
            Ray ray = myOrthoCam.ScreenPointToRay(Input.mousePosition);
            // if raycast hits a collider point
            if (Physics.Raycast(ray, out hit, leftClickableLayers))
                SelectionUpdater(ray, hit);
        }

        // After we release the mouse button.
        if (Input.GetMouseButtonUp(0))
            SelectionEnded();

    }// end of CheckPlayerInput()

    private void SelectionStart(Ray _ray, RaycastHit _hit)
    {
        // reset our cube's selection list
        if (selectorScript_DSC && selectorScript_DSC.allTriggeredObjects.Count > 0)
            selectorScript_DSC.allTriggeredObjects.Clear();

        isDraggingMouse = true;
        selectorCubeMesh.enabled = isDraggingMouse;
        selectorCollider.enabled = true;
        // mark he position
        initialLeftClickPosition = _hit.point;        
        // move the box to the position
        selectorCubeParent.position = initialLeftClickPosition;
    }// end of SelectionStart()

    private void SelectionUpdater(Ray _ray, RaycastHit _hit)
    {
        // get mouse position 2D
        var mousePos = Input.mousePosition;
        // Store the current mouse position in screen space.          
        //currentLeftClickPosition = myOrthoCam.ScreenToWorldPoint(mousePos);
        currentLeftClickPosition = _hit.point;
        // set the scale to the difference
        Vector3 boxSelectorScale = new Vector3(initialLeftClickPosition.x - currentLeftClickPosition.x, 0.5f, initialLeftClickPosition.z - currentLeftClickPosition.z);
        selectorCubeParent.localScale = boxSelectorScale;
    }// end of SelectionUpdater()

    private void SelectionEnded()
    {
        // Reset
        isDraggingMouse = false;
        selectorCollider.enabled = false;
        selectorCubeMesh.enabled = isDraggingMouse;
        initialLeftClickPosition = Vector3.zero;
        currentLeftClickPosition = Vector3.zero;
    }// end of SelectionEnded()

}// end of DragSelect class

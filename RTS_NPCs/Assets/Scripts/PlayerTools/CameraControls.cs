using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>
/// The controls for the camera
/// </para>
/// </summary>
public class CameraControls : MonoBehaviour
{
    public Transform transCamPlayer;
    public Transform transCamPlayerOverrideForward;
    private Vector3 camPos;
    public Space spaceRelative;
    public float speedCamMove;
    private float moveMultiplier = 1; // presh shift to make 2
    public float zoomScale;
    public Vector3 rangeCamZoom; //x = current , y = min, z = max
    

    //start
    private void Start()
    {
        //initalize refs
        if (transCamPlayer == null) transCamPlayer = Camera.main.transform;
        camPos = transCamPlayer.position;
    }//end of start

    // Update is called once per frame
    void Update()
    {
        //have a check if game is paused or menu is open

        CheckForInputs(); //let player move camera and zoom in/out
    }//end update

    //a function to check for inputs
    private void CheckForInputs()
    {
        //if (DebugHacks.showingHackCanvas) { return; } //if we have the hack menu open then we don't want to move the camera

        if (Input.GetKey("w") || Input.GetKey("up")) { transCamPlayer.Translate(transCamPlayerOverrideForward.forward * speedCamMove * moveMultiplier * Time.deltaTime, Space.World); } //Up
        if (Input.GetKey("a") || Input.GetKey("left")) { transCamPlayer.Translate(Vector3.left * speedCamMove * moveMultiplier * Time.deltaTime, spaceRelative); } //left
        if (Input.GetKey("d") || Input.GetKey("right")) { transCamPlayer.Translate(Vector3.right * speedCamMove * moveMultiplier * Time.deltaTime, spaceRelative); } //right
        if (Input.GetKey("s") || Input.GetKey("down")) { transCamPlayer.Translate(transCamPlayerOverrideForward.forward * -speedCamMove * moveMultiplier * Time.deltaTime, Space.World); } //down

        if (Input.GetKey("left shift") || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKey("right shift") || Input.GetKeyDown(KeyCode.RightShift)) { moveMultiplier = 2.5f; }
        else { moveMultiplier = 1; }
        //holding shift

        //rangeCamZoom.z += Input.mouseScrollDelta.y * zoomScale; // change the zoom by our middle mouse
        transCamPlayer.Translate(Vector3.forward * Input.mouseScrollDelta.y * zoomScale); //move camera to the zoomed in/out position

        //if (rangeCamZoom.x <= rangeCamZoom.y) // min zoom
        //    rangeCamZoom.x = rangeCamZoom.y;

        //if (rangeCamZoom.x >= rangeCamZoom.z) // max zoom
        //    rangeCamZoom.x = rangeCamZoom.z;

    }//end of check for inputs

}//end of camera controls script

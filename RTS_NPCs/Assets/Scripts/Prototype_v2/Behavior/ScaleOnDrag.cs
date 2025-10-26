using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ScaleOnDrag : MonoBehaviour
{
    private Vector3 dragStartMousePosition;
    private Vector3 initialObjectScale;
    private Vector3 factoryResetScale;
    private SphereCollider sphereCollider;

    public float scaleSensitivity = 0.01f; // Adjust this value for desired scaling speed

    private void Start()
    {
        // Store the object's initial scale
        factoryResetScale = transform.localScale;
        TryGetComponent(out sphereCollider);
        if (sphereCollider) { sphereCollider.isTrigger = true; sphereCollider.radius *= 0.5f; }
    }

    void OnMouseDown()
    {
        // Store the initial mouse position in screen coordinates
        dragStartMousePosition = Input.mousePosition;
        // Store the object's initial scale
        initialObjectScale = transform.localScale;
    }

    void OnMouseDrag()
    {
        // Calculate the difference in mouse position from the start of the drag
        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 mouseDelta = currentMousePosition - dragStartMousePosition;

        // Calculate the new scale based on mouse movement (e.g., horizontal drag for X scale)
        // You can adjust which axis of mouse movement affects which axis of scale
        float scaleChange = mouseDelta.x * scaleSensitivity;

        Vector3 newScale = initialObjectScale + new Vector3(scaleChange, scaleChange, scaleChange); // Scale uniformly

        // Clamp the scale to prevent it from becoming too small or too large
        newScale.x = Mathf.Max(0.1f, newScale.x); // Example: minimum scale of 0.1
        newScale.y = Mathf.Max(0.1f, newScale.y);
        newScale.z = Mathf.Max(0.1f, newScale.z);

        transform.localScale = newScale;
    }
}
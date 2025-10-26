using System.Collections.Generic;
using UnityEngine;

public class BrainPlayer : MonoBehaviour
{
    [Header("Selection Settings")]
    [Tooltip("Units must be on this layer to be selectable.")]
    public LayerMask unitLayerMask;
    public LayerMask commandLayerMask;

    [Header("Selection Box Visuals")]
    public Color boxFillColor = new Color(0.2f, 0.6f, 1f, 0.25f);
    public Color boxBorderColor = new Color(0.2f, 0.6f, 1f, 1f);
    public float dragThreshold = 10f;

    private Camera mainCam;
    private Vector2 dragStartPos;
    private Vector2 dragEndPos;
    private bool isDragging;

    private readonly List<Unit> selectedUnits = new List<Unit>();

    public GameObject hitpointVfxPrefab;
    private Transform hitpointVfx;
    private VFXClickMarker hitpointVFXParticle;


    private void Awake()
    {
        mainCam = Camera.main;

        print("CONTROLS FOR UNITS" +
            "Input	Result" +
            "\nLeft - click  Selects one unit(clears old selection) " +
            "\nShift + Left - click  Adds clicked unit to current selection" +
            "\nCtrl + Left - click   Removes clicked unit from selection" +
            "\nDrag box    Selects all inside(clears old selection)" +
            "\nShift + Drag box    Adds all inside to selection" +
            "\nCtrl + Drag box Removes all inside from selection");
    }

    private void Update()
    {
        HandleSelectionInput();
        HandleCommandInput();
    }

    private void HandleSelectionInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Input.mousePosition;
            isDragging = false;
        }

        if (Input.GetMouseButton(0))
        {
            if ((Vector2.Distance(dragStartPos, Input.mousePosition)) > dragThreshold)
            {
                isDragging = true;
                dragEndPos = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (!isDragging)
                HandleSingleClick(shift, ctrl);
            else
                HandleBoxSelect(shift, ctrl);

            isDragging = false;
        }      
    }

    private void HandleSingleClick(bool shift, bool ctrl)
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, unitLayerMask))
        {
            Unit clickedUnit = hit.collider.GetComponentInParent<Unit>();
            if (clickedUnit != null)
            {
                if (!shift && !ctrl)
                {
                    ClearSelection();
                    SelectUnit(clickedUnit);
                }
                else if (shift)
                {
                    AddToSelection(clickedUnit);
                }
                else if (ctrl)
                {
                    RemoveFromSelection(clickedUnit);
                }
            }
        }
        else if (!shift && !ctrl)
        {
            // Clear if clicked on empty space without modifier
            ClearSelection();
        }
    }

    private void HandleBoxSelect(bool shift, bool ctrl)
    {
        Vector2 min = Vector2.Min(dragStartPos, dragEndPos);
        Vector2 max = Vector2.Max(dragStartPos, dragEndPos);
        Rect selectionRect = new Rect(min, max - min);

        if (!shift && !ctrl)
            ClearSelection();
        
        Unit[] allUnits = ManagerUnits.unitsSpawned.ToArray(); //FindObjectsOfType<Unit>();
        foreach (Unit u in allUnits)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(u.transform.position);
            if (screenPos.z > 0 && selectionRect.Contains(screenPos, true))
            {
                if (ctrl)
                    RemoveFromSelection(u);
                else
                    AddToSelection(u);
            }
        }
    }

    private void HandleCommandInput()
    {
        if (Input.GetMouseButtonUp(1))
        {
            print("Right Click Release");

            Vector3 mousePos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;
            // Perform the raycast
            if (Physics.Raycast(ray, out hit))
            {
                print($"Raycast hit obj: {hit.transform.name}");
                PlaceVFX(hit);
                switch (hit.transform.tag)
                {
                    default:
                        ManagerUnits.Instance.CommandUnit(UnitData.ActionTaking.Walking, hit.point);
                        break;
                }
            }       
        }
    }


    public void PlaceVFX(RaycastHit _hit)
    {
        if (hitpointVfx)
        {
            hitpointVfx.transform.position = _hit.point;
            if (hitpointVFXParticle) hitpointVFXParticle.PlayParticle();
        }
        else
        {
            if (hitpointVfxPrefab) hitpointVfx = Instantiate(hitpointVfxPrefab.transform, _hit.point, hitpointVfxPrefab.transform.rotation);
            hitpointVfx.TryGetComponent(out hitpointVFXParticle);
        }
    }

    private void AddToSelection(Unit unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            unit.IsSelected = true;
            HighlightUnit(unit, true);
        }
    }

    private void RemoveFromSelection(Unit unit)
    {
        if (selectedUnits.Contains(unit))
        {
            selectedUnits.Remove(unit);
            unit.IsSelected = false;
            HighlightUnit(unit, false);
        }
    }

    private void SelectUnit(Unit unit)
    {
        selectedUnits.Add(unit);
        unit.IsSelected = true;
        HighlightUnit(unit, true);
    }

    private void ClearSelection()
    {
        foreach (Unit u in selectedUnits)
        {
            u.IsSelected = false;
            HighlightUnit(u, false);
        }
        selectedUnits.Clear();
    }

    private void HighlightUnit(Unit unit, bool highlight)
    {
        Renderer r = unit.GetComponentInChildren<Renderer>();
        if (r != null)
            r.material.color = highlight ? Color.cyan : Color.white;
    }

    // --- UI / Debug Drawing ---
    private void OnGUI()
    {
        if (isDragging)
        {
            Rect rect = GetScreenRect(dragStartPos, Input.mousePosition);
            DrawScreenRect(rect, boxFillColor);
            DrawScreenRectBorder(rect, 2, boxBorderColor);
        }
    }

    private Rect GetScreenRect(Vector2 start, Vector2 end)
    {
        start.y = Screen.height - start.y;
        end.y = Screen.height - end.y;
        Vector2 topLeft = Vector2.Min(start, end);
        Vector2 bottomRight = Vector2.Max(start, end);
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    private void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
    }
}

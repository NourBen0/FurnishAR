using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;

public class FurniturePlacementManager : MonoBehaviour
{
    [Tooltip("Prefab à instancier lorsque l’utilisateur touche un plan détecté")]
    public GameObject spawnableFurniture;

    public XROrigin xrOrigin;                // Référence au XROrigin
    public ARRaycastManager raycastManager;  // Doit être assigné
    public ARPlaneManager planeManager;      // Pour afficher / cacher les plans

    private readonly List<ARRaycastHit> raycastHits = new();

    private void Update()
{
    bool isTouchBegan = false;
    Vector2 touchPos = Vector2.zero;

    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
    {
        isTouchBegan = true;
        touchPos = Input.GetTouch(0).position;
    }
    else if (Input.GetMouseButtonDown(0))
    {
        isTouchBegan = true;
        touchPos = Input.mousePosition;
    }

    if (!isTouchBegan) return;

    // Ignore si tap sur UI
    if (isButtonPressedAt(touchPos))
    {
        Debug.Log("Tap ignoré : doigt ou souris sur UI.");
        return;
    }

    // Raycast AR
    bool hit = raycastManager.Raycast(touchPos, raycastHits, TrackableType.PlaneWithinPolygon);
    Debug.Log($"Raycast = {hit} | hits = {raycastHits.Count}");

    if (!hit) return;

    GameObject spawned = Instantiate(spawnableFurniture);
    spawned.transform.SetPositionAndRotation(
        raycastHits[0].pose.position,
        raycastHits[0].pose.rotation);

    Debug.Log($"Objet instancié : {spawned.name} à {raycastHits[0].pose.position}");
}

public bool isButtonPressedAt(Vector2 position)
{
    PointerEventData eventData = new PointerEventData(EventSystem.current)
    {
        position = position
    };

    List<RaycastResult> results = new();
    EventSystem.current.RaycastAll(eventData, results);

    foreach (var r in results)
    {
        if (r.gameObject.GetComponent<Button>() != null ||
            r.gameObject.GetComponent<Selectable>() != null ||
            r.gameObject.GetComponent<Toggle>() != null)
            return true;
    }
    return false;
}


    /// <summary>
    /// Change le prefab courant (appelé par tes boutons UI).
    /// </summary>
    public void SwitchFurniture(GameObject furniture)
    {
        spawnableFurniture = furniture;
        // Libère aussitôt le focus du bouton
        EventSystem.current.SetSelectedGameObject(null);
        Debug.Log($"Prefab sélectionné : {furniture.name}");
    }
}

using UnityEngine;

public class BottleController : MonoBehaviour
{
    [Header("Pouring Settings")]
    [SerializeField] private float pourAngle = 90f;
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private Transform pourPoint;

    private bool isPouring = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    private void Start()
    {
        initialRotation = transform.rotation;
        
        // If pourPoint is not assigned, try to find it or create it
        if (pourPoint == null)
        {
            Transform existingPoint = transform.Find("PourPoint");
            if (existingPoint != null)
            {
                pourPoint = existingPoint;
            }
            else
            {
                GameObject point = new GameObject("PourPoint");
                point.transform.SetParent(transform);
                point.transform.localPosition = new Vector3(0, 0.25f, 0); // Approximate top of bottle
                point.transform.localRotation = Quaternion.identity;
                pourPoint = point.transform;
            }
        }
    }

    private void Update()
    {
        HandleInput();
        HandleRotation();
        
        if (isPouring)
        {
            Pour();
        }
    }

    private void HandleInput()
    {
        // Input logic handled via OnMouseDown/Up for specific object interaction
        // But we can also check if we are currently pouring based on the flag set by mouse events
    }

    private void OnMouseDown()
    {
        isPouring = true;
    }

    private void OnMouseUp()
    {
        isPouring = false;
    }

    private void HandleRotation()
    {
        Quaternion target = isPouring ? 
            initialRotation * Quaternion.Euler(pourAngle, 0, 0) : 
            initialRotation;

        transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * rotateSpeed);
    }

    private void Pour()
    {
        Debug.Log("Pouring...");
        if (pourPoint != null)
        {
            // Draw a ray downwards from the pour point
            Debug.DrawRay(pourPoint.position, Vector3.down * 1.0f, Color.cyan);
        }
    }
}

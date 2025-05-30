using UnityEngine;
using UnityEngine.Networking;

public class OtterPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 10f;
    
    [Header("Building Settings")]
    public GameObject wallPrefab;
    public GameObject rampPrefab;
    public GameObject floorPrefab;
    public float buildDistance = 5f;
    
    private Rigidbody rb;
    private Camera playerCamera;
    private bool isGrounded;
    private float verticalRotation = 0f;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void Update()
    {
        HandleMovement();
        HandleBuilding();
        HandleJump();
    }
    
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = transform.right * horizontal + transform.forward * vertical;
        transform.position += movement * moveSpeed * Time.deltaTime;
        
        // Handle mouse look
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
        
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
    
    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    
    private void HandleBuilding()
    {
        if (Input.GetMouseButtonDown(1)) // Right click to build
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, buildDistance))
            {
                Vector3 buildPosition = hit.point + hit.normal * 0.5f;
                buildPosition = new Vector3(
                    Mathf.Round(buildPosition.x),
                    Mathf.Round(buildPosition.y),
                    Mathf.Round(buildPosition.z)
                );
                
                GameObject buildPiece = null;
                
                if (Input.GetKey(KeyCode.Q))
                    buildPiece = Instantiate(wallPrefab, buildPosition, Quaternion.identity);
                else if (Input.GetKey(KeyCode.E))
                    buildPiece = Instantiate(rampPrefab, buildPosition, Quaternion.identity);
                else if (Input.GetKey(KeyCode.F))
                    buildPiece = Instantiate(floorPrefab, buildPosition, Quaternion.identity);
            }
        }
    }
    
    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }
    
    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
} 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonControls : MonoBehaviour
{

    [Header("MOVEMENT SETTINGS")]
    [Space(5)]
    // Public variables to set movement and look speed, and the player camera
    public float moveSpeed; // Speed at which the player moves
    public float lookSpeed; // Sensitivity of the camera movement
    public float gravity = -9.81f; // Gravity value
    public float jumpHeight = 1.0f; // Height of the jump
    
    public Transform player; // Reference to the player's camera
                                   // Private variables to store input values and the character controller
    private Vector2 moveInput; // Stores the movement input from the player
    private Vector2 lookInput; // Stores the look input from the player
   
    private Vector3 velocity; // Velocity of the player
    private CharacterController characterController; // Reference to the CharacterController component

    private float originalMoveSpeed =2f;
    private float originalCrouchSpeed =2.5f;
    private float speedBoostEndTime = 0f;
    private bool isBoosted = false; // Whether the player is currently boosted
    public float speedBoostAmount = 5f;

    private HealthSystem healthSystem;

    [Header("speed  UP SETTINGS")]
    [Space(5)]
    private SpeedPowerUp speedPowerUp; // Position where the picked-up object will be held
    
    [Header("SHOOTING SETTINGS")]
    [Space(5)]
    public GameObject projectilePrefab; // Projectile prefab for shooting
    public Transform firePoint; // Point from which the projectile is fired
    public float projectileSpeed = 20f; // Speed at which the projectile is fired
    public float pickUpRange = 3f; // Range within which objects can be picked up
    private bool holdingGun = true;

    [Header("PICKING UP SETTINGS")]
    [Space(5)]
    public Transform holdPosition; // Position where the picked-up object will be held
    private GameObject heldObject; // Reference to the currently held object

    // Crouch settings
    [Header("CROUCH SETTINGS")]
    [Space(5)]
    public float crouchHeight = 1f; // Height of the player when crouching
    public float standingHeight = 2f; // Height of the player when standing
    public float crouchSpeed = 2.5f; // Speed at which the player moves when crouching
    private bool isCrouching = false; // Whether the player is currently crouching

    [Header("INTERACT SETTINGS")]
    [Space(5)]
    public Material switchMaterial; // Material to apply when switch is activated
    public GameObject[] objectsToChangeColor; // Array of objects to change color

    public GameObject gunGameObject; 
    [Header("VISUAL EFFECTS")]
    public ParticleSystem speedSpikes;


    [Header("PAUSE SETTINGS")]
    [Space(1)]
    public bool isPaused = false;
    public GameObject pauseScreen;


    private void Awake()
    {

        // Get and store the CharacterController component attached to this GameObject
        characterController = GetComponent<CharacterController>();
        speedPowerUp = GetComponent<SpeedPowerUp>(); // Initialize the SpeedPowerUp component
        healthSystem = GetComponent<HealthSystem>();
    }

    private void OnEnable()
    {
        // Create a new instance of the input actions
        var playerInput = new Controls();

        // Enable the input actions
        playerInput.Player.Enable();

        playerInput.Pause.Enable();

        // Subscribe to the movement input events
        playerInput.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>(); // Update moveInput when movement input is performed
        playerInput.Player.Move.canceled += ctx => moveInput = Vector2.zero; // Reset moveInput when movement input is canceled

        // Subscribe to the look input events
        //playerInput.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>(); // Update lookInput when look input is performed
        //playerInput.Player.Look.canceled += ctx => lookInput = Vector2.zero; // Reset lookInput when look input is canceled

        // Subscribe to the jump input event
        playerInput.Player.Jump.performed += ctx => Jump(); // Call the Jump method when jump input is performed

        // Subscribe to the shoot input event
        playerInput.Player.Shoot.performed += ctx => Shoot(); // Call the Shoot method when shoot input is performed

        // Subscribe to the pick-up input event
        playerInput.Player.PickUp.performed += ctx => PickUpObject(); // Call the PickUpObject method when pick-up input is performed

        // Subscribe to the crouch input event
        playerInput.Player.Crouch.performed += ctx => ToggleCrouch(); // Call the ToggleCrouch method when crouch input is performed

        // Subscribe to the interact input event
        //playerInput.Player.Interact.performed += ctx => Interact(); // Interact with switch

        playerInput.Player.Sprint.performed += ctx => Sprinting();

        playerInput.Player.Sprint.canceled += ctx => SprintingStopped();

        playerInput.Player.Sprint.canceled += ctx => Walking();

        playerInput.Pause.PauseMenu.performed += ctx => PauseMenu();
       


    }

    private void Start()
    {
            originalMoveSpeed = moveSpeed; // Use whatever value is set in Inspector
           originalCrouchSpeed = crouchSpeed;

    }

    private void Update()
    {
        // Call Move and LookAround methods every frame to handle player movement and camera rotation
        Move();
        LookAround();
        ApplyGravity();

        // Handle speed boost expiration
        isBoosted = Time.time < speedBoostEndTime;

        if (speedSpikes != null)
        {
            if (isBoosted && !speedSpikes.isPlaying)
            {
                speedSpikes.Play();
            }
            else if (!isBoosted && speedSpikes.isPlaying)
            {
                speedSpikes.Stop();
            }
        }
    }

    private void Move()
    {
        // Calculate current speed based on state
        float currentSpeed = isCrouching ? crouchSpeed : moveSpeed;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        if (move.magnitude >= 0.1f)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);
        }

        characterController.Move(move * currentSpeed * Time.deltaTime);
    }



    public void LookAround()
    {
        // Get horizontal and vertical look inputs and adjust based on sensitivity
        float LookX = lookInput.x * lookSpeed;
        float LookY = lookInput.y * lookSpeed;

       

        transform.Rotate(0, LookX, 0); // rotates player (fine)
       
    }

    public void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -0.5f; // Small value to keep the player grounded
        }

        velocity.y += gravity * Time.deltaTime; // Apply gravity to the velocity
        characterController.Move(velocity * Time.deltaTime); // Apply the velocity to the character
    }

    public void Jump()
    {
        if (characterController.isGrounded)
        {
            // Calculate the jump velocity
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void Shoot()
    {
        if (holdingGun == true)
        {
            gunGameObject.SetActive(true);
            // Make Gun Object Active when start in round 3 

            // Instantiate the projectile at the fire point
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            // Get the Rigidbody component of the projectile and set its velocity
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.velocity = firePoint.forward * projectileSpeed;

            // Destroy the projectile after 3 seconds
            Destroy(projectile, 3f);
        }
    }

    public void PickUpObject()
    {
        // Check if we are already holding an object
        if (heldObject != null)
        {
            heldObject.GetComponent<Rigidbody>().isKinematic = false; // Enable physics
            heldObject.transform.parent = null;
            holdingGun = false;
        }

        // Perform a raycast from the camera's position forward
        Ray ray = new Ray(player.position, player.forward);
        RaycastHit hit;

        // Debugging: Draw the ray in the Scene view
        Debug.DrawRay(player.position, player.forward * pickUpRange, Color.red, 2f);


        if (Physics.Raycast(ray, out hit, pickUpRange))
        {
            // Check if the hit object has the tag "PickUp"
            if (hit.collider.CompareTag("PickUp"))
            {
                // Pick up the object
                heldObject = hit.collider.gameObject;
                heldObject.GetComponent<Rigidbody>().isKinematic = true; // Disable physics

                // Attach the object to the hold position
                heldObject.transform.position = holdPosition.position;
                heldObject.transform.rotation = holdPosition.rotation;
                heldObject.transform.parent = holdPosition;
            }
            else if (hit.collider.CompareTag("Gun"))
            {
                // Pick up the object
                heldObject = hit.collider.gameObject;
                heldObject.GetComponent<Rigidbody>().isKinematic = true; // Disable physics

                // Attach the object to the hold position
                heldObject.transform.position = holdPosition.position;
                heldObject.transform.rotation = holdPosition.rotation;
                heldObject.transform.parent = holdPosition;

                holdingGun = true;
            }
        }
    }

    public void ToggleCrouch()
    {
        if (isCrouching)
        {
            // Stand up
            characterController.height = standingHeight;
            isCrouching = false;
        }
        else
        {
            // Crouch down
            characterController.height = crouchHeight;
            isCrouching = true;
        }
    }

/*public void Interact()
    {
        // get camera transform - adjust this based on your FPC2 script structure
        Transform cameraTransform = player; // for FirstPersonControls
        // if this is FPC2, you might need to adjust how you get the camera reference
        
        // perform a raycast to detect objects
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickUpRange))
        {
            // check for collectibles (pills and cures)
            if (hit.collider.CompareTag("Collectible") || hit.collider.CompareTag("Cure"))
            {
                CollectibleSystem collectible = hit.collider.GetComponent<CollectibleSystem>();
                if (collectible != null)
                {
                    collectible.OnInteracted(this); // pass this script (works for both FPC and FPC2)
                    return;
                }
            }
            
            // original switch interaction
            if (hit.collider.CompareTag("Switch"))
            {
                // change the material color of the objects in the array
                foreach (GameObject obj in objectsToChangeColor)
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = switchMaterial.color; // set the color to match the switch material color
                    }
                }
            }
        }
    }*/

    public void PauseMenu()
    
    {
        if (isPaused == false)
        {
            isPaused = true;
            pauseScreen.SetActive(true);
            Time.timeScale = 0;
            Debug.Log("should pause");
        }

        else if (isPaused == true)
        {
            isPaused = false;
            pauseScreen.SetActive(false);
            Time.timeScale = 1;
            Debug.Log("should unpaused");
        }
    }

 
    public void ApplySpeedBoost(float duration)
    {

        speedBoostEndTime = Time.time + duration;
        moveSpeed = originalMoveSpeed * speedBoostAmount; // Always apply boost
        crouchSpeed = originalCrouchSpeed * 1.5f; // Boost crouch speed too if needed
        Debug.Log("Speed Boost Applied");
    }

    public void Sprinting()
    {
        moveSpeed = isBoosted ? originalMoveSpeed * 2.5f : originalMoveSpeed * 2.0f;
        Debug.Log("Sprinting Started");
    }

    public void SprintingStopped()
    {
        moveSpeed = isBoosted ? originalMoveSpeed * 1.5f : originalMoveSpeed;
    }

    public void Walking()
    {
        moveSpeed = isBoosted ? originalMoveSpeed * 1.5f : originalMoveSpeed;
    }


}


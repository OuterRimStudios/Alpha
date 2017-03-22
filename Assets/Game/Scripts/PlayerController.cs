using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    public float speed;
    public float jumpForce;

    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;

    public bool clampVerticalRotation = true;
    public bool lockCursor;

    float horizontal;
    float horizontal2;
    float vertical;
    float vertical2;
    float currentSpeed;

    float rotY = 0.0f;
    float rotX = 0.0f;

    float groundCheckDistance = 1.1f;
    float originalGroundCheckDistance;

    float lastSynchronizationTime = 0f;
    float syncDelay = 0f;
    float syncTime = 0f;

    string saveData;

    bool isGrounded;

    Vector3 syncStartPosition = Vector3.zero;
    Vector3 syncEndPosition = Vector3.zero;

    Quaternion cameraTargetRot;
    Quaternion myRotation;

    Transform myCamera;

    Controls controls;

    Rigidbody rb;

    void OnEnable()
    {
        controls = Controls.CreateWithDefaultBindings();
    }

    void OnDisable()
    {
        controls.Destroy();
    }

    void Awake()
    {
        lastSynchronizationTime = Time.time;
    }

    void Start()
    {
        myCamera = transform.FindChild("Main Camera");
        rb = GetComponent<Rigidbody>();

        if (isLocalPlayer)
            myCamera.gameObject.SetActive(true);

        currentSpeed = speed;

        if (lockCursor)
            Cursor.visible = false;
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        else
        {
            if (controls.Fire.WasPressed)
                CmdFire();

            CheckGroundedStatus();

            if (controls.Jump.WasPressed && isGrounded)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                CheckGroundedStatus();
            }

            horizontal = controls.Move.X;
            horizontal2 = controls.Look.X;

            vertical = controls.Move.Y;
            vertical2 = controls.Look.Y;

            rotY += horizontal2 * mouseSensitivity * Time.deltaTime;
            rotX += -vertical2 * mouseSensitivity * Time.deltaTime;
            rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

            Quaternion localRotation = Quaternion.Euler(0f, rotY, 0f);
            transform.rotation = localRotation;
            myCamera.rotation = Quaternion.Euler(rotX, rotY, 0f);
            
            Vector3 moveDirection = new Vector3(horizontal * Time.deltaTime * currentSpeed, 0, vertical * Time.deltaTime * currentSpeed);
            moveDirection = transform.TransformDirection(moveDirection);
            rb.MovePosition(transform.position + moveDirection);
        }
    }

    private void SyncedMovement()
    {
        syncTime += Time.deltaTime;
        transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    void SaveBindings()
    {
        saveData = controls.Save();
        PlayerPrefs.SetString("Bindings", saveData);
    }

    void LoadBindings()
    {
        if (PlayerPrefs.HasKey("Bindings"))
        {
            saveData = PlayerPrefs.GetString("Bindings");
            controls.Load(saveData);
        }
    }

    [Command]
    void CmdFire()
    {
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);
        
        bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 8;
        
        NetworkServer.Spawn(bullet);
        
        Destroy(bullet, 2.0f);
    }
    void CheckGroundedStatus()
    {
        RaycastHit hitInfo;
        Ray ray = new Ray(new Vector3(transform.position.x, transform.localPosition.y * .1f, transform.position.z), Vector3.down);
        Debug.DrawLine(ray.origin, new Vector3(transform.position.x, 0, transform.position.z), Color.red);
        if (Physics.Raycast(ray.origin, Vector3.down, out hitInfo, 1.1f))
        {
            print("Raycast hitting ground");
            if ((hitInfo.collider.tag == "Ground" || hitInfo.collider.tag == "Environment") && hitInfo.distance <= groundCheckDistance)
            {
                print("IsGrounded");
                isGrounded = true;
            }
        }
        else
        {
            isGrounded = false;
        }
    }
}

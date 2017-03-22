using UnityEngine;
using System.Collections;
using InControl;
using UnityEngine.Networking;

public class Movement : NetworkBehaviour
{
    public float speed;
    public float rotationSpeed;

    public float minVerticalClamp = -65;
    public float maxVerticalClamp = 65;

    public bool clampVerticalRotation = true;

    Transform myCamera;

    float horizontal;
  //  float horizontal2;
    float vertical;
   // float vertical2;
    float currentSpeed;

    Quaternion cameraTargetRot;
    Quaternion myRotation;

    string saveData;

    Controls controls;

    void OnEnable()
    {
        controls = Controls.CreateWithDefaultBindings();
    }

    void OnDisable()
    {
        controls.Destroy();
    }

    void Start ()
    {
        myCamera = transform.FindChild("Main Camera");
        if (isLocalPlayer)
            myCamera.gameObject.SetActive(true);
        currentSpeed = speed;
	}
	
	void Update ()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        horizontal = controls.Move.X;
      //  horizontal2 = controls.Look.X;

        vertical = controls.Move.Y;
     //   vertical2 = controls.Look.Y;

        //cameraTargetRot *= Quaternion.Euler(-vertical2 * rotationSpeed, 0f, 0f);
        //myRotation *= Quaternion.Euler(0f, horizontal2 * rotationSpeed * Time.deltaTime, 0f);

        //if (clampVerticalRotation)
        //    cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);

        //myCamera.localRotation = Quaternion.Slerp(myCamera.localRotation, cameraTargetRot, 1);

        transform.Translate(new Vector3(horizontal * Time.deltaTime * currentSpeed, 0f, vertical * Time.deltaTime * currentSpeed));
       // transform.localRotation = Quaternion.Slerp(transform.localRotation, myRotation, 1);
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

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, minVerticalClamp, maxVerticalClamp);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
        return q;
    }
}
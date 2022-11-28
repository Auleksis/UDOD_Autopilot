using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class AirplaneController : MonoBehaviour
{
    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
    [SerializeField]
    List<WheelCollider> wheels = null;
    [SerializeField]
    float rollControlSensitivity = 0.2f;
    [SerializeField]
    float pitchControlSensitivity = 0.2f;
    [SerializeField]
    float yawControlSensitivity = 0.2f;

    [Range(-1, 1)]
    public float Pitch;
    [Range(-1, 1)]
    public float Yaw;
    [Range(-1, 1)]
    public float Roll;
    [Range(0, 1)]
    public float Flap;
    [SerializeField]
    Text displayText = null;

    float thrustPercent = 0f;
    float brakesTorque;

    public GameObject[] targets;
    public GameObject currentTarget;
    [HideInInspector] public int current = 0;

    AircraftPhysics aircraftPhysics;
    Rigidbody rb;

    //Параметры полёта
    private float velocity = 0f;
    private float angular_velocity = 0f;
    private float height;
    private float current_teta = 0f;

    //Параметры самолёта
    private float wings_s = 3; //площадь крыльев
   
    //Коэффициент полной аэродинамической силы
    private float СR = 0f;

    //Коэффициент подъёмной силы
    private float Cy = 2f;

    private float rise_veloity;


    //Плотность воздуха
    private const float DESTINY_OF_AIR = 1.2041f;

    private const float g = 9.81f;

    private const float DELTA = 0 ;



    //Маршрут
    private Vector3 point1;

    private void Start()
    {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();

        currentTarget = targets[current];

        //thrustPercent = 0.9f;
        //rb.velocity = new Vector3(0, 0, 100);

        height = transform.position.y;

        rise_veloity = Mathf.Sqrt(2 * rb.mass * g / Cy / wings_s / DESTINY_OF_AIR);

        point1 = transform.position + new Vector3(transform.position.x, 100, 3000);
    }

    private void Update()
    {
        /*
         * Pitch - тангаж
         * Roll - крен
         * Yaw - курс
         */

        Pitch = Input.GetAxis("Vertical");
        Roll = Input.GetAxis("Horizontal");
        Yaw = Input.GetAxis("Yaw");

        if (Input.GetKeyDown(KeyCode.K))
        {
            increaseThrust(1);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            decreaseThrust(0);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Flap = Flap > 0 ? 0 : 0.3f;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            brakesTorque = brakesTorque > 0 ? 0 : 100f;
        }

        displayText.text = "V: " + ((int)rb.velocity.magnitude).ToString("D3") + " m/s\n";
        displayText.text += "A: " + ((int)transform.position.y).ToString("D4") + " m\n";
        displayText.text += "T: " + (int)(thrustPercent * 100) + "%\n";
        displayText.text += brakesTorque > 0 ? "B: ON" : "B: OFF";

        AutoControl();
    }

    private void FixedUpdate()
    {
        SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustPercent(thrustPercent);
        foreach (var wheel in wheels)
        {
            wheel.brakeTorque = brakesTorque;
            // small torque to wake up wheel collider
            wheel.motorTorque = 0.01f;
        }
    }

    public void SetControlSurfecesAngles(float pitch, float roll, float yaw, float flap)
    {
        foreach (var surface in controlSurfaces)
        {
            if (surface == null || !surface.IsControlSurface) continue;
            switch (surface.InputType)
            {
                case ControlInputType.Pitch:
                    surface.SetFlapAngle(pitch * pitchControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll * rollControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw * yawControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(Flap * surface.InputMultiplyer);
                    break;
            }
        }
    }

    private void AutoControl()
    {
        velocity = rb.velocity.magnitude;
        height = transform.position.y;
        current_teta = transform.rotation.x;

        if (height < 10)
            Rise();
        else
            GetPoint(point1);

        /*Debug.Log("rise velocity = " + rise_veloity);
        Debug.Log("current velocity = " + velocity);
        Debug.Log("teta = " + current_teta);*/

        Vector3 direction = transform.forward;

        Vector3 directionToTarget = currentTarget.transform.position - transform.position;

        //CalculateRoll(direction, directionToTarget);
    }

    private void GetPoint(Vector3 target)
    {
        float delta_height = target.y - height;
        float distance = (target - transform.position).magnitude;

        if(delta_height > 10)
        {
            increaseThrust(0.7f);
        }
        else if(delta_height < 0)
        {
            decreaseThrust(0.1f);
        }

        float target_teta = Mathf.Atan(delta_height / distance) * (-1);

        Debug.Log("target_teta = " + target_teta);
        Debug.Log("vel = " + rb.velocity);
        Debug.Log("vector = " + (point1 - transform.position));

        GetTeta(target_teta);
    }

    private void Rise()
    {
        
        if(velocity < rise_veloity)
        {
            increaseThrust(1);
        }
        else
        {
            GetTeta(-15);
        }
        
    }

    private void GetTeta(float degrees)
    {
        if(current_teta > degrees + DELTA)
        {
            Pitch = -0.3f;
        }
        else if(current_teta < degrees - DELTA)
        {
            Pitch = 0.3f;
        }
        else
            Pitch = 0;
    }

    private void CalculateRoll(Vector3 direction, Vector3 directionToTarget)
    {
        float roll = Vector3.Angle(direction, directionToTarget);
        Debug.Log(roll);

        if(roll > 0) {
            Yaw = -1;
            
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
    }


    private void increaseThrust(float limit)
    {
        thrustPercent += 0.1f;
        if (thrustPercent > limit)
            thrustPercent = limit;
    }

    private void decreaseThrust(float limit)
    {
        thrustPercent -= 0.1f;
        if (thrustPercent < limit)
            thrustPercent = limit;
    }
}

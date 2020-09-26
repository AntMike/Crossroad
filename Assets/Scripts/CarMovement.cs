using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private CarWheels carWheels;

    [SerializeField] private float m_MaximumSteerAngle = 25.0f;
    [Range(0, 1)] [SerializeField] private float m_SteerHelper = 0.7f; // 0 is raw physics , 1 the car will grip in the direction it is facing
    [Range(0, 1)] [SerializeField] private float m_TractionControl = 1.0f; // 0 is no traction control, 1 is full interference
    [SerializeField] private float m_FullTorqueOverAllWheels = 2000.0f;
    [SerializeField] private float m_ReverseTorque = 150.0f;
    [SerializeField] private float m_Downforce = 100f;
    [SerializeField] private float m_Topspeed = 140.0f;
    [SerializeField] private static int NoOfGears = 5;
    [SerializeField] private float m_RevRangeBoundary = 1f;
    [SerializeField] private float m_SlipLimit = 0.4f;
    [SerializeField] private float m_BrakeTorque = 20000.0f;
    [SerializeField] private float m_BrakeTorqueForCrossroad = -500;

    private Vector3 m_Prevpos, m_Pos;
    private float m_SteerAngle;
    private int m_GearNum;
    private float m_GearFactor;
    private float m_OldRotation;
    private float m_CurrentTorque;
    private Rigidbody m_Rigidbody;
    private const float k_ReversingThreshold = 0.01f;

    public bool Skidding { get; private set; }
    public float obstaclesBrakeInput;
    public float brakeInput;
    public float CurrentSteerAngle { get { return m_SteerAngle; } }
    public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * 2.23693629f; } }
    public float MaxSpeed { get { return m_Topspeed; } }
    public float Revs { get; private set; }
    public float AccelInput { get; private set; }

    private void Awake()
    {
        carWheels = GetComponent<CarWheels>();
    }

    // Use this for initialization
    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl * m_FullTorqueOverAllWheels);
    }


    private void GearChanging()
    {
        float f = Mathf.Abs(CurrentSpeed / MaxSpeed);
        float upgearlimit = (1 / (float)NoOfGears) * (m_GearNum + 1);
        float downgearlimit = (1 / (float)NoOfGears) * m_GearNum;

        if (m_GearNum > 0 && f < downgearlimit)
        {
            m_GearNum--;
        }

        if (f > upgearlimit && (m_GearNum < (NoOfGears - 1)))
        {
            m_GearNum++;
        }
    }


    // simple function to add a curved bias towards 1 for a value in the 0-1 range
    private static float CurveFactor(float factor)
    {
        return 1 - (1 - factor) * (1 - factor);
    }


    // unclamped version of Lerp, to allow value to exceed the from-to range
    private static float UnclampedLerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }


    private void CalculateGearFactor()
    {
        float f = (1 / (float)NoOfGears);
        // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
        // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
        var targetGearFactor = Mathf.InverseLerp(f * m_GearNum, f * (m_GearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
        m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime * 5f);
    }


    private void CalculateRevs()
    {
        // calculate engine revs (for display / sound)
        // (this is done in retrospect - revs are not used in force/power calculations)
        CalculateGearFactor();
        var gearNumFactor = m_GearNum / (float)NoOfGears;
        var revsRangeMin = UnclampedLerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
        var revsRangeMax = UnclampedLerp(m_RevRangeBoundary, 1f, gearNumFactor);
        Revs = UnclampedLerp(revsRangeMin, revsRangeMax, m_GearFactor);
    }


    public void Move(float accel, float footbrake, float steering = 0)
    {
        //clamp input values
        steering = Mathf.Clamp(steering, -1, 1);
        AccelInput = accel = Mathf.Clamp(accel, 0, 1);
        footbrake = Mathf.Clamp(brakeInput + obstaclesBrakeInput, -1, 1);


        //Set the steer on the front wheels.
        //Assuming that wheels 0 and 1 are the front wheels.
        m_SteerAngle = steering * m_MaximumSteerAngle;
        carWheels.WheelColliders[0].steerAngle = m_SteerAngle;
        carWheels.WheelColliders[1].steerAngle = m_SteerAngle;

        SteerHelper();
        ApplyDrive(accel);
        CapSpeed();


        CalculateRevs();
        GearChanging();

        AddDownForce();
        TractionControl();
    }


    private void CapSpeed()
    {
        float speed = m_Rigidbody.velocity.magnitude;

        speed *= 3.6f;
        if (speed > m_Topspeed)
            m_Rigidbody.velocity = (m_Topspeed / 3.6f) * m_Rigidbody.velocity.normalized;

    }


    private void ApplyDrive(float accel)
    {

        float thrustTorque;

        thrustTorque = accel * (m_CurrentTorque / 2f);
        carWheels.WheelColliders[2].motorTorque = carWheels.WheelColliders[3].motorTorque = thrustTorque;


        for (int i = 0; i < carWheels.WheelColliders.Length; i++)
        {
            if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, m_Rigidbody.velocity) < 50f)
            {
                carWheels.WheelColliders[i].brakeTorque = m_BrakeTorque * brakeInput + m_BrakeTorqueForCrossroad*obstaclesBrakeInput;
            }
            else if (brakeInput+obstaclesBrakeInput > 0)
            {
                carWheels.WheelColliders[i].brakeTorque = 0f;
                carWheels.WheelColliders[i].motorTorque = -(m_BrakeTorque * brakeInput + m_BrakeTorqueForCrossroad * obstaclesBrakeInput);
            }
            else if (brakeInput + obstaclesBrakeInput < 0)
            {
                carWheels.WheelColliders[i].brakeTorque = m_BrakeTorque * brakeInput + m_BrakeTorqueForCrossroad * obstaclesBrakeInput;
            } else
            {
                carWheels.WheelColliders[i].brakeTorque = 0;
            }
        }
    }


    private void SteerHelper()
    {
        for (int i = 0; i < carWheels.WheelColliders.Length; i++)
        {
            WheelHit wheelhit;
            carWheels.WheelColliders[i].GetGroundHit(out wheelhit);
            if (wheelhit.normal == Vector3.zero)
                return; // wheels arent on the ground so dont realign the rigidbody velocity
        }

        // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
        if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
        {
            var turnadjust = (transform.eulerAngles.y - m_OldRotation) * m_SteerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
        }
        m_OldRotation = transform.eulerAngles.y;
    }


    // this is used to add more grip in relation to speed
    private void AddDownForce()
    {
        carWheels.WheelColliders[0].attachedRigidbody.AddForce(-transform.up * m_Downforce *
                                                        carWheels.WheelColliders[0].attachedRigidbody.velocity.magnitude);
    }

    // crude traction control that reduces the power to wheel if the car is wheel spinning too much
    private void TractionControl()
    {
        WheelHit wheelHit;

        carWheels.WheelColliders[2].GetGroundHit(out wheelHit);
        AdjustTorque(wheelHit.forwardSlip);

        carWheels.WheelColliders[3].GetGroundHit(out wheelHit);
        AdjustTorque(wheelHit.forwardSlip);

    }


    private void AdjustTorque(float forwardSlip)
    {
        if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
        {
            m_CurrentTorque -= 10 * m_TractionControl;
        }
        else
        {
            m_CurrentTorque += 10 * m_TractionControl;
            if (m_CurrentTorque > m_FullTorqueOverAllWheels)
            {
                m_CurrentTorque = m_FullTorqueOverAllWheels;
            }
        }
    }
}
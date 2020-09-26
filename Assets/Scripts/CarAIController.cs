using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CarMovement))]
public class CarAIController : MonoBehaviour
{
    private Rigidbody rigbody;
    private BoxCollider bc;
    [SerializeField] private Path movePath;
    private NavMeshPath navMeshPath;
    private CarMovement carMovement;
    private NavMeshAgent navigator;
    private float startSpeed;
    [SerializeField] private float curMoveSpeed;
    [SerializeField] private float angleBetweenPoint;
    private float targetSteerAngle;

    [SerializeField] private bool canGoToCrossroad = true;
    [SerializeField] private bool isOnCrossroad = false;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float speedIncrease;
    [SerializeField] private float speedDecrease;
    [SerializeField] private float maxAngleToMoveBreak = 8.0f;


    private int pathID = 0;
    private int wayID = 0;
    private int pointID = 0;
    private bool needNewPath = false;
    [SerializeField] private float poindDestinationDistance = .5f;

    private Vector3 nextPoint;

    public bool CanGoToCrossroad
    {
        get { return canGoToCrossroad; }
        set { canGoToCrossroad = value; }
    }

    public bool IsOnCrossroad
    {
        get { return isOnCrossroad; }
        set { isOnCrossroad = value; }
    }

    public float MoveSpeed
    {
        get { return moveSpeed; }
        set { moveSpeed = value; }
    }

    public float SpeedIncrease
    {
        get { return speedIncrease; }
        set { speedIncrease = value; }
    }

    public float SpeedDecrease
    {
        get { return speedDecrease; }
        set { speedDecrease = value; }
    }

    public float StartSpeed
    {
        get { return startSpeed; }
        private set { }
    }
    

    private void Awake()
    {
        rigbody = GetComponent<Rigidbody>();
        carMovement = GetComponent<CarMovement>();
        navigator = GetComponent<NavMeshAgent>();
        navMeshPath = new NavMeshPath();
        nextPoint = transform.position;
    }

    private void Start()
    {
        startSpeed = moveSpeed;

        WheelCollider[] wheelColliders = GetComponentsInChildren<WheelCollider>();
    }

    private void Update()
    {
        PushRay();
    }

    private void FixedUpdate()
    {
        GetPath();
    }

    //Calculate path and send move event to car script
    private void GetPath()
    {

        if (Vector3.Distance(transform.position, nextPoint) < poindDestinationDistance)
        {
            (nextPoint, pathID, wayID, pointID, needNewPath) = movePath.GetNextPoint(pathID, wayID, pointID);
            if(needNewPath)
            {
                movePath = GlobalController.Instance.allPaths[Random.Range(0, GlobalController.Instance.allPaths.Count)];
                pathID = 0;
                wayID = 0;
                pointID = 0;
                needNewPath = false;
                transform.position = new Vector3( movePath.path[pathID].ways[wayID].points[pointID].position.x, transform.position.y, movePath.path[pathID].ways[wayID].points[pointID].position.z);
                transform.LookAt(movePath.path[pathID].ways[wayID].points[pointID + 1].position);
                rigbody.velocity = Vector3.zero;
                (nextPoint, pathID, wayID, pointID, needNewPath) = movePath.GetNextPoint(pathID, wayID, pointID);
            }
            navigator.CalculatePath(nextPoint, navMeshPath);
        }

        Vector3 dir = (nextPoint - transform.position).normalized;
        float dot = Vector3.Dot(dir, transform.forward);
        float dot1 = Vector3.Dot(dir, transform.right);

        float footbreak = 0;

        if(CanGoToCrossroad)
        {
            carMovement.brakeInput = 0;
            footbreak = 0;
        } else
        {
            carMovement.brakeInput = -1;
            footbreak = -1;
        }

        if(IsOnCrossroad && GetCarTurnPath() != 2)
        {
            carMovement.brakeInput = 0;
            footbreak = 0;
        }

        carMovement.Move(dot, footbreak, dot1);


        //line for path
        for (int i = 0; i < navMeshPath.corners.Length - 1; i++)
            Debug.DrawLine(navMeshPath.corners[i], navMeshPath.corners[i + 1], Color.red);


    }

    //Check obstacles
    private void PushRay()
    {

        RaycastHit hit;

        Vector3 bamperPos = transform.position+(transform.forward * (transform.lossyScale.z / 1.9f));
        Vector3 weight = (transform.right * (transform.lossyScale.x / 2));

        Ray fwdRay = new Ray(bamperPos, transform.forward * 5);
        Ray LRay = new Ray(bamperPos - weight, transform.forward * 5);
        Ray RRay = new Ray(bamperPos + weight, transform.forward * 5);

        //Debug.DrawRay(bamperPos, transform.forward * 5);
        //Debug.DrawRay(bamperPos - weight, transform.forward * 5);
        //Debug.DrawRay(bamperPos + weight, transform.forward * 5);


        if (Physics.Raycast(fwdRay, out hit, 5) || Physics.Raycast(LRay, out hit, 5) || Physics.Raycast(RRay, out hit, 5))
        {
            float distance = Vector3.Distance(bamperPos, hit.point);

            if (hit.transform.CompareTag("Car"))
            {
                GameObject car = hit.transform.gameObject;

                if (car != null)
                {
                    carMovement.obstaclesBrakeInput = Mathf.Lerp(0,-1, (-(distance / 10)));
                } else
                {
                    carMovement.obstaclesBrakeInput = 0;
                }
            }
            else
            {
                carMovement.obstaclesBrakeInput = 0;

                    moveSpeed = startSpeed;

            }
        }
        else
        {
            carMovement.obstaclesBrakeInput = 0;

                moveSpeed = startSpeed;

        }
    }

    //Return current path way
    public int GetCarTurnPath()
    {
        if(movePath.path[pathID].ways.Count>1)
        {
            return wayID;
        }
        else
        {
            return 1;
        }
    }
}
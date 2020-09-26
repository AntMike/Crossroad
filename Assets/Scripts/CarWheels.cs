using UnityEngine;

public class CarWheels : MonoBehaviour
{
    public WheelCollider[] WheelColliders;
    public Transform[] tireMeshes;
    public Vector3 centerOfMassOffset;

    void Update()
    {
        UpdateMeshesPositions();
    }

    private void UpdateMeshesPositions()
    {
        //for (int i = 0; i < WheelColliders.Length; i++)
        //{
        //    Quaternion quat;
        //    Vector3 pos;
        //    WheelColliders[i].GetWorldPose(out pos, out quat);

        //    tireMeshes[i].position = pos;
        //    tireMeshes[i].rotation = quat;
        //}
    }
}

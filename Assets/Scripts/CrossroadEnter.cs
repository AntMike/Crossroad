using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossroadEnter : MonoBehaviour
{

    public CrossRoad crossRoad;
    [SerializeField] private int movedFrom;

    //Check if car is comming to the crossroad
    private void OnCollisionEnter(Collision collision)
    {
        var col = collision.collider.GetType();
        if (collision.collider.GetType() != typeof(WheelCollider) && collision.gameObject.tag == "Car")
        {
            Vector3 hit = collision.transform.position;

            float towartHit = Vector3.Dot(transform.forward, hit);
            float sideHit = Vector3.Dot(transform.right, hit);

            crossRoad.AddCarToList(movedFrom, collision.gameObject.GetComponent<CarAIController>());

        }

        //  z   (-0.5, 0.3,  3.4)        3,364743       -0,4795978       forward
        // -z   ( 0.5, 0.3, -3.4)       -3,374835        0,4827945        backward
        // -x   (-3.4, 0.3, -0.5)      -0,4733841      -3,357418       left
        //  x   ( 3.4, 0.3,  0.5)         0,4828034       3,385496        right
    }
}

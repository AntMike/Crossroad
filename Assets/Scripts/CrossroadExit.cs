using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossroadExit : MonoBehaviour
{
    public CrossRoad crossRoad;


    //Check if car exited from the crossroad
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetType() != typeof(WheelCollider) && collision.gameObject.tag == "Car")
        {
            crossRoad.RemoveCarFromLists(collision.gameObject.GetComponent<CarAIController>());
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossroadStay : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        var col = collision.collider.GetType();
        if (collision.collider.GetType() != typeof(WheelCollider) && collision.gameObject.tag == "Car")
        {
            collision.gameObject.GetComponent<CarAIController>().IsOnCrossroad = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.GetType() != typeof(WheelCollider) && collision.gameObject.tag == "Car")
        {

            collision.gameObject.GetComponent<CarAIController>().IsOnCrossroad = false;
        }
    }
}

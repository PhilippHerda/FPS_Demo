using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampolineBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            GameObject player = other.gameObject;

            //player.GetComponent<PlayerMovement>().velocity += Vector3.up;
            player.GetComponent<Rigidbody>().AddForce(Vector3.up * 100f, ForceMode.Force); ;
            //rb.AddForce(Vector3.up * 100f, ForceMode.Force);
        }
    }
}

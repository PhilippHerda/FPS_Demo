using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);

        //if (collision.transform.CompareTag("Player"))
        //{
        //    GameObject player = other.gameObject;

        //    player.GetComponent<Rigidbody>().AddForce(Vector3.up * 1000f, ForceMode.Acceleration);
        //}

        if (collision.transform.CompareTag("Enemy"))
        {
            GameObject enemy = collision.gameObject;

            enemy.GetComponent<BotAI>().TakeDamage(1);
        }
    }
}

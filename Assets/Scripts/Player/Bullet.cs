using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector3.right * (10f * Time.deltaTime));
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
      
        if (collision.transform.CompareTag("Enemy"))
        {
            if (!collision.TryGetComponent(out Enemy enemy))
                return;

            enemy.TakeDamage();
            Destroy(gameObject);
        }

    }

    private void OnEnable()
    {
        Destroy(gameObject, 4f);
    }
}

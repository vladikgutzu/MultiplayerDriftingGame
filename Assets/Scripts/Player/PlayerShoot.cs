using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bullet;
    private AudioSource source;
    public AudioClip shootSound;
    private void Start()
    {
        source = GetComponent<AudioSource>();
    }
    void Update()
    {
        Camera mainCamera = Camera.main;
        if (!mainCamera)
            return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 objectPos = mainCamera.WorldToScreenPoint(transform.position);
        mousePos.x = mousePos.x - objectPos.x;
        mousePos.y = mousePos.y - objectPos.y;

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        if (Input.GetMouseButtonDown(0)) 
        {
            Shoot();
        }
    }

    public void Shoot() 
    {
        if (bullet)
            Instantiate(bullet, transform.position, transform.rotation);

        if (source && shootSound)
            source.PlayOneShot(shootSound);
    }
}

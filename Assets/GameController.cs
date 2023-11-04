using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Transform dinosaur;
    public float jumpVelocity = 10f;
    public float gravity = -1f;

    private float groundY;
    private bool isJumping = false;
    private float dinosaurYVelocity = 0f;

    void Awake()
    {
        groundY = dinosaur.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (isJumping == false)
        {
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow))
            {
                isJumping = true;
                dinosaurYVelocity = jumpVelocity;
            }
        }
        else
        {
            dinosaurYVelocity += gravity * Time.deltaTime;
            dinosaur.position += dinosaurYVelocity * Vector3.up * Time.deltaTime;

            if (dinosaur.position.y < groundY)
            {
                dinosaur.position = new Vector3(dinosaur.position.x, groundY, dinosaur.position.z);
                isJumping = false;
            }
        }
    }
}

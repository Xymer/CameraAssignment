﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement"), SerializeField]
    float moveSpeed = 1.0f;
    [SerializeField]
    float turnRate = 1.0f;
    float horizontalInput;
    float verticalInput;
    float playerYRotation;

    Vector3 movementVector = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GetInput())
        {
            transform.Translate(movementVector.normalized * Time.deltaTime * moveSpeed);
        }
        GetMouseMovement();

    }
    bool GetInput()
    {
        if (Input.anyKey)
        {
            if (Input.GetAxis("Horizontal") != 0)
            {
                horizontalInput = Input.GetAxis("Horizontal");
                movementVector.x = horizontalInput;
            }
            if (Input.GetAxis("Vertical") != 0)
            {
                verticalInput = Input.GetAxis("Vertical");
                movementVector.z = verticalInput;
            }
            return true;
        }
        movementVector = Vector3.zero;
        return false;
    }
    bool GetMouseMovement()
    {
        
        if (Input.GetAxis("Mouse X") != 0 && !Input.GetMouseButton(1))
        {
            playerYRotation += Input.GetAxis("Mouse X") * turnRate;
            transform.rotation = Quaternion.Euler(0, playerYRotation , 0);
            return true;
        }
     
        return false;
    }
}

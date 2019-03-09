using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraControls : MonoBehaviour
{
    Boolean but0Pressed = false;

    float mouseMovementThreshold = 200.0f;
    const float mouseMovementSpeed = 0.2f;

    const float selectingDistance = 0.1f;

    GameObject selectedObject = null;
    Vector3 selectedObjectTarget = new Vector3(0, 0, 0);
    float moveSpeed = 0.5f;

    Camera cam = null;
    Renderer rend = null;

    // Start is called before the first frame update
    void Start()
    {
        mouseMovementThreshold = Math.Min(Screen.height * 0.2f, Screen.width * 0.2f);

        cam = Camera.main;
        Renderer rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !but0Pressed)
        {
            but0Pressed = true;

            selectedObject = null;

            Vector3 mp = getAbsMousePos();

            Vector3 relPos = new Vector3(0, 0, 0);

            foreach (GameObject GJ in GameObject.FindGameObjectsWithTag("Virus"))
            {

                relPos = mp - GJ.transform.position;
                if (Math.Abs(relPos.x) < selectingDistance && Math.Abs(relPos.z) < selectingDistance)
                {
                    selectedObject = GJ;
                    selectedObjectTarget = selectedObject.transform.position;
                    rend.material.shader = Shader.Find("Specular");
                    rend.material.SetColor("_SpecColor", Color.red);

                }
            }

        }
        else
        {
            but0Pressed = false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            selectedObjectTarget = getAbsMousePos();
        }
    }

    void FixedUpdate()
    {
        ScreenScrolling();

        if (selectedObject != null && selectedObject.transform.position != selectedObjectTarget)
        {
            float distance = Vector3.Distance(selectedObject.transform.position, selectedObjectTarget);

            if (distance < moveSpeed) {
                selectedObject.transform.position = selectedObjectTarget;
            }

            Vector3 newPos = selectedObject.transform.position;

            newPos.x += moveSpeed * (-selectedObject.transform.position.x + selectedObjectTarget.x) / distance;
            newPos.z += moveSpeed * (-selectedObject.transform.position.z + selectedObjectTarget.z) / distance;

            selectedObject.transform.position = newPos;
        }

    }

    void ScreenScrolling() {
        Vector3 mouseMovement = new Vector3(0, 0, 0);

        if (Input.mousePosition.x > 0 && Input.mousePosition.x <= mouseMovementThreshold)
        {
            mouseMovement += new Vector3(-mouseMovementSpeed * (mouseMovementThreshold - Input.mousePosition.x) / mouseMovementThreshold, 0, 0);
        }

        if (Input.mousePosition.x < Screen.width && Input.mousePosition.x >= Screen.width - mouseMovementThreshold)
        {
            mouseMovement += new Vector3(mouseMovementSpeed * (Input.mousePosition.x - (Screen.width - mouseMovementThreshold)) / mouseMovementThreshold, 0, 0);
        }

        if (Input.mousePosition.y > 0 && Input.mousePosition.y <= mouseMovementThreshold)
        {
            mouseMovement += new Vector3(0, 0, -mouseMovementSpeed * (mouseMovementThreshold - Input.mousePosition.y) / mouseMovementThreshold);
        }

        if (Input.mousePosition.y < Screen.width && Input.mousePosition.y >= Screen.height - mouseMovementThreshold)
        {
            mouseMovement += new Vector3(0, 0, mouseMovementSpeed * (Input.mousePosition.y - (Screen.height - mouseMovementThreshold)) / mouseMovementThreshold);
        }

        transform.position = transform.position + mouseMovement;
    }

    Vector3 getAbsMousePos() {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit);

        return hit.point;
    }
}

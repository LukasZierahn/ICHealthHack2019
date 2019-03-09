using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct Movement
{
    public Vector3 targetPos;
    public GameObject GJ;
    public float speed;

    public Movement(Vector3 tP, GameObject gameObject, float spd)
    {
        targetPos = tP;
        GJ = gameObject;
        speed = spd;
    }

};

public class CameraControls : MonoBehaviour
{
    Boolean but0Pressed = false;

    float mouseMovementThreshold = 200.0f;
    const float mouseMovementSpeed = 0.2f;

    const float selectingDistance = 0.1f;

    List<Movement> movements = new List<Movement>();

    GameObject selectedObject = null;
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

            if (selectedObject != null) {
                selectedObject.GetComponent<Renderer>().material.color = Color.white;
            }
            selectedObject = null;

            Vector3 mp = getAbsMousePos();

            Vector3 relPos = new Vector3(0, 0, 0);

            foreach (GameObject GJ in GameObject.FindGameObjectsWithTag("Virus"))
            {

                relPos = mp - GJ.transform.position;
                if (Math.Abs(relPos.x) < selectingDistance && Math.Abs(relPos.z) < selectingDistance)
                {
                    selectedObject = GJ;
                    GJ.GetComponent<Renderer>().material.color = Color.red;
                }
            }

        }
        else
        {
            but0Pressed = false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            bool found = false;
            for (int i = 0; i < movements.Count; i++)
            {
                if (movements[i].GJ == selectedObject)
                {
                    movements[i] = new Movement(getAbsMousePos(), movements[i].GJ, movements[i].speed);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                movements.Add(new Movement(getAbsMousePos(), selectedObject, moveSpeed));
            }
        }
    }

    void FixedUpdate()
    {
        ScreenScrolling();

        for (int i = movements.Count - 1; i >= 0; i--) {            
            float distance = Vector3.Distance(movements[i].GJ.transform.position, movements[i].targetPos);

            if (distance < moveSpeed)
            {
                movements[i].GJ.transform.position = movements[i].targetPos;
                movements.RemoveAt(i);
            }

            Vector3 newPos = movements[i].GJ.transform.position;

            newPos.x += moveSpeed * (-movements[i].GJ.transform.position.x + movements[i].targetPos.x) / distance;
            newPos.z += moveSpeed * (-movements[i].GJ.transform.position.z + movements[i].targetPos.z) / distance;

            movements[i].GJ.transform.position = newPos;
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

using Simulation;
using System.Drawing;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Camera targetCamera;
    public SimulationManager simulationManager;
    public float speed = 100;
    public float sensitivity = 10;
    public float maxYAngle = 90f;

    public bool followBestCar = true;
    public float followHoverHeight = 230;

    private Rigidbody body;
    private Vector2 currentRotation;


    private void Start()
    {
        body = targetCamera.GetComponent<Rigidbody>();
        currentRotation = new Vector2(0, 90);
    }

    void FixedUpdate()
    {
        bool follow = MoveByUserInput();

        if (followBestCar)
        {
            followBestCar = follow;
            FollowCar();
        }
    }

    private bool MoveByUserInput()
    {
        Vector3 direction = new Vector3();

        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector3.right;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            direction += Vector3.up;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            direction += Vector3.down;
        }
        body.AddForce(direction.normalized * speed, ForceMode.Force);

        if (Input.GetMouseButton(2))
        {
            currentRotation.x += Input.GetAxis("Mouse X") * sensitivity;
            currentRotation.y -= Input.GetAxis("Mouse Y") * sensitivity;
            currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
            currentRotation.y = Mathf.Clamp(currentRotation.y, -maxYAngle, maxYAngle);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            currentRotation = new Vector2(0, 90);
        }
        targetCamera.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);

        return direction.x == 0 && direction.z == 0;
    }

    private int FollowCar()
    {
        Engine simulationEngine = simulationManager.simulationEngine;
        int followingIndex = simulationManager.GetBestCar();

        if (followingIndex > -1 && simulationEngine.LoopSpeed >= 0.5)
        {
            PointF carCentre = simulationEngine.Cars[followingIndex].Centre;

            Vector3 centre = new Vector3(carCentre.X, followHoverHeight, simulationManager.terrainImage.height - carCentre.Y);

            Vector3 dist = centre - targetCamera.transform.position;
            dist.y = 0;
            body.AddForce(dist, ForceMode.VelocityChange);
        }

        return followingIndex;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
    private GameObject ball;
    private GameObject ballClick;
    private Rigidbody2D ballRB;
    private Vector3 ballPos;
    private Vector3 fingerPos;
    private Vector3 ballFingerDiff;
    private Vector2 shootForce;
    private float x1, y1;
    Vector2 startPoint;
    Vector2 endPoint;
    Vector2 controlPoint;

    private bool ballIsClicked = false;
    private bool ballIsClicked2 = false;

    public DashedCurve trajectory;
    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;

    public float shootingPowerX;
    public float shootingPowerY;
    
    public bool grabWhileMoving;

    void Start()
    {
        ball = gameObject;
        ballRB = GetComponent<Rigidbody2D>();
        
        trajectory.gameObject.SetActive(false);
        startPoint = Vector2.zero;
        controlPoint = Vector2.zero;
        endPoint = Vector2.zero;
        trajectory.setQuadraticPoints(startPoint, controlPoint, endPoint);

        ballClick = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null && ballIsClicked2 == false)
        {
            if (hit.collider.gameObject == ballClick)
                ballIsClicked = true;
            else
                ballIsClicked = false;
        }
        else
            ballIsClicked = false;

        if (ballIsClicked2 == true)
            ballIsClicked = true;

        if ((((ballRB.velocity.x * ballRB.velocity.x) + (ballRB.velocity.y * ballRB.velocity.y))) <= 0.0085f)
        {
            ballRB.velocity = new Vector2(0, 0);
            ballRB.angularVelocity = 0;
        }
        else
        {
            trajectory.gameObject.SetActive(false);
            startPoint = Vector2.zero;
            controlPoint = Vector2.zero;
            endPoint = Vector2.zero;
            trajectory.setQuadraticPoints(startPoint, controlPoint, endPoint);
        }

        ballPos = ball.transform.position;

        if ((Input.GetMouseButton(0) && ballIsClicked == true) && (ballRB.velocity == Vector2.zero || grabWhileMoving))
        {
            ballIsClicked2 = true;

            fingerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (grabWhileMoving == true)
            {
                ballRB.velocity = Vector2.zero;
                ballRB.angularVelocity = 0;
                ballRB.isKinematic = true;
            }

            ballFingerDiff = ballPos - fingerPos;

            shootForce = new Vector2(ballFingerDiff.x * shootingPowerX, ballFingerDiff.y * shootingPowerY);

            startPoint = ball.transform.position;

            float x = startPoint.x;
            float y = startPoint.y;
            float t = 0;
            for (; x > xMin && x < xMax && y > yMin && y < yMax;)
            {
                x = startPoint.x + shootForce.x * t;
                y = startPoint.y + proj(t);
                t += Time.fixedDeltaTime;
            }

            endPoint = new Vector2(x, y);

            Vector2 midPoint = (startPoint + endPoint) / 2;

            controlPoint = new Vector2(
                midPoint.x,
                (endPoint.x - startPoint.x) / 2 * projDer(startPoint.x) + startPoint.y
            );

            if (ballFingerDiff.magnitude > 0.4f)
            { 
                trajectory.setQuadraticPoints(startPoint, controlPoint, endPoint);
                trajectory.gameObject.SetActive(true);
            }
            else
            {
                trajectory.gameObject.SetActive(false);
                startPoint = Vector2.zero;
                controlPoint = Vector2.zero;
                endPoint = Vector2.zero;
                trajectory.setQuadraticPoints(startPoint, controlPoint, endPoint);
                if (ballRB.isKinematic == true)
                    ballRB.isKinematic = false;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ballIsClicked2 = false;

            if (trajectory.gameObject.activeInHierarchy)
            {
                trajectory.gameObject.SetActive(false);
                startPoint = Vector2.zero;
                controlPoint = Vector2.zero;
                endPoint = Vector2.zero;
                trajectory.setQuadraticPoints(startPoint, controlPoint, endPoint);
                ballRB.velocity = shootForce;

                if (ballRB.isKinematic == true)
                    ballRB.isKinematic = false;
            }
        }


    }

    float proj(float x)
    {
        return Physics2D.gravity.y / 2f * x * x + shootForce.y * x;
    }

    float projDer(float x)
    {
        return (Physics2D.gravity.y / shootForce.x * (x - startPoint.x) + shootForce.y) / shootForce.x;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startPoint, .1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(controlPoint, .1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPoint, .1f);
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject hero;
    private Rigidbody2D rb;

    //----------------------//

    private new Camera camera;
    private float horizontalSize;
    private float verticalSize;
    [SerializeField] float horizontalOffset = 1f;
    [SerializeField] float smoothTime = 0.01f;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = hero.GetComponent<Rigidbody2D>();
        camera = GetComponent<Camera>();
        horizontalSize = camera.orthographicSize * camera.aspect;
        verticalSize = camera.orthographicSize;
    }

    private void LateUpdate()
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        Vector3 camPosition = transform.position;

        HorizontalCamMovement(rb.velocity.x, camPosition);
        VerticalCamMovement(camPosition);
    }

    private void HorizontalCamMovement(float heroVelocity, Vector3 camPosition)
    {
        float velocityThreshold = 0.5f;
        if (heroVelocity > velocityThreshold)
        {
            camPosition.x = hero.transform.position.x + horizontalSize * horizontalOffset;
        }
        else if (heroVelocity < -velocityThreshold)
        {
            camPosition.x = hero.transform.position.x - horizontalSize * horizontalOffset;
        }
        else
        {
            camPosition.x = hero.transform.position.x;
        }

        transform.position = Vector3.Slerp(transform.position, camPosition, smoothTime * Time.deltaTime);
    }

    private void VerticalCamMovement(Vector3 camPosition)
    {        
        Vector3 screenPos = camera.WorldToScreenPoint(hero.transform.position);   
        
        float topThreshold = 0.9f * Screen.height; 
        float bottomThreshold = 0.1f * Screen.height;

        
        if (screenPos.y >= topThreshold) 
        {
            camPosition.y = hero.transform.position.y + (topThreshold - screenPos.y) * 2f * camera.orthographicSize / Screen.height;
        }
        
        else if (screenPos.y <= bottomThreshold)
        {   
            camPosition.y = hero.transform.position.y - (bottomThreshold - screenPos.y) * 2f * camera.orthographicSize / Screen.height;
        }
        else
        {   
            camPosition.y = hero.transform.position.y + 2f * camera.orthographicSize / Screen.height; 
        }

        transform.position = Vector3.Slerp(transform.position, camPosition, 3 * smoothTime * Time.deltaTime);
    }
}
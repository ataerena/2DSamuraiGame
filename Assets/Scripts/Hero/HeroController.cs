using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    private Hero hero;
    private float moveX;
    private bool jumpPressed;
    void Start()
    {
        hero = GetComponent<Hero>();
    }

    void Update()
    {
        if (KeyboardControls.IsKeyPressed(KeyboardControls.Button.Left))
        {
            moveX = -1f;
        }
        else if (KeyboardControls.IsKeyPressed(KeyboardControls.Button.Right))
        {
            moveX = 1f;
        }
        else
        {
            moveX = 0f;
        }

        if (KeyboardControls.IsKeyHit(KeyboardControls.Button.Jump))
        {
            jumpPressed = true;
        }
    }

    void FixedUpdate()
    {
        UpdateMovement();
    }

    void UpdateMovement()
    {
        hero.Move(moveX);

        if (jumpPressed)
        {
            hero.Jump();
            jumpPressed = false;
        }
    }
}

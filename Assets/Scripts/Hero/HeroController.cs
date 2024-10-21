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
        BufferMovement();
        BufferCombat();
    }

    void FixedUpdate()
    {
        UpdateMovement();
        UpdateAttack();
    }

    void UpdateAttack()
    {
        
    }
    void UpdateMovement()
    {
        if (hero.isAttacking == true) return;

        hero.Move(moveX);

        if (jumpPressed)
        {
            hero.Jump();
            jumpPressed = false;
        }
    }

    // clock buffers //

    void BufferCombat()
    {   
        if (hero.airState == Hero.AirState.Grounded && !hero.IsMoving() && KeyboardControls.IsKeyHit(KeyboardControls.Button.LightAttack))
        {
            hero.isAttacking = true;
            if (hero.comboActive)
            {
                hero.comboActivated = true;
            }
        }
    }

    void BufferMovement()
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
}

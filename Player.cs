using CodeMonkey;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rigidBody2D;
    private static Player instance;

    public static Player GetInstance() { return instance; }

    public float strength = 80f;

    private State state;

    private enum State
    {
        GetReady,
        Playing,
        GameOver,
    }

    public event EventHandler OnDie;
    public event EventHandler OnWait;
    public event EventHandler OnStart;

    private void Awake()
    {
        instance = this;
        rigidBody2D = GetComponent<Rigidbody2D>();
        rigidBody2D.bodyType = RigidbodyType2D.Static;
        state = State.GetReady;
    }

    private void Update()
    {
        switch (state)
        {
            case State.GetReady:
                state = State.GetReady;
                if (OnWait != null) OnWait(this, EventArgs.Empty);
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    state = State.Playing;
                    rigidBody2D.bodyType = RigidbodyType2D.Dynamic;
                    Fly();
                    if (OnStart != null) OnStart(this, EventArgs.Empty);
                }
                break;
            case State.Playing:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    Fly();
                }
                break;
            case State.GameOver:
                state = State.GameOver;
                break;
        }
    }

    private void Fly()
    {
        rigidBody2D.velocity = Vector2.up * strength;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        rigidBody2D.bodyType = RigidbodyType2D.Static;
        if (OnDie != null)
        {
            OnDie(this, EventArgs.Empty);
        }
    }
}

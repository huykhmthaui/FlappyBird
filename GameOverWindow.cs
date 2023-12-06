using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverWindow : MonoBehaviour
{
    private Text scoreText;

    private void Awake()
    {
        scoreText = transform.Find("Score").GetComponent<Text>();
        Hide();
    }

    private void Start()
    {
        Player.GetInstance().OnDie += Player_OnDie;
    }

    public void Player_OnDie(object sender, System.EventArgs e)
    {
        Debug.Log("You died");
        scoreText.text = Level.GetInstance().GetPipesPassed().ToString();
        Show();
    }

    private void Hide()
    {
        Debug.Log("Hide!");
        gameObject.SetActive(false);
    }

    private void Show()
    {
        Debug.Log("Show!");
        gameObject.SetActive(true);
    }
}

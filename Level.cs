using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;
using CodeMonkey.Utils;

public class Level : MonoBehaviour
{
    private const float CAMERA_SIZE = 50f;
    private const float PIPEBODY_WIDTH = 7.8f;
    private const float PIPEHEAD_HEIGHT = 3.75f;
    private const float PIPE_MOVEMENT_SPEED = 30f;
    private const float PIPE_DESTROY_X_POSITION = -100f;
    private const float PIPE_SPAWN_X_POSITION = 100f;
    private const float BIRD_X_POSITION = 0f;

    private List<Pipe> pipeList;

    private static Level instance;

    public static Level GetInstance()
    {
        return instance;
    }

    private int pipesSpawned;
    private int pipePassedCount;
    private float pipeSpawnTimer;
    private float pipeSpawnTimerMax;
    private float gapSize;
    private State state;

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
        Impossible,
        Asian,
    }

    private enum State
    {
        GetReady,
        Playing,
        GameOver,
    }

    private void Awake()
    {
        instance = this;
        pipeList = new List<Pipe>();
        pipesSpawned = 0;
        pipeSpawnTimerMax = 1f;
        SetDifficulty(Difficulty.Easy);
        state = State.Playing;
    }

    private void Start()
    {
        Player.GetInstance().OnDie += Player_OnDie;
        Player.GetInstance().OnWait += Player_OnWait;
        Player.GetInstance().OnStart += Player_OnStart;
    }

    private void Player_OnDie(object sender, System.EventArgs e)
    {
        state = State.GameOver;
    }
    
    private void Player_OnWait(object sender, System.EventArgs e)
    {
        state = State.GetReady;
    }
    private void Player_OnStart(object sender, System.EventArgs e)
    {
        state = State.Playing;
    }

    private void Update()
    {
        if(state == State.Playing)
        {
            HandlePipeMovement();
            HandlePipeSpawning();
        }
    }

    private void HandlePipeSpawning()
    {
        pipeSpawnTimer -= Time.deltaTime;
        if (pipeSpawnTimer < 0)
        {
            pipeSpawnTimer += pipeSpawnTimerMax;

            float heightEdgeLimit = 10f;
            float minHeight = gapSize * .5f + heightEdgeLimit;
            float totalHeight = CAMERA_SIZE * 2f;
            float maxHeight = totalHeight - gapSize * .5f - heightEdgeLimit;

            float height = Random.Range(minHeight, maxHeight);

            CreateGap(height, gapSize, PIPE_SPAWN_X_POSITION);
        }
    }

    private void HandlePipeMovement()
    {
        for (int i = 0; i < pipeList.Count; i++)
        {
            Pipe pipe = pipeList[i];
            bool isToTheRightOfTheBird = pipe.GetXPosition() > BIRD_X_POSITION;
            pipe.Move();
            if(isToTheRightOfTheBird && pipe.GetXPosition() <= BIRD_X_POSITION && pipe.IsBottom())
            {
                pipePassedCount++;
            }    
            if (pipe.GetXPosition() < PIPE_DESTROY_X_POSITION)
            {
                pipe.DestroySelf();
                pipeList.Remove(pipe);
                i--;
            }
        }
    }

    private void SetDifficulty(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                gapSize = 50f;
                pipeSpawnTimerMax = 1.2f;
                break;
            case Difficulty.Medium:
                gapSize = 45f;
                pipeSpawnTimerMax = 1.1f;
                break;
            case Difficulty.Hard:
                gapSize = 35f;
                pipeSpawnTimerMax = 1f;
                break;
            case Difficulty.Impossible:
                gapSize = 25f;
                pipeSpawnTimerMax = 0.9f;
                break;
            case Difficulty.Asian:
                gapSize = 15f;
                pipeSpawnTimerMax = 0.8f;
                break;
        }
    }

    private Difficulty GetDifficulty()
    {
        if (pipesSpawned >= 100) return Difficulty.Asian;
        if (pipesSpawned >= 80) return Difficulty.Impossible;
        if (pipesSpawned >= 50) return Difficulty.Hard;
        if (pipesSpawned >= 25) return Difficulty.Medium;
        return Difficulty.Easy;
    }

    private void CreateGap(float gapHeight, float gapSize, float xPosition)
    {
        CreatePipe(gapHeight - gapSize * .5f, xPosition, true);
        CreatePipe(CAMERA_SIZE * 2f - gapHeight - gapSize * .5f, xPosition, false);
        pipesSpawned++;
        SetDifficulty(GetDifficulty());
    }

    private void CreatePipe(float height, float xPosition, bool createBottom)
    {
        // Pipe head
        Transform pipeHead = Instantiate(GameAssets.GetInstance().pfPipeHead);
        float pipeHeadYPosition;
        if (createBottom)
        {
            pipeHeadYPosition = -CAMERA_SIZE + height - PIPEHEAD_HEIGHT * .5f;
        }
        else
        {
            pipeHeadYPosition = +CAMERA_SIZE - height + PIPEHEAD_HEIGHT * .5f;

        }
        pipeHead.position = new Vector3(xPosition, pipeHeadYPosition);

        //Pipe body
        Transform pipeBody = Instantiate(GameAssets.GetInstance().pfPipeBody);
        float pipeBodyYPosition;
        if (createBottom)
        {
            pipeBodyYPosition = -CAMERA_SIZE;
        }
        else
        {
            pipeBodyYPosition = +CAMERA_SIZE;
            pipeBody.localScale = new Vector3(1, -1, 1);
        }
        pipeBody.position = new Vector3(xPosition, pipeBodyYPosition);


        SpriteRenderer pipeBodySpriteRenderer = pipeBody.GetComponent<SpriteRenderer>();
        pipeBodySpriteRenderer.size = new Vector2(PIPEBODY_WIDTH, height);

        BoxCollider2D pipeBodyBoxCollider = pipeBody.GetComponent<BoxCollider2D>();
        pipeBodyBoxCollider.size = new Vector2(PIPEBODY_WIDTH, height);
        pipeBodyBoxCollider.offset = new Vector2(0f, height * .5f);

        Pipe pipe = new Pipe(pipeHead, pipeBody, createBottom);
        pipeList.Add(pipe);
    }

    public int GetPipesPassed()
    {
        return pipePassedCount;
    }

    private class Pipe
    {
        private Transform pipeHeadTransform;
        private Transform pipeBodyTransform;
        private bool isBottom;

        public Pipe(Transform pipeHeadTransform, Transform pipeBodyTransform, bool isBottom)
        {
            this.pipeHeadTransform = pipeHeadTransform;
            this.pipeBodyTransform = pipeBodyTransform;
            this.isBottom = isBottom;
        }

        public void Move()
        {
            pipeHeadTransform.position += PIPE_MOVEMENT_SPEED * Time.deltaTime * new Vector3(-1, 0, 0);
            pipeBodyTransform.position += PIPE_MOVEMENT_SPEED * Time.deltaTime * new Vector3(-1, 0, 0);
        }

        public float GetXPosition()
        {
            return pipeHeadTransform.position.x;
        }

        public bool IsBottom()
        {
            return isBottom;
        }

        public void DestroySelf()
        {
            Destroy(pipeHeadTransform.gameObject);
            Destroy(pipeBodyTransform.gameObject);
        }
    }
}

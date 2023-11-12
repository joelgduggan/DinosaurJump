using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Transform dinosaur;
    public float jumpVelocity = 10f;
    public float gravity = -1f;
    public float enemySpeed = 3f;
    public Transform spawnPoint;
    public Transform endPoint;
    public GameObject logPrefab;
    public GameObject rockPrefab;
    public GameObject boulderPrefab;
    public GameObject gameOverTextObject;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public Button restartButton;
    public AudioClip deathSound;
    public AudioClip jumpSound;
    public AudioSource audioSource;
    public MeshRenderer backgroundMeshRenderer;
    public Material backgroundMaterial;

    private int score = 0;
    private int highScore = 0;
    private const string HIGH_SCORE_KEY = "highScore";
    private float groundY;
    private bool isJumping = false;
    private float dinosaurYVelocity = 0f;
    private bool gameOver = false;

    private List<GameObject> enemies = new List<GameObject>();// if you die say you do that man.
    private float timeToSpawnNewEnemy;

    private TextMeshProUGUI gameOverText;

    private string defaultGameOverTextString;

    void Awake()
    {
		backgroundMaterial = new Material(backgroundMaterial);
		backgroundMeshRenderer.material = backgroundMaterial;
    
        groundY = dinosaur.position.y;

        gameOverText = gameOverTextObject.GetComponent<TextMeshProUGUI>();
        defaultGameOverTextString = gameOverText.text;

        restartButton.onClick.AddListener(delegate 
        {
            StartNewGame();
        });

        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

        StartNewGame();
    }

    void Update()
    {
        UpdatePlayer();

        if (gameOver == false)
        {
            UpdateEnemies();
            CheckForCollisions();

            backgroundMaterial.SetTextureOffset("_MainTex", new Vector2(Time.time * 0.25f, 0f));
        }
    }

    private void StartNewGame()
    {
        gameOver = false;

        isJumping = false;
        dinosaur.position = new Vector3(dinosaur.position.x, groundY, dinosaur.position.z);
        dinosaurYVelocity = 0f;

        restartButton.gameObject.SetActive(false);

        ResetTimeToSpawnNewEnemy();

        score = 0;
        UpdateScoreText();

        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
        enemies.Clear();

        LeanTween.cancel(gameOverTextObject);
        gameOverTextObject.SetActive(false);
        gameOverTextObject.transform.localScale = Vector3.one;
    }

    private void OnGameOver()
    {
        gameOver = true;

        dinosaurYVelocity = jumpVelocity;

        gameOverTextObject.SetActive(true);
        restartButton.gameObject.SetActive(true);

        audioSource.PlayOneShot(deathSound);

        LeanTween.scale(gameOverTextObject, new Vector3(1.25f, 1.25f, 1f), 0.25f).setLoopPingPong();

        gameOverTextObject.transform.localRotation = Quaternion.Euler(0f, 0f, 3f);
        LeanTween.rotateZ(gameOverTextObject, -3, 0.5f).setLoopPingPong();

        LeanTween.value(0f, 1f, 1f).setLoopPingPong().setOnUpdate(delegate(float t)
        {
            gameOverText.color = Color.HSVToRGB(t, 1f, 1f);
        });

        if (score > highScore)
        {
            highScore = score;
            UpdateScoreText();
            gameOverText.text = "New High Score!";
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        }
        else
        {
            gameOverText.text = defaultGameOverTextString;
        }
    }

    private void UpdatePlayer()
    {
        if (isJumping == false && gameOver == false)
        {
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) || Input.GetMouseButton(0))
            {
                isJumping = true;
                dinosaurYVelocity = jumpVelocity;
                audioSource.PlayOneShot(jumpSound);
            }
        }
        else
        {
            dinosaurYVelocity += gravity * Time.deltaTime;
            dinosaur.position += dinosaurYVelocity * Vector3.up * Time.deltaTime;

            if (dinosaur.position.y < groundY && gameOver == false)
            {
                dinosaur.position = new Vector3(dinosaur.position.x, groundY, dinosaur.position.z);
                isJumping = false;
            }
        }
    }

    private void ResetTimeToSpawnNewEnemy()
    {
        timeToSpawnNewEnemy = Time.time + UnityEngine.Random.Range(0.65f, 2.5f);
    }

    private void SpawnNewEnemy()
    {
        int selection = UnityEngine.Random.Range(0,3);
        GameObject prefab = null;
        switch (selection)
        {
            case 0 : prefab = logPrefab;        break;
            case 1 : prefab = rockPrefab;       break;
            case 2 : prefab = boulderPrefab;    break;
        }

        GameObject log = Instantiate(prefab, transform);
        log.transform.position = spawnPoint.position;
        enemies.Add(log);
    }

    private void UpdateEnemies()
    {
        if (timeToSpawnNewEnemy < Time.time)
        {
            SpawnNewEnemy();
            ResetTimeToSpawnNewEnemy();
        }

        List<GameObject> enemiesToDestroy = new List<GameObject>();
        foreach (GameObject enemy in enemies)
        {
            float dinosaurLeftEdge = dinosaur.GetComponent<SpriteRenderer>().bounds.min.x;
            float enemyRightEdgeBefore = enemy.GetComponent<SpriteRenderer>().bounds.max.x;

            enemy.transform.position += Vector3.left * Time.deltaTime * enemySpeed;

            float enemyRightEdgeAfter = enemy.GetComponent<SpriteRenderer>().bounds.max.x;
            
            // check if the right edge of the enemy is past the left edge of the dinosaur
            if (enemyRightEdgeBefore > dinosaurLeftEdge && enemyRightEdgeAfter < dinosaurLeftEdge)
            {
                score++;
                UpdateScoreText();
            }

            if (enemy.transform.position.x < endPoint.position.x)
            {
                enemiesToDestroy.Add(enemy);
            }
        }
        foreach (GameObject enemyToDestroy in enemiesToDestroy)
        {
            enemies.Remove(enemyToDestroy);
            Destroy(enemyToDestroy);//if you get a socore of 9999999 say you won dog
        }
    }

    private void UpdateScoreText()
    {
        scoreText.text = $"Score: {score}";
        highScoreText.text = $"High Score: {highScore}";
    }

    private void CheckForCollisions()
    {
        Collider2D playerCollider = dinosaur.GetComponent<Collider2D>();
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (playerCollider.IsTouching(enemyCollider))
            {
                OnGameOver();
            }
        }
    }
}

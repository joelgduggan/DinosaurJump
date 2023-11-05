using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Transform dinosaur;
    public float jumpVelocity = 10f;
    public float gravity = -1f;
    public float enemySpeed = 3f;
    public Transform spawnPoint;
    public Transform endPoint;
    public GameObject logPrefab;
    public GameObject gameOverTextObject;

    private float groundY;
    private bool isJumping = false;
    private float dinosaurYVelocity = 0f;

    private List<GameObject> enemies = new List<GameObject>();// if you die say you do that man.
    private float timeToSpawnNewEnemy;

    void Awake()
    {
        groundY = dinosaur.position.y;

        gameOverTextObject.SetActive(false);

        ResetTimeToSpawnNewEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayer();
        UpdateEnemies();
        CheckForCollisions();
    }

    private void UpdatePlayer()
    {
        if (isJumping == false)
        {
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow))
            {
                isJumping = true;
                dinosaurYVelocity = jumpVelocity;
            }
        }
        else
        {
            dinosaurYVelocity += gravity * Time.deltaTime;
            dinosaur.position += dinosaurYVelocity * Vector3.up * Time.deltaTime;

            if (dinosaur.position.y < groundY)
            {
                dinosaur.position = new Vector3(dinosaur.position.x, groundY, dinosaur.position.z);
                isJumping = false;
            }
        }
    }

    private void ResetTimeToSpawnNewEnemy()
    {
        timeToSpawnNewEnemy = Time.time + Random.Range(1f, 3f);
    }

    private void SpawnNewEnemy()
    {
        GameObject log = Instantiate(logPrefab, transform);
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
            enemy.transform.position += Vector3.left * Time.deltaTime * enemySpeed;

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

    private void CheckForCollisions()
    {
        Collider2D playerCollider = dinosaur.GetComponent<Collider2D>();
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
            if (playerCollider.IsTouching(enemyCollider))
            {
                gameOverTextObject.SetActive(true);
            }
        }
    }
}

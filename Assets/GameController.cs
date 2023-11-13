using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


//-//////////////////////////////////////////////////////////////////////
///
public enum ItemType
{
	Enemy,
	Coin
}

//-//////////////////////////////////////////////////////////////////////
///
public class Item
{
	public GameObject gameObject;
	public ItemType itemType;

	public Item(GameObject gameObject, ItemType itemType)
	{
		this.gameObject = gameObject;
		this.itemType = itemType;
	}
}

public class GameController : MonoBehaviour
{
    public Transform dinosaur;
    public float jumpVelocity = 10f;
    public float gravity = -1f;
    public float enemySpeed = 3f;
    public float backgroundSpeed = 1f;
    public Transform spawnPoint;
    public Transform endPoint;
    public GameObject logPrefab;
    public GameObject rockPrefab;
    public GameObject boulderPrefab;
    public GameObject coinPrefab;
    public GameObject gameOverTextObject;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public Button restartButton;
    public TextMeshProUGUI restartButtonText;
    public AudioClip deathSound;
    public AudioClip jumpSound;
    public AudioClip scoreSound;
    public AudioClip bigScoreSound;
    public AudioSource audioSource;
    public MeshRenderer backgroundMeshRenderer;
    public Material backgroundMaterial;
    public ParticleSystem deathParticleSystem;

    private int score = 0;
    private int highScore = 0;
    private const string HIGH_SCORE_KEY = "highScore";
    private float groundY;
    private bool isJumping = false;
    private float dinosaurYVelocity = 0f;
    private bool gameStarted = false;
    private bool gameOver = false;

    private List<Item> items = new List<Item>();// if you die say you do that man.
    private float timeToSpawnNewItem;

    private TextMeshProUGUI gameOverText;

    private string defaultGameOverTextString;

    //-//////////////////////////////////////////////////////////////////////
    ///
    void Awake()
    {
		backgroundMaterial = new Material(backgroundMaterial);
		backgroundMeshRenderer.material = backgroundMaterial;
    
		groundY = dinosaur.position.y;

        gameOverText = gameOverTextObject.GetComponent<TextMeshProUGUI>();
        gameOverTextObject.SetActive(false);
        defaultGameOverTextString = gameOverText.text;

        restartButton.onClick.AddListener(delegate 
        {
            StartNewGame();
        });
		restartButtonText.text = "Start";

        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

		gameOver = true;
    }

    //-//////////////////////////////////////////////////////////////////////
    ///
    void Update()
    {
        UpdatePlayer();

        if (gameOver == false)
        {
            UpdateItems();
            CheckForCollisions();

            backgroundMaterial.SetTextureOffset("_MainTex", new Vector2(Time.time * backgroundSpeed, 0f));
        }
    }

    //-//////////////////////////////////////////////////////////////////////
    ///
    private void StartNewGame()
    {
        gameOver = false;
        gameStarted = true;
        isJumping = false;
        
        Vector3 pos = dinosaur.position;
        dinosaur.position = new Vector3(pos.x, groundY, pos.z);
        
        dinosaurYVelocity = 0f;

        restartButton.gameObject.SetActive(false);

        ResetTimeToSpawnNewItem();

        score = 0;
        UpdateScoreText();

        foreach (Item item in items)
        {
            Destroy(item.gameObject);
        }
        items.Clear();

        LeanTween.cancel(gameOverTextObject);
        gameOverTextObject.SetActive(false);
        gameOverTextObject.transform.localScale = Vector3.one;
        
        restartButtonText.text = "Restart";
    }

    //-//////////////////////////////////////////////////////////////////////
    ///
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
        
        deathParticleSystem.transform.position = dinosaur.position;
        deathParticleSystem.Play();

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

    //-//////////////////////////////////////////////////////////////////////
    ///
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
        else if (gameStarted)
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

    //-//////////////////////////////////////////////////////////////////////
    ///
    private void ResetTimeToSpawnNewItem()
    {
        timeToSpawnNewItem = Time.time + UnityEngine.Random.Range(0.65f, 2.5f);
    }

    //-//////////////////////////////////////////////////////////////////////
    ///
    private void SpawnNewEnemy()
    {
        int selection = UnityEngine.Random.Range(0,4);
        GameObject prefab = null;
        ItemType type = ItemType.Enemy;
        switch (selection)
        {
            case 0 : prefab = logPrefab;
					 type = ItemType.Enemy;
					 break;
            case 1 : prefab = rockPrefab;
					 type = ItemType.Enemy;
					 break;
            case 2 : prefab = boulderPrefab;
					 type = ItemType.Enemy;
					 break;
            case 3 : prefab = coinPrefab;
					 type = ItemType.Coin;
					 break;
        }

        GameObject log = Instantiate(prefab, transform);
        log.transform.position = spawnPoint.position;
        
        items.Add(new Item(log, type));
    }

    //-//////////////////////////////////////////////////////////////////////
    ///
    private void UpdateItems()
    {
        if (timeToSpawnNewItem < Time.time)
        {
            SpawnNewEnemy();
            ResetTimeToSpawnNewItem();
        }

        List<Item> itemsToDestroy = new List<Item>();
        foreach (Item item in items)
        {
	        GameObject itemObject = item.gameObject;
	        
            float dinosaurLeftEdge = dinosaur.GetComponent<SpriteRenderer>().bounds.min.x;
            float itemRightEdgeBefore = itemObject.GetComponent<SpriteRenderer>().bounds.max.x;

            itemObject.transform.position += Vector3.left * Time.deltaTime * enemySpeed;

            float itemRightEdgeAfter = itemObject.GetComponent<SpriteRenderer>().bounds.max.x;
            
            // check if the right edge of the enemy is past the left edge of the dinosaur
            if (itemRightEdgeBefore > dinosaurLeftEdge && itemRightEdgeAfter < dinosaurLeftEdge)
            {
				AddToScore(1);
            }

            if (itemObject.transform.position.x < endPoint.position.x)
            {
                itemsToDestroy.Add(item);
            }
        }
        foreach (Item item in itemsToDestroy)
        {
            items.Remove(item);
            Destroy(item.gameObject);
        }
    }
    
    //-//////////////////////////////////////////////////////////////////////
    ///
    private void AddToScore(int amount)
    {
        score += amount;
        UpdateScoreText();
        
        var clip = scoreSound;
        if (amount > 1)
        {
	        clip = bigScoreSound;
        }
        
        audioSource.PlayOneShot(clip);
	}

    //-//////////////////////////////////////////////////////////////////////
    ///
    private void UpdateScoreText()
    {
        scoreText.text = $"Score: {score}";
        highScoreText.text = $"High Score: {highScore}";
    }

    //-//////////////////////////////////////////////////////////////////////
    ///
    private void CheckForCollisions()
    {
        Collider2D playerCollider = dinosaur.GetComponent<Collider2D>();
        List<Item> itemsToDestroy = new List<Item>();
        
        foreach (Item item in items)
        {
            Collider2D itemCollider = item.gameObject.GetComponent<Collider2D>();
            if (playerCollider.IsTouching(itemCollider))
            {
	            if (item.itemType == ItemType.Enemy)
		        {
					OnGameOver();
				}
	            else if (item.itemType == ItemType.Coin)
	            {
		            AddToScore(5);
		            itemsToDestroy.Add(item);
	            }
            }
        }
        foreach (Item item in itemsToDestroy)
        {
	        items.Remove(item);
	        Destroy(item.gameObject);
        }
    }
}

using UnityEngine;

public class GameController : MonoBehaviour
{
    public Spec spec;
    public Transform levelContainer;

    public Color[] levelObstacleColors;
    public Color[] levelBackgroundColors;

    public GameObject specPrefab;
    public GameObject goalPrefab;
    public GameObject wallPrefab;
    public GameObject roofPrefab;
    public GameObject floorPrefab;
    public GameObject smallSquarePrefab;
    public GameObject mediumSquarePrefab;
    public GameObject bigSquarePrefab;

    int currentLevel = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LevelBeat()
    {
        currentLevel++;
        if (currentLevel >= 3)
        {
            currentLevel = 0;
        }
        GenerateLevel();
    }

    void GenerateLevel()
    {
        // clear all children of levelContainer
        foreach (Transform child in levelContainer)
        {
            Destroy(child.gameObject);
        }

        this.spec = Instantiate(specPrefab, levelContainer).GetComponent<Spec>();
        this.spec.gameController = this;
        this.spec.transform.position = new Vector2(-2f, -3.5f);
        if (currentLevel == 1)
        {
            this.spec.movingRight = false;
            this.spec.transform.position = new Vector2(2f, 3.5f);
        }

        GameObject goal = Instantiate(goalPrefab, levelContainer);
        goal.transform.position = new Vector2(2.6f, 4.56f);
        if (currentLevel == 1)
        {
            goal.transform.position = new Vector2(-2.6f, -4.56f);
        }

        GameObject leftWall = Instantiate(wallPrefab, levelContainer);
        leftWall.transform.position = new Vector2(-3.1f, 0f);
        leftWall.GetComponent<SpriteRenderer>().color = levelObstacleColors[currentLevel];

        GameObject rightWall = Instantiate(wallPrefab, levelContainer);
        rightWall.transform.position = new Vector2(3.1f, 0f);
        rightWall.GetComponent<SpriteRenderer>().color = levelObstacleColors[currentLevel];

        GameObject roof = Instantiate(roofPrefab, levelContainer);
        roof.transform.position = new Vector2(0f, 5.37f);
        roof.GetComponent<SpriteRenderer>().color = levelObstacleColors[currentLevel];

        GameObject floor = Instantiate(floorPrefab, levelContainer);
        floor.transform.position = new Vector2(0f, -5.37f);
        floor.GetComponent<SpriteRenderer>().color = levelObstacleColors[currentLevel];
        
        Camera.main.backgroundColor = levelBackgroundColors[currentLevel];
    }
}

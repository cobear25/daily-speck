using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public enum LevelType
{
    FullRandom, 
    AllSquares,
    AllSquaresTilted,
    SmallSquares, 
    SmallSquaresTilted,
    MediumSquares, 
    MediumSquaresTilted,
    BigSquares, 
    BigSquaresTilted,
    AllLedges, 
    ThinLedges, 
    ThickLedges,
    CurvyLedges,
    AllCircles,
    SmallCircles,
    MediumCircles,
    BigCircles,
    AllTriangles,
    SmallTriangles,
    MediumTriangles,
    BigTriangles
}

public class GameController : MonoBehaviour
{
    public Spec spec;
    public Transform levelContainer;
    public TextMeshProUGUI countdownText;

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
    public GameObject smallCirclePrefab;
    public GameObject mediumCirclePrefab;
    public GameObject bigCirclePrefab;
    public GameObject smallTrianglePrefab;
    public GameObject mediumTrianglePrefab;
    public GameObject bigTrianglePrefab;
    public GameObject thinLedgePrefab;
    public GameObject thickLedgePrefab;
    public GameObject curvyLedgePrefab;
    public GameObject jamPrefab;

    [Header("Daily Obstacles")]
    [SerializeField] Vector2Int[] squareObstacleCountRangesByLevel =
    {
        new Vector2Int(5, 10),
        new Vector2Int(10, 15),
        new Vector2Int(15, 20)
    };
    [SerializeField] Vector2Int[] ledgeObstacleCountRangesByLevel =
    {
        new Vector2Int(3, 5),
        new Vector2Int(5, 7),
        new Vector2Int(7, 9)
    };
    [SerializeField] float[] squareMovementChancesByLevel = { 0f, 0.1f, 0.5f };
    [SerializeField] float[] ledgeRotationChancesByLevel = { 0f, 0.1f, 0.5f };
    [SerializeField] int extraSquareObstaclesPerWeekday = 1;
    [SerializeField] int extraLedgeObstaclesPerWeekday = 1;
    [SerializeField] int smallObstacleCountBonus = 2;
    [SerializeField] int bigObstacleCountPenalty = 2;
    [SerializeField] float extraMovementChancePerWeekday = 0.03f;
    [SerializeField] int maxObstaclePlacementAttempts = 100;
    [SerializeField] float obstaclePadding = 0.15f;
    [SerializeField] float startAndGoalPadding = 0.75f;
    [SerializeField] Vector2 obstacleSpawnMin = new Vector2(-2.35f, -4.35f);
    [SerializeField] Vector2 obstacleSpawnMax = new Vector2(2.35f, 4.35f);
    [SerializeField] float leftLedgeX = -2.65f;
    [SerializeField] float rightLedgeX = 2.65f;
    [SerializeField] float ledgeXJitter = 0.5f;
    [SerializeField] float ledgeInwardJitterChance = 0.75f;
    [SerializeField] float maxLedgeRotation = 10f;
    [SerializeField] float minLedgeVerticalSpacing = 1f;

    public int currentLevel = 0;
    static bool hasShownInitialCountdown = false;
    bool useDebugRandomSeed = false;
    int debugRandomSeed = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateLevel();

        if (!hasShownInitialCountdown)
        {
            StartCoroutine(PlayInitialCountdown());
        }
        else if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            useDebugRandomSeed = true;
            debugRandomSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            GenerateLevel();
        }
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
        {
            LevelBeat();
        }
#endif
    }

    public void LevelBeat()
    {
        useDebugRandomSeed = false;
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
        this.spec.canMove = hasShownInitialCountdown;
        Vector2 specPosition = new Vector2(-2f, -3.5f);
        this.spec.transform.position = specPosition;
        if (currentLevel == 1)
        {
            this.spec.movingRight = false;
            specPosition = new Vector2(2f, 3.5f);
            this.spec.transform.position = specPosition;
        }

        GameObject goal = Instantiate(goalPrefab, levelContainer);
        Vector2 goalPosition = new Vector2(2.6f, 4.56f);
        goal.transform.position = goalPosition;
        if (currentLevel == 1)
        {
            goalPosition = new Vector2(-2.6f, -4.56f);
            goal.transform.position = goalPosition;
        }

        GameObject leftWall = Instantiate(wallPrefab, levelContainer);
        leftWall.transform.position = new Vector2(-3.3f, 0f);
        leftWall.GetComponent<SpriteRenderer>().color = GetLevelObstacleColor();

        GameObject rightWall = Instantiate(wallPrefab, levelContainer);
        rightWall.transform.position = new Vector2(3.3f, 0f);
        rightWall.GetComponent<SpriteRenderer>().color = GetLevelObstacleColor();

        GameObject roof = Instantiate(roofPrefab, levelContainer);
        roof.transform.position = new Vector2(0f, 5.37f);
        roof.GetComponent<SpriteRenderer>().color = GetLevelObstacleColor();

        GameObject floor = Instantiate(floorPrefab, levelContainer);
        floor.transform.position = new Vector2(0f, -5.37f);
        floor.GetComponent<SpriteRenderer>().color = GetLevelObstacleColor();

        System.Random random = new System.Random(GetLevelSeed());
        LevelType levelType = GetRandomLevelType(random);
        GenerateObstacles(specPosition, goalPosition, levelType, random);
        
        Camera.main.backgroundColor = GetLevelBackgroundColor();
    }

    IEnumerator PlayInitialCountdown()
    {
        if (countdownText == null)
        {
            hasShownInitialCountdown = true;
            spec.canMove = true;
            yield break;
        }

        countdownText.gameObject.SetActive(true);

        for (int count = 3; count > 0; count--)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSeconds(1f);
        }

        countdownText.gameObject.SetActive(false);
        hasShownInitialCountdown = true;
        spec.canMove = true;
    }

    void GenerateObstacles(Vector2 specPosition, Vector2 goalPosition, LevelType levelType, System.Random random)
    {
        List<Rect> occupiedSpaces = new List<Rect>
        {
            RectFromCenter(specPosition, Vector2.one, startAndGoalPadding),
            RectFromCenter(goalPosition, Vector2.one, startAndGoalPadding)
        };
        List<float> placedLedgeYs = new List<float>();
        bool nextLedgeOnLeft = random.Next(2) == 0;

        Vector2Int obstacleCountRange = GetObstacleCountRange(levelType);
        int obstacleCount = random.Next(obstacleCountRange.x, obstacleCountRange.y + 1);

        for (int i = 0; i < obstacleCount; i++)
        {
            if (ShouldPlaceLedge(levelType, random))
            {
                TryPlaceLedge(levelType, occupiedSpaces, placedLedgeYs, ref nextLedgeOnLeft, random);
            }
            else
            {
                TryPlaceFloatingObstacle(levelType, occupiedSpaces, random);
            }
        }
    }

    bool TryPlaceFloatingObstacle(LevelType levelType, List<Rect> occupiedSpaces, System.Random random)
    {
        GameObject prefab = GetRandomFloatingObstaclePrefab(levelType, random);
        if (prefab == null)
        {
            return false;
        }

        Vector2 obstacleSize = GetPrefabWorldSize(prefab);

        for (int attempt = 0; attempt < maxObstaclePlacementAttempts; attempt++)
        {
            Vector2 position = GetRandomObstaclePosition(random, obstacleSize);
            Rect obstacleSpace = RectFromCenter(position, obstacleSize, obstaclePadding);

            if (OverlapsAny(obstacleSpace, occupiedSpaces))
            {
                continue;
            }

            GameObject obstacle = Instantiate(prefab, levelContainer);
            obstacle.transform.position = position;
            obstacle.transform.rotation = GetFloatingObstacleRotation(levelType, random);
            obstacle.GetComponent<SpriteRenderer>().color = GetLevelObstacleColor();
            ConfigureSquareMovement(obstacle, prefab, random);
            occupiedSpaces.Add(obstacleSpace);
            return true;
        }

        return false;
    }

    bool TryPlaceLedge(LevelType levelType, List<Rect> occupiedSpaces, List<float> placedLedgeYs, ref bool nextLedgeOnLeft, System.Random random)
    {
        GameObject prefab = GetRandomLedgePrefab(levelType, random);
        if (prefab == null)
        {
            return false;
        }

        Vector2 ledgeSize = GetPrefabWorldSize(prefab);

        for (int attempt = 0; attempt < maxObstaclePlacementAttempts; attempt++)
        {
            float y = RandomRange(random, obstacleSpawnMin.y + ledgeSize.y / 2f, obstacleSpawnMax.y - ledgeSize.y / 2f);
            if (!IsFarEnoughFromOtherLedges(y, placedLedgeYs))
            {
                continue;
            }

            bool isOnLeft = nextLedgeOnLeft;
            float baseX = isOnLeft ? leftLedgeX : rightLedgeX;
            float x = baseX + GetLedgeXJitter(isOnLeft, random);
            Vector2 position = new Vector2(x, y);
            Rect ledgeSpace = RectFromCenter(position, ledgeSize, obstaclePadding);

            if (OverlapsAny(ledgeSpace, occupiedSpaces))
            {
                continue;
            }

            GameObject ledge = Instantiate(prefab, levelContainer);
            ledge.transform.position = position;
            ledge.transform.rotation = GetLedgeRotation(random);
            ledge.GetComponent<SpriteRenderer>().color = GetLevelObstacleColor();
            occupiedSpaces.Add(ledgeSpace);
            placedLedgeYs.Add(y);
            nextLedgeOnLeft = !nextLedgeOnLeft;
            return true;
        }

        return false;
    }

    float GetLedgeXJitter(bool isOnLeft, System.Random random)
    {
        bool moveTowardCenter = random.NextDouble() < Mathf.Clamp01(ledgeInwardJitterChance);

        if (isOnLeft)
        {
            return moveTowardCenter
                ? RandomRange(random, 0f, ledgeXJitter)
                : RandomRange(random, -ledgeXJitter, 0f);
        }

        return moveTowardCenter
            ? RandomRange(random, -ledgeXJitter, 0f)
            : RandomRange(random, 0f, ledgeXJitter);
    }

    LevelType GetRandomLevelType(System.Random random)
    {
        List<LevelType> levelTypes = new List<LevelType>();

        if (HasAnyFloatingObstaclePrefab() || HasAnyLedgePrefab())
        {
            levelTypes.Add(LevelType.FullRandom);
        }

        AddFloatingLevelTypes(levelTypes, LevelType.AllSquares, LevelType.AllSquaresTilted, smallSquarePrefab, mediumSquarePrefab, bigSquarePrefab);
        AddFloatingLevelTypes(levelTypes, LevelType.AllCircles, smallCirclePrefab, mediumCirclePrefab, bigCirclePrefab);
        AddFloatingLevelTypes(levelTypes, LevelType.AllTriangles, smallTrianglePrefab, mediumTrianglePrefab, bigTrianglePrefab);

        AddSinglePrefabLevelTypes(levelTypes, LevelType.SmallSquares, LevelType.SmallSquaresTilted, smallSquarePrefab);
        AddSinglePrefabLevelTypes(levelTypes, LevelType.MediumSquares, LevelType.MediumSquaresTilted, mediumSquarePrefab);
        AddSinglePrefabLevelTypes(levelTypes, LevelType.BigSquares, LevelType.BigSquaresTilted, bigSquarePrefab);
        AddSinglePrefabLevelTypes(levelTypes, LevelType.SmallCircles, smallCirclePrefab);
        AddSinglePrefabLevelTypes(levelTypes, LevelType.MediumCircles, mediumCirclePrefab);
        AddSinglePrefabLevelTypes(levelTypes, LevelType.BigCircles, bigCirclePrefab);
        AddSinglePrefabLevelTypes(levelTypes, LevelType.SmallTriangles, smallTrianglePrefab);
        AddSinglePrefabLevelTypes(levelTypes, LevelType.MediumTriangles, mediumTrianglePrefab);
        AddSinglePrefabLevelTypes(levelTypes, LevelType.BigTriangles, bigTrianglePrefab);

        AddFloatingLevelTypes(levelTypes, LevelType.AllLedges, thinLedgePrefab, thickLedgePrefab, curvyLedgePrefab);
        AddSinglePrefabLevelTypes(levelTypes, LevelType.ThinLedges, thinLedgePrefab);
        AddSinglePrefabLevelTypes(levelTypes, LevelType.ThickLedges, thickLedgePrefab);
        AddSinglePrefabLevelTypes(levelTypes, LevelType.CurvyLedges, curvyLedgePrefab);

        if (levelTypes.Count == 0)
        {
            return LevelType.AllSquares;
        }

        return levelTypes[random.Next(levelTypes.Count)];
    }

    void AddFloatingLevelTypes(List<LevelType> levelTypes, LevelType levelType, params GameObject[] prefabs)
    {
        if (HasAnyPrefab(prefabs))
        {
            levelTypes.Add(levelType);
        }
    }

    void AddFloatingLevelTypes(List<LevelType> levelTypes, LevelType levelType, LevelType tiltedLevelType, params GameObject[] prefabs)
    {
        if (HasAnyPrefab(prefabs))
        {
            levelTypes.Add(levelType);
            levelTypes.Add(tiltedLevelType);
        }
    }

    void AddSinglePrefabLevelTypes(List<LevelType> levelTypes, LevelType levelType, GameObject prefab)
    {
        if (prefab != null)
        {
            levelTypes.Add(levelType);
        }
    }

    void AddSinglePrefabLevelTypes(List<LevelType> levelTypes, LevelType levelType, LevelType tiltedLevelType, GameObject prefab)
    {
        if (prefab != null)
        {
            levelTypes.Add(levelType);
            levelTypes.Add(tiltedLevelType);
        }
    }

    bool ShouldPlaceLedge(LevelType levelType, System.Random random)
    {
        if (IsLedgeLevelType(levelType))
        {
            return true;
        }

        if (levelType != LevelType.FullRandom || !HasAnyLedgePrefab())
        {
            return false;
        }

        if (!HasAnyFloatingObstaclePrefab())
        {
            return true;
        }

        return random.NextDouble() < 0.35f;
    }

    GameObject GetRandomFloatingObstaclePrefab(LevelType levelType, System.Random random)
    {
        switch (levelType)
        {
            case LevelType.AllSquares:
            case LevelType.AllSquaresTilted:
                return GetRandomAvailablePrefab(random, smallSquarePrefab, mediumSquarePrefab, bigSquarePrefab);
            case LevelType.SmallSquares:
            case LevelType.SmallSquaresTilted:
                return smallSquarePrefab;
            case LevelType.MediumSquares:
            case LevelType.MediumSquaresTilted:
                return mediumSquarePrefab;
            case LevelType.BigSquares:
            case LevelType.BigSquaresTilted:
                return bigSquarePrefab;
            case LevelType.AllCircles:
                return GetRandomAvailablePrefab(random, smallCirclePrefab, mediumCirclePrefab, bigCirclePrefab);
            case LevelType.SmallCircles:
                return smallCirclePrefab;
            case LevelType.MediumCircles:
                return mediumCirclePrefab;
            case LevelType.BigCircles:
                return bigCirclePrefab;
            case LevelType.AllTriangles:
                return GetRandomAvailablePrefab(random, smallTrianglePrefab, mediumTrianglePrefab, bigTrianglePrefab);
            case LevelType.SmallTriangles:
                return smallTrianglePrefab;
            case LevelType.MediumTriangles:
                return mediumTrianglePrefab;
            case LevelType.BigTriangles:
                return bigTrianglePrefab;
            case LevelType.FullRandom:
                return GetRandomAvailablePrefab(
                    random,
                    smallSquarePrefab,
                    mediumSquarePrefab,
                    bigSquarePrefab,
                    smallCirclePrefab,
                    mediumCirclePrefab,
                    bigCirclePrefab,
                    smallTrianglePrefab,
                    mediumTrianglePrefab,
                    bigTrianglePrefab
                );
            default:
                return null;
        }
    }

    GameObject GetRandomLedgePrefab(LevelType levelType, System.Random random)
    {
        switch (levelType)
        {
            case LevelType.AllLedges:
            case LevelType.FullRandom:
                return GetRandomAvailablePrefab(random, thinLedgePrefab, thickLedgePrefab, curvyLedgePrefab);
            case LevelType.ThinLedges:
                return thinLedgePrefab;
            case LevelType.ThickLedges:
                return thickLedgePrefab;
            case LevelType.CurvyLedges:
                return curvyLedgePrefab;
            default:
                return null;
        }
    }

    GameObject GetRandomAvailablePrefab(System.Random random, params GameObject[] prefabs)
    {
        List<GameObject> availablePrefabs = new List<GameObject>();
        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
            {
                availablePrefabs.Add(prefab);
            }
        }

        if (availablePrefabs.Count == 0)
        {
            return null;
        }

        return availablePrefabs[random.Next(availablePrefabs.Count)];
    }

    bool HasAnyFloatingObstaclePrefab()
    {
        return HasAnyPrefab(
            smallSquarePrefab,
            mediumSquarePrefab,
            bigSquarePrefab,
            smallCirclePrefab,
            mediumCirclePrefab,
            bigCirclePrefab,
            smallTrianglePrefab,
            mediumTrianglePrefab,
            bigTrianglePrefab
        );
    }

    bool HasAnyLedgePrefab()
    {
        return HasAnyPrefab(thinLedgePrefab, thickLedgePrefab, curvyLedgePrefab);
    }

    bool HasAnyPrefab(params GameObject[] prefabs)
    {
        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
            {
                return true;
            }
        }

        return false;
    }

    bool IsLedgeLevelType(LevelType levelType)
    {
        return levelType == LevelType.AllLedges
            || levelType == LevelType.ThinLedges
            || levelType == LevelType.ThickLedges
            || levelType == LevelType.CurvyLedges;
    }

    Quaternion GetFloatingObstacleRotation(LevelType levelType, System.Random random)
    {
        if (!IsTiltedLevelType(levelType))
        {
            return Quaternion.identity;
        }

        return Quaternion.Euler(0f, 0f, 45f);
    }

    Quaternion GetLedgeRotation(System.Random random)
    {
        if (random.NextDouble() >= GetLedgeRotationChance())
        {
            return Quaternion.identity;
        }

        return Quaternion.Euler(0f, 0f, RandomRange(random, -maxLedgeRotation, maxLedgeRotation));
    }

    bool IsTiltedLevelType(LevelType levelType)
    {
        return levelType == LevelType.AllSquaresTilted
            || levelType == LevelType.SmallSquaresTilted
            || levelType == LevelType.MediumSquaresTilted
            || levelType == LevelType.BigSquaresTilted;
    }

    bool IsFarEnoughFromOtherLedges(float y, List<float> placedLedgeYs)
    {
        foreach (float placedY in placedLedgeYs)
        {
            if (Mathf.Abs(y - placedY) < minLedgeVerticalSpacing)
            {
                return false;
            }
        }

        return true;
    }

    Vector2Int GetObstacleCountRange(LevelType levelType)
    {
        if (IsLedgeLevelType(levelType))
        {
            return GetObstacleCountRange(ledgeObstacleCountRangesByLevel, extraLedgeObstaclesPerWeekday);
        }

        Vector2Int range = GetObstacleCountRange(squareObstacleCountRangesByLevel, extraSquareObstaclesPerWeekday);
        int sizeModifier = GetFloatingObstacleSizeCountModifier(levelType);

        return new Vector2Int(
            Mathf.Max(0, range.x + sizeModifier),
            Mathf.Max(0, range.y + sizeModifier)
        );
    }

    int GetFloatingObstacleSizeCountModifier(LevelType levelType)
    {
        if (IsSmallFloatingLevelType(levelType))
        {
            return smallObstacleCountBonus;
        }

        if (IsBigFloatingLevelType(levelType))
        {
            return -bigObstacleCountPenalty;
        }

        return 0;
    }

    bool IsSmallFloatingLevelType(LevelType levelType)
    {
        return levelType == LevelType.SmallSquares
            || levelType == LevelType.SmallSquaresTilted
            || levelType == LevelType.SmallCircles
            || levelType == LevelType.SmallTriangles;
    }

    bool IsBigFloatingLevelType(LevelType levelType)
    {
        return levelType == LevelType.BigSquares
            || levelType == LevelType.BigSquaresTilted
            || levelType == LevelType.BigCircles
            || levelType == LevelType.BigTriangles;
    }

    Vector2Int GetObstacleCountRange(Vector2Int[] obstacleCountRangesByLevel, int extraObstaclesPerWeekday)
    {
        if (obstacleCountRangesByLevel.Length == 0)
        {
            return new Vector2Int(0, 0);
        }

        Vector2Int range = obstacleCountRangesByLevel[currentLevel % obstacleCountRangesByLevel.Length];
        int weekdayObstacleBonus = GetWeekdayDifficulty() * extraObstaclesPerWeekday;
        int min = Mathf.Min(range.x, range.y) + weekdayObstacleBonus;
        int max = Mathf.Max(range.x, range.y) + weekdayObstacleBonus;

        return new Vector2Int(min, max);
    }

    void ConfigureSquareMovement(GameObject obstacle, GameObject prefab, System.Random random)
    {
        FloatingObstacle floatingObstacle = obstacle.GetComponent<FloatingObstacle>();

        if (floatingObstacle == null || random.NextDouble() >= GetSquareMovementChance())
        {
            return;
        }

        float movementTypeChance = 0.5f;
        float enabledMovementPenalty = 0.5f;
        bool canRotate = !IsCirclePrefab(prefab);
    
        floatingObstacle.shouldRotate = canRotate && random.NextDouble() < movementTypeChance;

        float upAndDownChance = floatingObstacle.shouldRotate
            ? movementTypeChance * enabledMovementPenalty
            : movementTypeChance;
        floatingObstacle.shouldMoveUpAndDown = random.NextDouble() < upAndDownChance;

        float leftAndRightChance = movementTypeChance;
        if (floatingObstacle.shouldRotate)
        {
            leftAndRightChance *= enabledMovementPenalty;
        }
        if (floatingObstacle.shouldMoveUpAndDown)
        {
            leftAndRightChance *= enabledMovementPenalty;
        }
        floatingObstacle.shouldMoveLeftAndRight = random.NextDouble() < leftAndRightChance;

        if (!floatingObstacle.shouldRotate && !floatingObstacle.shouldMoveUpAndDown && !floatingObstacle.shouldMoveLeftAndRight)
        {
            int fallbackMovementType = random.Next(canRotate ? 3 : 2);
            floatingObstacle.shouldRotate = canRotate && fallbackMovementType == 0;
            floatingObstacle.shouldMoveUpAndDown = fallbackMovementType == (canRotate ? 1 : 0);
            floatingObstacle.shouldMoveLeftAndRight = fallbackMovementType == (canRotate ? 2 : 1);
        }
    }

    bool IsCirclePrefab(GameObject prefab)
    {
        return prefab == smallCirclePrefab
            || prefab == mediumCirclePrefab
            || prefab == bigCirclePrefab;
    }

    float GetSquareMovementChance()
    {
        if (squareMovementChancesByLevel.Length == 0)
        {
            return 0f;
        }

        float chance = squareMovementChancesByLevel[currentLevel % squareMovementChancesByLevel.Length];
        chance += GetWeekdayDifficulty() * extraMovementChancePerWeekday;
        return Mathf.Clamp01(chance);
    }

    float GetLedgeRotationChance()
    {
        if (ledgeRotationChancesByLevel.Length == 0)
        {
            return 0f;
        }

        float chance = ledgeRotationChancesByLevel[currentLevel % ledgeRotationChancesByLevel.Length];
        chance += GetWeekdayDifficulty() * extraMovementChancePerWeekday;
        return Mathf.Clamp01(chance);
    }

    int GetWeekdayDifficulty()
    {
        int dayOfWeek = (int)DateTime.Today.DayOfWeek;
        Debug.Log("Day of week: " + dayOfWeek);
        return dayOfWeek == 0 ? 6 : dayOfWeek - 1;
    }

    int GetLevelSeed()
    {
        if (useDebugRandomSeed)
        {
            return debugRandomSeed;
        }

        return GetDailySeed();
    }

    int GetDailySeed()
    {
        DateTime today = DateTime.Today;

        unchecked
        {
            return (today.Year * 10000 + today.Month * 100 + today.Day) * 397 ^ currentLevel;
        }
    }

    Vector2 GetRandomObstaclePosition(System.Random random, Vector2 obstacleSize)
    {
        float halfWidth = obstacleSize.x / 2f;
        float halfHeight = obstacleSize.y / 2f;
        float xMin = obstacleSpawnMin.x + halfWidth;
        float xMax = obstacleSpawnMax.x - halfWidth;
        float yMin = obstacleSpawnMin.y + halfHeight;
        float yMax = obstacleSpawnMax.y - halfHeight;

        return new Vector2(
            RandomRange(random, xMin, xMax),
            RandomRange(random, yMin, yMax)
        );
    }

    float RandomRange(System.Random random, float min, float max)
    {
        if (max <= min)
        {
            return min;
        }

        return min + (float)random.NextDouble() * (max - min);
    }

    Vector2 GetPrefabWorldSize(GameObject prefab)
    {
        Vector2 size = Vector2.one;
        BoxCollider2D boxCollider = prefab.GetComponent<BoxCollider2D>();
        CircleCollider2D circleCollider = prefab.GetComponent<CircleCollider2D>();
        CapsuleCollider2D capsuleCollider = prefab.GetComponent<CapsuleCollider2D>();
        SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();

        if (boxCollider != null)
        {
            size = boxCollider.size;
        }
        else if (circleCollider != null)
        {
            size = Vector2.one * circleCollider.radius * 2f;
        }
        else if (capsuleCollider != null)
        {
            size = capsuleCollider.size;
        }
        else if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            size = spriteRenderer.sprite.bounds.size;
        }
        else if (spriteRenderer != null)
        {
            size = spriteRenderer.size;
        }

        Vector3 scale = prefab.transform.localScale;

        return new Vector2(Mathf.Abs(size.x * scale.x), Mathf.Abs(size.y * scale.y));
    }

    Rect RectFromCenter(Vector2 center, Vector2 size, float padding)
    {
        Vector2 paddedSize = size + Vector2.one * padding * 2f;
        return new Rect(center - paddedSize / 2f, paddedSize);
    }

    bool OverlapsAny(Rect rect, List<Rect> occupiedSpaces)
    {
        foreach (Rect occupiedSpace in occupiedSpaces)
        {
            if (rect.Overlaps(occupiedSpace))
            {
                return true;
            }
        }

        return false;
    }

    Color GetLevelObstacleColor()
    {
        if (levelObstacleColors.Length == 0)
        {
            return Color.white;
        }

        return levelObstacleColors[currentLevel % levelObstacleColors.Length];
    }

    Color GetLevelBackgroundColor()
    {
        if (levelBackgroundColors.Length == 0)
        {
            return Camera.main.backgroundColor;
        }

        return levelBackgroundColors[currentLevel % levelBackgroundColors.Length];
    }
}

using UnityEngine;
using System.Collections;

[AddComponentMenu("Tetris3D/GameManager")]
public class GameManager : MonoBehaviour
{

    public GameObject[] blocks;
    public Transform cube;
    public Transform leftWall;
    public Transform rightWall;
    public int maxBlockSize = 4;
    public int _fieldWidth = 10;
    public int _fieldHeight = 13;
    public float blockNormalFallSpeed = 0.01f;
    public float blockDropSpeed = 30f;
    public Texture2D cubeTexture;

    private int fieldWidth;
    private int fieldHeight;
    private bool[,] fields;
    private int[] cubeYposition;
    private Transform[] cubeTransforms;
    private int clearTimes;
    private float addSpeed = 0.01f;
    private int TimeToAddSpeed = 10;
    public static int RotateSpeed = 10;

    private int Score = 0;
    public static int Highest = 0;
    private int blockRandom;
    private GameObject nextBlock;
    private Block nextB;
    private int nextSize;
    private string[] nextblock;

    public static GameManager manager;

    public static bool gameOver = false;
    public static float BottonW = Screen.width * 0.3f;
    public static float BottonH = Screen.height / 10;
    public GUISkin ReturnGUISkin;
    public GUISkin RestartGUISkin;
    public Texture2D LoseTexture;

    public static bool GetReturn = false;
    public static float LabelW = 360;
    public static float LabelH = 80;

    public static int Num;

    public static float MyTime;

    // Use this for initialization
    void Start()
    {
        MyTime += Time.deltaTime;

        if (MyTime > 60 && RotateSpeed < 90)
        {
            RotateSpeed += 5;
        }
        if (manager == null)
        {
            manager = this;
        }

        if (PlayerPrefs.HasKey("Highest"))
        {
            Highest = PlayerPrefs.GetInt("Highest");
        }
        else
        {
            PlayerPrefs.SetInt("Highest", 0);
        }

        blockRandom = Random.Range(0, blocks.Length);

        fieldWidth = _fieldWidth + maxBlockSize * 2;//生成位置横坐标
        fieldHeight = _fieldHeight + maxBlockSize + 5;//生成位置竖坐标
        fields = new bool[fieldWidth, fieldHeight];
        cubeYposition = new int[fieldHeight * fieldWidth];
        cubeTransforms = new Transform[fieldHeight * fieldWidth];

        for (int i = 0; i < fieldHeight; i++)
        {

            for (int j = 0; j < maxBlockSize; j++)
            {

                fields[j, i] = true;
                fields[fieldWidth - 1 - j, i] = true;

            }

        }

        for (int i = 0; i < fieldWidth; i++)
        {
            fields[i, 0] = true;
        }

        CreateBlock(blockRandom);
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android && (Input.GetKeyDown(KeyCode.Escape)))
        {

            if (!GetReturn)
            {
                Num = 0;
            }
            else
            {
                Num = 1;
            }

            switch (Num)
            {
                case 0:
                    {
                        GetReturn = true;
                        Return();
                    }; break;
                case 1:
                    {
                        GetReturn = false;
                        Return();
                    }; break;
            }
        }
    }

    void Return()
    {
        if (GetReturn)
        {
            Time.timeScale = 0;
        }
        else
            Time.timeScale = 1;
    }

    void CreateBlock(int random)
    {
        Instantiate(blocks[random]);
        blockRandom = Random.Range(0, blocks.Length);
        nextBlock = blocks[blockRandom];
        nextB = (Block)nextBlock.GetComponent("Block");
        nextSize = nextB.block.GetLength(0);
        nextblock = new string[nextSize];
        nextblock = nextB.block;
    }

    public int GetFieldWidth()
    {
        return fieldWidth;
    }

    public int GetFieldHeight()
    {
        return fieldHeight;
    }

    public int GetBlockRandom()
    {
        return blockRandom;
    }

    public bool CheckBlock(bool[,] blockMatrix, int xPos, int yPos)
    {

        var size = blockMatrix.GetLength(0);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (blockMatrix[y, x] && fields[xPos + x, yPos - y])
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void SetBlock(bool[,] blockMatrix, int xPos, int yPos)
    {
        int size = blockMatrix.GetLength(0);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (blockMatrix[y, x])
                {
                    Instantiate(cube, new Vector3(xPos + x, yPos - y, 0), Quaternion.identity);
                    fields[xPos + x, yPos - y] = true;
                }
            }
        }
        StartCoroutine(CheckRows(yPos - size, size));
    }

    IEnumerator CheckRows(int yStart, int size)
    {
        yield return null;
        if (yStart < 1) yStart = 1;
        int count = 1;
        for (int y = yStart; y < yStart + size; y++)
        {
            int x;
            for (x = maxBlockSize; x < fieldWidth - maxBlockSize; x++)
            {
                if (!fields[x, y])
                {
                    break;
                }
            }
            if (x == fieldWidth - maxBlockSize)
            {
                yield return StartCoroutine(SetRows(y));
                Score += 10 * count;
                y--;
                count++;
            }
        }
        CreateBlock(blockRandom);
    }

    IEnumerator SetRows(int yStart)
    {
        for (int y = yStart; y < fieldHeight - 1; y++)
        {
            for (int x = maxBlockSize; x < fieldWidth - maxBlockSize; x++)
            {
                fields[x, y] = fields[x, y + 1];
            }
        }

        for (int x = maxBlockSize; x < fieldWidth - maxBlockSize; x++)
        {
            fields[x, fieldHeight - 1] = false;
        }

        var cubes = GameObject.FindGameObjectsWithTag("Cube");
        int cubeToMove = 0;
        for (int i = 0; i < cubes.Length; i++)
        {
            GameObject cube = cubes[i];
            if (cube.transform.position.y > yStart)
            {
                cubeYposition[cubeToMove] = (int)(cube.transform.position.y);
                cubeTransforms[cubeToMove++] = cube.transform;
            }
            else if (cube.transform.position.y == yStart)
            {
                Destroy(cube);
            }
        }

        float t = 0;
        while (t <= 1f)
        {
            t += Time.deltaTime * 5f;
            for (int i = 0; i < cubeToMove; i++)
            {
                cubeTransforms[i].position = new Vector3(cubeTransforms[i].position.x, Mathf.Lerp(cubeYposition[i], cubeYposition[i] - 1, t),
                    cubeTransforms[i].position.z);
            }
            yield return null;
        }

        if (++clearTimes == TimeToAddSpeed && blockNormalFallSpeed < 1)
        {
            blockNormalFallSpeed += addSpeed;
            clearTimes = 0;
        }

    }

    public void GameOver()
    {
        if (Score > PlayerPrefs.GetInt("Highest"))
        {
            PlayerPrefs.SetInt("Highest", Score);
        }
        //print("Game Over!!!");
        gameOver = true;
    }

    void OnGUI()
    {
        GUIStyle aa = new GUIStyle();
        aa.normal.background = null;    //这是设置背景填充的
        aa.normal.textColor = new Color(255, 255, 255);   //设置字体颜色的
        aa.fontSize = 50;       //当然，这是字体颜色

        GUIStyle bb = new GUIStyle();
        bb.normal.background = null;    //这是设置背景填充的
        bb.normal.textColor = new Color(255, 255, 255);   //设置字体颜色的
        bb.fontSize = 50;       //当然，这是字体大小

        GUIStyle cc = new GUIStyle();
        cc.normal.background = LoseTexture;    //这是设置背景填充的
        cc.normal.textColor = new Color(255, 255, 255);   //设置字体颜色的
        cc.fontSize = 50;       //当然，这是字体大小

        if (!gameOver)
        {
            GUI.Label(new Rect(10, 10, 80, 40), "Score:" + Score.ToString() + "   Highest:" + Highest.ToString(), bb);

            for (int y = 0; y < nextSize; y++)
            {
                for (int x = 0; x < nextSize; x++)
                {
                    if (nextblock[y][x] == '1')
                    {
                        GUI.Button(new Rect(10 + 20 * x, 80 + 20 * y, 30, 30), cubeTexture);
                    }
                }
            }
        }

        if (GetReturn)
        {
            string Attention = "游戏已暂停\n返回键回到游戏";
            GUI.Label(new Rect(10, Screen.height - LabelH - 10, LabelW, LabelH), Attention, aa);
        }

        if (gameOver)
        {
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "你总共砌了  " + (Score * 0.1f).ToString() + "  层城墙\n" + "共获得  " + Score.ToString() + "  分", cc);
            GUI.skin = ReturnGUISkin;
            if (GUI.Button(new Rect(Screen.width - BottonW, Screen.height - 2 * BottonH, BottonW, BottonH), ""))
            {
                if (Application.loadedLevelName == "ZTMain")
                    Application.LoadLevel("ZTMain");
                gameOver = false;
            }

            GUI.skin = RestartGUISkin;
            if (GUI.Button(new Rect(Screen.width - BottonW, Screen.height - BottonH, BottonW, BottonH), ""))
            {
                if (Application.loadedLevelName == "ZTMain")
                    Application.LoadLevel("ZTStart");
            }
        }

    }

}

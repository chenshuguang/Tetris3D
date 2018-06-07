using UnityEngine;
using System.Collections;

[AddComponentMenu("Tetris3D/Block")]
public class Block : MonoBehaviour
{

    public string[] block;

    private bool[,] blockMatrix;

    private int size;
    ////private float halfSize;
    ////private float halfSizeFloat;
    private float childSize;
    private int xPosition;
    private int yPosition;
    private float fallSpeed;
    private bool drop = false;

    public static float BottonW = Screen.width * 0.3f;
    public static float BottonH = Screen.height / 10;

    // Use this for initialization
    void Start()
    {

        size = block.Length;
        ////int width = block[0].Length;

        ////halfSize = (size + 1) * .5f;
        childSize = (size - 1) * .5f;
        ////halfSizeFloat = size * .5f;

        blockMatrix = new bool[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (block[y][x] == '1')
                {
                    blockMatrix[y, x] = true;
                    var cube = (Transform)Instantiate(GameManager.manager.cube, new Vector3(x - childSize, childSize - y, 0), Quaternion.identity);
                    cube.parent = transform;
                }
            }
        }

        yPosition = GameManager.manager.GetFieldHeight() - 1;
        transform.position = new Vector3(GameManager.manager.GetFieldWidth() / 2 + (size % 2 == 0 ? 0.5f : 0), yPosition, 0);
        xPosition = (int)(transform.position.x - childSize);
        fallSpeed = GameManager.manager.blockNormalFallSpeed;

        if (GameManager.manager.CheckBlock(blockMatrix, xPosition, yPosition))
        {
            GameManager.manager.GameOver();
            return;
        }

        StartCoroutine(CheckInput());
        StartCoroutine(Delay((1 / GameManager.manager.blockNormalFallSpeed) * 2));
        StartCoroutine(Fall());
    }

    // Update is called once per frame
    void Update()
    {
        drop = false;
        if ((Input.touchCount > 0 && (Input.GetTouch(0).position.y < GameManager.BottonH + 10) && (Input.GetTouch(0).position.x < Screen.width - GameManager.BottonW - 10) && (Input.GetTouch(0).position.x > GameManager.BottonW + 10)) || Input.GetKey(KeyCode.DownArrow))
        {
            fallSpeed = GameManager.manager.blockDropSpeed;
            drop = true;
        }
        else
            fallSpeed = GameManager.manager.blockNormalFallSpeed;
    }

    IEnumerator Delay(float time)
    {
        var t = 0f;
        while (t <= time && !drop)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Fall()
    {
        while (true)
        {
            yPosition--;
            if (GameManager.manager.CheckBlock(blockMatrix, xPosition, yPosition))
            {
                GameManager.manager.SetBlock(blockMatrix, xPosition, yPosition + 1);
                Destroy(gameObject);
                break;
            }

            for (float i = yPosition + 1; i > yPosition; i -= Time.deltaTime * fallSpeed)
            {
                transform.position = new Vector3(transform.position.x, i - childSize, transform.position.z);
                yield return null;
            }
        }
    }

    IEnumerator MoveHorizontal(int distance)
    {

        if (!GameManager.manager.CheckBlock(blockMatrix, xPosition + distance, yPosition))
        {
            transform.position = new Vector3(transform.position.x + distance, transform.position.y, transform.position.z);
            xPosition += distance;
            yield return new WaitForSeconds(.05f);
        }

    }

    void RotateBlock()
    {

        var tempMatrix = new bool[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tempMatrix[y, x] = blockMatrix[x, (size - 1) - y];
            }
        }

        if (!GameManager.manager.CheckBlock(tempMatrix, xPosition, yPosition))
        {
            System.Array.Copy(tempMatrix, blockMatrix, size * size);
            transform.Rotate(0, 0, 90);
        }
    }

    IEnumerator CheckInput()
    {

        while (true)
        {
            var input = Input.GetAxisRaw("Horizontal");
            if ((Input.touchCount > 0 && Input.GetTouch(0).position.x < (Screen.width / 2) && (Input.GetTouch(0).position.y > GameManager.BottonH + 10)) || input < 0)
            {
                yield return StartCoroutine(MoveHorizontal(1));
            }

            if ((Input.touchCount > 0 && Input.GetTouch(0).position.x > (Screen.width / 2) && (Input.GetTouch(0).position.y > GameManager.BottonH + 10)) || input > 0)
            {
                yield return StartCoroutine(MoveHorizontal(-1));
            }

            ////if (ZTCamareController.rotate || Input.GetKey(KeyCode.UpArrow))
            ////{
            ////    RotateBlock();
            ////    ZTCamareController.rotate = false;
            ////}
            yield return new WaitForSeconds(.1f);
        }

    }
}

using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Painting : MonoBehaviour
{
    public RawImage raw;                   //使用UGUI的RawImage显示，方便进行添加UI,将pivot设为(0.5,0.5)
    private float lastDistance;
    private Vector3[] PositionArray = new Vector3[3];
    private int a = 0;
    private Vector3[] PositionArray1 = new Vector3[4];
    private int b = 0;
    private float[] speedArray = new float[4];
    private int s = 0;
    public int num = 50;
    public float myScale = 0.5f;
    int screenWidth;
    int screenHeight;
    float SetScale(float distance)
    {
        float Scale = 0;
        if (distance < 100)
        {
            Scale = (0.8f - 0.005f * distance) * myScale;
        }
        else
        {
            Scale = (0.425f - 0.00125f * distance) * myScale;
        }
        if (Scale <= 0.05f)
        {
            Scale = 0.05f * myScale;
        }
        return Scale;
    }

  
    void DrawImage()
    {
        raw.texture = texRender;
    }
    
    //三阶贝塞尔曲线，获取连续4个点坐标，通过调整中间2点坐标，画出部分（我使用了num/1.5实现画出部分曲线）来使曲线平滑;通过速度控制曲线宽度。
    private void ThreeOrderBézierCurse(Vector3 pos, float distance, float targetPosOffset)
    {
        //记录坐标
        PositionArray1[b] = pos;
        b++;
        //记录速度
        speedArray[s] = distance;
        s++;
        if (b == 4)
        {
            Vector3 temp1 = PositionArray1[1];
            Vector3 temp2 = PositionArray1[2];

            //修改中间两点坐标
            Vector3 middle = (PositionArray1[0] + PositionArray1[2]) / 2;
            PositionArray1[1] = (PositionArray1[1] - middle) * 1.5f + middle;
            middle = (temp1 + PositionArray1[3]) / 2;
            PositionArray1[2] = (PositionArray1[2] - middle) * 2.1f + middle;

            for (int index1 = 0; index1 < num / 1.5f; index1++)
            {
                float t1 = (1.0f / num) * index1;
                Vector3 target = Mathf.Pow(1 - t1, 3) * PositionArray1[0] +
                                 3 * PositionArray1[1] * t1 * Mathf.Pow(1 - t1, 2) +
                                 3 * PositionArray1[2] * t1 * t1 * (1 - t1) + PositionArray1[3] * Mathf.Pow(t1, 3);
                //float deltaspeed = (float)(distance - lastDistance) / num;
                //获取速度差值（存在问题，参考）
                float deltaspeed = (float)(speedArray[3] - speedArray[0]) / num;
                //float randomOffset = Random.Range(-1/(speedArray[0] + (deltaspeed * index1)), 1 / (speedArray[0] + (deltaspeed * index1)));
                //模拟毛刺效果
                float randomOffset = Random.Range(-targetPosOffset, targetPosOffset);
                DrawBrush(texRender, (int)(target.x + randomOffset), (int)(target.y + randomOffset), brushTypeTexture, brushColor[(int)brushColorType], SetScale(speedArray[0] + (deltaspeed * index1)));
            }

            PositionArray1[0] = temp1;
            PositionArray1[1] = temp2;
            PositionArray1[2] = PositionArray1[3];

            speedArray[0] = speedArray[1];
            speedArray[1] = speedArray[2];
            speedArray[2] = speedArray[3];
            b = 3;
            s = 3;
        }
        else
        {
            DrawBrush(texRender, (int)endPosition.x, (int)endPosition.y, brushTypeTexture,
                brushColor[(int)brushColorType], brushScale);
        }

    }

    private RenderTexture texRender;
    public Material mat;
    public Texture brushTypeTexture;
    private enum BrushColor
    {
        red,
        green,
        blue,
        pink,
        yellow,
        gray,
        black,
        white,
        count,
    }
    private float brushScale = 0.5f;
    private BrushColor brushColorType = BrushColor.black;
    private Color[] brushColor = new Color[(int)BrushColor.count] { Color.red, Color.green, Color.blue, new Color(255, 0, 255), Color.yellow, Color.gray, Color.black, Color.white };

    void Start()
    {

        screenWidth = (int)(Display.main.renderingWidth * raw.GetComponent<RectTransform>().localScale.x);
        screenHeight = (int)(Display.main.renderingHeight * raw.GetComponent<RectTransform>().localScale.y);
        texRender = new RenderTexture(screenWidth, screenHeight, 24, RenderTextureFormat.ARGB32);
      
         Clear(texRender);
    }

    Vector3 startPosition = Vector3.zero;
    Vector3 endPosition = Vector3.zero;
    void Update()
    {

        if (Input.GetMouseButton(0))
        {
            OnMouseMove(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
        }
        DrawImage();
    }

    void OnMouseUp()
    {
        startPosition = Vector3.zero;
        a = 0;
        b = 0;
        s = 0;
    }

    void OnMouseMove(Vector3 pos)
    {
        endPosition = pos;
    
       
        float distance = Vector3.Distance(startPosition, endPosition);
       
        if (startPosition == Vector3.zero)
        {
            startPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        }

        endPosition = pos;
        brushScale = SetScale(distance);
        ThreeOrderBézierCurse(pos, distance, 4.5f);
        startPosition = endPosition;
        lastDistance = distance;
    }

    void Clear(RenderTexture destTexture)
    {
        Graphics.SetRenderTarget(destTexture);
        GL.PushMatrix();
        //GL.Clear(true, true, Color.black);
        GL.PopMatrix();
    }

    void DrawBrush(RenderTexture destTexture, Vector2 pos, Texture sourceTexture, Color color, float scale)
    {
        DrawBrush(destTexture, (int)pos.x, (int)pos.y, sourceTexture, color, scale);
    }

    void DrawBrush(RenderTexture destTexture, int x, int y, Texture sourceTexture, Color color, float scale)
    {
        DrawBrush(destTexture, new Rect(x, y, sourceTexture.width, sourceTexture.height), sourceTexture, color, scale);
    }

    void DrawBrush(RenderTexture destTexture, Rect destRect, Texture sourceTexture, Color color, float scale)
    {
        float left = destRect.left - destRect.width * scale / 2.0f;
        float right = destRect.left + destRect.width * scale / 2.0f;
        float top = destRect.top - destRect.height * scale / 2.0f;
        float bottom = destRect.top + destRect.height * scale / 2.0f;

        Graphics.SetRenderTarget(destTexture);

        GL.PushMatrix();
        GL.LoadOrtho();

        mat.SetTexture("_MainTex", brushTypeTexture);
        mat.SetColor("_Color", color);
        mat.SetPass(0);

        GL.Begin(GL.QUADS);

        GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(left / screenWidth, top / screenHeight, 0);
        GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(right / screenWidth, top / screenHeight, 0);
        GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(right / screenWidth, bottom / screenHeight, 0);
        GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(left / screenWidth, bottom / screenHeight, 0);

        GL.End();
        GL.PopMatrix();
    }

    //bool bshow = true;
    void OnGUI()
    {

       
          //  GUI.DrawTexture(new Rect(0, 0, screenWidth, screenHeight), texRender, ScaleMode.StretchToFill);
        

        //    if (GUI.Button(new Rect(0, 150, 100, 30), "clear"))
        //    {
        //        Clear(texRender);
        //    }

        //    if (GUI.Button(new Rect(100, 150, 100, 30), "hide"))
        //    {
        //        bshow = !bshow;
        //    }

        //    int width = screenWidth / (int)BrushColor.count;

        //    //for (int i = 0; i < (int)BrushColor.count; i++)
        //    //{
        //    //    if (GUI.Button(new Rect(i * width, 0, width, 30), Enum.GetName(typeof(BrushColor), i)))
        //    //    {
        //    //        brushColorType = (BrushColor)i;
        //    //    }
        //    //}

        //    GUI.Label(new Rect(0, 200, 300, 30), "brushScale : " + brushScale.ToString("F2"));
        //    brushScale = (int)GUI.HorizontalSlider(new Rect(120, 205, 200, 30), brushScale * 10.0f, 1, 50) / 10.0f;
        //    if (brushScale < 0.1f)
        //        brushScale = 0.1f;
        }

    }

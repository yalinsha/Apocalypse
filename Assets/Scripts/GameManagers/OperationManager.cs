using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 处理除了按钮以外的所有交互逻辑
/// </summary>
public class OperationManager : MonoBehaviour
{
    public static OperationManager Instance
    {
        get; private set;
    }
    private void Awake()
    {
        Instance = this;
    }

    const float range = 10;//相机移动边界
    const float padding = 10;//屏幕边缘平移区域宽度
    bool isInConstructionMode = false;
    bool flip = false;
    public Grid grid;
    string currentBuildingName = "";

    public void EnterConstructionMode(string buildingName)
    {
        isInConstructionMode = true;
        currentBuildingName = buildingName;
    }
    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.S))
        {
            ResourceManager.Instance.ChangeResources(new Dictionary<string, float>
            {
                {"electric", 10000},
                {"mine", 10000},
                {"food", 10000},
                {"water", 10000},
                {"oil", 10000},
                {"chip", 10000},
                {"ti", 10000},
                {"carbon", 10000},
                {"nuclear_part", 100},
                {"life_part", 100},
                {"shell_part", 100},
                {"chip_part", 100},
            });
        }
#endif
        //鼠标滚轮控制缩放
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - 300 * Time.unscaledDeltaTime * Input.GetAxis("Mouse ScrollWheel"), 2, 18);
        //定义移动速度
        float speed = 5 * Time.unscaledDeltaTime;
        //检测鼠标位置并移动摄像机
        if (Input.mousePosition.x < padding)
        {
            Camera.main.transform.Translate(Vector2.left * speed);
        }
        else if (Input.mousePosition.x > Screen.width - padding)
        {
            Camera.main.transform.Translate(Vector2.right * speed);
        }
        if (Input.mousePosition.y < padding)
        {
            Camera.main.transform.Translate(Vector2.down * speed);
        }
        else if (Input.mousePosition.y > Screen.height - padding)
        {
            Camera.main.transform.Translate(Vector2.up * speed);
        }
        // 限制摄像机位置
        Vector3 clampedPosition = Camera.main.transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -range, range);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -range, range);
        Camera.main.transform.position = clampedPosition;
        Vector2Int currentPosition = grid.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition)).ToVector2Int();
        if (isInConstructionMode)
        {
            bool b = MapManager.Instance.CanBuild(currentBuildingName, currentPosition, flip);
            MapRenderer.Instance.ChangePosition(currentPosition, b);
            if (Input.GetKeyDown(KeyCode.F))
            {
                flip = !flip;
            }
            if (Input.GetMouseButtonDown(0) && b && !EventSystem.current.IsPointerOverGameObject())//不在UI对象上，且该处可建造
            {
                MapRenderer.Instance.Build();
                MapManager.Instance.Build(currentBuildingName, currentPosition, flip);
                MapRenderer.Instance.ExitConstructionMode();
                isInConstructionMode = false;
            }
            if (Input.GetMouseButtonDown(1))
            {
                //退出建造模式
                isInConstructionMode = false;
                MapRenderer.Instance.ExitConstructionMode();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                if(MapManager.Instance.landUseMap.ContainsKey(currentPosition) && MapManager.Instance.landUseMap[currentPosition] != null)
                {
                    SingleInfoPanel.Instance.UpdateInfo(MapManager.Instance.landUseMap[currentPosition]);
                    SingleInfoPanel.Instance.Show();
                }
                else
                {
                    SingleInfoPanel.Instance.Hide();
                }
            }
        }
    }
}

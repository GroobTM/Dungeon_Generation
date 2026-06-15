using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    private const string HEADING_TEXT_SIZE_KEY = "DG_HeadingTextSize";
    private int headingTextSize;
    private const string GRID_CELL_SIZE_KEY = "DG_GridCellSize";
    private int gridCellSize;
    private const string GRID_CELL_MARGIN_KEY = "DG_GridCellMargin";
    private int gridCellMargin;

    private VisualElement gridContainer;

    private SerializedProperty gridWidth;
    private SerializedProperty gridHeight;

    private void OnEnable()
    {
        headingTextSize = EditorPrefs.GetInt(HEADING_TEXT_SIZE_KEY, 20);
        gridCellSize = EditorPrefs.GetInt(GRID_CELL_SIZE_KEY, 24);
        gridCellMargin = EditorPrefs.GetInt(GRID_CELL_MARGIN_KEY, 2);
    }


    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        DungeonGenerator targetScript = (DungeonGenerator)target;

        gridWidth = serializedObject.FindProperty("GridWidth");
        gridHeight = serializedObject.FindProperty("GridHeight");

        Label title = new Label("Dungeon Generator");
        title.style.fontSize = headingTextSize;
        root.Add(title);

        root.Add(CreateSeparator(2, 10));

        root.Add(CreateGridControls(targetScript));

        ScrollView gridScrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
        gridScrollView.style.maxHeight = 600;
        gridContainer = new VisualElement();
        gridContainer.style.flexDirection = FlexDirection.Row;
        gridContainer.style.flexWrap = Wrap.Wrap;
        gridScrollView.Add(gridContainer);
        root.Add(gridScrollView);
        CreateGrid(targetScript);


        root.TrackPropertyValue(gridWidth, _ => CreateGrid(targetScript));
        root.TrackPropertyValue(gridHeight, _ => CreateGrid(targetScript));
        root.Bind(serializedObject);
        return root;
    }

    private VisualElement CreateSeparator(float thickness, float margin)
    {
        VisualElement separator = new VisualElement();
        separator.style.height = thickness;
        separator.style.marginTop = margin;
        separator.style.marginBottom = margin;
        separator.style.backgroundColor = Color.gray;

        return separator;
    }

    private void CreateGrid(DungeonGenerator targetScript)
    {
        if (targetScript.EnabledGrid == null)
        {
            targetScript.OnValidate();
        }

        gridContainer.Clear();
        gridContainer.style.width = targetScript.GridWidth * gridCellSize;

        for (int y = 0; y < targetScript.EnabledGrid.GetLength(1); y++)
        {
            for (int x = 0; x < targetScript.EnabledGrid.GetLength(0); x++)
            {
                VisualElement gridCell = new VisualElement();

                int cellSize = gridCellSize - gridCellMargin;
                gridCell.style.width = cellSize;
                gridCell.style.height = cellSize;
                gridCell.style.marginRight = gridCellMargin;
                gridCell.style.marginBottom = gridCellMargin;

                gridCell.style.backgroundColor = targetScript.EnabledGrid[x,y] ? Color.white : Color.gray;

                int localX = x;
                int localY = y;

                gridCell.RegisterCallback<ClickEvent>(_ =>
                {
                    targetScript.EnabledGrid[localX, localY] = !targetScript.EnabledGrid[localX, localY];
                    gridCell.style.backgroundColor = targetScript.EnabledGrid[localX, localY] ? Color.white : Color.gray;
                    EditorUtility.SetDirty(targetScript);
                });

                gridContainer.Add(gridCell);
            }
        }
    }

    private VisualElement CreateGridControls(DungeonGenerator targetScript)
    {
        VisualElement controls = new VisualElement();
        controls.style.flexDirection = FlexDirection.Column;

        PropertyField gridWidthField = new PropertyField(gridWidth, "Grid Width");
        controls.Add(gridWidthField);

        PropertyField gridHeightField = new PropertyField(gridHeight, "Grid Height");
        controls.Add(gridHeightField);

        Button enableButton = new Button();
        enableButton.text = "Enable All Cells";
        enableButton.RegisterCallback<ClickEvent>(_ =>
        {
            targetScript.ResetGrid(true);
            CreateGrid(targetScript);
        });
        controls.Add(enableButton);

        Button disableButton = new Button();
        disableButton.text = "Disable All Cells";
        disableButton.RegisterCallback<ClickEvent>(_ =>
        {
            targetScript.ResetGrid(false);
            CreateGrid(targetScript);
        });
        controls.Add(disableButton);


        SliderInt cellSizeSlider = new SliderInt("Grid Cell Size", 8, 30);
        cellSizeSlider.value = gridCellSize;
        cellSizeSlider.RegisterValueChangedCallback(_ =>
        {
            gridCellSize = _.newValue;
            EditorPrefs.SetInt(GRID_CELL_SIZE_KEY, gridCellSize);

            CreateGrid(targetScript);
        });
        controls.Add(cellSizeSlider);

        return controls;
    }
}

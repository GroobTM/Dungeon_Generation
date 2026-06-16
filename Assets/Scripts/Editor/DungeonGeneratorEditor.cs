using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    private const int MAX_DRUNKARDS = 5;

    private const string HEADING_TEXT_SIZE_KEY = "DG_HeadingTextSize";
    private int headingTextSize;
    private const string GRID_CELL_SIZE_KEY = "DG_GridCellSize";
    private int gridCellSize;
    private const string GRID_CELL_MARGIN_KEY = "DG_GridCellMargin";
    private int gridCellMargin;

    private VisualElement gridContainer;
    private VisualElement drunkardTargetsContainer;

    private SerializedProperty gridWidth;
    private SerializedProperty gridHeight;

    private bool manuallyEditedGrid = true;

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

        root.Add(CreateSeparator(0, 10));

        Foldout drunkardsWalkFoldout = new Foldout();
        drunkardsWalkFoldout.text = "Populate using Drunkard's Walk";
        drunkardsWalkFoldout.Add(CreateDrunkardsWalkControls(targetScript));
        root.Add(drunkardsWalkFoldout);

        root.TrackPropertyValue(gridWidth, _ =>
        {
            DrunkardsWalkTargetControls(targetScript);
            CreateGrid(targetScript);
        });
        root.TrackPropertyValue(gridHeight, _ => 
        {
            DrunkardsWalkTargetControls(targetScript);
            CreateGrid(targetScript);
        });
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

                gridCell.tooltip = $"({x}, {y})";

                int localX = x;
                int localY = y;

                gridCell.RegisterCallback<ClickEvent>(_ =>
                {
                    targetScript.EnabledGrid[localX, localY] = !targetScript.EnabledGrid[localX, localY];
                    gridCell.style.backgroundColor = targetScript.EnabledGrid[localX, localY] ? Color.white : Color.gray;
                    EditorUtility.SetDirty(targetScript);

                    manuallyEditedGrid = true;
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
        cellSizeSlider.showInputField = true;
        cellSizeSlider.AddToClassList(Slider.alignedFieldUssClassName);
        cellSizeSlider.RegisterValueChangedCallback(value =>
        {
            gridCellSize = value.newValue;
            EditorPrefs.SetInt(GRID_CELL_SIZE_KEY, gridCellSize);

            CreateGrid(targetScript);
        });
        controls.Add(cellSizeSlider);

        return controls;
    }

    private VisualElement CreateDrunkardsWalkControls(DungeonGenerator targetScript)
    {
        VisualElement controls = new VisualElement();
        controls.style.flexDirection = FlexDirection.Column;

        SliderInt stepSlider = new SliderInt("Drunkard's Walk Max Steps", 0, 10000);
        stepSlider.value = targetScript.DrunkardsWalkParameters.StepCount;
        stepSlider.showInputField = true;
        stepSlider.AddToClassList(Slider.alignedFieldUssClassName);
        stepSlider.RegisterValueChangedCallback(value =>
        {
            targetScript.DrunkardsWalkParameters.StepCount = value.newValue;
            EditorUtility.SetDirty(targetScript);
        });
        controls.Add(stepSlider);

        Slider maxGridFillSlider = new Slider("Max Grid Fill Percentage", 0, 1);
        maxGridFillSlider.value = targetScript.DrunkardsWalkParameters.MaxGridFill;
        maxGridFillSlider.showInputField = true;
        maxGridFillSlider.AddToClassList(Slider.alignedFieldUssClassName);
        maxGridFillSlider.RegisterValueChangedCallback(value =>
        {
            targetScript.DrunkardsWalkParameters.MaxGridFill = value.newValue;
            EditorUtility.SetDirty(targetScript);
            serializedObject.Update();
        });
        controls.Add(maxGridFillSlider);

        SliderInt drunkardsSlider = new SliderInt("Drunkard Count", 1, MAX_DRUNKARDS);
        drunkardsSlider.value = targetScript.DrunkardsWalkParameters.Targets.Count;
        drunkardsSlider.showInputField = true;
        drunkardsSlider.AddToClassList(Slider.alignedFieldUssClassName);
        drunkardsSlider.RegisterValueChangedCallback(value =>
        {
            int diff = value.newValue - value.previousValue;

            if (diff != 0)
            {

                if (diff > 0)
                {
                    for (int i = 0; i < diff; i++)
                    {
                        targetScript.DrunkardsWalkParameters.Targets.Add(new DGDrunkardsWalkTarget(new Vector2Int(-1, -1), 0.5f));
                    }
                }
                else if (diff < 0)
                {
                    for (int i = 0; i > diff; i--)
                    {
                        targetScript.DrunkardsWalkParameters.Targets.RemoveAt(targetScript.DrunkardsWalkParameters.Targets.Count - 1);
                    }
                }

                EditorUtility.SetDirty(targetScript);
                serializedObject.Update();
                DrunkardsWalkTargetControls(targetScript);
            }
        });
        controls.Add(drunkardsSlider);

        controls.Add(CreateSeparator(1, 10));

        drunkardTargetsContainer = new VisualElement();
        controls.Add(drunkardTargetsContainer);
        DrunkardsWalkTargetControls(targetScript);

        Button drunkardsWalkButton = new Button();
        drunkardsWalkButton.text = "Perform Drunkards Walk";
        drunkardsWalkButton.RegisterCallback<ClickEvent>(_ =>
        {
            if (manuallyEditedGrid)
            {
                manuallyEditedGrid = !EditorUtility.DisplayDialog(
                    "Are you sure?",
                    "This action will overwrite the current grid layout. Are you sure you want to continue?",
                    "Continue",
                    "Cancel"
                );
            }

            if (!manuallyEditedGrid)
            {
                targetScript.PerformDrunkardsWalk();
                EditorUtility.SetDirty(targetScript);
                CreateGrid(targetScript);
                manuallyEditedGrid = false;
            }
        });
        controls.Add(drunkardsWalkButton);

        return controls;
    }

    private void DrunkardsWalkTargetControls(DungeonGenerator targetScript)
    {
        drunkardTargetsContainer.Clear();

        List<DGDrunkardsWalkTarget> targets = targetScript.DrunkardsWalkParameters.Targets;
        int maxX = targetScript.EnabledGrid.GetLength(0);
        int maxY = targetScript.EnabledGrid.GetLength(1);

        for (int i = 0; i < targets.Count; i++)
        {
            int localI = i;

            targets[i].Position = ClampVector2Int(targets[i].Position, maxX, maxY);

            Vector2IntField targetField = new Vector2IntField($"Drunkard {i + 1}'s Target");
            targetField.value = targets[i].Position;
            targetField.AddToClassList(Vector2IntField.alignedFieldUssClassName);
            targetField.RegisterValueChangedCallback(value =>
            {
                Vector2Int clampedValue = ClampVector2Int(value.newValue, maxX, maxY);
                targetField.SetValueWithoutNotify(clampedValue);
                targets[localI].Position = clampedValue;
                EditorUtility.SetDirty(targetScript);
                serializedObject.Update();
            });
            drunkardTargetsContainer.Add(targetField);

            Slider biasField = new Slider($"Drunkard {i + 1}'s Bias", 0, 1);
            biasField.value = targets[i].Bias;
            biasField.showInputField = true;
            biasField.AddToClassList(Slider.alignedFieldUssClassName);
            biasField.RegisterValueChangedCallback(value =>
            {
                targets[localI].Bias = value.newValue;
                EditorUtility.SetDirty(targetScript);
                serializedObject.Update();
            });
            drunkardTargetsContainer.Add(biasField);

        }
    }

    private Vector2Int ClampVector2Int(Vector2Int value, int maxX, int maxY)
    {
        return new Vector2Int(
            Mathf.Clamp(value.x, -1, maxX - 1),
            Mathf.Clamp(value.y, -1, maxY - 1)
        );
    }
}

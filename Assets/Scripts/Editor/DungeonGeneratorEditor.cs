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
    private const string DRUNKARDS_WALK_FOLDOUT_STATE_KEY = "DG_DrunkardsWalkFoldoutState";

    private VisualElement gridContainer;
    private VisualElement drunkardTargetsContainer;

    private Button wfcSeededButton;
    private Button wfcRandomButton;
    private Button convertTo3DButton;

    private SerializedProperty gridWidth;
    private SerializedProperty gridHeight;
    private SerializedProperty drunkardsWalkStepCount;
    private SerializedProperty drunkardsWalkMaxGridFill;
    private SerializedProperty drunkardsWalkTargets;
    private SerializedProperty drunkardsWalkSeed;
    private SerializedProperty tileSet;
    private SerializedProperty wfcBacktrackLimit;
    private SerializedProperty wfcSeed;
    private SerializedProperty roomCell;

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

        gridWidth = serializedObject.FindProperty("gridWidth");
        gridHeight = serializedObject.FindProperty("gridHeight");
        drunkardsWalkStepCount = serializedObject.FindProperty("drunkardsWalkStepCount");
        drunkardsWalkMaxGridFill = serializedObject.FindProperty("drunkardsWalkMaxGridFill");
        drunkardsWalkTargets = serializedObject.FindProperty("drunkardsWalkTargets");
        drunkardsWalkSeed = serializedObject.FindProperty("drunkardsWalkSeed");
        tileSet = serializedObject.FindProperty("tileSet");
        wfcBacktrackLimit = serializedObject.FindProperty("wfcBacktrackLimit");
        wfcSeed = serializedObject.FindProperty("wfcSeed");
        roomCell = serializedObject.FindProperty("roomCell");

        root.Add(CreateSeparator(0, 10));

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
        drunkardsWalkFoldout.value = SessionState.GetBool(DRUNKARDS_WALK_FOLDOUT_STATE_KEY, false);
        drunkardsWalkFoldout.RegisterValueChangedCallback(value =>
        {
            SessionState.SetBool(DRUNKARDS_WALK_FOLDOUT_STATE_KEY, value.newValue);
        });
        drunkardsWalkFoldout.Add(CreateDrunkardsWalkControls(targetScript));
        root.Add(drunkardsWalkFoldout);

        root.Add(CreateSeparator(2, 10));

        root.Add(CreateWFCControls(targetScript));

        root.Add(CreateSeparator(2, 10));

        root.Add(Create3DConverterControls(targetScript));

        root.TrackPropertyValue(gridWidth, _ =>
        {
            DrunkardsWalkTargetControls();
            CreateGrid(targetScript);
        });
        root.TrackPropertyValue(gridHeight, _ => 
        {
            DrunkardsWalkTargetControls();
            CreateGrid(targetScript);
        });
        root.TrackPropertyValue(tileSet, value =>
        {
            wfcSeededButton.SetEnabled(value.objectReferenceValue != null);
            wfcRandomButton.SetEnabled(value.objectReferenceValue != null);
        });
        root.TrackPropertyValue(roomCell, value =>
        {
            convertTo3DButton.SetEnabled(value.objectReferenceValue != null);
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
        if (targetScript.EnabledGrid == null
            || targetScript.EnabledGrid.GetLength(0) != gridWidth.intValue
            || targetScript.EnabledGrid.GetLength(1) != gridHeight.intValue)
        {
            targetScript.OnValidate();
        }

        gridContainer.Clear();
        gridContainer.style.width = gridWidth.intValue * gridCellSize;

        for (int y = gridHeight.intValue - 1; y >= 0; y--)
        {
            for (int x = 0; x < gridWidth.intValue; x++)
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

        PropertyField stepField = new PropertyField(drunkardsWalkStepCount, "Max Steps");
        controls.Add(stepField);

        PropertyField maxGridFillField = new PropertyField(drunkardsWalkMaxGridFill, "Max Grid Fill Percentage");
        controls.Add(maxGridFillField);

        SliderInt drunkardsSlider = new SliderInt("Drunkard Count", 1, MAX_DRUNKARDS);
        drunkardsSlider.value = drunkardsWalkTargets.arraySize;
        drunkardsSlider.showInputField = true;
        drunkardsSlider.AddToClassList(Slider.alignedFieldUssClassName);
        drunkardsSlider.RegisterValueChangedCallback(value =>
        {
            if (drunkardsWalkTargets.arraySize != value.newValue)
            {
                drunkardsWalkTargets.arraySize = value.newValue;
                serializedObject.ApplyModifiedProperties();
                DrunkardsWalkTargetControls();
            }
        });
        controls.Add(drunkardsSlider);

        controls.Add(CreateSeparator(1, 10));

        drunkardTargetsContainer = new VisualElement();
        controls.Add(drunkardTargetsContainer);
        DrunkardsWalkTargetControls();

        controls.Add(CreateSeparator(1, 10));

        PropertyField seedField = new PropertyField(drunkardsWalkSeed, "Seed");
        controls.Add(seedField);

        Button drunkardsWalkSeededButton = new Button();
        drunkardsWalkSeededButton.text = "Perform Drunkards Walk Using Seed";
        drunkardsWalkSeededButton.RegisterCallback<ClickEvent>(_ =>
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
        controls.Add(drunkardsWalkSeededButton);

        Button drunkardsWalkRandomButton = new Button();
        drunkardsWalkRandomButton.text = "Perform Drunkards Walk Random Random Seed";
        drunkardsWalkRandomButton.RegisterCallback<ClickEvent>(_ =>
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
                drunkardsWalkSeed.intValue = Random.Range(int.MinValue, int.MaxValue);
                serializedObject.ApplyModifiedProperties();

                targetScript.PerformDrunkardsWalk();
                EditorUtility.SetDirty(targetScript);
                CreateGrid(targetScript);
                manuallyEditedGrid = false;
            }
        });
        controls.Add(drunkardsWalkRandomButton);

        return controls;
    }

    private void DrunkardsWalkTargetControls()
    {
        serializedObject.Update();
        drunkardTargetsContainer.Clear();

        int maxX = gridWidth.intValue;
        int maxY = gridHeight.intValue;

        for (int i = 0; i < drunkardsWalkTargets.arraySize; i++)
        {
            SerializedProperty target = drunkardsWalkTargets.GetArrayElementAtIndex(i);
            SerializedProperty position = target.FindPropertyRelative("Position");
            SerializedProperty bias = target.FindPropertyRelative("Bias");

            position.vector2IntValue = ClampVector2Int(position.vector2IntValue, maxX, maxY);

            PropertyField positionField = new PropertyField(position, $"Drunkard {i + 1}'s Target");
            positionField.RegisterValueChangeCallback(value =>
            {
                value.changedProperty.vector2IntValue = ClampVector2Int(value.changedProperty.vector2IntValue, maxX, maxY);
                value.changedProperty.serializedObject.ApplyModifiedProperties();
            });
            drunkardTargetsContainer.Add(positionField);

            PropertyField biasField = new PropertyField(bias, $"Drunkard {i + 1}'s Bias");
            drunkardTargetsContainer.Add(biasField);
        }

        serializedObject.ApplyModifiedProperties();
        drunkardTargetsContainer.Bind(serializedObject);
    }

    private Vector2Int ClampVector2Int(Vector2Int value, int maxX, int maxY)
    {
        return new Vector2Int(
            Mathf.Clamp(value.x, -1, maxX - 1),
            Mathf.Clamp(value.y, -1, maxY - 1)
        );
    }

    private VisualElement CreateWFCControls(DungeonGenerator targetScript)
    {
        VisualElement controls = new VisualElement();
        controls.style.flexDirection = FlexDirection.Column;

        Label heading = new Label("Wave Function Collapse");
        heading.style.fontSize = headingTextSize * 0.9f;
        controls.Add(heading);

        controls.Add(CreateSeparator(0, 10));

        PropertyField tileSetField = new PropertyField(tileSet, "Tile Set");
        controls.Add(tileSetField);

        PropertyField backtrackingLimitField = new PropertyField(wfcBacktrackLimit, "Backtracking Limit");
        controls.Add(backtrackingLimitField);

        PropertyField seedField = new PropertyField(wfcSeed, "Seed");
        controls.Add(seedField);

        wfcSeededButton = new Button();
        wfcSeededButton.text = "Perform Wave Function Collapse Using Seed";
        wfcSeededButton.SetEnabled(tileSet.objectReferenceValue != null);
        wfcSeededButton.RegisterCallback<ClickEvent>(_ =>
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Are you sure?",
                "This action will overwrite the current layout. Are you sure you want to continue?",
                "Continue",
                "Cancel"
            );

            if (confirmed)
            {
                targetScript.PerformWaveFunctionCollapse();
            }
        });
        controls.Add(wfcSeededButton);

        wfcRandomButton = new Button();
        wfcRandomButton.text = "Perform Wave Function Collapse Using Seed";
        wfcRandomButton.SetEnabled(tileSet.objectReferenceValue != null);
        wfcRandomButton.RegisterCallback<ClickEvent>(_ =>
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Are you sure?",
                "This action will overwrite the current layout. Are you sure you want to continue?",
                "Continue",
                "Cancel"
            );

            if (confirmed)
            {
                wfcSeed.intValue = Random.Range(int.MinValue, int.MaxValue);
                serializedObject.ApplyModifiedProperties();

                targetScript.PerformWaveFunctionCollapse();
            }
        });
        controls.Add(wfcRandomButton);

        return controls;
    }

    private VisualElement Create3DConverterControls(DungeonGenerator targetScript)
    {
        VisualElement controls = new VisualElement();

        Label heading = new Label("3D Conversion");
        heading.style.fontSize = headingTextSize * 0.9f;
        controls.Add(heading);

        controls.Add(CreateSeparator(0, 10));

        PropertyField roomCellField = new PropertyField(roomCell, "Room Cell");
        controls.Add(roomCellField);

        convertTo3DButton = new Button();
        convertTo3DButton.text = "Convert to 3D";
        convertTo3DButton.SetEnabled(roomCell.objectReferenceValue != null);
        convertTo3DButton.RegisterCallback<ClickEvent>(_ =>
        {
            Transform parent = targetScript.transform;
            DGRoomCell[] childCells = parent.GetComponentsInChildren<DGRoomCell>(true);

            if (childCells != null && childCells.Length > 0)
            {
                int confirmed = EditorUtility.DisplayDialogComplex(
                    "Existing Layout Found",
                    "An existing layout was found. Would you like to remove the existing layout?",
                    "Remove",
                    "Cancel",
                    "Keep"
                );

                switch (confirmed)
                {
                    case 0:
                        for (int i = 0; i < childCells.Length; i++)
                        {
                            if (childCells[i] != null)
                            {
                                DestroyImmediate(childCells[i].gameObject);
                            }
                        }

                        targetScript.Perform3DConversion();
                        break;

                    case 1:
                        break;

                    case 2:
                        GameObject layout = new GameObject("Layout");
                        layout.transform.SetParent(parent, false);

                        bool layoutUsed = false;
                        for (int i = 0; i < childCells.Length; i++)
                        {
                            if (childCells[i].transform.parent == targetScript.transform)
                            {
                                layoutUsed = true;
                                childCells[i].transform.SetParent(layout.transform, false);
                            }
                        }

                        if (layoutUsed)
                        {
                            layout = new GameObject("Layout");
                            layout.transform.SetParent(parent, false);
                        }

                        targetScript.Perform3DConversion(layout.transform);
                        break;
                }
            }
            else
            {
                targetScript.Perform3DConversion();
            }

                
        });
        controls.Add(convertTo3DButton);

        Button remove3DLayout = new Button();
        remove3DLayout.text = "Remove Layout";
        remove3DLayout.RegisterCallback<ClickEvent>(_ => {
            DGRoomCell[] childCells = targetScript.transform.GetComponentsInChildren<DGRoomCell>(true);

            if (childCells != null && childCells.Length > 0)
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Are you sure?",
                    "This action will remove the current layout. Are you sure you want to continue?",
                    "Continue",
                    "Cancel"
                );

                if (confirmed)
                {
                    for (int i = 0; i < childCells.Length; i++)
                    {
                        if (childCells[i] != null)
                        {
                            DestroyImmediate(childCells[i].gameObject);
                        }
                    }
                }
            }
        });
        controls.Add(remove3DLayout);

        return controls;
    }
}

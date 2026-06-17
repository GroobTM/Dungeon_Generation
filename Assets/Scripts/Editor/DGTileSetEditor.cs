using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(DGTileSet))]
public class DGTileSetEditor : Editor
{
    private const int TILE_CELL_SIZE = 20;
    private const int TILE_CELL_MARGIN = 1;

    private const string HEADING_TEXT_SIZE_KEY = "DG_HeadingTextSize";
    private int headingTextSize;

    private VisualElement tileContainer;

    private void OnEnable()
    {
        headingTextSize = EditorPrefs.GetInt(HEADING_TEXT_SIZE_KEY, 20);
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        DGTileSet targetScript = (DGTileSet)target;

        Label title = new Label("Tiles Set");
        title.style.fontSize = headingTextSize;
        root.Add(title);

        root.Add(CreateSeparator(2, 10));

        Button addTileButton = new Button();
        addTileButton.text = "Add Tile";
        addTileButton.RegisterCallback<ClickEvent>(_ =>
        {
            targetScript.Tiles.Add(new DGTile());
            EditorUtility.SetDirty(targetScript);

            CreateTileSettings(targetScript);
        });
        root.Add(addTileButton);

        root.Add(CreateSeparator(0, 5));

        tileContainer = new VisualElement();
        tileContainer.style.flexDirection = FlexDirection.Column;
        root.Add(tileContainer);
        CreateTileSettings(targetScript);

        return root;
    }

    private void CreateTileSettings(DGTileSet targetScript)
    {
        tileContainer.Clear();
        
        for (int i = 0; i < targetScript.Tiles.Count; i++)
        {
            string foloutStateKey = $"DGTileSet_{targetScript.GetEntityId()}Tile{i}";

            Foldout tileSettingsFoldout = new Foldout();
            tileSettingsFoldout.text = $"Tile {i}";
            tileSettingsFoldout.value = SessionState.GetBool(foloutStateKey, false);
            tileSettingsFoldout.RegisterValueChangedCallback(value =>
            {
                SessionState.SetBool(foloutStateKey, value.newValue);
            });

            VisualElement tileSettings = new VisualElement();
            tileSettings.style.width = 3 * TILE_CELL_SIZE + 2;
            tileSettings.style.flexDirection = FlexDirection.Row;
            tileSettings.style.flexWrap = Wrap.Wrap;

            int localI = i;

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    VisualElement tileCell = new VisualElement();

                    int cellSize = TILE_CELL_SIZE - TILE_CELL_MARGIN;
                    tileCell.style.width = cellSize;
                    tileCell.style.height = cellSize;
                    tileCell.style.marginRight = TILE_CELL_MARGIN;
                    tileCell.style.marginBottom = TILE_CELL_MARGIN;

                    if (x == 1 && y == 1)
                    {
                        tileCell.style.backgroundColor = Color.gray2;
                    }
                    else
                    {
                        tileCell.style.backgroundColor = targetScript.Tiles[i].Values[x, y] ? Color.white : Color.gray;

                        int localX = x;
                        int localY = y;
                        tileCell.RegisterCallback<ClickEvent>(_ =>
                        {
                            targetScript.Tiles[localI].Values[localX, localY] = !targetScript.Tiles[localI].Values[localX, localY];
                            tileCell.style.backgroundColor = targetScript.Tiles[localI].Values[localX, localY] ? Color.white : Color.gray;
                            EditorUtility.SetDirty(targetScript);
                        });
                    }

                    tileSettings.Add(tileCell);
                }
            }
            tileSettingsFoldout.Add(tileSettings);
            
            Button removeTileButton = new Button();
            removeTileButton.text = "Remove Tile";
            removeTileButton.RegisterCallback<ClickEvent>(_ =>
            {
                targetScript.Tiles.RemoveAt(localI);
                EditorUtility.SetDirty(targetScript);

                CreateTileSettings(targetScript);
            });
            tileSettingsFoldout.Add(removeTileButton);

            tileContainer.Add(tileSettingsFoldout);
        }
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
}
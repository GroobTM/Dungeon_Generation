# Dungeon Generation
The project used the drunkard's walk and wave function collapse procedural content generation techniques to generate dungeon layouts.

![Demo image showing generated layout.](https://github.com/GroobTM/Dungeon_Generation/blob/main/Layout%20Image.png)

![Demo image showing layout converted into 3D.](https://github.com/GroobTM/Dungeon_Generation/blob/main/3D%20Image.png)

## Instructions
This project was created using the Unity game engine (6000.4.0f1).
This project used assets from [Low Poly Modular Village - LOWPOLY MEDIEVAL FANTASY SERIES](https://assetstore.unity.com/packages/3d/environments/low-poly-modular-village-lowpoly-medieval-fantasy-series-258073). You do not need to buy these assets to use the dungeon generator, as alternative primitive assets are provided.
1. Install the Unity Hub from the [Unity website](https://unity.com/).
2. Clone this repository.
3. Launch the Unity Hub and select the "Projects" tab.
4. Select the "Add" button followed by the "Add project from disk" button from the resulting drop-down menu.
5. Select the repository on your local disk.
6. Select the "Dungeon Generation" entry from the list of projects.
7. Install the required Unity game engine version when prompted.
8. (Optional) Add the required assets:
    1. Add [Low Poly Modular Village - LOWPOLY MEDIEVAL FANTASY SERIES](https://assetstore.unity.com/packages/3d/environments/low-poly-modular-village-lowpoly-medieval-fantasy-series-258073) to your assets.
    2. Navigate to "Asset Store" -> "My Assets" -> "Low Poly Modular Village - LOWPOLY MEDIEVAL FANTASY SERIES", then download and add the assets to the project.
    3. Navigate to the "Polytope Studio\Lowpoly_Village\URP" folder.
    4. Open and import the contents of "PT_Village_URP_17.unitypackage".
9. Select the "SampleScene" scene from the "Scenes" folder in the Unity Editor.
10. Select the "Dungeon Generator" game object from the hierarchy.
    * The "Dungeon Generator" script can be assigned to any game object.
    * Tile sets can be found in the "Scripts/Tile Sets" folder.
    * The room cell prefab can be found in the "Prefabs" folder.

# Jelly Link

Jelly Link is an interactive, grid-based game developed in Unity that showcases custom editor windows, custom dependecy injection system, grid management, and the use of Unity's Job System for certain optimizations.

### What can be learned:

* **Custom Editor Window Usage:** Learn how to create editor windows with a variety of features for level design and management.
* **Custom Dependency Injection (DI) System:** The project implements a custom DI system to manage dependencies between game components, ensuring better modularity, testability, and flexibility in extending the game.
* **Job System Optimization:** Practical implementation of Unity's Job System to handle mesh deformations for a squishy "jelly" effect, ensuring smooth and efficient performance.
* **UniTask for Async Operations:** Simple use of UniTask to handle asynchronous operations, such as jelly reactions.
* **Scriptable Object Events:** Explore the architecture and usage of Scriptable Object events to manage gameplay interactions in a modular way.
* **Breadth-First Search Algorithm:** Implementation of a breadth-first search algorithm that allows for linked reactions across the grid.


*Note:* To keep the project focused, non-essential gameplay systems have been stripped.

# How To Use

1. **Open Tools/Grid Spawner:** Access the custom tool from the Unity Editor.
2. **Configure Grid Size and Spacing:** Set your desired grid dimensions and spacing.
3. **Select Prefabs and Paint the Grid:** Choose prefabs from the drop area and paint the grid. Any prefab deriving from `BaseGridItem` is automatically added to the preview, but you can also manually drop prefabs.
4. **Copy Grid from Scene (Optional):** If there is at least one active level in the scene, you can copy the existing grid layout.
5. **Spawn and Save:** Spawn your modified grid into the scene and save it by selecting a path within the project.

# Example Visuals
### Gameplay
https://github.com/user-attachments/assets/add2263f-fe98-4709-96b0-7fd74d31ad99
### Editor Display
![ezgif-4-3f7fd1c41a](https://github.com/user-attachments/assets/97e4656f-633c-4579-a570-fda3bab1d0ed)
### Example Level - 1
![Editor](https://github.com/user-attachments/assets/bd4f1900-cada-49b0-bf40-d4ad1e04ff91)
### Example Level - 2
![Editor2](https://github.com/user-attachments/assets/baef6807-9595-4749-91e3-410630ab475c)

### Licensing Information

This project utilizes third-party assets, which require separate licenses.

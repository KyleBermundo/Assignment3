using benjohnson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonManager : Singleton<DungeonManager>
{
    public GenerationSettingsSO gen;

    public Room CurrentRoom { get { return currentRoom; } }
    Room currentRoom;
    List<Room> bossRooms = new List<Room>();
    List<Room> rooms;

    // Generation variables
    List<EC_Door> doorsToFill; // List of doors with no destination yet, generate a room for these doors
    bool portalGenerated = false;

    [Header("Components")]
    [SerializeField] DungeonMapRenderer mapRenderer;
    [HideInInspector] public ArrangeGrid gridLayout;

    protected override void Awake()
    {
        base.Awake();

        // Initialize variables
        rooms = new List<Room>();
        doorsToFill = new List<EC_Door>();

        // Assign components
        gridLayout = GetComponent<ArrangeGrid>();
    }

    void Start()
    {
        GenerateDungeon(GameManager.instance.stage);
    }

    public void GenerateDungeon(int stage)
    {
        if (stage > gen.rooms.Count - 1)
        {
            GameManager.instance.LoadWinScreen();
            stage = gen.rooms.Count;
            return;
        }

        // Generate first room
        CreateRoom(stage, 0, null);
        // Iteratively generate layers
        GenerateLayer(stage, 1);
        // Generate boss room
        GenerateBossRoom(stage);

        // Enter first room
        SwitchRoom(rooms[0]);

        GameManager.instance.DungeonLoaded();
    }

    /// <summary>
    /// Generates boss door at deepest point in dungeon, generates boss room behind boss door,
    /// generates boss key at next lowest point excluding boss door room and boss door room parent
    /// to avoid having the key generate near boss door
    /// </summary>
    List<Room> bossRooms = new List<Room>(); // Allows multiple boss rooms if needed

    void GenerateBossRoom(int _stage)
    {
        // Find deepest rooms
        int _depth = 0;
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].depth > _depth)
                _depth = rooms[i].depth;
        }

        // Randomly determine boss room
        List<Room> _bossDoorRooms = RoomsAtDepth(_depth, null);
        Room _bossDoorRoom = _bossDoorRooms[Random.Range(0, _bossDoorRooms.Count)];

        // Spawn boss door
        EC_Entity _bossDoor = SpawnEntity(gen.bossDoorPrefab, _bossDoorRoom);
        _bossDoor.GetComponent<EC_Door>().SetLocked(true);

        // Create boss room and store in list instead of single variable
        Room newBossRoom = CreateBossRoom(_stage, _depth + 1, _bossDoorRoom);
        newBossRoom.parentRoom.children.Add(newBossRoom);
        _bossDoor.GetComponent<EC_Door>().destination = newBossRoom;

        bossRooms.Add(newBossRoom); // Store boss room in the list

        // Randomly determine boss key room
        List<Room> _keyRooms = RoomsAtDepth(_depth, new List<Room>() { _bossDoorRoom, _bossDoorRoom.parentRoom });
        Room _keyRoom = _keyRooms[Random.Range(0, _keyRooms.Count)];

        EC_Entity _bossKey = SpawnEntity(gen.bossKeyPrefab, _keyRoom);
        _bossKey.GetComponent<EC_StageKey>().keyDoor = _bossDoor.GetComponent<EC_Door>();
    }


    /// <summary>
    /// Generates rooms for each door with no destination
    /// Iteratively generates dungeon layers by calling GenerateLayer(depth + 1)
    /// Stops when maxDepth is reached or no doors are left to be filled
    /// </summary>
    void GenerateLayer(int stage, int depth)
    {
        // Stop when reached max depth or no doors to fill
        if (depth > gen.maxDepth || doorsToFill.Count <= 0) return;

        // Copy doors list and clear old list
        List<EC_Door> _doorsToFill = new List<EC_Door>();
        for (int i = 0; i < doorsToFill.Count; i++)
            _doorsToFill.Add(doorsToFill[i]);
        doorsToFill.Clear();

        // Create new room for all empty doors
        for (int i = 0; i < _doorsToFill.Count; i++)
        {
            Room _parentRoom = _doorsToFill[i].GetComponent<EC_Entity>().room;
            Room _newRoom = CreateRoom(stage, depth, _parentRoom);
            _doorsToFill[i].destination = _newRoom;
            _parentRoom.children.Add(_newRoom);
        }

        // Generate next layer
        GenerateLayer(stage, depth + 1);
    }

    /// <summary>
    /// Handles logic for safely exiting current room, and entering next room
    /// </summary>
    public void SwitchRoom(Room nextRoom)
    {
        // Check if room exists
        if (nextRoom == null) return;

        // Exit current room
        currentRoom?.ExitRoom();

        // Enter next room
        currentRoom = nextRoom;
        currentRoom.EnterRoom();

        // Update grid
        gridLayout.Arrange(true);

        // Display
        mapRenderer.DisplayMap(rooms, currentRoom);

        //TESTPORTAL();
    }

    /// <summary>
    /// Creates room at depth, fills with doors and entities, returns newly created room
    /// </summary>
    Room CreateRoom(int _stage, int _depth, Room _parentRoom)
    {
        Room _room = new Room(_depth, _parentRoom);
        rooms.Add(_room);

        // If not starting room, spawn back door
        if (_depth > 0)
        {
            EC_Entity _back = SpawnEntity(gen.backDoorPrefab, _room);
            _back.GetComponent<EC_Door>().destination = _parentRoom;
        }
        // Spawn doors according to depth
        for (int i = 0; i < gen.maxDoors; i++)
        {
            if (Random.Range(0.0f, 1.0f) <= 1 - Mathf.Pow((float)_depth / (float)gen.maxDepth, gen.oddsPower))
            {
                EC_Entity _door = SpawnEntity(gen.doorPrefab, _room);
                doorsToFill.Add(_door.GetComponent<EC_Door>());
            }
        }
        // Add entities based on room depth
        if (_depth > 0)
        {
            List<GameObject> entities = gen.rooms[_stage].RandomRoom(_depth - 1).entities;
            for (int i = 0; i < entities.Count; i++)
                SpawnEntity(entities[i].gameObject, _room);
        }

        return _room;
    }

    /// <summary>
    /// Creates boss room
    /// </summary>
    Room CreateBossRoom(int _stage, int _depth, Room _parentRoom)
    {
        Room _room = new Room(_depth, _parentRoom, true);
        rooms.Add(_room);

        // Back door
        EC_Entity _back = SpawnEntity(gen.backDoorPrefab, _room);
        _back.GetComponent<EC_Door>().destination = _parentRoom;
        // Spawn boss room entities
        List<GameObject> _entities = gen.rooms[_stage].RandomBossRoom().entities;
        for (int i = 0; i < _entities.Count; i++)
            SpawnEntity(_entities[i].gameObject, _room);

        return _room;
    }

    /// <summary>
    /// Returns list of rooms at desired depth excluding list of excluded rooms, if no valid rooms exist search previous depth
    /// </summary>
    List<Room> RoomsAtDepth(int _depth, List<Room> _excluded)
    {
        // Invalid or zero depth reached, return starting room as default
        if (_depth <= 0)
            return new List<Room>() { rooms[0] };

        // Excluded list null error
        if (_excluded == null)
            _excluded = new List<Room>();

        // Get rooms at depth
        List<Room> _rooms = new List<Room>();
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].depth == _depth && !_excluded.Contains(rooms[i]))
                _rooms.Add(rooms[i]);
        }

        // No valid rooms found, search previous depth
        if (_rooms.Count <= 0)
            _rooms = RoomsAtDepth(_depth - 1, _excluded);

        return _rooms;
    }

    /// <summary>
    /// Spawns entity gameobject to scene given EC_Entity type prefab and room to spawn into, returns new created entity
    /// </summary>
    public EC_Entity SpawnEntity(GameObject entityToSpawn, Room room)
    {
        EC_Entity _entity = Instantiate(entityToSpawn, transform).GetComponent<EC_Entity>();
        room.roomEntities.Add(_entity);
        _entity.IsEnabled(false);
        _entity.room = room;
        return _entity;
    }

    public void SpawnPortal()
    {
        if (portalGenerated) return;

        EC_Entity portal = SpawnEntity(gen.portalPrefab, bossRoom);
        portal.IsEnabled(true);
        portalGenerated = true;
        gridLayout.Arrange();

        ArtifactManager.instance.TriggerBossDefeated();
    }

    public void TESTPORTAL()
    {
        if (portalGenerated) return;

        EC_Entity portal = SpawnEntity(gen.portalPrefab, rooms[0]);
        portal.IsEnabled(true);
        portalGenerated = true;
        gridLayout.Arrange();

        Player.instance.Wallet.AddMoney(200);
    }
}

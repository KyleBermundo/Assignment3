using benjohnson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonManager : Singleton<DungeonManager>
{
    public GenerationSettingsSO gen;

    public Room CurrentRoom => currentRoom;
    private Room currentRoom;
    private Room bossRoom;
    private List<Room> bossRooms = new List<Room>();
    private List<Room> rooms = new List<Room>();
    private List<EC_Door> doorsToFill = new List<EC_Door>();
    private bool portalGenerated = false;

    [Header("Components")]
    [SerializeField] private DungeonMapRenderer mapRenderer;
    [HideInInspector] public ArrangeGrid gridLayout;

    protected override void Awake()
    {
        base.Awake();
        gridLayout = GetComponent<ArrangeGrid>();
    }

    void Start()
    {
        GenerateDungeon(GameManager.instance.stage);
    }

    public void GenerateDungeon(int stage)
    {
        if (stage >= gen.rooms.Count)
        {
            GameManager.instance.LoadWinScreen();
            return;
        }

        CreateRoom(stage, 0, null);
        GenerateLayer(stage, 1);
        GenerateBossRooms(stage);
        SwitchRoom(rooms[0]);

        GameManager.instance.DungeonLoaded();
    }

    private void GenerateBossRooms(int stage, int bossRoomCount = 2)
    {
        int deepestDepth = rooms.Max(room => room.depth);
        List<int> usedDepths = new List<int>();

        for (int i = 0; i < bossRoomCount; i++)
        {
            int depthToUse = deepestDepth - i;
            if (depthToUse < 0) break;

            usedDepths.Add(depthToUse);
            var possibleBossRooms = RoomsAtDepth(depthToUse, null);
            if (possibleBossRooms.Count == 0) continue;

            Room bossDoorRoom = possibleBossRooms[Random.Range(0, possibleBossRooms.Count)];
            EC_Door bossDoor = SpawnEntity(gen.bossDoorPrefab, bossDoorRoom).GetComponent<EC_Door>();
            bossDoor.SetLocked(true);

            Room newBossRoom = CreateBossRoom(stage, depthToUse + 1, bossDoorRoom);
            newBossRoom.parentRoom.children.Add(newBossRoom);
            bossDoor.destination = newBossRoom;
            bossRooms.Add(newBossRoom);

            List<Room> keyRooms = RoomsAtDepth(depthToUse, new List<Room> { bossDoorRoom, bossDoorRoom.parentRoom });
            if (keyRooms.Count > 0)
            {
                Room keyRoom = keyRooms[Random.Range(0, keyRooms.Count)];
                EC_StageKey bossKey = SpawnEntity(gen.bossKeyPrefab, keyRoom).GetComponent<EC_StageKey>();
                bossKey.keyDoor = bossDoor;
            }
        }
    }

    private void GenerateLayer(int stage, int depth)
    {
        if (depth > gen.maxDepth || doorsToFill.Count == 0) return;

        List<EC_Door> pendingDoors = new List<EC_Door>(doorsToFill);
        doorsToFill.Clear();

        foreach (var door in pendingDoors)
        {
            Room parentRoom = door.GetComponent<EC_Entity>().room;
            Room newRoom = CreateRoom(stage, depth, parentRoom);
            door.destination = newRoom;
            parentRoom.children.Add(newRoom);
        }

        GenerateLayer(stage, depth + 1);
    }

    public void SwitchRoom(Room nextRoom)
    {
        if (nextRoom == null) return;

        currentRoom?.ExitRoom();
        currentRoom = nextRoom;
        currentRoom.EnterRoom();

        gridLayout.Arrange(true);
        mapRenderer.DisplayMap(rooms, currentRoom);
    }

    private Room CreateRoom(int stage, int depth, Room parentRoom)
    {
        Room room = new Room(depth, parentRoom);
        rooms.Add(room);

        if (depth > 0)
        {
            EC_Door backDoor = SpawnEntity(gen.backDoorPrefab, room).GetComponent<EC_Door>();
            backDoor.destination = parentRoom;
        }

        for (int i = 0; i < gen.maxDoors; i++)
        {
            if (Random.Range(0.0f, 1.0f) <= 1 - Mathf.Pow((float)depth / gen.maxDepth, gen.oddsPower))
            {
                doorsToFill.Add(SpawnEntity(gen.doorPrefab, room).GetComponent<EC_Door>());
            }
        }

        if (depth > 0)
        {
            foreach (var entity in gen.rooms[stage].RandomRoom(depth - 1).entities)
            {
                SpawnEntity(entity.gameObject, room);
            }
        }

        return room;
    }

    private Room CreateBossRoom(int stage, int depth, Room parentRoom)
    {
        Room room = new Room(depth, parentRoom, true);
        rooms.Add(room);

        EC_Door backDoor = SpawnEntity(gen.backDoorPrefab, room).GetComponent<EC_Door>();
        backDoor.destination = parentRoom;

        foreach (var entity in gen.rooms[stage].RandomBossRoom().entities)
        {
            SpawnEntity(entity.gameObject, room);
        }

        return room;
    }

    private List<Room> RoomsAtDepth(int depth, List<Room> excluded)
    {
        if (depth <= 0) return new List<Room> { rooms[0] };
        excluded ??= new List<Room>();

        List<Room> validRooms = rooms.Where(room => room.depth == depth && !excluded.Contains(room)).ToList();
        return validRooms.Count > 0 ? validRooms : RoomsAtDepth(depth - 1, excluded);
    }

    public EC_Entity SpawnEntity(GameObject entityPrefab, Room room)
    {
        EC_Entity entity = Instantiate(entityPrefab, transform).GetComponent<EC_Entity>();
        room.roomEntities.Add(entity);
        entity.IsEnabled(false);
        entity.room = room;
        return entity;
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


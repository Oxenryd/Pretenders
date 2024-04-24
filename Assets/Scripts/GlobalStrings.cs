public static class GlobalStrings
{
    // DEBUGS
    public const string NAME_RESULTSCREEN_DEBUG = "ResultScreenDebug";

    //NAMES
    public const string NAME_INPUTMANAGER = "InputManager";
    public const string NAME_GAMEMANAGER = "GameManager";
    public const string NAME_SCENEMANAGER = "SceneManager";
    public static string NAME_BOMBERCRATE = "BombCrate";
    public static string NAME_BOMBERMANWALL = "BombermanWall";
    public static string NAME_BOMBERGRID = "BombermanGrid";
    public static string NAME_BOMBERTREE = "BomberTree";
    public static string NAME_UIOVERLAY = "UiOverlay";

    // PreFabs
    public static string PREFAB_TUG = "TugPrefab";

    //CONTAINERS
    public const string CONT_HEROCONTAINER = "HeroContainer";
    public static string CONT_MISCCONTAINER = "MiscContainer";

    //LAYERS
    public const string LAYER_GROUND = "Ground";

    //INPUT/CONTROLS
    public const string INPUT_IGNORE = "";
    public const string INPUT_HEROMOVEMENT = "HeroMovement";
    public const string INPUT_AI_HEROMOVEMENT = "AiHeroMovement";

    // Action names
    public const string INPUT_MOVE = "Move";
    public const string INPUT_MOVE_JUMP = "Jump";
    public const string INPUT_MOVE_GRAB = "Grab";
    public const string INPUT_MOVE_TRIGGER = "Trigger";
    public const string INPUT_MOVE_PUSH = "Push";

    //Error msg
    public const string ERR_NUMBER_OF_PLAYERS1 = "Number of players can not be lower than 0. Forced to 0.";
    public const string ERR_NUMBER_OF_PLAYERS2 = "Number of players can not be higher than 4. Forced to 4.";

    //Tags
    public const string PLATFORM_TAG = "Platform";
    public const string CHARACTER_TAG = "Character";
    public const string TRANSITIONS_TAG = "Transitions";

    //Colliders
    public const string HERO_BODY_COLLIDER_TAG = "BodyCollider";
    public const string HERO_STOMP_COLLIDER_TAG = "StompCollider";

    //Scenes
    public const string SCENE_LOADINGSCREEN = "LoadingScreen";
    public const string SCENE_LOBBY = "Lobby";
    public const string SCENE_RESULTS = "ResultScreen";
    public const string SCENE_PRAJSPAL = "PricePall";

    // Start menu
    public const string START_MSG0 = "From the within the grand laboratories of Department-63";
    public const string START_MSG1 = "SynthMed Solutions presents entertainment like no other\n where particpants think they are heroes,";
    public const string START_MSG2 = "but the rest of us know that in reality,\nthey are nothing but....";



    // GAMES
    public static string[] MATCHES_NAMES =
    { 
        "HenriksForceFeeder", "BomberManV1", "HecticTestScene"
    };
    public static string[] MINIGAMES_NAMES =
    {
        "SetPath", "FallingBlocks"
    };
    public const string TOUR_IDENTIFIER = "TOUR";
}

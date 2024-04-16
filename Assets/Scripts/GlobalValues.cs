public static class GlobalValues
{
    public const float SHOVE_HEIGHT_BUMP_TOPDOWN = 3f;
    public const float SHOVE_OFFENDCOL_DIS_DUR = 0.1f;
    public const float SHOVE_DEFAULT_SHOVEPOWER = 22f;
    public const float SHOVE_DAMPING_MULTIPLIER = 0.95f;

    public const int   BOMBS_MAXBOMBS = 10;
    public const int   BOMBS_MAXEXPLOSIONSPERDIRECTION = 10;

    public const float JUMPDIRECTION_SLOWDOWN_DOT = 0.71f;
    public const float JUMPDIRECTION_SLOWDOWN_MULTIPLIER = 0.3f;

    public const float CHAR_BUMP_DOT_LIMIT = -0.7f;
    public const float CHAR_BUMPFORCE = 2f;
    public const float CHAR_BUMPDURATION = 0.28f;

    public const int   TRAY_MAX_SIZE = 5;
    public const int   WINNING_POINTS_FORCE_FEEDER = 30;
    public const float CHAR_GRAB_CHECK_DISTANCE = 1f;
    public const float CHAR_GRAB_POSITION_OFFSET = 0.5f;
    public const float CHAR_GRAB_CYLINDER_COLLIDER_Y_OFFSET = 0f;
    public const float CHAR_GRAB_RADIUS = 0.5f;
    public const float CHAR_GRAB_PICKUPTIME = 0.3f;
    public const float CHAR_GRAB_DROPFORCE = 6f;
    public const float CHAR_GRAB_RADIUS_DEFAULT_TIMETOGRAB = 0.3f;
    public const float CHAR_GRAB_TIMEOUT = 0.5f;

    public const float CHAR_MOVEMENT_GRIDTARGET_EPSILON = 0.15f;

    public const float CHAR_PUSH_CHALLENGE_TIME = 1f;
    public const float CHAR_PUSH_FAILED_STUN_TIME = 1.5f;
    public const float CHAR_PUSH_SPEED = 1.75f;
    public const float CHAR_PUSH_POWER = 6.5f;
    public const float CHAR_PUSH_MIN_DISTANCE = 1.25f;
    public const float CHAR_PUSH_PUSHED_TIME = 1.25f;

    public const float CHAR_DRAG_SPEED_MULTIPLIER = 0.6f;
    public const float CHAR_DRAG_DRAGGER_DECREASE = 0.03f;
    public const float CHAR_DRAG_DRAGGED_INCREASE = 0.06f;
    public const float CHAR_DRAG_DRAGGED_COOLDOWN = 1.2f;
    public const float CHAR_DRAG_DOT_MIN = 0.4f;

    public const float CHAR_STRUGGLE_MAX_TIME = 6f;
    public const float CHAR_KEEP_GROUNDED_AFTER_LANDING_TIME = 0.2f;

    public const float TUG_DEFAULT_TUGPOWER = 0.09f;
    public const float TUG_MAX_TUG_TIME = 8f;
    public const float TUG_TIME_MULTIPLIER = 8f;
    public const float TUG_DIRECTION_DOT_LIMIT = -0.8f;

    public const float GRABBABLE_COLLIDER_TIMEOUT_DEFAULTTIME = 0.3f;
    public const float GRABBABLE_MAX_STORED_VELOCITY_MAGNITUDE = 10f;
    public const float GRABBABLE_DEFAULT_MAX_DETACH_POWER = 10f;

    public const int   PLATFORM_LAYER = 8;
    public const int   OBJECTS_LAYER = 22;
    public const int   GROUND_LAYER = 10;
    public const int   WALL_LAYER = 9;
    public const int   GROUNDABLE_LAYER = 23;

    public const int   STRINGS_MAX_PRECACHED_NUMBERSTRINGS = 10000;

    public const float SCENE_CIRCLETRANSIT_TIME = 1f;

    public static float[] TOURNAMENT_SCORES = new float[] { 6f, 4f, 2f, 1f };


}
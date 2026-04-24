namespace SpriteAtlasForge.Core.Models;

public enum GridGroupType
{
    Character,
    Enemy,
    Boss,
    Tile,
    Parallax,
    Item,
    Effect,
    UI,
    Animal,
    Decoration,
    Projectile
}

public enum TileKind
{
    GroundLeft,
    GroundCenter,
    GroundRight,
    GroundSingle,
    CornerLeft,
    CornerRight,
    SlopeLeft,
    SlopeRight,
    Decoration,
    Hazard,
    Platform
}

public enum AnimationType
{
    Idle,
    Walk,
    Run,
    Jump,
    Fall,
    Attack1,
    Attack2,
    Attack3,
    Hurt,
    Death,
    Special
}

using SpriteAtlasForge.Core.Models;
using System;
using System.Linq;

namespace SpriteAtlasForge.App.Converters;

public static class EnumHelper
{
    public static Array GetGridGroupTypes() => Enum.GetValues(typeof(GridGroupType));
    public static Array GetAnimationTypes() => Enum.GetValues(typeof(AnimationType));
    public static Array GetTileKinds() => Enum.GetValues(typeof(TileKind));
}

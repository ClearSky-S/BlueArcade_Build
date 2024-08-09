using UnityEngine;

namespace BlueArcade
{
    public static class LayerManager
    {
        public const int Player = 3;
        public const int Platform = 6;
        public const int Enemy = 7;
        
        public static LayerMask PlayerMask =>1 << Player;
        public static LayerMask PlatformMask => 1 << Platform;
        public static LayerMask EnemyMask => 1 << Enemy;
    }
}
using System;
using GameCore;

namespace GameData
{
    public enum EnemyType : byte
    {
        Soldier,
        Archer,
        Knight,
    }

    public class AI : IDataUnit
    {
        public string Signature => "55b3e56b-9803-4a45-ab6b-1e739e9162cd";
        public CurveDataFile ReactionCurve = new("AI\\ReactionCurve.curve");
        public string Description = "This is AI data";

        public Enemy[] BlueprintsAsArray = { new Enemy(), new Enemy() };
        public List<Enemy> BlueprintsAsList = new() { new Enemy(), new Enemy() };
    }

    public class Enemy
    {
        public EnemyType EnemyType = EnemyType.Soldier;
        public short Health = 100;
        public float Speed = 1.1f;
        public float Aggresiveness = 0.8f;
        public bool IsAggressive = true;
        public bool WillFollowPlayer = true;
        public bool WillCallReinforcements = false;
    }

}

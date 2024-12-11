using System;

namespace GameData
{
        public enum EEnemyType
        {
            Soldier,
            Archer,
            Knight,
        }

        public class AI : IDataUnit
        {
            public string UnitId { get; } = "AI-56e889c7-1051-4147-9544-c37ee7bc927e";

            public CurveDataFile ReactionCurve = new("AI\\ReactionCurve.curve");
            public string Description = "This is AI data";

            public Enemy[] BlueprintsAsArray = { new Enemy() };
            public List<Enemy> BlueprintsAsList = new() { new Enemy() };
        }

        public class Enemy
        {
            public EEnemyType EnemyType = EEnemyType.Soldier;
            public float Speed = 1.1f;
            public float Aggresiveness = 0.8f;
        }

}

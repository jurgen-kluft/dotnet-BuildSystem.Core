using System;

namespace GameData
{
	namespace AI
	{
		public enum EEnemyType
		{
			Soldier,
			Archer,
			Knight,
		}

		public class AI : IDataUnit
		{
			public string Name { get { return "AI"; } }
			public CurveFile ReactionCurve = new ("AI\\ReactionCurve.curve");
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
}

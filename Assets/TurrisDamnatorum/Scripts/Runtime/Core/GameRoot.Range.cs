using System.Collections.Generic;
using UnityEngine;

namespace Turris
{
    /// <summary>Poligon umiejętności: osobne „piętro” bez fal i bez zapisu postępów (jak próba wroga).</summary>
    public partial class GameRoot
    {
        /// <summary>Aktywny poligon (null poza nim).</summary>
        public SkillRange Range { get; private set; }
        FloorDefinition pendingRangeFloor;

        public bool InSkillRange => Range != null;

        /// <summary>Start poligonu klasą bohatera (rodzina klasy wyznacza listę umiejętności).</summary>
        public void StartSkillRange(ClassDefinition hero)
        {
            Range?.End();
            Range = null;
            var plan = Meta.LastPlan();
            if (hero != null && plan.classDef != hero) Meta.SelectClass(plan, hero);
            plan.trialEnemy = null;
            pendingRangeFloor = new FloorDefinition { name = "Poligon umiejętności", arena = config.tower.floors[0].arena, enemies = new List<EnemyDefinition>() };
            StartRun(plan);
            pendingRangeFloor = null;
            Range = new SkillRange(this, plan.classDef);
            Range.Begin();
        }

        /// <summary>Manekin na poligonie: wróg z katalogu bez AI, z mnożnikiem zdrowia.</summary>
        internal EnemyBrain SpawnRangeDummy(EnemyDefinition def, Vector3 spot, float health)
        {
            if (def == null) return null;
            var e = WorldBuilder.CreateEnemy(def, spot, world);
            Vector3 toPlayer = Player.transform.position - spot; toPlayer.y = 0;
            if (toPlayer.sqrMagnitude > 0.01f) e.transform.rotation = Quaternion.LookRotation(toPlayer);
            e.Setup(def, Player, health, Run.difficulty, false, config.balance);
            e.TrainingDummy = true;
            e.DummySpot = spot;
            enemies.Add(e);
            return e;
        }

        internal void RemoveRangeEnemy(EnemyBrain e) => enemies.Remove(e);
    }
}

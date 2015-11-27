using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;
using Ensage.Common.Extensions;
using System.Threading;
using Ensage.Common;


namespace TehPucuk
{
    internal class Program
    {

        private static bool ownTowers = true;
        private static bool enemyTowers = true;
        private static bool jarSer = true;
        private static ParticleEffect rangeDisplay;
        private static float lastRange;
        private static Hero me;
        private static readonly Menu Menu = new Menu("Display", "towerRange", true);
        private static readonly Dictionary<Unit, ParticleEffect> Efek = new Dictionary<Unit, ParticleEffect>();
        private static readonly List<ParticleEffect> Effects = new List<ParticleEffect>(); // keep references
        private static void Main()
        {
            var ally = new MenuItem("ownTowers", "Range of allied towers").SetValue(true);
            var enemy = new MenuItem("enemyTowers", "Range of enemy towers").SetValue(true);
            var jarak = new MenuItem("jarSer", "Range of hero attack").SetValue(true);

            ownTowers = ally.GetValue<bool>();
            enemyTowers = enemy.GetValue<bool>();
            jarSer = jarak.GetValue<bool>();

            ally.ValueChanged += MenuItem_ValueChanged;
            enemy.ValueChanged += MenuItem_ValueChanged;
            jarak.ValueChanged += MenuItem_ValueChanged;

            Menu.AddItem(ally);
            Menu.AddItem(enemy);
            Menu.AddItem(jarak);

            Menu.AddToMainMenu();

            DisplayRange();
            Game.OnFireEvent += Game_OnFireEvent;
        }

        // ReSharper disable once InconsistentNaming
        private static void MenuItem_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            var item = sender as MenuItem;

            // ReSharper disable once PossibleNullReferenceException
            if (item.Name == "ownTowers") ownTowers = e.GetNewValue<bool>();
            else enemyTowers = e.GetNewValue<bool>();

            DisplayRange();
        }

        private static void Game_OnFireEvent(FireEventEventArgs args)
        {
            if (args.GameEvent.Name == "dota_game_state_change")
            {
                var state = (GameState)args.GameEvent.GetInt("new_state");
                if (state == GameState.Started || state == GameState.Prestart)
                    DisplayRange();
            }




            //Aura Keliatan di map
            static bool spam = false;
            var player = ObjectMgr.LocalPlayer;
            var units = ObjectMgr.GetEntities<Unit>().Where(
            x =>
            (x.ClassID != ClassID.CDOTA_BaseNPC_Creep_Lane) && x.Team == player.Team).ToList();
            var playerkita = ObjectMgr.GetEntities<Hero>().Where(
                y =>
                (y.Team == player.Team && y.IsInvisible()&& y.IsAlive));
            foreach (var unit in units)
            {
                HandleEffect(unit);
            }
            foreach (var kita in playerkita)
            {
                cekinvis(kita, spam);
            }
        }

        static void cekinvis(Hero kita, bool spam)
        {
            if (kita.IsInvisible() && kita.IsVisibleToEnemies && spam == false)
            {
                Game.ExecuteCommand("say_team " + kita.Name + " keliatan");
                spam = true;
            }
            else if (!kita.IsVisibleToEnemies)
                spam =false;
            else
                spam =  true;
      
        }
        static void HandleEffect(Unit unit)
        {
            if (unit.IsVisibleToEnemies && unit.IsAlive)
            {
                ParticleEffect effect;
                if (!Efek.TryGetValue(unit, out effect))
                {
                    effect = unit.AddParticleEffect("particles/items_fx/aura_shivas.vpcf");
                    Efek.Add(unit, effect);
                }
            }
            else
            {
                ParticleEffect effect;
                if (Efek.TryGetValue(unit, out effect))
                {
                    effect.Dispose();
                    Efek.Remove(unit);
                }
            }
        }



        private static void DisplayRange()
        {
            if (!Game.IsInGame)
                return;

            foreach (var e in Effects)
            {
                e.Dispose();
            }
            Effects.Clear();
            me = ObjectMgr.LocalHero;
            var player = ObjectMgr.LocalPlayer;
            rangeDisplay = null;
            if (player == null)
                return;
            var towers =
                ObjectMgr.GetEntities<Building>()
                    .Where(x => x.IsAlive && x.ClassID == ClassID.CDOTA_BaseNPC_Tower)
                    .ToList();
            if (!towers.Any())
                return;
            if (rangeDisplay == null)
            {
                rangeDisplay = me.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                lastRange = me.GetAttackRange() + me.HullRadius + 25;
                rangeDisplay.SetControlPoint(1, new Vector3(lastRange, 0, 0));
            }
            else
            {
                if (lastRange != (me.GetAttackRange() + me.HullRadius + 25))
                {
                    lastRange = me.GetAttackRange() + me.HullRadius + 25;
                    rangeDisplay.Dispose();
                    rangeDisplay = me.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                    rangeDisplay.SetControlPoint(1, new Vector3(lastRange, 0, 0));
                }
            }
            if (player.Team == Team.Observer)
            {
                foreach (var effect in towers.Select(tower => tower.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf")))
                {
                    effect.SetControlPoint(1, new Vector3(850, 0, 0));
                    Effects.Add(effect);
                }
            }
            else
            {
                if (enemyTowers)
                {
                    foreach (var effect in towers.Where(x => x.Team != player.Team).Select(tower => tower.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf")))
                    {
                        effect.SetControlPoint(1, new Vector3(850, 0, 0));
                        Effects.Add(effect);
                    }
                }
                if (ownTowers)
                {
                    foreach (var effect in towers.Where(x => x.Team == player.Team).Select(tower => tower.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf")))
                    {
                        effect.SetControlPoint(1, new Vector3(850, 0, 0));
                        Effects.Add(effect);
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using SharpDX;
using Ensage.Common.Menu;
using Ensage.Common.Extensions;


namespace Teh_Pucuk
{
    class Program
    {
        private static bool towerku = true;
        private static bool towermu = true;
        private static bool jarakku = true;
        private static bool _loaded;
        private static float maxjarak;
        private static Hero gue;
        private static ParticleEffect rangeDisplay;
        private static readonly Menu menu = new Menu("Display", "display", true);
        private static readonly Dictionary<Unit, ParticleEffect> Efek = new Dictionary<Unit, ParticleEffect>();
        private static readonly List<ParticleEffect> Effects = new List<ParticleEffect>(); // keep references

        
        static void Main(string[] args)
        {
            var punyakita = new MenuItem("Tower Kita", "Range of allied towers").SetValue(true);
            var punyamereka = new MenuItem("Tower Musuh", "Range of enemy towers").SetValue(true);
            var jarak = new MenuItem("Jarak Serang", "Range of hero attack").SetValue(true);

            menu.AddItem(punyakita);
            menu.AddItem(punyamereka);
            menu.AddItem(jarak);
            menu.AddToMainMenu();

            towerku = punyakita.GetValue<bool>();
            towermu = punyamereka.GetValue<bool>();
            jarakku = jarak.GetValue<bool>();

            punyakita.ValueChanged += MenuItem_ValueChanged;
            punyamereka.ValueChanged += MenuItem_ValueChanged;
            jarak.ValueChanged += MenuItem_ValueChanged;

            Game.OnUpdate += Game_OnUpdate;
            _loaded = false;
        }


        private static void MenuItem_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            var item = sender as MenuItem;
            if (item.Name == "Tower Kita") towerku = e.GetNewValue<bool>();
            else if (item.Name == "Tower Musuh") towermu = e.GetNewValue<bool>();
            else jarakku = e.GetNewValue<bool>();

            LiatJarak();
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            var gue = ObjectMgr.LocalHero;
            if (!_loaded)
            {
                if (!Game.IsInGame || gue == null)
                {
                    return;
                }
                LiatJarak();
                keliatan();
                _loaded = true;
                Game.PrintMessage("<font face='Comic Sans MS, cursive'><font color='#00aaff'>Teh Pucuk</font>", MessageType.ChatMessage);
            }
            if (!Game.IsInGame || gue == null)
            {
                _loaded = false;
                Game.PrintMessage("<font face='Comic Sans MS, cursive'><font color='#00aaff'>Teh Pucuk Mati</font>", MessageType.ChatMessage);
                return;
            }
        }

        private static void LiatJarak()
        {
            if (!Game.IsInGame)
                return;

            foreach (var e in Effects)
            {
                e.Dispose();
            }
            rangeDisplay.Dispose();
            Effects.Clear();
            gue = ObjectMgr.LocalHero;
            var towers = ObjectMgr.GetEntities<Building>().Where(x => x.IsAlive && x.ClassID == ClassID.CDOTA_BaseNPC_Tower).ToList();
            var player = ObjectMgr.LocalPlayer;
            if (towermu)
            {
                foreach (var effect in towers.Where(x => x.Team != player.Team).Select(tower => tower.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf")))
                {
                    effect.SetControlPoint(1, new Vector3(850, 0, 0));
                    Effects.Add(effect);
                }
            }
            if (towerku)
            {
               foreach (var effect in towers.Where(x => x.Team == player.Team).Select(tower => tower.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf")))
               {
                    effect.SetControlPoint(1, new Vector3(850,0,0));
                    Effects.Add(effect);
               }
            }
            if (jarakku)
            {
                if (rangeDisplay == null)
                {
                    rangeDisplay = gue.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                    maxjarak = gue.GetAttackRange() + gue.HullRadius + 25;
                    rangeDisplay.SetControlPoint(1, new Vector3(maxjarak, 0, 0));
                }
                else if (maxjarak != gue.GetAttackRange() + gue.HullRadius + 25)
                {
                    rangeDisplay.Dispose();
                    maxjarak = gue.GetAttackRange() + gue.HullRadius + 25;
                    rangeDisplay = gue.AddParticleEffect(@"particles\ui_mouseactions\range_display.vpcf");
                    rangeDisplay.SetControlPoint(1, new Vector3(maxjarak, 0, 0));
                }
            }
            else
                rangeDisplay = null;
        }

        private static void keliatan()
        {
            bool spam = false;
            var player = ObjectMgr.LocalPlayer;
            var playerkita = ObjectMgr.GetEntities<Hero>().Where(y => y.Team == player.Team).ToList();
            var units = ObjectMgr.GetEntities<Unit>().Where(x => x.ClassID != ClassID.CDOTA_BaseNPC_Creep_Lane && x.Team == player.Team).ToList();

            foreach (var unit in units)
            {
                ParticleEffect effect;
                effect = unit.AddParticleEffect("particles/items_fx/aura_shivas.vpcf");
                if (unit.IsAlive && unit.IsVisibleToEnemies)
                {
                    if (!Efek.TryGetValue(unit, out effect))
                    {
                        Efek.Add(unit, effect);
                    }
                }
                else
                {
                    if (Efek.TryGetValue(unit, out effect))
                    {
                        effect.Dispose();
                        Efek.Remove(unit);
                    }
                }
            }
            foreach (var kita in playerkita)
            {
                if (kita.IsAlive && kita.IsVisibleToEnemies && kita.IsInvisible() && !spam)
                {
                    Game.ExecuteCommand("say_team " + kita.Name + " alias " + kita.NetworkName + " keliatan ");
                    spam = true;
                }
                if (kita.IsAlive && !kita.IsVisibleToEnemies && kita.IsInvisible() && !spam)
                {
                    spam = false; 
                }
            }
        }

    }
}

using System;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using System.Globalization;

namespace Celeste.Mod.EmoteMod
{
    internal class Stretcher
    {
        public static float x_stretch = 1;
        public static float y_stretch = 1;

        public static bool stretch_lock = false;

        private static Player player;

        public static ILHook OnPlayerUpdateSpriteHook;
        private static List<ILHook> ScaleHooks;

        internal static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);

            // player = self;
            //
            // if (stretch_lock)
            // {
            //     if (MadhuntNerf.inRound)
            //     {
            //         self.Sprite.Scale.X = x_stretch >= 0 ? 1 : -1;
            //         self.Sprite.Scale.Y = 1;
            //         self.Hair.Sprite.Scale.X = x_stretch >= 0 ? 1 : -1;
            //         self.Hair.Sprite.Scale.Y = 1;
            //     }
            //     else
            //     {
            //         self.Sprite.Scale.X = x_stretch;
            //         self.Sprite.Scale.Y = y_stretch;
            //         self.Hair.Sprite.Scale.X = x_stretch;
            //         self.Hair.Sprite.Scale.Y = y_stretch;
            //     }
            // }
        }

        public static void stretch_x(float x)
        {
            // player.Sprite.Scale.X = x;
            // player.Hair.Sprite.Scale.X = x;
            x_stretch = x;
        }
        public static void stretch_y(float y)
        {
            // player.Sprite.Scale.Y = y;
            // player.Hair.Sprite.Scale.Y = y;
            y_stretch = y;
        }

        internal static void lock_stretch()
        {
            stretch_lock = !stretch_lock;
            x_stretch = 1;
            y_stretch = 1;
        }

        internal static void Load()
        {
            On.Celeste.Player.Update += Player_Update;

            bool patched = Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "Everest", Version = new(1, 1432) }); // stolen from ex variants
            OnPlayerUpdateSpriteHook = new(typeof(Player).GetMethod(patched ? "orig_UpdateSprite" : "UpdateSprite", BindingFlags.Instance | BindingFlags.NonPublic), Player_UpdateSprite);

            // Jump, SuperJump, SuperWallJump, Bounce, SuperBounce, SideBounce, NormalUpdate x3, CasetteFlyCoroutine x2, IntroRespawnBegin, orig_Update, orig_WallJump
            ScaleHooks = new();
            List<MethodInfo> player_methods = new();
            string[] vector_public = { "Jump", "Bounce", "SuperBounce", "SideBounce", "orig_Update" };
            string[] vector_private = { "SuperJump", "SuperWallJump", "NormalUpdate", "CassetteFlyCoroutine", "IntroRespawnBegin", "orig_WallJump" };
            string[] xy_private = { "OnCollideV", "NormalUpdate" };

            //private
            foreach (string m in vector_private)
            {
                MethodInfo info = typeof(Player).GetMethod(m, BindingFlags.NonPublic | BindingFlags.Instance);
                player_methods.Add(info);
            }
            foreach (string m in xy_private)
            {
                MethodInfo info = typeof(Player).GetMethod(m, BindingFlags.NonPublic | BindingFlags.Instance);
                ScaleHooks.Add(new(info, FindAndReplaceScaleXY));
            }

            // public
            foreach (string m in vector_public)
            {
                MethodInfo info = typeof(Player).GetMethod(m, BindingFlags.Public | BindingFlags.Instance);
                player_methods.Add(info);
            }

            foreach (MethodInfo i in player_methods)
                try
                {
                    ScaleHooks.Add(new(i, FindAndReplaceScale));
                }
                catch (Exception e)
                {
                    DebugCommands.ass += $"ERROR ON '{i.Name}: {e}";

                }
        }
        private static void Player_UpdateSprite(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(1f)))
            {
                cursor.EmitDelegate<Func<float>>(() => x_stretch);
                cursor.EmitMul();
            }
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(1f)))
            {
                cursor.EmitDelegate<Func<float>>(() => y_stretch);
                cursor.EmitMul();
            }
        }

        private static void FindAndReplaceScale(ILContext il)
        {
            ILCursor cursor = new(il);

            while (cursor.TryGotoNext(MoveType.Before, instr => instr.Match(OpCodes.Stfld)))
            {
                string name = Convert.ToString(cursor.Next.Operand, CultureInfo.InvariantCulture);
                // DebugCommands.ass += name + "\n";
                if (!name.Contains("Scale"))
                    continue;

                cursor.Index -= 2;
                // DebugCommands.ass += $"multiplying after '{cursor.Prev}' before '{cursor.Next}'";
                if (cursor.Prev.OpCode == OpCodes.Ldc_R4)
                    cursor.EmitDelegate<Func<float, float>>((orig) => orig * x_stretch);
                cursor.Index++;
                // DebugCommands.ass += $"multiplying after '{cursor.Prev}' before '{cursor.Next}'";
                if (cursor.Prev.OpCode == OpCodes.Ldc_R4)
                    cursor.EmitDelegate<Func<float, float>>((orig) => orig * y_stretch);
                cursor.Index++;
                cursor.Index++;
            }

        }

        private static void FindAndReplaceScaleXY(ILContext il)
        {
            ILCursor cursor = new(il);

            // X
            // while (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchStfld<Vector2>("X")))
            while (cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Ldflda)))
            {
                string name = Convert.ToString(cursor.Prev.Operand, CultureInfo.InvariantCulture);
                if (!name.Contains("Scale"))
                    continue;
                int index = cursor.Index;

                cursor.GotoNext(MoveType.Before, instr => instr.MatchStfld<Vector2>("X"));
                cursor.EmitDelegate<Func<float, float>>((orig) => orig * x_stretch);
                cursor.Index = index;
                cursor.GotoNext(MoveType.Before, instr => instr.MatchStfld<Vector2>("Y"));
                cursor.EmitDelegate<Func<float, float>>((orig) => orig * y_stretch);
            }

            // Y
            // while (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchStfld<Vector2>("Y")))
            // {
            //     cursor.EmitDelegate<Func<float, float>>((orig) => orig * y_stretch);
            //     cursor.Index++;
            // }
        }

        internal static void Unload()
        {
            On.Celeste.Player.Update -= Player_Update;
        }
    }
}

using System;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using System.Globalization;
using Celeste.Mod;
using Celeste;
using EmoteMod.Module;

using static EmoteMod.Features.Stretch;

namespace EmoteMod.Hooks
{
    internal class StretchHooks
    {
        public static ILHook OnPlayerUpdateSpriteHook;
        private static List<ILHook> ScaleHooks;

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

        internal static void Load()
        {
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

        internal static void Unload()
        {
            // TODO: dispose of dynamic hooks
        }
    }
}

using Monocle;
using MonoMod.Utils;
using System;
using System.Linq;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections;
using System.Reflection;
using System.Collections;
using System;

using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.CelesteNet.Client.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.CelesteNet.DataTypes;
namespace Celeste.Mod.EmoteMod
{
    public class Speed
    {
        private static Player player;

        // speed multipliers
        public static float[] speeds = { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1, 2, 4, 5, 6, 10, 20, 30, 40, 50, 60, 70, 100 };
        /// <summary>
        /// current animation delay
        /// </summary>
        // public static float currentDelay;
        // public static bool speedChanged;
        internal static string changedAnimaitonID = null;
        internal static float delayBeforeChange;
        internal static string spritebank;


        // private static ILHook getDelayHook = null;

        // internal static string test = "";

        //speed formatter
        public static Func<int, string> speedFormatter = arg =>
        {
            return $"{speeds[arg]}x";
        };

        // public static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        // {
        //     orig(self);
        //
        //     player = self;
        // }
        //
        // public static void Player_UpdateSprite(On.Celeste.Player.orig_UpdateSprite orig, Player self)
        // {
        //     orig(self);
        //
        //
        // }

        internal static void SetSpeed()
        {
            if (PlayerHelper.GetPlayer() == null)
            {
                return;
            }
            Sprite sp = PlayerHelper.GetPlayer().Sprite;
            int sm = ((int)PlayerHelper.GetPlayer().Sprite.Mode);
            DynData<Sprite> data = new DynData<Sprite>(sp);
            spritebank = new List<string>() { "player", "player_no_backpack", "badeline", "player_badeline", "player_playback" }[sm];
            EmoteModModule.echo($"spritebank: {spritebank}");

            ResetSpeed();

            delayBeforeChange = sp.Animations[sp.CurrentAnimationID].Delay;
            changedAnimaitonID = sp.CurrentAnimationID;

            float delay = delayBeforeChange / speeds[EmoteModModule.Settings.AnimationSpeed];

            (data["currentAnimation"] as Sprite.Animation).Delay = delay;
            EmoteModModule.echo($"changed sprite speed on {sp.CurrentAnimationID}: {delayBeforeChange} -> {delay}");
            Dictionary<string, Sprite.Animation> madeline_bp = GFX.SpriteBank.SpriteData["player"].Sprite.Animations;

            if (madeline_bp.ContainsKey(changedAnimaitonID))
            {
                EmoteModModule.echo($"delay in sprite data: {madeline_bp[changedAnimaitonID].Delay}");
            }
        }

        internal static void ResetSpeed()
        {
            if (changedAnimaitonID != null)
            {
                // try
                // {
                if (!GFX.SpriteBank.SpriteData[spritebank].Sprite.Animations.ContainsKey(changedAnimaitonID))
                    return;

                Sprite sp = PlayerHelper.GetPlayer().Sprite;
                // DynData<Sprite> data = new DynData<Sprite>(sp);

                EmoteModModule.echo($"resetting delay on {sp.CurrentAnimationID}: {sp.Animations[changedAnimaitonID].Delay} -> {delayBeforeChange}");

                // sp.Animations[changedAnimaitonID].Delay = delayBeforeChange;
                GFX.SpriteBank.SpriteData[spritebank].Sprite.Animations[changedAnimaitonID].Delay = delayBeforeChange;

                changedAnimaitonID = null;
                // }
                // catch (Exception e)
                // {
                //     Logger.Log(LogLevel.Warn, "[EmoteMod]", $"Could not reset delay on '{changedAnimaitonID}': {e}");
                //     EmoteModModule.echo($"Could not reset delay on '{changedAnimaitonID}': {e}");
                // }
            }


        }

        // internal static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        // {
        //     //speed changing
        //     if (EmoteModModule.anim_by_game == 1)
        //     {
        //
        //         // TODO: why do allat
        //         // foreach (Entity entity in self.Entities)
        //         // {
        //         //     foreach (Sprite sprite in entity.Components.GetAll<Sprite>())
        //         //     {
        //         //         if (sprite == player.Sprite && sprite.Animating)
        //         //         {
        //         //             DynData<Sprite> data = new DynData<Sprite>(sprite);
        //         //             if (EmoteModModule.Settings.AnimationSpeed != 9)
        //         //                 (data["currentAnimation"] as Sprite.Animation).Delay = currentDelay /
        //         //                         speeds[EmoteModModule.Settings.AnimationSpeed];
        //         //             else
        //         //                 (data["currentAnimation"] as Sprite.Animation).Delay = sprite.Animations[sprite.CurrentAnimationID].Delay;
        //         //         }
        //         //     }
        //         // }
        //
        //
        //     }
        //     orig(self);
        // }

        // private static void OnGetDelay(ILContext il)
        // {
        //     ILCursor c = new(il);
        //     if (c.TryGotoNext(MoveType.Before, instr => instr.Match(OpCodes.Ret)))
        //     {
        //         c.EmitDelegate<Func<float, float>>((orig) =>
        //         {
        //             EmoteModModule.echo("tryna read delay");
        //             return orig * speeds[EmoteModModule.Settings.AnimationSpeed];
        //         });
        //     }
        // }

        internal static void Load()
        {
            // On.Celeste.Player.Update += Player_Update;
            // On.Celeste.Level.Update += Level_Update;
            // On.Celeste.Player.UpdateSprite += Player_UpdateSprite;
            // PlayerHelper.GetPlayer().Sprite.Animations[0].Delay
            // getDelayHook = new(typeof(Sprite.Animation).GetProperty("Delay").GetGetMethod(), OnGetDelay);
            // foreach (FieldInfo p in typeof(Sprite.Animation).GetFields())
            // {
            //     // PlayerHelper.GetPlayer().Sprite
            //     // p.get
            //     // test += p.Name + "\n";
            // }
            // typeof(Sprite.Animation).GetProperty("Delay").GetGetMethod();
        }
        internal static void Unload()
        {
            // On.Celeste.Player.Update -= Player_Update;
            // On.Celeste.Level.Update -= Level_Update;
        }
    }
}

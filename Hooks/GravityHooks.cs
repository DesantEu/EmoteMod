using Celeste;
using EmoteMod.Module;

using static EmoteMod.Features.Gravity;

namespace EmoteMod.Hooks
{
    public static class GravityHooks
    {


        public static void Load()
        {
            On.Celeste.Player.Update += Player_Update;
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);

            if (EmoteModModule.Settings.CancelGravity && EmoteModModule.anim_by_game == 1)
                self.Y = playerY;
        }

        internal static void Unload()
        {
            On.Celeste.Player.Update -= Player_Update;
        }
    }

}

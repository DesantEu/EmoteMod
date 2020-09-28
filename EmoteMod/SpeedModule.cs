using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.EmoteMod
{
    public class SpeedModule
    {
        private static Player player;
        public static bool speedChanged;
        public static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);

            player = self;
        }

        internal static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        {
            //EmoteModMain.echo($"settings: {EmoteModMain.Settings.AnimationSpeed}, value: {EmoteModMain.speeds[EmoteModMain.Settings.AnimationSpeed]}");
            //speed changing
            if (EmoteModMain.anim_by_game == 1)
            {
                foreach (Entity entity in self.Entities)
                {   
                    foreach (Sprite sprite in entity.Components.GetAll<Sprite>())
                    {
                        if (sprite == player.Sprite && sprite.Animating)
                        {
                            DynData<Sprite> data = new DynData<Sprite>(sprite);
                            if (EmoteModMain.Settings.AnimationSpeed != 9)
                                (data["currentAnimation"] as Sprite.Animation).Delay = EmoteModMain.currentDelay /
                                        EmoteModMain.speeds[EmoteModMain.Settings.AnimationSpeed];
                            else
                                (data["currentAnimation"] as Sprite.Animation).Delay = sprite.Animations[sprite.CurrentAnimationID].Delay;
                        }
                    }
                }

            }
            orig(self);
        }
    }
}

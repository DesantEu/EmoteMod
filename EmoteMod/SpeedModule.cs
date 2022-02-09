using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.EmoteMod
{
	public class SpeedModule
	{
		private static Player player;

		// speed multipliers
		public static float[] speeds = { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1, 2, 4, 5, 6, 10, 20, 30, 40, 50, 60, 70, 100 };
		/// <summary>
		/// current animation delay
		/// </summary>
		public static float currentDelay;
		public static bool speedChanged;

		//speed formatter
		public static Func<int, string> speedFormatter = arg =>
		{
			return $"{speeds[arg]}x";
		};

		public static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
		{
			orig(self);

			player = self;
		}

		internal static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
		{
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
								(data["currentAnimation"] as Sprite.Animation).Delay = currentDelay /
										speeds[EmoteModMain.Settings.AnimationSpeed];
							else
								(data["currentAnimation"] as Sprite.Animation).Delay = sprite.Animations[sprite.CurrentAnimationID].Delay;
						}
					}
				}

			}
			orig(self);
		}

		internal static void Load()
		{
			On.Celeste.Player.Update += Player_Update;
			On.Celeste.Level.Update += Level_Update;
		}
		internal static void Unload()
		{
			On.Celeste.Player.Update -= Player_Update;
			On.Celeste.Level.Update -= Level_Update;
		}
	}
}

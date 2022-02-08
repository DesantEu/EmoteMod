using Monocle;
using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.EmoteMod
{
	public class BackpackModule
	{
		public static Player player;

		public enum MadelineBackpackModes { Default, NoBackpack, Backpack, Playback };

		internal static void PlayerSprite(On.Celeste.PlayerSprite.orig_ctor orig, PlayerSprite self, PlayerSpriteMode mode)
		{
			// code stolen from max (extended variant mode)
			if (EmoteModMain.Settings.Backpack != (int)MadelineBackpackModes.Default)
            {
				if (mode == PlayerSpriteMode.Madeline || mode == PlayerSpriteMode.MadelineNoBackpack)
				{
					if (EmoteModMain.Settings.Backpack == (int)PlayerSpriteMode.Madeline)
					{
						mode = PlayerSpriteMode.Madeline;
					}
					else if (EmoteModMain.Settings.Backpack == (int)MadelineBackpackModes.NoBackpack)
					{
						mode = PlayerSpriteMode.MadelineNoBackpack;
					}
					else if (EmoteModMain.Settings.Backpack == (int)PlayerSpriteMode.Playback)
					{
						mode = PlayerSpriteMode.Playback;
					}
				}
            }

			orig(self, mode);
		}
		private static void onLevelLoader(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
		{
			orig(self, session, startPosition);

			// Everest reinitializes GFX.SpriteBank in the LevelLoader constructor, so we need to initialize the sprites again.
			initializeRollBackpackSprites();
		}

		private static void initializeRollBackpackSprites()
		{
			Dictionary<string, Sprite.Animation> player = GFX.SpriteBank.SpriteData["player"].Sprite.Animations;
			Dictionary<string, Sprite.Animation> playerNoBackpack = GFX.SpriteBank.SpriteData["player_no_backpack"].Sprite.Animations;

			// copy the roll animations from player_no_backpack to player, to prevent crashes in Farewell if the backpack is forced.
			if (!player.ContainsKey("roll"))
			{
				player.Add("roll", playerNoBackpack["roll"]);
			}
			if (!player.ContainsKey("rollGetUp"))
			{
				player.Add("rollGetUp", playerNoBackpack["rollGetUp"]);
			}
			if (!player.ContainsKey("downed"))
			{
				player.Add("downed", playerNoBackpack["downed"]);
			}
		}

		public static void Load()
		{
			On.Celeste.PlayerSprite.ctor += PlayerSprite;
			On.Celeste.LevelLoader.ctor += onLevelLoader;


			if (Engine.Scene is Level)
			{
				initializeRollBackpackSprites();
			}
		}


		public static void Unload()
		{
			On.Celeste.PlayerSprite.ctor -= PlayerSprite;
			On.Celeste.LevelLoader.ctor -= onLevelLoader;
		}
	}
}
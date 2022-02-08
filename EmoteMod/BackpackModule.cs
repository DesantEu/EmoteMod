using Monocle;
using System;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.EmoteMod
{
	public class BackpackModule
	{
		// public static Player player;
		public enum MadelineBackpackModes { Default, Backpack, NoBackpack, Playback };

		// when constructing the player set the sprite to the one we need (needs resetting)
		internal static void PlayerSprite(On.Celeste.PlayerSprite.orig_ctor orig, PlayerSprite self, PlayerSpriteMode mode)
		{
			// code stolen from max (extended variant mode)
			if (mode == PlayerSpriteMode.Madeline || mode == PlayerSpriteMode.MadelineNoBackpack)
			{
				mode = GetMode(EmoteModMain.Settings.Backpack, mode);
			}

			orig(self, mode);
		}

		// load missing animatons
		private static void onLevelLoader(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
		{
			orig(self, session, startPosition);

			initializeRollBackpackSprites();
		}

		// scroll through backpack modes
		public static void ScrollBackpack()
		{
			if (EmoteModMain.Settings.Backpack + 1 > 2)
				SetBackpack(0);
			else
				SetBackpack(EmoteModMain.Settings.Backpack + 1);

			switch (EmoteModMain.Settings.Backpack)
			{
				case 0:
					EmoteModMain.echo("backpack default");
					break;
				case 1:
					EmoteModMain.echo("backpack force on");
					break;
				case 2:
					EmoteModMain.echo("backpack force off");
					break;
			}
		}

		// sets and reloads player sprite
		public static void SetBackpack(int value)
		{
			EmoteModMain.Settings.Backpack = value;

			EmoteModMain.Instance.SaveSettings();
			EmoteModMain.Instance.LoadSettings();

			PlayerModule.GetPlayer().ResetSprite(GetMode(EmoteModMain.Settings.Backpack));

		}

		// playback mode
		public static void EnterSickoMode()
		{

			if (EmoteModMain.Settings.Backpack != 3)
			{
				SetBackpack(3);
				EmoteModMain.echo("W H I T E L I N E  A C T I V A T E D");
			}
			else
			{
				SetBackpack(0);
				EmoteModMain.echo("backpack default");
			}
		}


		private static PlayerSpriteMode GetMode(int settings, PlayerSpriteMode mode = PlayerSpriteMode.Madeline)
		{
			if (settings == (int)MadelineBackpackModes.Backpack)
			{
				return PlayerSpriteMode.Madeline;
			}
			else if (settings == (int)MadelineBackpackModes.NoBackpack)
			{
				return PlayerSpriteMode.MadelineNoBackpack;
			}
			else if (settings == (int)MadelineBackpackModes.Playback)
			{
				return PlayerSpriteMode.Playback;
			}
			else if (PlayerModule.GetPlayer() is Player)
            {
				return PlayerModule.GetPlayer().DefaultSpriteMode;
            }
			else
            {
				return mode;
            }
		}


		private static void initializeRollBackpackSprites()
		{
			Dictionary<string, Sprite.Animation> player = GFX.SpriteBank.SpriteData["player"].Sprite.Animations;
			Dictionary<string, Sprite.Animation> playerNoBackpack = GFX.SpriteBank.SpriteData["player_no_backpack"].Sprite.Animations;

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
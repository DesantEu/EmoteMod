using System;
using System.Linq;

namespace Celeste.Mod.EmoteMod
{
	internal class MadhuntNerf
	{

		public static bool madhuntLoaded = false;
		public static EverestModule MadhuntModule = null;

		public static bool inRound = false;


		internal static void Load()
		{
			if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "Madhunt", Version = new Version(2, 4, 4) }))
			{
				madhuntLoaded = true;
				MadhuntModule = Everest.Modules.Where(module => module.GetType().FullName == "Celeste.Mod.Madhunt.Module").First();

				On.Celeste.Level.LoadLevel += Level_LoadLevel;
			}
		}

		private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
		{
			orig(self, playerIntro, isFromLoader);

			if (madhuntLoaded)
			{
				// thank you Brokemia - 189 from Mt. Celeste Climbing Association for this monstrocity
				inRound = (bool)MadhuntModule.GetType().Assembly
					.GetType("Celeste.Mod.Madhunt.Manager")
					.GetProperty("InRound").GetValue(
					 MadhuntModule.GetType()
					.GetProperty("MadhuntManager").GetValue(null));
			}
		}

		internal static void Unload()
		{
			if (madhuntLoaded)
			{
				On.Celeste.Level.LoadLevel -= Level_LoadLevel;
			}
		}
	}
}

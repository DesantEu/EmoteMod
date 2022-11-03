using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.EmoteMod
{
	internal class MadhuntModule
	{

		public static bool madhuntLoaded = false;
		public static EverestModule madhuntModule = null;

		private static ILHook onMadhuntStartRoundHook;
		private static ILHook onMadhuntStopRoundHook;

		public static bool inRound = false;

		public static bool cursor_before_ret = false;
		public static string test = "";


		internal static void Load()
		{
			if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "Madhunt", Version = new Version(2, 4, 4) }))
			{
				madhuntLoaded = true;
				madhuntModule = Everest.Modules.Where(module => module.GetType().FullName == "Celeste.Mod.Madhunt.Module").First();

				MethodInfo roundStartHook = madhuntModule.GetType().Assembly.GetType("Celeste.Mod.Madhunt.Manager").GetMethod("StartRound");
				onMadhuntStartRoundHook = new ILHook(roundStartHook, onMadhuntStartRound);

				MethodInfo roundEndHook = madhuntModule.GetType().Assembly.GetType("Celeste.Mod.Madhunt.Manager").GetMethod("StopRound");
				onMadhuntStopRoundHook = new ILHook(roundEndHook, onMadhuntStopRound);
			}
		}

		public static void onMadhuntStartRound(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			// go before return true
			cursor.TryGotoNext(MoveType.After, instr => instr.MatchRet());
			cursor.Index-=3;

			cursor.EmitDelegate<Action>(() => { inRound = true; });
		}

		public static void onMadhuntStopRound(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			// this one has no extra steps so just yea
			cursor.Index = 0;
			cursor.EmitDelegate<Action>(() => { inRound = false; });
		}


		internal static void Unload()
		{
			if (madhuntLoaded)
			{
				onMadhuntStartRoundHook.Dispose();
				onMadhuntStopRoundHook.Dispose ();
			}
		}
	}
}

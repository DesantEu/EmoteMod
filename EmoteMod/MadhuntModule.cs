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
		private static ILHook onMadhuntGetInRoundHook;

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

				//MethodInfo roundEndHook = madhuntModule.GetType().Assembly.GetType("Celeste.Mod.Madhunt.Manager").GetMethod("StopRound");
				//onMadhuntStopRoundHook = new ILHook(roundEndHook, onMadhuntStopRound);

				foreach (PropertyInfo m in madhuntModule.GetType()
					.GetProperty("Instance")
					.GetValue(null).GetType()
					.GetProperty("MadhuntManager").GetValue(null).GetType()
					.GetProperties())
				{
					test += m.Name + "; ";
				}

				
				On.Celeste.Level.LoadLevel += Level_LoadLevel;

				MethodInfo getInRoundHook = madhuntModule.GetType().Assembly.GetType("Celeste.Mod.Madhunt.Manager").GetProperty("InRound").GetMethod;
				onMadhuntGetInRoundHook = new ILHook(getInRoundHook, onMadhuntGetInRound);


			}
		}

		private static void onMadhuntGetInRound(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			cursor.TryGotoNext(MoveType.After, instr => instr.MatchRet());
			cursor.Index--;

			cursor.EmitDelegate<Func<bool,bool>>( (r) => { inRound = r; return r; }); 
		}

		private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
		{
			orig(self, playerIntro, isFromLoader);

			EmoteModMain.echo("level loaded");
			if (madhuntLoaded)
			{
				//bool asd = (bool)Type.GetType("Celeste.Mod.Madhunt.Manager").GetProperty("InRound").GetValue(Type.GetType("Celeste.Mod.Madhunt.Module").GetProperty("MadhuntManager").GetValue(null));
				inRound = (bool)madhuntModule.GetType().Assembly.GetType("Celeste.Mod.Madhunt.Manager").GetProperty("InRound").GetValue(madhuntModule.GetType().GetProperty("MadhuntManager").GetValue(null));


				EmoteModMain.echo(asd.ToString());

				//EmoteModMain.echo(Type.GetType("Celeste.Mod.Madhunt.Manager").ToString());

				//madhuntModule.GetType().getGetType().GetField("Instance").GetType()
				//	.GetMethod("get_MadhuntManager").Invoke(null,null)
				//	.GetType().GetMethod("get_InGame").Invoke(null,null);


				//PropertyInfo inRoundProp = madhuntModule.GetType()
				//	//.GetProperty("Instance")
				//	//.GetValue(null).GetType()
				//	.GetProperty("MadhuntManager").GetValue(null).GetType()

				//	.GetProperty("InRound");


				//if (madhuntModule.GetType().GetProperty("MadhuntManager").GetValue(null) == null) EmoteModMain.echo("LMAO ASDFIGHKUYSADHIKGJASKFHJG");


				//else
				//try
				//{
				//	EmoteModMain.echo(inRoundProp.GetGetMethod().Invoke(null,null).ToString());
				//}catch (Exception ex)
				//{
				//	EmoteModMain.echo(ex.ToString());
				//}

				//EmoteModMain.echo(madhuntModule.GetType().GetProperty("MadhuntManager").GetValue(null).GetType().Name);



			}
			//EmoteModMain.echo(Madhunt.Module.MadhuntManager.InRound.ToString());

		}

		public static void onMadhuntStartRound(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			//// go before return
			cursor.TryGotoNext(MoveType.After, instr => instr.MatchRet());
			//// steal the return value
			//cursor.Index--;
			//cursor.Emit(cursor.Prev.OpCode, cursor.Prev.Operand);
			//cursor.Index--;
			//cursor.EmitDelegate<Action<bool>>((res) => inRound = res);


			cursor.Index -= 3;

			//test = cursor.Prev.OpCode.ToString() + " " + cursor.Prev.Operand?.ToString() + "; " + test;
			//cursor.Index--;
			//test = cursor.Prev.OpCode.ToString() + " " + cursor.Prev.Operand?.ToString() + "; " + test;
			//cursor.Index--;
			//test = cursor.Prev.OpCode.ToString() + " " + cursor.Prev.Operand?.ToString() + "; " + test;
			//cursor.Index--;

			cursor.EmitDelegate<Action>(() => { inRound = true; });
			//test = cursor.Prev.OpCode.ToString() + " " + cursor.Prev.Operand?.ToString() + "; " + test;
			//cursor.Index--;
			//test = cursor.Prev.OpCode.ToString() + " " + cursor.Prev.Operand?.ToString() + "; " + test;
			//cursor.Index--;


		}

		public static void onMadhuntStopRound(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			// this one has no extra steps so just yea
			cursor.Index = 0;
			cursor.EmitDelegate<Action>(() => { inRound = false; EmoteModMain.echo(inRound.ToString()); });
		}


		internal static void Unload()
		{
			if (madhuntLoaded)
			{
				//onMadhuntStartRoundHook.Dispose();
				//onMadhuntStopRoundHook.Dispose ();
			}
		}
	}
}

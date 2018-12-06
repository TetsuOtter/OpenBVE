using OpenBveShared;

namespace OpenBve
{
	internal static partial class Renderer
	{

		/// <summary>Re-adds all objects within the world, for example after a screen resolution change</summary>
		internal static void ReAddObjects()
		{
			RendererObject[] list = new RendererObject[OpenBveShared.Renderer.ObjectCount];
			for (int i = 0; i < OpenBveShared.Renderer.ObjectCount; i++)
			{
				list[i] = OpenBveShared.Renderer.Objects[i];
			}
			for (int i = 0; i < list.Length; i++)
			{
				OpenBveShared.Renderer.HideObject(list[i].ObjectIndex);
			}
			for (int i = 0; i < list.Length; i++)
			{
				OpenBveShared.Renderer.ShowObject(list[i].ObjectIndex, list[i].Type, Interface.CurrentOptions.TransparencyMode, Camera.CameraRestriction != CameraRestrictionMode.NotAvailable);
			}
		}

		internal static void UnloadUnusedTextures(double TimeElapsed)
		{
#if DEBUG
			//HACK: If when running in debug mode the frame time exceeds 1s, we can assume VS has hit a breakpoint
			//Don't unload textures in this case, as it just causes texture bugs
			if (TimeElapsed > 1000)
			{
				foreach (var Texture in Textures.RegisteredTextures)
				{
					if (Texture != null)
					{
						Texture.LastAccess = CPreciseTimer.GetClockTicks();
					}
				}
			}
#endif
			if (Game.CurrentInterface == Game.InterfaceType.Normal)
			{
				foreach (var Texture in Textures.RegisteredTextures)
				{
					if (Texture != null && (CPreciseTimer.GetClockTicks() - Texture.LastAccess) > 20000)
					{
						Textures.UnloadTexture(Texture);
					}
				}
			}
			else
			{
				//Don't unload textures if we are in a menu/ paused, as they may be required immediately after unpause
				foreach (var Texture in Textures.RegisteredTextures)
				{
					//Texture can be null in certain cases....
					if (Texture != null)
					{
						Texture.LastAccess = CPreciseTimer.GetClockTicks();
					}
				}
			}
		}
	}
}

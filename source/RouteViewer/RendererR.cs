// ╔═════════════════════════════════════════════════════════════╗
// ║ Renderer.cs for the Route Viewer                            ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenBveShared;
using TrackManager;

namespace OpenBve {
	internal static partial class Renderer {

		// screen (output window)
		internal static int ScreenWidth = 960;
		internal static int ScreenHeight = 600;

		//Stats
		internal static bool RenderStatsOverlay = true;

		// textures
		private static Texture BackgroundChangeTexture = null;
		private static Texture BrightnessChangeTexture = null;
		private static Texture TransponderTexture = null;
		private static Texture SectionTexture = null;
		private static Texture LimitTexture = null;
		private static Texture StationStartTexture = null;
		private static Texture StationEndTexture = null;
		private static Texture StopTexture = null;
		private static Texture BufferTexture = null;
		private static Texture SoundTexture = null;

		// options
		internal static bool OptionEvents = false;
		internal static bool OptionInterface = true;

		// constants
		private const float inv255 = 1.0f / 255.0f;

		// reset
		internal static void Reset() {
			OpenBveShared.Renderer.Objects = new RendererObject[256];
			OpenBveShared.Renderer.ObjectCount = 0;
			OpenBveShared.Renderer.StaticOpaque = new ObjectGroup[] { };
			OpenBveShared.Renderer.StaticOpaqueForceUpdate = true;
			OpenBveShared.Renderer.DynamicOpaque = new ObjectList();
			OpenBveShared.Renderer.DynamicAlpha = new ObjectList();
			OpenBveShared.Renderer.OverlayOpaque = new ObjectList();
			OpenBveShared.Renderer.OverlayAlpha = new ObjectList();
			OpenBveShared.Renderer.OptionLighting = true;
			OpenBveShared.Renderer.OptionAmbientColor = new Color24(160, 160, 160);
			OpenBveShared.Renderer.OptionDiffuseColor = new Color24(160, 160, 160);
			OpenBveShared.Renderer.OptionLightPosition = new Vector3(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);
			OpenBveShared.Renderer.OptionLightingResultingAmount = 1.0f;
			GL.Disable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = false;
		}

		// initialize
		internal static void LoadEventTextures() {
			string Folder = OpenBveApi.Path.CombineDirectory(Program.FileSystem.GetDataFolder(), "RouteViewer");
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "background.png"), out BackgroundChangeTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "brightness.png"), out BrightnessChangeTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "transponder.png"), out TransponderTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "section.png"), out SectionTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "limit.png"), out LimitTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "station_start.png"), out StationStartTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "station_end.png"), out StationEndTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "stop.png"), out StopTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "buffer.png"), out BufferTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "sound.png"), out SoundTexture);
			Textures.LoadTexture(BackgroundChangeTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(BrightnessChangeTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(TransponderTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(SectionTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(LimitTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(StationStartTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(StationEndTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(StopTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(BufferTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(SoundTexture, OpenGlTextureWrapMode.ClampClamp);
		}

		internal static void RenderScene(double TimeElapsed) {
			// initialize
			OpenBveShared.Renderer.ResetOpenGlState();
			int OpenGlTextureIndex = 0;

			if (OpenBveShared.Renderer.OptionWireframe | OpenGlTextureIndex == 0)
			{
				if (OpenBveShared.Renderer.CurrentFog.Start < OpenBveShared.Renderer.CurrentFog.End)
				{
					const float fogdistance = 600.0f;
					float n = (fogdistance - OpenBveShared.Renderer.CurrentFog.Start) / (OpenBveShared.Renderer.CurrentFog.End - OpenBveShared.Renderer.CurrentFog.Start);
					float cr = n * inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.R;
					float cg = n * inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.G;
					float cb = n * inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.B;
					if (string.IsNullOrEmpty(Program.CurrentRoute))
					{
						GL.ClearColor(0.67f, 0.67f, 0.67f, 1.0f);
					}
					else
					{
						GL.ClearColor(cr, cg, cb, 1.0f);
					}
					
				}
				else
				{
					GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
				}
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			}
			else
			{
				GL.Clear(ClearBufferMask.DepthBufferBit);
			}
			GL.PushMatrix();
			// set up camera
			double cx = Camera.AbsoluteCameraPosition.X;
			double cy = Camera.AbsoluteCameraPosition.Y;
			double cz = Camera.AbsoluteCameraPosition.Z;
			double dx = Camera.AbsoluteCameraDirection.X;
			double dy = Camera.AbsoluteCameraDirection.Y;
			double dz = Camera.AbsoluteCameraDirection.Z;
			double ux = Camera.AbsoluteCameraUp.X;
			double uy = Camera.AbsoluteCameraUp.Y;
			double uz = Camera.AbsoluteCameraUp.Z;
			Matrix4d lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);
			GL.Light(LightName.Light0, LightParameter.Position, new float[] { (float)OpenBveShared.Renderer.OptionLightPosition.X, (float)OpenBveShared.Renderer.OptionLightPosition.Y, (float)OpenBveShared.Renderer.OptionLightPosition.Z, 0.0f });
			// fog
			double fd = OpenBveShared.Renderer.NextFog.TrackPosition - OpenBveShared.Renderer.PreviousFog.TrackPosition;
			if (fd != 0.0)
			{
				float fr = (float)((World.CameraTrackFollower.TrackPosition - OpenBveShared.Renderer.PreviousFog.TrackPosition) / fd);
				float frc = 1.0f - fr;
				OpenBveShared.Renderer.CurrentFog.Start = OpenBveShared.Renderer.PreviousFog.Start * frc + OpenBveShared.Renderer.NextFog.Start * fr;
				OpenBveShared.Renderer.CurrentFog.End = OpenBveShared.Renderer.PreviousFog.End * frc + OpenBveShared.Renderer.NextFog.End * fr;
				OpenBveShared.Renderer.CurrentFog.Color.R = (byte)((float)OpenBveShared.Renderer.PreviousFog.Color.R * frc + (float)OpenBveShared.Renderer.NextFog.Color.R * fr);
				OpenBveShared.Renderer.CurrentFog.Color.G = (byte)((float)OpenBveShared.Renderer.PreviousFog.Color.G * frc + (float)OpenBveShared.Renderer.NextFog.Color.G * fr);
				OpenBveShared.Renderer.CurrentFog.Color.B = (byte)((float)OpenBveShared.Renderer.PreviousFog.Color.B * frc + (float)OpenBveShared.Renderer.NextFog.Color.B * fr);
			}
			else
			{
				OpenBveShared.Renderer.CurrentFog = OpenBveShared.Renderer.PreviousFog;
			}
			// render background
			if (OpenBveShared.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = false;
			}
			GL.Disable(EnableCap.DepthTest);
			OpenBveShared.Renderer.UpdateBackground(Game.SecondsSinceMidnight, TimeElapsed);
			
			RenderEvents(Camera.AbsoluteCameraPosition);
			// fog
			float aa = OpenBveShared.Renderer.CurrentFog.Start;
			float bb = OpenBveShared.Renderer.CurrentFog.End;
			if (aa < bb & aa < OpenBveShared.Renderer.BackgroundImageDistance)
			{
				if (!OpenBveShared.Renderer.FogEnabled)
				{
					GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
				}
				GL.Fog(FogParameter.FogStart, aa);
				GL.Fog(FogParameter.FogEnd, bb);
				GL.Fog(FogParameter.FogColor, new float[] { inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.R, inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.G, inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.B, 1.0f });
				if (!OpenBveShared.Renderer.FogEnabled)
				{
					GL.Enable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = true;
				}
			}
			else if (OpenBveShared.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = false;
			}
			// world layer
			bool optionLighting = OpenBveShared.Renderer.OptionLighting;
			OpenBveShared.Renderer.LastBoundTexture = null;
			if (OpenBveShared.Renderer.OptionLighting)
			{
				if (!OpenBveShared.Renderer.LightingEnabled)
				{
					GL.Enable(EnableCap.Lighting); OpenBveShared.Renderer.LightingEnabled = true;
				}
				if (OpenBveShared.Camera.CameraRestriction == CameraRestrictionMode.NotAvailable)
				{
					GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { inv255 * (float)OpenBveShared.Renderer.OptionAmbientColor.R, inv255 * (float)OpenBveShared.Renderer.OptionAmbientColor.G, inv255 * (float)OpenBveShared.Renderer.OptionAmbientColor.B, 1.0f });
					GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { inv255 * (float)OpenBveShared.Renderer.OptionDiffuseColor.R, inv255 * (float)OpenBveShared.Renderer.OptionDiffuseColor.G, inv255 * (float)OpenBveShared.Renderer.OptionDiffuseColor.B, 1.0f });
				}
			}
			else if (OpenBveShared.Renderer.LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting); OpenBveShared.Renderer.LightingEnabled = false;
			}
			// static opaque
			bool f = false;
			if (f) //TODO: Implement display list disabling
			{
				OpenBveShared.Renderer.ResetOpenGlState();
				for (int i = 0; i < OpenBveShared.Renderer.StaticOpaque.Length; i++)
				{
					if (OpenBveShared.Renderer.StaticOpaque[i] != null)
					{
						if (OpenBveShared.Renderer.StaticOpaque[i].List != null)
						{
							for (int j = 0; j < OpenBveShared.Renderer.StaticOpaque[i].List.FaceCount; j++)
							{
								if (OpenBveShared.Renderer.StaticOpaque[i].List.Faces[j] != null)
								{
									RenderFace(ref OpenBveShared.Renderer.StaticOpaque[i].List.Faces[j], cx, cy, cz);
								}
							}
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < OpenBveShared.Renderer.StaticOpaque.Length; i++)
				{
					if (OpenBveShared.Renderer.StaticOpaque[i] != null)
					{
						if (OpenBveShared.Renderer.StaticOpaque[i].Update | OpenBveShared.Renderer.StaticOpaqueForceUpdate)
						{
							OpenBveShared.Renderer.StaticOpaque[i].Update = false;
							if (OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable)
							{
								GL.DeleteLists(OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayList, 1);
								OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable = false;
							}
							if (OpenBveShared.Renderer.StaticOpaque[i].List.FaceCount != 0)
							{
								OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayList = GL.GenLists(1);
								OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable = true;
								OpenBveShared.Renderer.ResetOpenGlState();
								GL.NewList(OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayList, ListMode.Compile);
								for (int j = 0; j < OpenBveShared.Renderer.StaticOpaque[i].List.FaceCount; j++)
								{
									if (OpenBveShared.Renderer.StaticOpaque[i].List.Faces[j] != null)
									{
										RenderFace(ref OpenBveShared.Renderer.StaticOpaque[i].List.Faces[j], cx, cy, cz);
									}
								}
								GL.EndList();
							}

							OpenBveShared.Renderer.StaticOpaque[i].WorldPosition = Camera.AbsoluteCameraPosition;
						}
					}
				}

				OpenBveShared.Renderer.StaticOpaqueForceUpdate = false;
				for (int i = 0; i < OpenBveShared.Renderer.StaticOpaque.Length; i++)
				{
					if (OpenBveShared.Renderer.StaticOpaque[i] != null && OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable)
					{
						OpenBveShared.Renderer.ResetOpenGlState();
						GL.PushMatrix();
						GL.Translate(OpenBveShared.Renderer.StaticOpaque[i].WorldPosition.X - Camera.AbsoluteCameraPosition.X, OpenBveShared.Renderer.StaticOpaque[i].WorldPosition.Y - Camera.AbsoluteCameraPosition.Y, OpenBveShared.Renderer.StaticOpaque[i].WorldPosition.Z - Camera.AbsoluteCameraPosition.Z);
						GL.CallList(OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayList);
						GL.PopMatrix();
					}
				}
				//Update bounding box positions now we've rendered the objects
				int currentBox = 0;
				for (int i = 0; i < OpenBveShared.Renderer.StaticOpaque.Length; i++)
				{
					if (OpenBveShared.Renderer.StaticOpaque[i] != null)
					{
						currentBox++;

					}
				}
			}
			// dynamic opaque
			OpenBveShared.Renderer.ResetOpenGlState();
			for (int i = 0; i < OpenBveShared.Renderer.DynamicOpaque.FaceCount; i++)
			{
				RenderFace(ref OpenBveShared.Renderer.DynamicOpaque.Faces[i], cx, cy, cz);
			}
			// dynamic alpha
			OpenBveShared.Renderer.ResetOpenGlState();
			SortPolygons(OpenBveShared.Renderer.DynamicAlpha);
			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
			{
				GL.Enable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = true;
				GL.DepthMask(false);
				OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
				for (int i = 0; i < OpenBveShared.Renderer.DynamicAlpha.FaceCount; i++)
				{
					RenderFace(ref OpenBveShared.Renderer.DynamicAlpha.Faces[i], cx, cy, cz);
				}
			}
			else
			{
				GL.Disable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = false;
				OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);
				for (int i = 0; i < OpenBveShared.Renderer.DynamicAlpha.FaceCount; i++)
				{
					int r = (int)GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[OpenBveShared.Renderer.DynamicAlpha.Faces[i].FaceIndex].Material;
					if (GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
					{
						if (GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
						{
							RenderFace(ref OpenBveShared.Renderer.DynamicAlpha.Faces[i], cx, cy, cz);
						}
					}
				}
				GL.Enable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = true;
				OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
				GL.DepthMask(false);
				bool additive = false;
				for (int i = 0; i < OpenBveShared.Renderer.DynamicAlpha.FaceCount; i++)
				{
					int r = (int)GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[OpenBveShared.Renderer.DynamicAlpha.Faces[i].FaceIndex].Material;
					if (GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
					{
						if (!additive)
						{
							OpenBveShared.Renderer.UnsetAlphaFunc();
							additive = true;
						}
						RenderFace(ref OpenBveShared.Renderer.DynamicAlpha.Faces[i], cx, cy, cz);
					}
					else
					{
						if (additive)
						{
							OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
							additive = false;
						}
						RenderFace(ref OpenBveShared.Renderer.DynamicAlpha.Faces[i], cx, cy, cz);
					}
				}
			}
			GL.LoadIdentity();
			//UpdateViewport(ViewPortChangeMode.ChangeToCab);
			lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);
			// render overlays
			OpenBveShared.Renderer.BlendEnabled = false; GL.Disable(EnableCap.Blend);
			OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.9f);
			OpenBveShared.Renderer.AlphaTestEnabled = false; GL.Disable(EnableCap.AlphaTest);
			GL.Disable(EnableCap.DepthTest);
			if (OpenBveShared.Renderer.LightingEnabled) {
				GL.Disable(EnableCap.Lighting);
				OpenBveShared.Renderer.LightingEnabled = false;
			}
			RenderOverlays(TimeElapsed);
			// finalize rendering
			GL.PopMatrix();
		}
		
		
		// render face
		private static void RenderFace(ref ObjectFace Face, double CameraX, double CameraY, double CameraZ)
		{
			if (OpenBveShared.Renderer.CullEnabled)
			{
				if (!OpenBveShared.Renderer.OptionBackfaceCulling || (GameObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) != 0)
				{
					GL.Disable(EnableCap.CullFace);
					OpenBveShared.Renderer.CullEnabled = false;
				}
			}
			else if (OpenBveShared.Renderer.OptionBackfaceCulling)
			{
				if ((GameObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) == 0)
				{
					GL.Enable(EnableCap.CullFace);
					OpenBveShared.Renderer.CullEnabled = true;
				}
			}
			int r = (int)GameObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Material;
			OpenBveShared.Renderer.RenderFace(ref GameObjectManager.Objects[Face.ObjectIndex].Mesh.Materials[r], GameObjectManager.Objects[Face.ObjectIndex].Mesh.Vertices, Face.Wrap, ref GameObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex], CameraX, CameraY, CameraZ);
		}
		
		// render events
		private static void RenderEvents(Vector3 CameraPosition) {
			if (OptionEvents == false)
			{
				return;
			}
			if (TrackManager.CurrentTrack.Elements == null) {
				return;
			}
			GL.Enable(EnableCap.CullFace); OpenBveShared.Renderer.CullEnabled = true;
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);
			if (OpenBveShared.Renderer.LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting);
				OpenBveShared.Renderer.LightingEnabled = false;
			}
			if (OpenBveShared.Renderer.AlphaTestEnabled)
			{
				GL.Disable(EnableCap.AlphaTest);
				OpenBveShared.Renderer.AlphaTestEnabled = false;
			}
			double da = -OpenBveShared.World.BackwardViewingDistance - OpenBveShared.World.ExtraViewingDistance;
			double db = OpenBveShared.World.ForwardViewingDistance + OpenBveShared.World.ExtraViewingDistance;
			bool[] sta = new bool[Game.Stations.Length];
			// events
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++) {
				double p = TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
				double d = p - World.CameraTrackFollower.TrackPosition;
				if (d >= da & d <= db) {
					for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
						GeneralEvent e = TrackManager.CurrentTrack.Elements[i].Events[j];
						double dy, dx = 0.0, dz = 0.0;
						double s; Texture t;
						if (e is TrackManager.BrightnessChangeEvent) {
							s = 0.15;
							dy = 4.0;
							t = BrightnessChangeTexture;
						} else if (e is BackgroundChangeEvent) {
							s = 0.25;
							dy = 3.5;
							t = BackgroundChangeTexture;
						} else if (e is TrackManager.StationStartEvent) {
							s = 0.25;
							dy = 1.6;
							t = StationStartTexture;
							TrackManager.StationStartEvent f = (TrackManager.StationStartEvent)e;
							sta[f.StationIndex] = true;
						} else if (e is TrackManager.StationEndEvent) {
							s = 0.25;
							dy = 1.6;
							t = StationEndTexture;
							TrackManager.StationEndEvent f = (TrackManager.StationEndEvent)e;
							sta[f.StationIndex] = true;
						} else if (e is TrackManager.LimitChangeEvent) {
							s = 0.2;
							dy = 1.1;
							t = LimitTexture;
						} else if (e is TrackManager.SectionChangeEvent) {
							s = 0.2;
							dy = 0.8;
							t = SectionTexture;
						} else if (e is TrackManager.TransponderEvent) {
							s = 0.15;
							dy = 0.4;
							t = TransponderTexture;
						} else if (e is TrackManager.SoundEvent) {
							TrackManager.SoundEvent f = (TrackManager.SoundEvent)e;
							s = 0.2;
							dx = f.Position.X;
							dy = f.Position.Y < 0.1 ? 0.1 : f.Position.Y;
							dz = f.Position.Z;
							t = SoundTexture;
						} else {
							s = 0.2;
							dy = 1.0;
							t = null;
						}
						if (t != null) {
							TrackFollower f = new TrackFollower();
							f.TriggerType = EventTriggerType.None;
							f.TrackPosition = p;
							f.Update(TrackManager.CurrentTrack, p + e.TrackPositionDelta, true, false);
							f.WorldPosition.X += dx * f.WorldSide.X + dy * f.WorldUp.X + dz * f.WorldDirection.X;
							f.WorldPosition.Y += dx * f.WorldSide.Y + dy * f.WorldUp.Y + dz * f.WorldDirection.Y;
							f.WorldPosition.Z += dx * f.WorldSide.Z + dy * f.WorldUp.Z + dz * f.WorldDirection.Z;
							OpenBveShared.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, CameraPosition, t);
						}
					}
				}
			}
			// stops
			for (int i = 0; i < sta.Length; i++) {
				if (sta[i]) {
					for (int j = 0; j < Game.Stations[i].Stops.Length; j++) {
						const double dy = 1.4;
						const double s = 0.2;
						double p = Game.Stations[i].Stops[j].TrackPosition;
						TrackFollower f = new TrackFollower();
						f.TriggerType = EventTriggerType.None;
						f.TrackPosition = p;
						f.Update(TrackManager.CurrentTrack, p, true, false);
						f.WorldPosition.X += dy * f.WorldUp.X;
						f.WorldPosition.Y += dy * f.WorldUp.Y;
						f.WorldPosition.Z += dy * f.WorldUp.Z;
						OpenBveShared.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, CameraPosition, StopTexture);
					}
				}
			}
			// buffers
			for (int i = 0; i < Game.BufferTrackPositions.Length; i++) {
				double p = Game.BufferTrackPositions[i];
				double d = p - World.CameraTrackFollower.TrackPosition;
				if (d >= da & d <= db) {
					const double dy = 2.5;
					const double s = 0.25;
					TrackFollower f = new TrackFollower();
					f.TriggerType = EventTriggerType.None;
					f.TrackPosition = p;
					f.Update(TrackManager.CurrentTrack, p, true, false);
					f.WorldPosition.X += dy * f.WorldUp.X;
					f.WorldPosition.Y += dy * f.WorldUp.Y;
					f.WorldPosition.Z += dy * f.WorldUp.Z;
					OpenBveShared.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, CameraPosition, BufferTexture);
				}
			}
		}
		
		// render overlays
		private static void RenderOverlays(double TimeElapsed) {
			// initialize
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			GL.Enable(EnableCap.Blend);
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.MatrixMode(MatrixMode.Modelview);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.Ortho(0.0, (double)Renderer.ScreenWidth, (double)Renderer.ScreenHeight, 0.0, -1.0, 1.0);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// marker
			if (OptionInterface)
			{
				int y = 150;
				for (int i = 0; i < Game.MarkerTextures.Length; i++)
				{
					Textures.LoadTexture(Game.MarkerTextures[i], OpenGlTextureWrapMode.ClampClamp);
					if (Game.MarkerTextures[i] != null) {
						int w = Game.MarkerTextures[i].Width;
						int h = Game.MarkerTextures[i].Height;
						GL.Color4(1.0, 1.0, 1.0, 1.0);
						OpenBveShared.Renderer.DrawRectangle(Game.MarkerTextures[i], new Point(ScreenWidth - w - 8, y), new Size(w,h), null);
						y += h + 8;
					}
				}
			}
			// render
			if (!Program.CurrentlyLoading) {
				if (GameObjectManager.ObjectsUsed == 0) {
					string[][] Keys = { new string[] { "F7" }, new string[] { "F8" } };
					OpenBveShared.Renderer.RenderKeys(4, 4, 24, Keys);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Open route", new Point(32,4), TextAlignment.TopLeft, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the options window", new Point(32, 24), TextAlignment.TopLeft, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "v" + System.Windows.Forms.Application.ProductVersion, new Point(ScreenWidth - 8, ScreenHeight - 8), TextAlignment.BottomRight, Color128.White);
				} else if (OptionInterface) {
					// keys
					string[][] Keys = { new string[] { "F5" }, new string[] { "F7" }, new string[] { "F8" } };
					OpenBveShared.Renderer.RenderKeys(4, 4, 24, Keys);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Reload route", new Point(32, 4), TextAlignment.TopLeft, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Open route", new Point(32, 24), TextAlignment.TopLeft, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the options window", new Point(32, 44), TextAlignment.TopLeft, Color128.White, true);
					Keys = new string[][] { new string[] { "F" }, new string[] { "N" }, new string[] { "E" }, new string[] { "C" }, new string[] { "M" }, new string[] { "I" }};
					OpenBveShared.Renderer.RenderKeys(ScreenWidth - 20, 4, 16, Keys);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Wireframe:", new Point(ScreenWidth -32, 4), TextAlignment.TopRight, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Normals:", new Point(ScreenWidth - 32, 24), TextAlignment.TopRight, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Events:", new Point(ScreenWidth - 32, 44), TextAlignment.TopRight, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "CPU:", new Point(ScreenWidth - 32, 64), TextAlignment.TopRight, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Mute:", new Point(ScreenWidth - 32, 84), TextAlignment.TopRight, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Hide interface:", new Point(ScreenWidth - 32, 104), TextAlignment.TopRight, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, (RenderStatsOverlay ? "Hide" : "Show") + " renderer statistics", new Point(ScreenWidth - 32, 124), TextAlignment.TopRight, Color128.White, true);
					Keys = new string[][] { new string[] { "F10" } };
					OpenBveShared.Renderer.RenderKeys(ScreenWidth - 32, 124, 30, Keys);
					Keys = new string[][] { new string[] { null, "W", null }, new string[] { "A", "S", "D" } };
					OpenBveShared.Renderer.RenderKeys(4, ScreenHeight - 40, 16, Keys);
					Keys = new string[][] { new string[] { null, "↑", null }, new string[] { "←", "↓", "→" } };
					OpenBveShared.Renderer.RenderKeys(0 * ScreenWidth - 48, ScreenHeight - 40, 16, Keys);
					Keys = new string[][] { new string[] { "P↑" }, new string[] { "P↓" } };
					OpenBveShared.Renderer.RenderKeys((int)(0.5 * ScreenWidth + 32), ScreenHeight - 40, 24, Keys);
					Keys = new string[][] { new string[] { null, "/", "*" }, new string[] { "7", "8", "9" }, new string[] { "4", "5", "6" }, new string[] { "1", "2", "3" }, new string[] { null, "0", "." } };
					OpenBveShared.Renderer.RenderKeys(ScreenWidth - 60, ScreenHeight - 100, 16, Keys);
					if (Program.JumpToPositionEnabled) {
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Jump to track position:", new Point(4, 80),TextAlignment.TopLeft, Color128.White, true);
						double distance;
						if (Double.TryParse(Program.JumpToPositionValue, out distance))
						{
							if (distance < Program.MinimumJumpToPositionValue - 100)
							{
								OpenBveShared.Renderer.DrawString(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? Program.JumpToPositionValue + "_" : Program.JumpToPositionValue), new Point(4, 100), TextAlignment.TopLeft, Color128.Red, true);
							}
							else
							{
								OpenBveShared.Renderer.DrawString(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? Program.JumpToPositionValue + "_" : Program.JumpToPositionValue), new Point(4, 100), TextAlignment.TopLeft, distance > TrackManager.CurrentTrack.Elements[TrackManager.CurrentTrack.Elements.Length - 1].StartingTrackPosition + 100
								? Color128.Red : Color128.Yellow, true);
							}
							
						}
					}
					// info
					double x = 0.5 * (double)ScreenWidth - 256.0;
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Position: " + GetLengthString(OpenBveShared.Camera.CameraCurrentAlignment.TrackPosition) + " (X=" + GetLengthString(OpenBveShared.Camera.CameraCurrentAlignment.Position.X) + ", Y=" + GetLengthString(OpenBveShared.Camera.CameraCurrentAlignment.Position.Y) + "), Orientation: (Yaw=" + (OpenBveShared.Camera.CameraCurrentAlignment.Yaw * 57.2957795130824).ToString("0.00", Culture) + "°, Pitch=" + (OpenBveShared.Camera.CameraCurrentAlignment.Pitch * 57.2957795130824).ToString("0.00", Culture) + "°, Roll=" + (OpenBveShared.Camera.CameraCurrentAlignment.Roll * 57.2957795130824).ToString("0.00", Culture) + "°)", new Point((int)x, 4), TextAlignment.TopLeft, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Radius: " + GetLengthString(World.CameraTrackFollower.CurveRadius) + ", Cant: " + (1000.0 * World.CameraTrackFollower.CurveCant).ToString("0", Culture) + " mm, Adhesion=" + (100.0 * World.CameraTrackFollower.AdhesionMultiplier).ToString("0", Culture), new Point((int)x, 20), TextAlignment.TopLeft, Color128.White, true);
					if (Program.CurrentStation >= 0) {
						System.Text.StringBuilder t = new System.Text.StringBuilder();
						t.Append(Game.Stations[Program.CurrentStation].Name);
						if (Game.Stations[Program.CurrentStation].ArrivalTime >= 0.0) {
							t.Append(", Arrival: " + GetTime(Game.Stations[Program.CurrentStation].ArrivalTime));
						}
						if (Game.Stations[Program.CurrentStation].DepartureTime >= 0.0) {
							t.Append(", Departure: " + GetTime(Game.Stations[Program.CurrentStation].DepartureTime));
						}
						if (Game.Stations[Program.CurrentStation].OpenLeftDoors & Game.Stations[Program.CurrentStation].OpenRightDoors) {
							t.Append(", [L][R]");
						} else if (Game.Stations[Program.CurrentStation].OpenLeftDoors) {
							t.Append(", [L][-]");
						} else if (Game.Stations[Program.CurrentStation].OpenRightDoors) {
							t.Append(", [-][R]");
						} else {
							t.Append(", [-][-]");
						}
						switch (Game.Stations[Program.CurrentStation].StopMode) {
							case Game.StationStopMode.AllStop:
								t.Append(", Stop");
								break;
							case Game.StationStopMode.AllPass:
								t.Append(", Pass");
								break;
							case Game.StationStopMode.PlayerStop:
								t.Append(", Player stops - others pass");
								break;
							case Game.StationStopMode.PlayerPass:
								t.Append(", Player passes - others stop");
								break;
						}
						if (Game.Stations[Program.CurrentStation].StationType == Game.StationType.ChangeEnds) {
							t.Append(", Change ends");
						}
						t.Append(", Ratio=").Append((100.0 * Game.Stations[Program.CurrentStation].PassengerRatio).ToString("0", Culture)).Append("%");
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, t.ToString(), new Point((int)x, 36), TextAlignment.TopLeft, Color128.White, true);
					}
					if (Interface.MessageCount == 1) {
						Keys = new string[][] { new string[] { "F9" } };
						OpenBveShared.Renderer.RenderKeys(4, 72, 24, Keys);
						if (Interface.LogMessages[0].Type != MessageType.Information)
						{
							OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the 1 error message recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							//If all of our messages are information, then print the message text in grey
							OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the 1 message recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					} else if (Interface.MessageCount > 1) {
						Keys = new string[][] { new string[] { "F9" } };
						OpenBveShared.Renderer.RenderKeys(4, 72, 24, Keys);
						bool error = false;
						for (int i = 0; i < Interface.MessageCount; i++)
						{
							if (Interface.LogMessages[i].Type != MessageType.Information)
							{
								error = true;
								break;
							}

						}
						if (error)
						{
							OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the " + Interface.MessageCount + " error messages recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the " + Interface.MessageCount + " messages recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					}
					if (RenderStatsOverlay)
					{
						OpenBveShared.Renderer.RenderKeys(4, ScreenHeight - 126, 116, new string[][] { new string[] { "Renderer Statistics" } });
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Total static objects: " + GameObjectManager.ObjectsUsed, new Point(4, ScreenHeight - 112), TextAlignment.TopLeft, Color128.White, true);
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Total animated objects: " + GameObjectManager.AnimatedWorldObjectsUsed, new Point(4, ScreenHeight - 100), TextAlignment.TopLeft, Color128.White, true);
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Current framerate: " + Game.InfoFrameRate.ToString("0.0", Culture) + "fps", new Point(4, ScreenHeight - 88), TextAlignment.TopLeft, Color128.White, true);
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Total opaque faces: " + Game.InfoStaticOpaqueFaceCount, new Point(4, ScreenHeight - 76), TextAlignment.TopLeft, Color128.White, true);
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Total alpha faces: " + (OpenBveShared.Renderer.DynamicAlpha.FaceCount), new Point(4, ScreenHeight - 64), TextAlignment.TopLeft, Color128.White, true);
					}
				}

			}
			GL.PopMatrix();
			GL.LoadIdentity();
			// finalize
			GL.PopMatrix();
			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
			GL.MatrixMode(MatrixMode.Modelview);
			GL.Disable(EnableCap.Blend);
		}
		private static string GetTime(double Time) {
			int h = (int)Math.Floor(Time / 3600.0);
			Time -= (double)h * 3600.0;
			int m = (int)Math.Floor(Time / 60.0);
			Time -= (double)m * 60.0;
			int s = (int)Math.Floor(Time);
			return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
		}

		// get length string 
		private static string GetLengthString(double Value)
		{
			System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
			if (Game.RouteUnitOfLength.Length == 1 && Game.RouteUnitOfLength[0] == 1.0)
			{
				return Value.ToString("0.00", culture);
			}
			else
			{
				double[] values = new double[Game.RouteUnitOfLength.Length];
				for (int i = 0; i < Game.RouteUnitOfLength.Length - 1; i++)
				{
					values[i] = Math.Floor(Value/Game.RouteUnitOfLength[i]);
					Value -= values[i]*Game.RouteUnitOfLength[i];
				}
				values[Game.RouteUnitOfLength.Length - 1] = Value/Game.RouteUnitOfLength[Game.RouteUnitOfLength.Length - 1];
				System.Text.StringBuilder builder = new System.Text.StringBuilder();
				for (int i = 0; i < values.Length - 1; i++)
				{
					builder.Append(values[i].ToString(culture) + ":");
				}
				builder.Append(values[values.Length - 1].ToString("0.00", culture));
				return builder.ToString();
			}
		}

		// show object
		/// <summary>Makes an object visible within the world</summary>
		/// <param name="ObjectIndex">The object's index</param>
		/// <param name="Type">Whether this is a static or dynamic object</param>
		internal static void ShowObject(int ObjectIndex, ObjectType Type)
		{
			if (GameObjectManager.Objects[ObjectIndex] == null)
			{
				return;
			}
			if (GameObjectManager.Objects[ObjectIndex].RendererIndex == 0)
			{
				if (OpenBveShared.Renderer.ObjectCount >= OpenBveShared.Renderer.Objects.Length)
				{
					Array.Resize<RendererObject>(ref OpenBveShared.Renderer.Objects, OpenBveShared.Renderer.Objects.Length << 1);
				}

				OpenBveShared.Renderer.Objects[OpenBveShared.Renderer.ObjectCount].ObjectIndex = ObjectIndex;
				OpenBveShared.Renderer.Objects[OpenBveShared.Renderer.ObjectCount].Type = Type;
				int f = GameObjectManager.Objects[ObjectIndex].Mesh.Faces.Length;
				OpenBveShared.Renderer.Objects[OpenBveShared.Renderer.ObjectCount].FaceListReferences = new ObjectListReference[f];
				for (int i = 0; i < f; i++)
				{
					bool alpha = false;
					int k = GameObjectManager.Objects[ObjectIndex].Mesh.Faces[i].Material;
					OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;
					if (GameObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture != null | GameObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture != null)
					{
						if (GameObjectManager.Objects[ObjectIndex].Mesh.Materials[k].WrapMode == null)
						{
							// If the object does not have a stored wrapping mode, determine it now
							for (int v = 0; v < GameObjectManager.Objects[ObjectIndex].Mesh.Vertices.Length; v++)
							{
								if (GameObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.X < 0.0f |
								    GameObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.X > 1.0f)
								{
									wrap |= OpenGlTextureWrapMode.RepeatClamp;
								}
								if (GameObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.Y < 0.0f |
								    GameObjectManager.Objects[ObjectIndex].Mesh.Vertices[v].TextureCoordinates.Y > 1.0f)
								{
									wrap |= OpenGlTextureWrapMode.ClampRepeat;
								}
							}							
						}
						else
						{
							//Yuck cast, but we need the null, as otherwise requires rewriting the texture indexer
							wrap = (OpenGlTextureWrapMode)GameObjectManager.Objects[ObjectIndex].Mesh.Materials[k].WrapMode;
						}
						if (GameObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture != null)
						{
							if (Textures.LoadTexture(GameObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture, wrap))
							{
								TextureTransparencyType type =
									GameObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTexture.Transparency;
								if (type == TextureTransparencyType.Alpha)
								{
									alpha = true;
								}
								else if (type == TextureTransparencyType.Partial &&
								         Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality)
								{
									alpha = true;
								}
							}
						}
						if (GameObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture != null)
						{
							if (Textures.LoadTexture(GameObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture, wrap))
							{
								TextureTransparencyType type =
									GameObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTexture.Transparency;
								if (type == TextureTransparencyType.Alpha)
								{
									alpha = true;
								}
								else if (type == TextureTransparencyType.Partial &
								         Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality)
								{
									alpha = true;
								}
							}
						}
					}
					if (Type == ObjectType.Overlay & OpenBveShared.Camera.CameraRestriction != CameraRestrictionMode.NotAvailable)
					{
						alpha = true;
					}
					else if (GameObjectManager.Objects[ObjectIndex].Mesh.Materials[k].Color.A != 255)
					{
						alpha = true;
					}
					else if (GameObjectManager.Objects[ObjectIndex].Mesh.Materials[k].BlendMode == MeshMaterialBlendMode.Additive)
					{
						alpha = true;
					}
					else if (GameObjectManager.Objects[ObjectIndex].Mesh.Materials[k].GlowAttenuationData != 0)
					{
						alpha = true;
					}
					ObjectListType listType;
					switch (Type)
					{
						case ObjectType.Static:
							listType = alpha ? ObjectListType.DynamicAlpha : ObjectListType.StaticOpaque;
							break;
						case ObjectType.Dynamic:
							listType = alpha ? ObjectListType.DynamicAlpha : ObjectListType.DynamicOpaque;
							break;
						case ObjectType.Overlay:
							listType = alpha ? ObjectListType.OverlayAlpha : ObjectListType.OverlayOpaque;
							break;
						default:
							throw new InvalidOperationException();
					}
					if (listType == ObjectListType.StaticOpaque)
					{
						/*
						 * For the static opaque list, insert the face into
						 * the first vacant position in the matching group's list.
						 * */
						int groupIndex = (int)GameObjectManager.Objects[ObjectIndex].GroupIndex;
						if (groupIndex >= OpenBveShared.Renderer.StaticOpaque.Length)
						{
							if (OpenBveShared.Renderer.StaticOpaque.Length == 0)
							{
								OpenBveShared.Renderer.StaticOpaque = new ObjectGroup[16];
							}
							while (groupIndex >= OpenBveShared.Renderer.StaticOpaque.Length)
							{
								Array.Resize<ObjectGroup>(ref OpenBveShared.Renderer.StaticOpaque, OpenBveShared.Renderer.StaticOpaque.Length << 1);
							}
						}
						if (OpenBveShared.Renderer.StaticOpaque[groupIndex] == null)
						{
							OpenBveShared.Renderer.StaticOpaque[groupIndex] = new ObjectGroup();
						}
						ObjectList list = OpenBveShared.Renderer.StaticOpaque[groupIndex].List;
						int newIndex = list.FaceCount;
						for (int j = 0; j < list.FaceCount; j++)
						{
							if (list.Faces[j] == null)
							{
								newIndex = j;
								break;
							}
						}
						if (newIndex == list.FaceCount)
						{
							if (list.FaceCount == list.Faces.Length)
							{
								Array.Resize<ObjectFace>(ref list.Faces, list.Faces.Length << 1);
							}
							list.FaceCount++;
						}
						list.Faces[newIndex] = new ObjectFace
						{
							ObjectListIndex = OpenBveShared.Renderer.ObjectCount,
							ObjectIndex = ObjectIndex,
							FaceIndex = i,
							Wrap = wrap
						};

						// HACK: Let's store the wrapping mode.

						OpenBveShared.Renderer.StaticOpaque[groupIndex].Update = true;
						OpenBveShared.Renderer.Objects[OpenBveShared.Renderer.ObjectCount].FaceListReferences[i] = new ObjectListReference(listType, newIndex);
						Game.InfoStaticOpaqueFaceCount++;

						/*
						 * Check if the given object has a bounding box, and insert it to the end of the list of bounding boxes if required
						 */
						if (GameObjectManager.Objects[ObjectIndex].Mesh.BoundingBox != null)
						{
							int Index = list.BoundingBoxes.Length;
							for (int j = 0; j < list.BoundingBoxes.Length; j++)
							{
								if (list.Faces[j] == null)
								{
									Index = j;
									break;
								}
							}
							if (Index == list.BoundingBoxes.Length)
							{
								Array.Resize<BoundingBox>(ref list.BoundingBoxes, list.BoundingBoxes.Length << 1);
							}
							list.BoundingBoxes[Index].Upper = GameObjectManager.Objects[ObjectIndex].Mesh.BoundingBox[0];
							list.BoundingBoxes[Index].Lower = GameObjectManager.Objects[ObjectIndex].Mesh.BoundingBox[1];
						}
					}
					else
					{
						/*
						 * For all other lists, insert the face at the end of the list.
						 * */
						ObjectList list;
						switch (listType)
						{
							case ObjectListType.DynamicOpaque:
								list = OpenBveShared.Renderer.DynamicOpaque;
								break;
							case ObjectListType.DynamicAlpha:
								list = OpenBveShared.Renderer.DynamicAlpha;
								break;
							case ObjectListType.OverlayOpaque:
								list = OpenBveShared.Renderer.OverlayOpaque;
								break;
							case ObjectListType.OverlayAlpha:
								list = OpenBveShared.Renderer.OverlayAlpha;
								break;
							default:
								throw new InvalidOperationException();
						}
						if (list.FaceCount == list.Faces.Length)
						{
							Array.Resize<ObjectFace>(ref list.Faces, list.Faces.Length << 1);
						}
						list.Faces[list.FaceCount] = new ObjectFace
						{
							ObjectListIndex = OpenBveShared.Renderer.ObjectCount,
							ObjectIndex = ObjectIndex,
							FaceIndex = i,
							Wrap = wrap
						};

						// HACK: Let's store the wrapping mode.

						OpenBveShared.Renderer.Objects[OpenBveShared.Renderer.ObjectCount].FaceListReferences[i] = new ObjectListReference(listType, list.FaceCount);
						list.FaceCount++;
					}

				}
				GameObjectManager.Objects[ObjectIndex].RendererIndex = OpenBveShared.Renderer.ObjectCount + 1;
				OpenBveShared.Renderer.ObjectCount++;
			}
		}

		/// <summary>Hides an object within the world</summary>
		/// <param name="ObjectIndex">The object's index</param>
		internal static void HideObject(int ObjectIndex)
		{
			if (GameObjectManager.Objects[ObjectIndex] == null)
			{
				return;
			}
			int k = GameObjectManager.Objects[ObjectIndex].RendererIndex - 1;
			if (k >= 0)
			{		
				// remove faces
				for (int i = 0; i < OpenBveShared.Renderer.Objects[k].FaceListReferences.Length; i++)
				{
					ObjectListType listType = OpenBveShared.Renderer.Objects[k].FaceListReferences[i].Type;
					if (listType == ObjectListType.StaticOpaque)
					{
						/*
						 * For static opaque faces, set the face to be removed
						 * to a null reference. If there are null entries at
						 * the end of the list, update the number of faces used
						 * accordingly.
						 * */
						int groupIndex = (int)GameObjectManager.Objects[OpenBveShared.Renderer.Objects[k].ObjectIndex].GroupIndex;
						ObjectList list = OpenBveShared.Renderer.StaticOpaque[groupIndex].List;
						int listIndex = OpenBveShared.Renderer.Objects[k].FaceListReferences[i].Index;
						list.Faces[listIndex] = null;
						if (listIndex == list.FaceCount - 1)
						{
							int count = 0;
							for (int j = list.FaceCount - 2; j >= 0; j--)
							{
								if (list.Faces[j] != null)
								{
									count = j + 1;
									break;
								}
							}
							list.FaceCount = count;
						}

						OpenBveShared.Renderer.StaticOpaque[groupIndex].Update = true;
						Game.InfoStaticOpaqueFaceCount--;
					}
					else
					{
						/*
						 * For all other kinds of faces, move the last face into place
						 * of the face to be removed and decrement the face counter.
						 * */
						ObjectList list;
						switch (listType)
						{
							case ObjectListType.DynamicOpaque:
								list = OpenBveShared.Renderer.DynamicOpaque;
								break;
							case ObjectListType.DynamicAlpha:
								list = OpenBveShared.Renderer.DynamicAlpha;
								break;
							case ObjectListType.OverlayOpaque:
								list = OpenBveShared.Renderer.OverlayOpaque;
								break;
							case ObjectListType.OverlayAlpha:
								list = OpenBveShared.Renderer.OverlayAlpha;
								break;
							default:
								throw new InvalidOperationException();
						}
						int listIndex = OpenBveShared.Renderer.Objects[k].FaceListReferences[i].Index;
						list.Faces[listIndex] = list.Faces[list.FaceCount - 1];
						OpenBveShared.Renderer.Objects[list.Faces[listIndex].ObjectListIndex].FaceListReferences[list.Faces[listIndex].FaceIndex].Index = listIndex;
						list.FaceCount--;
					}
				}
				// remove object
				if (k == OpenBveShared.Renderer.ObjectCount - 1)
				{
					OpenBveShared.Renderer.ObjectCount--;
				}
				else
				{
					OpenBveShared.Renderer.Objects[k] = OpenBveShared.Renderer.Objects[OpenBveShared.Renderer.ObjectCount - 1];
					OpenBveShared.Renderer.ObjectCount--;
					for (int i = 0; i < OpenBveShared.Renderer.Objects[k].FaceListReferences.Length; i++)
					{
						ObjectListType listType = OpenBveShared.Renderer.Objects[k].FaceListReferences[i].Type;
						ObjectList list;
						switch (listType)
						{
							case ObjectListType.StaticOpaque:
								{
									int groupIndex = (int)GameObjectManager.Objects[OpenBveShared.Renderer.Objects[k].ObjectIndex].GroupIndex;
									list = OpenBveShared.Renderer.StaticOpaque[groupIndex].List;
								}
								break;
							case ObjectListType.DynamicOpaque:
								list = OpenBveShared.Renderer.DynamicOpaque;
								break;
							case ObjectListType.DynamicAlpha:
								list = OpenBveShared.Renderer.DynamicAlpha;
								break;
							case ObjectListType.OverlayOpaque:
								list = OpenBveShared.Renderer.OverlayOpaque;
								break;
							case ObjectListType.OverlayAlpha:
								list = OpenBveShared.Renderer.OverlayAlpha;
								break;
							default:
								throw new InvalidOperationException();
						}
						int listIndex = OpenBveShared.Renderer.Objects[k].FaceListReferences[i].Index;
						list.Faces[listIndex].ObjectListIndex = k;
					}
					GameObjectManager.Objects[OpenBveShared.Renderer.Objects[k].ObjectIndex].RendererIndex = k + 1;
				}
				GameObjectManager.Objects[ObjectIndex].RendererIndex = 0;
			}
		}

		/// <summary>Sorts the polgons contained within this ObjectList, near to far</summary>
		private static void SortPolygons(ObjectList list)
		{
			// calculate distance
			double cx = Camera.AbsoluteCameraPosition.X;
			double cy = Camera.AbsoluteCameraPosition.Y;
			double cz = Camera.AbsoluteCameraPosition.Z;
			for (int i = 0; i < list.FaceCount; i++)
			{
				int o = list.Faces[i].ObjectIndex;
				int f = list.Faces[i].FaceIndex;
				if (GameObjectManager.Objects[o].Mesh.Faces[f].Vertices.Length >= 3)
				{
					int v0 = GameObjectManager.Objects[o].Mesh.Faces[f].Vertices[0].Index;
					int v1 = GameObjectManager.Objects[o].Mesh.Faces[f].Vertices[1].Index;
					int v2 = GameObjectManager.Objects[o].Mesh.Faces[f].Vertices[2].Index;
					double v0x = GameObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.X;
					double v0y = GameObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.Y;
					double v0z = GameObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.Z;
					double v1x = GameObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.X;
					double v1y = GameObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.Y;
					double v1z = GameObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.Z;
					double v2x = GameObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.X;
					double v2y = GameObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.Y;
					double v2z = GameObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.Z;
					double w1x = v1x - v0x, w1y = v1y - v0y, w1z = v1z - v0z;
					double w2x = v2x - v0x, w2y = v2y - v0y, w2z = v2z - v0z;
					double dx = -w1z * w2y + w1y * w2z;
					double dy = w1z * w2x - w1x * w2z;
					double dz = -w1y * w2x + w1x * w2y;
					double t = dx * dx + dy * dy + dz * dz;
					if (t == 0.0) continue;
					t = 1.0 / Math.Sqrt(t);
					dx *= t;
					dy *= t;
					dz *= t;
					double w0x = v0x - cx, w0y = v0y - cy, w0z = v0z - cz;
					t = dx * w0x + dy * w0y + dz * w0z;
					list.Faces[i].Distance = -t * t;
				}
			}

			// sort
			double[] distances = new double[list.FaceCount];
			for (int i = 0; i < list.FaceCount; i++)
			{
				distances[i] = list.Faces[i].Distance;
			}

			Array.Sort<double, ObjectFace>(distances, list.Faces, 0, list.FaceCount);
			// update objects
			for (int i = 0; i < list.FaceCount; i++)
			{
				OpenBveShared.Renderer.Objects[list.Faces[i].ObjectListIndex].FaceListReferences[list.Faces[i].FaceIndex].Index = i;
			}
		}
	}
}

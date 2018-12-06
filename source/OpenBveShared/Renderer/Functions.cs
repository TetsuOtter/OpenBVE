using OpenTK.Graphics.OpenGL;

namespace OpenBveShared
{
	public static partial class Renderer
	{
		/// <summary>Clears all currently registered OpenGL display lists</summary>
		public static void ClearDisplayLists()
		{
			for (int i = 0; i < StaticOpaque.Length; i++)
			{
				if (StaticOpaque[i] != null)
				{
					if (StaticOpaque[i].OpenGlDisplayListAvailable)
					{
						GL.DeleteLists(StaticOpaque[i].OpenGlDisplayList, 1);
						StaticOpaque[i].OpenGlDisplayListAvailable = false;
					}
				}
			}

			StaticOpaqueForceUpdate = true;
		}

		/// <summary>Performs a reset of OpenGL to the default state</summary>
        public static void ResetOpenGlState()
        {
            LastBoundTexture = null;
            GL.Enable(EnableCap.CullFace); CullEnabled = true;
            GL.Disable(EnableCap.Lighting); LightingEnabled = false;
            GL.Disable(EnableCap.Texture2D); TexturingEnabled = false;
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.Blend); BlendEnabled = false;
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
            SetAlphaFunc(AlphaFunction.Greater, 0.9f);
        }

        /// <summary>Specifies the OpenGL alpha function to perform</summary>
        /// <param name="Comparison">The comparison to use</param>
        /// <param name="Value">The value to compare</param>
        public static void SetAlphaFunc(AlphaFunction Comparison, float Value)
        {
            AlphaTestEnabled = true;
            AlphaFuncComparison = Comparison;
            AlphaFuncValue = Value;
            GL.AlphaFunc(Comparison, Value);
            GL.Enable(EnableCap.AlphaTest);
        }

        /// <summary>Disables OpenGL alpha testing</summary>
        public static void UnsetAlphaFunc()
        {
            AlphaTestEnabled = false;
            GL.Disable(EnableCap.AlphaTest);
        }

        /// <summary>Restores the OpenGL alpha function to it's previous state</summary>
        public static void RestoreAlphaFunc()
        {
            if (AlphaTestEnabled)
            {
                GL.AlphaFunc(AlphaFuncComparison, AlphaFuncValue);
                GL.Enable(EnableCap.AlphaTest);
            }
            else
            {
                GL.Disable(EnableCap.AlphaTest);
            }
        }


	}
}

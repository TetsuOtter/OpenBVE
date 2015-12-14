﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Globalization;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CSScriptLibrary;
using OpenTK.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using ButtonState = OpenTK.Input.ButtonState;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace OpenBve {
	internal static partial class MainLoop {

		// declarations
		internal static bool LimitFramerate = false;
	    internal static bool Quit = false;
        /// <summary>BlockKeyRepeat should be set to 'true' whilst processing a KeyUp or KeyDown event.</summary>
	    internal static bool BlockKeyRepeat;
        /// <summary>The current simulation time-factor</summary>
	    internal static int TimeFactor = 1;
		private static ViewPortMode CurrentViewPortMode = ViewPortMode.Scenery;
	    internal static bool OpenTKWindow;
	    internal static formMain.MainDialogResult currentResult;
	    internal static formRouteInformation RouteInformationForm;
	    internal static Thread RouteInfoThread;
	    internal static bool RouteInfoActive;


	    internal static AppDomain RouteInfoFormDomain;

		internal static void StartLoopEx(formMain.MainDialogResult result)
		{
            Sounds.Initialize();
		    currentResult = result;   
		    if (OpenTKWindow == false)
		    {
                GraphicsMode currentGraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, Interface.CurrentOptions.AntiAliasingLevel);
		        
                 /*
                 * TODO: This should be able to be moved back into the screen initialisation file
                 */
                
		        if (Interface.CurrentOptions.FullscreenMode)
		        {
		            IList<DisplayResolution> resolutions = OpenTK.DisplayDevice.Default.AvailableResolutions;

		            for (int i = 0; i < resolutions.Count; i++)
		            {
		                //Test each resolution
		                if (resolutions[i].Width == Interface.CurrentOptions.FullscreenWidth &&
		                    resolutions[i].Height == Interface.CurrentOptions.FullscreenHeight &&
		                    resolutions[i].BitsPerPixel == Interface.CurrentOptions.FullscreenBits)
		                {
                            OpenTK.DisplayDevice.Default.ChangeResolution(resolutions[i]);
		                    Program.currentGameWindow = new OpenBVEGame(resolutions[i].Width, resolutions[i].Height,currentGraphicsMode, "OpenBve", GameWindowFlags.Default);
		                    
		                    Program.currentGameWindow.WindowState = WindowState.Fullscreen;
		                    break;
		                }
		            }
		        }
		        else
		        {
                    Program.currentGameWindow = new OpenBVEGame(Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenWidth : Interface.CurrentOptions.WindowWidth, Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenHeight : Interface.CurrentOptions.WindowHeight, currentGraphicsMode, "OpenBve", GameWindowFlags.Default);
                    Program.currentGameWindow.Visible = true;
		            Program.currentGameWindow.TargetUpdateFrequency = 0;
                    Program.currentGameWindow.TargetRenderFrequency = 0;
                    Program.currentGameWindow.Title = "OpenBVE";
		        }
		        if (Program.currentGameWindow == null)
		        {
		            MessageBox.Show("An error occured whilst attempting to launch the graphics subsystem." + Environment.NewLine +
                                    "Please check your resolution settings.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    Program.RestartArguments = " ";
		            return;
		        }
                if (Interface.CurrentOptions.VerticalSynchronization)
                {
                    Program.currentGameWindow.VSync = VSyncMode.On;
                }
                else
                {
                    Program.currentGameWindow.VSync = VSyncMode.Off;
                }
		        Program.currentGameWindow.Closing += OpenTKQuit;
                Program.currentGameWindow.Run();
		    }
		}
		// --------------------------------

		// repeats


        private static void ThreadProc()
        {
            RouteInformationForm = new formRouteInformation();
            Application.Run(RouteInformationForm);
        }

	    private static void OpenTKQuit(object sender, CancelEventArgs e)
	    {
	        Quit = true;
	    }


        
		// process events
		private static Interface.KeyboardModifier CurrentKeyboardModifier = Interface.KeyboardModifier.None;

	    internal static void mouseDownEvent(object sender, MouseButtonEventArgs e)
	    {
	        if (e.Button == MouseButton.Right)
	        {
	            World.MouseGrabEnabled = !World.MouseGrabEnabled;
	            World.MouseGrabIgnoreOnce = true;
	        }
	    }

	    

	    internal static MouseState currentMouseState, previousMouseState;

        internal static void ProcessKeyboard()
        {
            if (World.MouseGrabEnabled)
            {
                previousMouseState = currentMouseState;
                currentMouseState = Mouse.GetState();
                if (previousMouseState != currentMouseState)
                {
                    if (World.MouseGrabIgnoreOnce)
                    {
                        World.MouseGrabIgnoreOnce = false;
                    }
                    else if (World.MouseGrabEnabled)
                    {
                        World.MouseGrabTarget = new OpenBveApi.Math.Vector2(currentMouseState.X - previousMouseState.X, currentMouseState.Y - previousMouseState.Y);
                    }
                }
            }

            //Traverse the controls array
            for (int i = 0; i < Interface.CurrentControls.Length; i++)
            {
                if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Joystick)
                {
                    if (!OpenTK.Input.Joystick.GetCapabilities(Interface.CurrentControls[i].Device).IsConnected) continue;
                    switch (Interface.CurrentControls[i].Component)
                    {
                        case Interface.JoystickComponent.Axis:
                            var axisState = OpenTK.Input.Joystick.GetState(Interface.CurrentControls[i].Device).GetAxis((JoystickAxis)Interface.CurrentControls[i].Element);
                            if (axisState.ToString(CultureInfo.InvariantCulture) != Interface.CurrentControls[i].LastState)
                            {
                                Interface.CurrentControls[i].LastState = axisState.ToString(CultureInfo.InvariantCulture);
                                if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogHalf)
                                {
                                    if (Math.Sign(axisState) == Math.Sign(Interface.CurrentControls[i].Direction))
                                    {
                                        axisState = Math.Abs(axisState);
                                        if (axisState < Interface.CurrentOptions.JoystickAxisThreshold)
                                        {
                                            Interface.CurrentControls[i].AnalogState = 0.0;
                                        }
                                        else if (Interface.CurrentOptions.JoystickAxisThreshold != 1.0)
                                        {
                                            Interface.CurrentControls[i].AnalogState = (axisState - Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
                                        }
                                        else
                                        {
                                            Interface.CurrentControls[i].AnalogState = 1.0;
                                        }
                                    }
                                }
                                else if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogFull)
                                {
                                    axisState *= (float)Interface.CurrentControls[i].Direction;
                                    if (axisState > -Interface.CurrentOptions.JoystickAxisThreshold & axisState < Interface.CurrentOptions.JoystickAxisThreshold)
                                    {
                                        Interface.CurrentControls[i].AnalogState = 0.0;
                                    }
                                    else if (Interface.CurrentOptions.JoystickAxisThreshold != 1.0)
                                    {
                                        if (axisState < 0.0)
                                        {
                                            Interface.CurrentControls[i].AnalogState = (axisState + Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
                                        }
                                        else if (axisState > 0.0)
                                        {
                                            Interface.CurrentControls[i].AnalogState = (axisState - Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
                                        }
                                        else
                                        {
                                            Interface.CurrentControls[i].AnalogState = 0.0;
                                        }
                                    }
                                    else
                                    {
                                        Interface.CurrentControls[i].AnalogState = (double)Math.Sign(axisState);
                                    }
                                }
                                else
                                {
                                    if (Math.Sign(axisState) == Math.Sign(Interface.CurrentControls[i].Direction))
                                    {
                                        axisState = Math.Abs(axisState);
                                        if (axisState < Interface.CurrentOptions.JoystickAxisThreshold)
                                        {
                                            axisState = 0.0f;
                                        }
                                        else if (Interface.CurrentOptions.JoystickAxisThreshold != 1.0)
                                        {
                                            axisState = (float)((axisState - Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold));
                                        }
                                        else
                                        {
                                            axisState = 1.0f;
                                        }
                                        if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Released | Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.ReleasedAcknowledged)
                                        {
                                            if (axisState > 0.67) Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
                                        }
                                        else
                                        {
                                            if (axisState < 0.33) Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
                                        }
                                    }
                                }
                            }
                            break;
                        case Interface.JoystickComponent.Button:
                            //Load the current state
                            var buttonState = OpenTK.Input.Joystick.GetState(Interface.CurrentControls[i].Device).GetButton((JoystickButton)Interface.CurrentControls[i].Element);
                            //Test whether the state is the same as the last frame
                            if (buttonState.ToString() != Interface.CurrentControls[i].LastState)
                            {
                                if (buttonState == ButtonState.Pressed)
                                {
                                    Interface.CurrentControls[i].AnalogState = 1.0;
                                    Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
                                    AddControlRepeat(i);
                                }
                                else
                                {
                                    Interface.CurrentControls[i].AnalogState = 0.0;
                                    Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
                                    RemoveControlRepeat(i);
                                }
                                //Store the state
                                Interface.CurrentControls[i].LastState = buttonState.ToString();
                            }
                            break;
                        case Interface.JoystickComponent.Hat:
                            //Load the current state
                            var hatState = OpenTK.Input.Joystick.GetState(Interface.CurrentControls[i].Device).GetHat((JoystickHat)Interface.CurrentControls[i].Element).Position;
                            //Test if the state is the same as last frame
                            if (hatState.ToString() != Interface.CurrentControls[i].LastState)
                            {
                                if ((int)hatState == Interface.CurrentControls[i].Direction)
                                {
                                    Interface.CurrentControls[i].AnalogState = 1.0;
                                    Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
                                    AddControlRepeat(i);
                                }
                                else
                                {
                                    Interface.CurrentControls[i].AnalogState = 0.0;
                                    Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
                                    RemoveControlRepeat(i);
                                }
                                //Store the state
                                Interface.CurrentControls[i].LastState = hatState.ToString();
                            }
                            break;
                    }
                }
            }
        }

		// save camera setting
		private static void SaveCameraSettings() {
			switch (World.CameraMode) {
				case World.CameraViewMode.Interior:
				case World.CameraViewMode.InteriorLookAhead:
					World.CameraSavedInterior = World.CameraCurrentAlignment;
					break;
				case World.CameraViewMode.Exterior:
					World.CameraSavedExterior = World.CameraCurrentAlignment;
					break;
				case World.CameraViewMode.Track:
				case World.CameraViewMode.FlyBy:
				case World.CameraViewMode.FlyByZooming:
					World.CameraSavedTrack = World.CameraCurrentAlignment;
					break;
			}
		}
		
		// restore camera setting
		private static void RestoreCameraSettings() {
			switch (World.CameraMode) {
				case World.CameraViewMode.Interior:
				case World.CameraViewMode.InteriorLookAhead:
					World.CameraCurrentAlignment = World.CameraSavedInterior;
					break;
				case World.CameraViewMode.Exterior:
					World.CameraCurrentAlignment = World.CameraSavedExterior;
					break;
				case World.CameraViewMode.Track:
				case World.CameraViewMode.FlyBy:
				case World.CameraViewMode.FlyByZooming:
					World.CameraCurrentAlignment = World.CameraSavedTrack;
					TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, World.CameraSavedTrack.TrackPosition, true, false);
					World.CameraCurrentAlignment.TrackPosition = World.CameraTrackFollower.TrackPosition;
					break;
			}
			World.CameraCurrentAlignment.Zoom = 0.0;
			World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
		}

		// --------------------------------

		// update viewport
		internal enum ViewPortMode {
			Scenery = 0,
			Cab = 1
		}
		internal enum ViewPortChangeMode {
			ChangeToScenery = 0,
			ChangeToCab = 1,
			NoChange = 2
		}
		internal static void UpdateViewport(ViewPortChangeMode Mode) {
			if (Mode == ViewPortChangeMode.ChangeToCab) {
				CurrentViewPortMode = ViewPortMode.Cab;
			} else {
				CurrentViewPortMode = ViewPortMode.Scenery;
			}

            GL.Viewport(0,0,Screen.Width, Screen.Height);
			World.AspectRatio = (double)Screen.Width / (double)Screen.Height;
			World.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * World.VerticalViewingAngle) * World.AspectRatio);
			GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            //This value was used to convert radians to degrees
            //OpenTK works in radians, so removed.....
			//const double invdeg = 57.295779513082320877;
			if (CurrentViewPortMode == ViewPortMode.Cab) {

                //Glu.Perspective(World.VerticalViewingAngle * invdeg, -World.AspectRatio, 0.025, 50.0);
			    Matrix4d perspective = Matrix4d.Perspective(World.VerticalViewingAngle,-World.AspectRatio, 0.025, 50.0);
                GL.MultMatrix(ref perspective);
			} else {
                Matrix4d perspective = Matrix4d.Perspective(World.VerticalViewingAngle, -World.AspectRatio, 0.5, World.BackgroundImageDistance);
                GL.MultMatrix(ref perspective);
			}
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
		}

		// initialize motion blur
		internal static void InitializeMotionBlur() {
			if (Interface.CurrentOptions.MotionBlur != Interface.MotionBlurMode.None) {
				if (Renderer.PixelBufferOpenGlTextureIndex != 0) {
                    GL.DeleteTextures(1, new int[] { Renderer.PixelBufferOpenGlTextureIndex });
					Renderer.PixelBufferOpenGlTextureIndex = 0;
				}
				int w = Interface.CurrentOptions.NoTextureResize ? Screen.Width : Textures.RoundUpToPowerOfTwo(Screen.Width);
				int h = Interface.CurrentOptions.NoTextureResize ? Screen.Height : Textures.RoundUpToPowerOfTwo(Screen.Height);
				Renderer.PixelBuffer = new byte[4 * w * h];
				int[] a = new int[1];
                GL.GenTextures(1,a);
                GL.BindTexture(TextureTarget.Texture2D, a[0]);
			    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMagFilter.Linear);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, w, h, 0, PixelFormat.Rgb,PixelType.UnsignedByte, Renderer.PixelBuffer);
				Renderer.PixelBufferOpenGlTextureIndex = a[0];
                GL.CopyTexImage2D(TextureTarget.Texture2D, 0,PixelInternalFormat.Rgb, 0,0,w,h,0);
			}
		}
		
		#if DEBUG

        /// <summary>Checks whether an OpenGL error has occured this frame</summary>
        /// <param name="Location">The location of the caller (The main loop or the loading screen loop)</param>
	    internal static void CheckForOpenGlError(string Location) {
		    var error = GL.GetError();
			if (error != ErrorCode.NoError) {
				string message = Location + ": ";
				switch (error) {
					case ErrorCode.InvalidEnum:
						message += "GL_INVALID_ENUM";
						break;
					case ErrorCode.InvalidValue:
						message += "GL_INVALID_VALUE";
						break;
					case ErrorCode.InvalidOperation:
						message += "GL_INVALID_OPERATION";
						break;
					case ErrorCode.StackOverflow:
						message += "GL_STACK_OVERFLOW";
						break;
					case ErrorCode.StackUnderflow:
						message += "GL_STACK_UNDERFLOW";
						break;
					case ErrorCode.OutOfMemory:
						message += "GL_OUT_OF_MEMORY";
						break;
					case ErrorCode.TableTooLargeExt:
						message += "GL_TABLE_TOO_LARGE";
						break;
					default:
						message += error.ToString();
						break;
				}
				throw new InvalidOperationException(message);
			}
		}
		#endif
	}
}
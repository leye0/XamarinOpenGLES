/*
 * Copyright (C) 2011 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
//package com.example.android.opengl;

//import javax.microedition.khronos.egl.EGLConfig;
//import javax.microedition.khronos.opengles.GL10;

//import android.opengl.GLES20;
//import android.opengl.GLSurfaceView;
//import android.opengl.Matrix;
//import android.util.Log;


using Android.Content;
using Android.Opengl;
using Android.Util;
using Java.Lang;
using Java.Nio;
/**
* Provides drawing instructions for a GLSurfaceView object. This class
* must override the OpenGL ES drawing lifecycle methods:
* <ul>
*   <li>{@link android.opengl.GLSurfaceView.Renderer#onSurfaceCreated}</li>
*   <li>{@link android.opengl.GLSurfaceView.Renderer#onDrawFrame}</li>
*   <li>{@link android.opengl.GLSurfaceView.Renderer#onSurfaceChanged}</li>
* </ul>
*/

namespace XamarinOpenGL.Droid
{
	public class MyGLRenderer : Object, GLSurfaceView.IRenderer
	{
		Context _context;
		public MyGLRenderer(Context context)
		{
			_context = context;
		}

		static string TAG = "MyGLRenderer";
		Triangle _triangle;
		Square _square;
		Image _image;


		// mMVPMatrix is an abbreviation for "Model View Projection Matrix"
		float[] _MVPMatrix = new float[16];
		float[] _projectionMatrix = new float[16];
		float[] _viewMatrix = new float[16];
		float[] _rotationMatrix = new float[16];

		float _angle;

		public float X { get; set; }

		public float Y { get; set; }

		public void OnSurfaceCreated(Javax.Microedition.Khronos.Opengles.IGL10 unused, Javax.Microedition.Khronos.Egl.EGLConfig config)
		{
			// Set the background frame color
			//_triangle = new Triangle();
			//_square = new Square();
			_image = new Image(_context);

			GLES20.GlClearColor(0.0f, 0.0f, 0.0f, 1.0f);


		}

		public void OnDrawFrame(Javax.Microedition.Khronos.Opengles.IGL10 unused)
		{
			float[] scratch = new float[16];

			// Draw background color
			GLES20.GlClear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit);

			// Set the camera position (View matrix)
			Matrix.SetLookAtM(_viewMatrix, 0, 0f + (X / 100f), 0f + (Y / 100f), -6f, 0f + (X / 100f), 0f + (Y / 100f), 0f, 0f, 1.0f, 0.0f);

			// Calculate the projection and view transformation
			Matrix.MultiplyMM(_MVPMatrix, 0, _projectionMatrix, 0, _viewMatrix, 0);

			// Draw square
			//_square.Draw(_MVPMatrix);

			// Draw square
			_image.Draw(_MVPMatrix);


			// Create a rotation for the triangle

			// Use the following code to generate constant rotation.
			// Leave this code out when using TouchEvents.
			// long time = SystemClock.uptimeMillis() % 4000L;
			// float angle = 0.090f * ((int) time);

			Matrix.SetRotateM(_rotationMatrix, 0, Angle, 0, 0, 1.0f);

			// Combine the rotation matrix with the projection and camera view
			// Note that the mMVPMatrix factor *must be first* in order
			// for the matrix multiplication product to be correct.
			Matrix.MultiplyMM(scratch, 0, _MVPMatrix, 0, _rotationMatrix, 0);

			// Draw triangle
			//_triangle.Draw(scratch);
		}

		public void OnSurfaceChanged(Javax.Microedition.Khronos.Opengles.IGL10 unused, int width, int height)
		{
			// Adjust the viewport based on geometry changes,
			// such as screen rotation
			GLES20.GlViewport(0, 0, width, height);

			var ratio = (float)width / height;

			// this projection matrix is applied to object coordinates
			// in the onDrawFrame() method
			Matrix.FrustumM(_projectionMatrix, 0, -ratio, ratio, -1, 1, 3, 7);

		}

		/**
		 * Utility method for compiling a OpenGL shader.
		 *
		 * <p><strong>Note:</strong> When developing shaders, use the checkGlError()
		 * method to debug shader coding errors.</p>
		 *
		 * @param type - Vertex or fragment shader type.
		 * @param shaderCode - String containing the shader code.
		 * @return - Returns an id for the shader.
		 */
		public static int LoadShader(int type, string shaderCode)
		{

			// create a vertex shader type (GLES20.GL_VERTEX_SHADER)
			// or a fragment shader type (GLES20.GL_FRAGMENT_SHADER)
			int shader = GLES20.GlCreateShader(type);

			// add the source code to the shader and compile it
			GLES20.GlShaderSource(shader, shaderCode);
			GLES20.GlCompileShader(shader);

			return shader;
		}

		/**
		* Utility method for debugging OpenGL calls. Provide the name of the call
		* just after making it:
		*
		* <pre>
		* mColorHandle = GLES20.glGetUniformLocation(mProgram, "vColor");
		* MyGLRenderer.checkGlError("glGetUniformLocation");</pre>
		*
		* If the operation is not successful, the check throws an error.
		*
		* @param glOperation - Name of the OpenGL call to check.
*/
		public static void CheckGlError(string glOperation)
		{
			int error;
			while ((error = GLES20.GlGetError()) != GLES20.GlNoError)
			{
				Log.Error(TAG, glOperation + ": glError " + error);
				throw new RuntimeException(glOperation + ": glError " + error);
			}
		}

		/**
		 * Returns the rotation angle of the triangle shape (mTriangle).
		 *
		 * @return - A float representing the rotation angle.
		 */
		public float Angle
		{
			set { _angle = value; }
			get { return _angle; }
		}

		//public int LoadTexture(Context context, int resourceId)
		//{
		//	var textureHandle = new int[1];

		//	GLES20.GlGenTextures(1, textureHandle, 0);

		//	if (textureHandle[0] != 0)
		//	{
		//		var options = new Android.Graphics.BitmapFactory.Options();
		//		options.InScaled = false;   // No pre-scaling

		//		// Read in the resource
		//		var bitmap = Android.Graphics.BitmapFactory.DecodeResource(context.Resources, resourceId, options);

		//		// Bind to the texture in OpenGL
		//		GLES20.GlBindTexture(GLES20.GlTexture2d, textureHandle[0]);

		//		// Set filtering
		//		GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlNearest);
		//		GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlNearest);

		//		// Load the bitmap into the bound texture.
		//		GLUtils.TexImage2D(GLES20.GlTexture2d, 0, bitmap, 0);

		//		// Recycle the bitmap, since its data has been loaded into OpenGL.
		//		bitmap.Recycle();
		//	}

		//	if (textureHandle[0] == 0)
		//	{
		//		throw new RuntimeException("Error loading texture.");
		//	}

		//	return textureHandle[0];
		//}
	}
}
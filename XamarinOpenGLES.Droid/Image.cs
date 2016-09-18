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

//import java.nio.ByteBuffer;
//import java.nio.ByteOrder;
//import java.nio.FloatBuffer;
//import java.nio.ShortBuffer;

//import android.opengl.GLES20;


using Android.Content;
using Android.Opengl;
using Java.Nio;
namespace XamarinOpenGL.Droid
{
	/**
	* A two-dimensional square for use as a drawn object in OpenGL ES 2.0.
*/
	public class Image
	{
		Context _context;

		string vertexShaderCode =
			"uniform mat4 uMVPMatrix;" +
			"attribute vec4 vPosition;" +
			"attribute vec2 a_texCoord;" +
			"varying vec2 v_texCoord;" +
			"void main() {" +
			"  gl_Position = uMVPMatrix * vPosition;" +
			"  v_texCoord = a_texCoord;" +
			"}";

		string fragmentShaderCode =
			"precision mediump float;" +
			"varying vec2 v_texCoord;" +
			"uniform sampler2D s_texture;" +
			"void main() {" +
			"  gl_FragColor = texture2D( s_texture, v_texCoord );" +
			"}";


		private FloatBuffer vertexBuffer;
		private ShortBuffer drawListBuffer;
		private FloatBuffer uvBuffer;

		private int _program;
		private int mPositionHandle;
		private int _colorHandle;
		private int _MVPMatrixHandle;

		private int _imageHandle;

		// number of coordinates per vertex in this array
		static int COORDS_PER_VERTEX = 3;
		static float[] squareCoords = {
			-0.5f,  0.5f, 0.0f,   // top left
            -0.5f, -0.5f, 0.0f,   // bottom left
             0.5f, -0.5f, 0.0f,   // bottom right
             0.5f,  0.5f, 0.0f }; // top right

		short[] drawOrder = { 0, 1, 2, 0, 2, 3 }; // order to draw vertices

		int vertexStride = COORDS_PER_VERTEX * 4; // 4 bytes per vertex

		float[] color = { 0.2f, 0.709803922f, 0.898039216f, 1.0f };
		static float[] uvs;

		/**
		 * Sets up the drawing object data for use in an OpenGL ES context.
		 */
		public Image(Context context)
		{
			_context = context;

			uvs = new float[] {
				0.0f, 0.0f,
				0.0f, 1.0f,
				1.0f, 1.0f,
				1.0f, 0.0f
				};

			// The texture buffer
			var bbText = ByteBuffer.AllocateDirect(uvs.Length * 4);
			bbText.Order(ByteOrder.NativeOrder());
			uvBuffer = bbText.AsFloatBuffer();
			uvBuffer.Put(uvs);
			uvBuffer.Position(0);

			// Generate Textures, if more needed, alter these numbers.
			var texturenames = new int[1];
			GLES20.GlGenTextures(1, texturenames, 0);

			// Temporary create a bitmap
			var options = new Android.Graphics.BitmapFactory.Options();
			options.InJustDecodeBounds = true;
			Android.Graphics.BitmapFactory.DecodeResource(_context.Resources, Resource.Drawable.image, options);
			int imageHeight = options.OutHeight;
			int imageWidth = options.OutWidth;
			var imageType = options.OutMimeType;

			var inputStream = _context.Resources.OpenRawResource(Resource.Drawable.image);
			var bmp = Android.Graphics.BitmapFactory.DecodeStream(inputStream);

			//var bmp = Android.Graphics.BitmapFactory.DecodeResource(_context.Resources, Resource.Drawable.image);

			//////
			// Bind texture to texturename
			GLES20.GlActiveTexture(GLES20.GlTexture0);
			GLES20.GlBindTexture(GLES20.GlTexture2d, texturenames[0]);

			// Set filtering
			GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter,
						  GLES20.GlLinear);
			GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter,
						  GLES20.GlLinear);

			// Set wrapping mode
			GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapS,
						  GLES20.GlClampToEdge);
			GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapT,
						  GLES20.GlClampToEdge);

			// Load the bitmap into the bound texture.
			GLUtils.TexImage2D(GLES20.GlTexture2d, 0, bmp, 0);

			// We are done using the bitmap so we should recycle it.
			bmp.Recycle();

			// initialize vertex byte buffer for shape coordinates
			var bb = ByteBuffer.AllocateDirect(
					// (# of coordinate values * 4 bytes per float)
					squareCoords.Length * 4);
			bb.Order(ByteOrder.NativeOrder());
			vertexBuffer = bb.AsFloatBuffer();
			vertexBuffer.Put(squareCoords);
			vertexBuffer.Position(0);

			// initialize byte buffer for the draw list
			var dlb = ByteBuffer.AllocateDirect(
					// (# of coordinate values * 2 bytes per short)
					drawOrder.Length * 2);

			dlb.Order(ByteOrder.NativeOrder());
			drawListBuffer = dlb.AsShortBuffer();
			drawListBuffer.Put(drawOrder);
			drawListBuffer.Position(0);

			// prepare shaders and OpenGL program
			int vertexShader = MyGLRenderer.LoadShader(
					GLES20.GlVertexShader,
					vertexShaderCode);
			int fragmentShader = MyGLRenderer.LoadShader(
					GLES20.GlFragmentShader,
					fragmentShaderCode);

			_program = GLES20.GlCreateProgram();             // create empty OpenGL Program
			GLES20.GlAttachShader(_program, vertexShader);   // add the vertex shader to program
			GLES20.GlAttachShader(_program, fragmentShader); // add the fragment shader to program
			GLES20.GlLinkProgram(_program);                  // create OpenGL program executables

			// TODO: Use this program???


		}

		/**
		 * Encapsulates the OpenGL ES instructions for drawing this shape.
		 *
		 * @param mvpMatrix - The Model View Project matrix in which to draw
		 * this shape.
		 */
		public void Draw(float[] mvpMatrix)
		{
			// Add program to OpenGL environment
			GLES20.GlUseProgram(_program);

			// get handle to vertex shader's vPosition member
			mPositionHandle = GLES20.GlGetAttribLocation(_program, "vPosition");

			// Enable a handle to the triangle vertices
			GLES20.GlEnableVertexAttribArray(mPositionHandle);

			// Prepare the triangle coordinate data
			GLES20.GlVertexAttribPointer(
					mPositionHandle, COORDS_PER_VERTEX,
					GLES20.GlFloat, false,
					vertexStride, vertexBuffer);

			// NO COLOR SINCE WE HAVE A TEXTURE

			//// get handle to fragment shader's vColor member
			//_colorHandle = GLES20.GlGetUniformLocation(_program, "vColor");

			//// Set color for drawing the triangle
			//GLES20.GlUniform4fv(_colorHandle, 1, color, 0);

			// But texture COORD:
			// Get handle to texture coordinates location
			var mTexCoordLoc = GLES20.GlGetAttribLocation(_program, "a_texCoord");

			// Enable generic vertex attribute array
			GLES20.GlEnableVertexAttribArray(mTexCoordLoc);

			// Prepare the texturecoordinates
			GLES20.GlVertexAttribPointer(mTexCoordLoc, 2, GLES20.GlFloat,
					false,
					0, uvBuffer);

			// get handle to shape's transformation matrix
			_MVPMatrixHandle = GLES20.GlGetUniformLocation(_program, "uMVPMatrix");
			MyGLRenderer.CheckGlError("GlGetUniformLocation");

			// Apply the projection and view transformation
			GLES20.GlUniformMatrix4fv(_MVPMatrixHandle, 1, false, mvpMatrix, 0);
			MyGLRenderer.CheckGlError("glUniformMatrix4fv");

			// Get handle to textures locations
			int mSamplerLoc = GLES20.GlGetUniformLocation(_program, "s_texture");

			// Set the sampler texture unit to 0, where we have saved the texture.
			GLES20.GlUniform1i(mSamplerLoc, 0);

			// Draw the square
			GLES20.GlDrawElements(
					GLES20.GlTriangles, drawOrder.Length,
					GLES20.GlUnsignedShort, drawListBuffer);

			// Disable vertex array
			GLES20.GlDisableVertexAttribArray(mPositionHandle);
			GLES20.GlDisableVertexAttribArray(mTexCoordLoc);
		}
	}
}
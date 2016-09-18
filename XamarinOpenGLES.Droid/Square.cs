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


using Android.Opengl;
using Java.Nio;
namespace XamarinOpenGL.Droid
{
	/**
	* A two-dimensional square for use as a drawn object in OpenGL ES 2.0.
*/
	public class Square
	{

		string vertexShaderCode =
				// This matrix member variable provides a hook to manipulate
				// the coordinates of the objects that use this vertex shader
				"uniform mat4 uMVPMatrix;" +
				"attribute vec4 vPosition;" +
				"void main() {" +
				// The matrix must be included as a modifier of gl_Position.
				// Note that the uMVPMatrix factor *must be first* in order
				// for the matrix multiplication product to be correct.
				"  gl_Position = uMVPMatrix * vPosition;" +
				"}";

		string fragmentShaderCode =
				"precision mediump float;" +
				"uniform vec4 vColor;" +
				"void main() {" +
				"  gl_FragColor = vColor;" +
				"}";

		private FloatBuffer vertexBuffer;
		private ShortBuffer drawListBuffer;
		private int _program;
		private int mPositionHandle;
		private int _colorHandle;
		private int _MVPMatrixHandle;

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

		/**
		 * Sets up the drawing object data for use in an OpenGL ES context.
		 */
		public Square()
		{
			// initialize vertex byte buffer for shape coordinates
			ByteBuffer bb = ByteBuffer.AllocateDirect(
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

			// get handle to fragment shader's vColor member
			_colorHandle = GLES20.GlGetUniformLocation(_program, "vColor");

			// Set color for drawing the triangle
			GLES20.GlUniform4fv(_colorHandle, 1, color, 0);

			// get handle to shape's transformation matrix
			_MVPMatrixHandle = GLES20.GlGetUniformLocation(_program, "uMVPMatrix");
			MyGLRenderer.CheckGlError("GlGetUniformLocation");

			// Apply the projection and view transformation
			GLES20.GlUniformMatrix4fv(_MVPMatrixHandle, 1, false, mvpMatrix, 0);
			MyGLRenderer.CheckGlError("glUniformMatrix4fv");

			// Draw the square
			GLES20.GlDrawElements(
					GLES20.GlTriangles, drawOrder.Length,
					GLES20.GlUnsignedShort, drawListBuffer);

			// Disable vertex array
			GLES20.GlDisableVertexAttribArray(mPositionHandle);
		}
	}
}
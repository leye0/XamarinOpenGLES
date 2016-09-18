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

//import android.opengl.GLES20;


using Android.Opengl;
using Java.Nio;
namespace XamarinOpenGL.Droid
{
	/**
	* A two-dimensional triangle for use as a drawn object in OpenGL ES 2.0.
*/
	public class Triangle
	{

		string vertexShaderCode =
				// This matrix member variable provides a hook to manipulate
				// the coordinates of the objects that use this vertex shader
				"uniform mat4 uMVPMatrix;" +
				"attribute vec4 vPosition;" +
				"void main() {" +
				// the matrix must be included as a modifier of gl_Position
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

		FloatBuffer vertexBuffer;
		int _program;
		int _positionHandle;
		int _colorHandle;
		int _MVPMatrixHandle;

		// number of coordinates per vertex in this array
		int COORDS_PER_VERTEX = 3;
		float[] triangleCoords = {
            // in counterclockwise order:
            0.0f,  0.622008459f, 0.0f,   // top
           -0.5f, -0.311004243f, 0.0f,   // bottom left
            0.5f, -0.311004243f, 0.0f    // bottom right
    };

		int vertexCount = 3; // triangleCoords.length / COORDS_PER_VERTEX;
		int vertexStride = 12;// COORDS_PER_VERTEX * 4; // 4 bytes per vertex

		float[] color = { 0.63671875f, 0.76953125f, 0.22265625f, 0.0f };

		/**
		 * Sets up the drawing object data for use in an OpenGL ES context.
		 */
		public Triangle()
		{
			// initialize vertex byte buffer for shape coordinates
			ByteBuffer bb = ByteBuffer.AllocateDirect(
					// (number of coordinate values * 4 bytes per float)
					triangleCoords.Length * 4);
			// use the device hardware's native byte order
			bb.Order(ByteOrder.NativeOrder());

			// create a floating point buffer from the ByteBuffer
			vertexBuffer = bb.AsFloatBuffer();
			// add the coordinates to the FloatBuffer
			vertexBuffer.Put(triangleCoords);
			// set the buffer to read the first coordinate
			vertexBuffer.Position(0);

			// prepare shaders and OpenGL program
			int vertexShader = MyGLRenderer.LoadShader(
					GLES20.GlVertexShader, vertexShaderCode);
			int fragmentShader = MyGLRenderer.LoadShader(
					GLES20.GlFragmentShader, fragmentShaderCode);

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
			_positionHandle = GLES20.GlGetAttribLocation(_program, "vPosition");

			// Enable a handle to the triangle vertices
			GLES20.GlEnableVertexAttribArray(_positionHandle);

			// Prepare the triangle coordinate data
			GLES20.GlVertexAttribPointer(
					_positionHandle, COORDS_PER_VERTEX,
					GLES20.GlFloat, false,
					vertexStride, vertexBuffer);

			// get handle to fragment shader's vColor member
			_colorHandle = GLES20.GlGetUniformLocation(_program, "vColor");

			// Set color for drawing the triangle
			GLES20.GlUniform4fv(_colorHandle, 1, color, 0);

			// get handle to shape's transformation matrix
			_MVPMatrixHandle = GLES20.GlGetUniformLocation(_program, "uMVPMatrix");
			MyGLRenderer.CheckGlError("glGetUniformLocation");

			// Apply the projection and view transformation
			GLES20.GlUniformMatrix4fv(_MVPMatrixHandle, 1, false, mvpMatrix, 0);
			MyGLRenderer.CheckGlError("glUniformMatrix4fv");

			// Draw the triangle
			GLES20.GlDrawArrays(GLES20.GlTriangles, 0, vertexCount);

			// Disable vertex array
			GLES20.GlDisableVertexAttribArray(_positionHandle);
		}
	}
}

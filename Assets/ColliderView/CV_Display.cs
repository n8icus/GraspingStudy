/*****************************************************************************/
/* ColliderView                                                              */
/* support@project-jack.com                                                  */
/*                                                                           */
/* Module Description:                                                       */
/*                                                                           */
/* Main rendering functionality for Collider primitives. Sets up the shaders */
/* and provides access to GL Quad and Triangle routines.                     */
/*                                                                           */
/* Copyright © 2014-2015 project|JACK, LLC                                   */
/*****************************************************************************/

                           /* MODIFICATION LOG */
/*****************************************************************************/
/*  Date     * Who     * Comment                                             */
/*---------------------------------------------------------------------------*/
/*  1/28/14  * Austin  * Initial beta release                                */
/*****************************************************************************/

using UnityEngine;

namespace ColliderView
{
	[System.Flags]
	public enum Options
	{
		None			= 0,
		NoDepthTest		= ( 1 << 0 )
	}

	static class Display
	{
		public const float ALPHA_DEFAULT_EDGE = 1f;
		public const float ALPHA_DEFAULT_FACE = 0.2f;

		private static Material material = null;
		private static Material materialNoDepthTest = null;
		
		private static Color lastColor = Color.clear;
		private static int lastFrame = -1;
		private static bool lastNoDepth = false;
		private static float lastAlpha = 0f;

		public static bool SetColor( Color color, float alpha, bool noDepthTest )
		{
			if ( Mathf.Approximately( alpha, 0f ) )
			{
				return false;
			}

			if ( lastFrame == Time.frameCount )
			{
				if ( color == lastColor && alpha == lastAlpha && lastNoDepth == noDepthTest )
				{ 
					return true;
				}
			}

			lastFrame = Time.frameCount;
			lastColor = color;
			lastAlpha = alpha;
			lastNoDepth = noDepthTest;

			if ( material == null )
			{
				material = new Material( Shader.Find( "ColliderView" ) );
				materialNoDepthTest = new Material( Shader.Find( "ColliderViewNoDepth" ) );
			}

			if ( noDepthTest )
			{
				materialNoDepthTest.color = new Color( color.r, color.g, color.b, alpha );
				materialNoDepthTest.SetPass( 0 );
			}
			else
			{
				material.color = new Color( color.r, color.g, color.b, alpha );
				material.SetPass( 0 );
			}

			return true;
		}

		public static bool IsSet( Options options, Options flag )
		{
			return ( ( options & flag ) != Options.None );
		}

		// caller needs to set the GL immediate drawing mode and material before calling these render functions
		public static void RenderLineList( Vector2 [] verts )
		{
			for ( int i = 0; i < verts.Length; i++ )
			{ 
				GL.Vertex( verts[i] );
				GL.Vertex( verts[ (i + 1) % verts.Length ] );
			}
		}

		public static void RenderWireframeQuads( Vector3 [] verts, int [] quads )
		{
			for ( int i = 0; i < quads.Length; i += 4 )
			{
				Vector3 v0 = verts[ quads[i + 0] ];
				Vector3 v1 = verts[ quads[i + 1] ];
				Vector3 v2 = verts[ quads[i + 2] ];
				Vector3 v3 = verts[ quads[i + 3] ];

				GL.Vertex( v0 );
				GL.Vertex( v1 );
				GL.Vertex( v1 );
				GL.Vertex( v2 );
				GL.Vertex( v2 );
				GL.Vertex( v3 );
				GL.Vertex( v3 );
				GL.Vertex( v0 );
			}
		}

		public static void RenderWireframeQuads( Vector2 [] verts, int [] quads )
		{
			for ( int i = 0; i < quads.Length; i += 4 )
			{
				Vector2 v0 = verts[ quads[i + 0] ];
				Vector2 v1 = verts[ quads[i + 1] ];
				Vector2 v2 = verts[ quads[i + 2] ];
				Vector2 v3 = verts[ quads[i + 3] ];

				GL.Vertex( v0 );
				GL.Vertex( v1 );
				GL.Vertex( v1 );
				GL.Vertex( v2 );
				GL.Vertex( v2 );
				GL.Vertex( v3 );
				GL.Vertex( v3 );
				GL.Vertex( v0 );
			}
		}

		public static void RenderSolidQuads( Vector3 [] verts, int [] quads )
		{
			for ( int i = 0; i < quads.Length; i += 4 )
			{
				Vector3 v0 = verts[ quads[i + 0] ];
				Vector3 v1 = verts[ quads[i + 1] ];
				Vector3 v2 = verts[ quads[i + 2] ];
				Vector3 v3 = verts[ quads[i + 3] ];

				GL.Vertex( v0 );
				GL.Vertex( v1 );
				GL.Vertex( v2 );
				GL.Vertex( v3 );
			}
		}

		public static void RenderSolidQuads( Vector2 [] verts, int [] quads )
		{
			for ( int i = 0; i < quads.Length; i += 4 )
			{
				Vector2 v0 = verts[ quads[i + 0] ];
				Vector2 v1 = verts[ quads[i + 1] ];
				Vector2 v2 = verts[ quads[i + 2] ];
				Vector2 v3 = verts[ quads[i + 3] ];

				GL.Vertex( v0 );
				GL.Vertex( v1 );
				GL.Vertex( v2 );
				GL.Vertex( v3 );
			}
		}

		public static void RenderWireframeTris( Vector3 [] verts, int [] tris )
		{
			for ( int i = 0; i < tris.Length; i += 3 )
			{
				Vector3 v0 = verts[ tris[i + 0] ];
				Vector3 v1 = verts[ tris[i + 1] ];
				Vector3 v2 = verts[ tris[i + 2] ];

				GL.Vertex( v0 );
				GL.Vertex( v1 );
				GL.Vertex( v1 );
				GL.Vertex( v2 );
				GL.Vertex( v2 );
				GL.Vertex( v0 );
			}
		}

		public static void RenderSolidTris( Vector3 [] verts, int [] tris )
		{
			for ( int i = 0; i < tris.Length; i += 3 )
			{
				Vector3 v0 = verts[ tris[i + 0] ];
				Vector3 v1 = verts[ tris[i + 1] ];
				Vector3 v2 = verts[ tris[i + 2] ];

				GL.Vertex( v0 );
				GL.Vertex( v1 );
				GL.Vertex( v2 );
			}
		}
	}
}
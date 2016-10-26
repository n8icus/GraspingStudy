/*****************************************************************************/
/* ColliderView                                                              */
/* support@project-jack.com                                                  */
/*                                                                           */
/* Module Description:                                                       */
/*                                                                           */
/* Provides the primitive rendering for the 3D Sphere Collider.              */
/*                                                                           */
/* Copyright © 2014-2015 project|JACK, LLC                                   */
/*****************************************************************************/

                           /* MODIFICATION LOG */
/*****************************************************************************/
/*  Date     * Who     * Comment                                             */
/*---------------------------------------------------------------------------*/
/*  3/30/15  * Austin  * Refactored rendering code path to reduce draw calls */
/*---------------------------------------------------------------------------*/
/* 11/21/14  * Austin  * Support for game object scale (though Unity advises */
/*           *         * against game object scaling for physics)            */
/*---------------------------------------------------------------------------*/
/*  2/15/14  * Austin  * Fixed incorrect world position when center property */
/*           *         * is set                                              */
/*---------------------------------------------------------------------------*/
/*  1/28/14  * Austin  * Initial beta release                                */
/*****************************************************************************/

using UnityEngine;
using ColliderView;
using System.Collections.Generic;

namespace ColliderView
{
	static class Sphere3D
	{
		static readonly Vector3[] verts = new Vector3[]
		{
			new Vector3( -0.382683f, 0.923880f, 0.000000f ),
			new Vector3( -0.707107f, 0.707107f, 0.000000f ),
			new Vector3( -0.923880f, 0.382683f, 0.000000f ),
			new Vector3( -1.000000f, -0.000000f, 0.000000f ),
			new Vector3( -0.923880f, -0.382684f, 0.000000f ),
			new Vector3( -0.707107f, -0.707107f, 0.000000f ),
			new Vector3( -0.382683f, -0.923880f, 0.000000f ),
			new Vector3( -0.353553f, 0.923880f, -0.146447f ),
			new Vector3( -0.653281f, 0.707107f, -0.270598f ),
			new Vector3( -0.853553f, 0.382683f, -0.353553f ),
			new Vector3( -0.923880f, -0.000000f, -0.382683f ),
			new Vector3( -0.853553f, -0.382684f, -0.353553f ),
			new Vector3( -0.653281f, -0.707107f, -0.270598f ),
			new Vector3( -0.353553f, -0.923880f, -0.146447f ),
			new Vector3( -0.270598f, 0.923880f, -0.270598f ),
			new Vector3( -0.500000f, 0.707107f, -0.500000f ),
			new Vector3( -0.653281f, 0.382683f, -0.653281f ),
			new Vector3( -0.707107f, -0.000000f, -0.707107f ),
			new Vector3( -0.653281f, -0.382684f, -0.653281f ),
			new Vector3( -0.500000f, -0.707107f, -0.500000f ),
			new Vector3( -0.270598f, -0.923880f, -0.270598f ),
			new Vector3( -0.000000f, -1.000000f, 0.000000f ),
			new Vector3( -0.146447f, 0.923880f, -0.353553f ),
			new Vector3( -0.270598f, 0.707107f, -0.653281f ),
			new Vector3( -0.353553f, 0.382683f, -0.853553f ),
			new Vector3( -0.382683f, -0.000000f, -0.923879f ),
			new Vector3( -0.353553f, -0.382684f, -0.853553f ),
			new Vector3( -0.270598f, -0.707107f, -0.653281f ),
			new Vector3( -0.146447f, -0.923880f, -0.353553f ),
			new Vector3( -0.000000f, 0.923880f, -0.382683f ),
			new Vector3( -0.000000f, 0.707107f, -0.707107f ),
			new Vector3( -0.000000f, 0.382683f, -0.923879f ),
			new Vector3( -0.000000f, -0.000000f, -1.000000f ),
			new Vector3( -0.000000f, -0.382684f, -0.923879f ),
			new Vector3( -0.000000f, -0.707107f, -0.707107f ),
			new Vector3( -0.000000f, -0.923880f, -0.382683f ),
			new Vector3( -0.000000f, 1.000000f, 0.000000f ),
			new Vector3( 0.146446f, 0.923880f, -0.353553f ),
			new Vector3( 0.270598f, 0.707107f, -0.653281f ),
			new Vector3( 0.353553f, 0.382683f, -0.853553f ),
			new Vector3( 0.382683f, -0.000000f, -0.923879f ),
			new Vector3( 0.353553f, -0.382684f, -0.853553f ),
			new Vector3( 0.270598f, -0.707107f, -0.653281f ),
			new Vector3( 0.146446f, -0.923880f, -0.353553f ),
			new Vector3( 0.270598f, 0.923880f, -0.270598f ),
			new Vector3( 0.500000f, 0.707107f, -0.500000f ),
			new Vector3( 0.653281f, 0.382683f, -0.653281f ),
			new Vector3( 0.707106f, -0.000000f, -0.707106f ),
			new Vector3( 0.653281f, -0.382684f, -0.653281f ),
			new Vector3( 0.500000f, -0.707107f, -0.500000f ),
			new Vector3( 0.270598f, -0.923880f, -0.270598f ),
			new Vector3( 0.353553f, 0.923880f, -0.146446f ),
			new Vector3( 0.653281f, 0.707107f, -0.270598f ),
			new Vector3( 0.853553f, 0.382683f, -0.353553f ),
			new Vector3( 0.923879f, -0.000000f, -0.382683f ),
			new Vector3( 0.853553f, -0.382684f, -0.353553f ),
			new Vector3( 0.653281f, -0.707107f, -0.270598f ),
			new Vector3( 0.353553f, -0.923880f, -0.146446f ),
			new Vector3( 0.382683f, 0.923880f, 0.000000f ),
			new Vector3( 0.707106f, 0.707107f, 0.000000f ),
			new Vector3( 0.923879f, 0.382683f, 0.000000f ),
			new Vector3( 0.999999f, -0.000000f, 0.000000f ),
			new Vector3( 0.923879f, -0.382684f, 0.000000f ),
			new Vector3( 0.707106f, -0.707107f, 0.000000f ),
			new Vector3( 0.382683f, -0.923880f, 0.000000f ),
			new Vector3( 0.353553f, 0.923880f, 0.146447f ),
			new Vector3( 0.653281f, 0.707107f, 0.270598f ),
			new Vector3( 0.853553f, 0.382683f, 0.353553f ),
			new Vector3( 0.923879f, -0.000000f, 0.382683f ),
			new Vector3( 0.853553f, -0.382684f, 0.353553f ),
			new Vector3( 0.653281f, -0.707107f, 0.270598f ),
			new Vector3( 0.353553f, -0.923880f, 0.146447f ),
			new Vector3( 0.270598f, 0.923880f, 0.270598f ),
			new Vector3( 0.500000f, 0.707107f, 0.500000f ),
			new Vector3( 0.653281f, 0.382683f, 0.653281f ),
			new Vector3( 0.707106f, -0.000000f, 0.707107f ),
			new Vector3( 0.653281f, -0.382684f, 0.653281f ),
			new Vector3( 0.500000f, -0.707107f, 0.500000f ),
			new Vector3( 0.270598f, -0.923880f, 0.270598f ),
			new Vector3( 0.146446f, 0.923880f, 0.353553f ),
			new Vector3( 0.270598f, 0.707107f, 0.653281f ),
			new Vector3( 0.353553f, 0.382683f, 0.853553f ),
			new Vector3( 0.382683f, -0.000000f, 0.923879f ),
			new Vector3( 0.353553f, -0.382684f, 0.853553f ),
			new Vector3( 0.270598f, -0.707107f, 0.653281f ),
			new Vector3( 0.146446f, -0.923880f, 0.353553f ),
			new Vector3( -0.000000f, 0.923880f, 0.382683f ),
			new Vector3( -0.000000f, 0.707107f, 0.707107f ),
			new Vector3( -0.000000f, 0.382683f, 0.923879f ),
			new Vector3( -0.000000f, -0.000000f, 0.999999f ),
			new Vector3( -0.000000f, -0.382684f, 0.923879f ),
			new Vector3( -0.000000f, -0.707107f, 0.707107f ),
			new Vector3( -0.000000f, -0.923880f, 0.382683f ),
			new Vector3( -0.146447f, 0.923880f, 0.353553f ),
			new Vector3( -0.270598f, 0.707107f, 0.653281f ),
			new Vector3( -0.353554f, 0.382683f, 0.853553f ),
			new Vector3( -0.382683f, -0.000000f, 0.923879f ),
			new Vector3( -0.353554f, -0.382684f, 0.853553f ),
			new Vector3( -0.270598f, -0.707107f, 0.653281f ),
			new Vector3( -0.146447f, -0.923880f, 0.353553f ),
			new Vector3( -0.270598f, 0.923880f, 0.270598f ),
			new Vector3( -0.500000f, 0.707107f, 0.500000f ),
			new Vector3( -0.653281f, 0.382683f, 0.653281f ),
			new Vector3( -0.707107f, -0.000000f, 0.707106f ),
			new Vector3( -0.653281f, -0.382684f, 0.653281f ),
			new Vector3( -0.500000f, -0.707107f, 0.500000f ),
			new Vector3( -0.270598f, -0.923880f, 0.270598f ),
			new Vector3( -0.353553f, 0.923880f, 0.146446f ),
			new Vector3( -0.653281f, 0.707107f, 0.270598f ),
			new Vector3( -0.853553f, 0.382683f, 0.353553f ),
			new Vector3( -0.923879f, -0.000000f, 0.382683f ),
			new Vector3( -0.853553f, -0.382684f, 0.353553f ),
			new Vector3( -0.653281f, -0.707107f, 0.270598f ),
			new Vector3( -0.353553f, -0.923880f, 0.146446f ),
		};

		static Vector3[] xformVerts = new Vector3[ verts.Length ];

		static readonly int[] quads = new int[]
		{
			6, 5, 12, 13,
			5, 4, 11, 12,
			4, 3, 10, 11,
			3, 2, 9, 10,
			2, 1, 8, 9,
			1, 0, 7, 8,
			13, 12, 19, 20,
			12, 11, 18, 19,
			11, 10, 17, 18,
			10, 9, 16, 17,
			9, 8, 15, 16, 
			8, 7, 14, 15,
			20, 19, 27, 28,
			19, 18, 26, 27,
			18, 17, 25, 26,
			17, 16, 24, 25,
			16, 15, 23, 24,
			15, 14, 22, 23,
			28, 27, 34, 35,
			27, 26, 33, 34,
			26, 25, 32, 33,
			25, 24, 31, 32,
			24, 23, 30, 31,
			23, 22, 29, 30,
			35, 34, 42, 43,
			34, 33, 41, 42,
			33, 32, 40, 41,
			32, 31, 39, 40,
			31, 30, 38, 39,
			30, 29, 37, 38,
			43, 42, 49, 50,
			42, 41, 48, 49,
			41, 40, 47, 48,
			40, 39, 46, 47,
			39, 38, 45, 46,
			38, 37, 44, 45,
			50, 49, 56, 57,
			49, 48, 55, 56,
			48, 47, 54, 55,
			47, 46, 53, 54,
			46, 45, 52, 53,
			45, 44, 51, 52,
			57, 56, 63, 64,
			56, 55, 62, 63,
			55, 54, 61, 62,
			54, 53, 60, 61,
			53, 52, 59, 60,
			52, 51, 58, 59,
			64, 63, 70, 71,
			63, 62, 69, 70,
			62, 61, 68, 69,
			61, 60, 67, 68,
			60, 59, 66, 67,
			59, 58, 65, 66,
			71, 70, 77, 78,
			70, 69, 76, 77,
			69, 68, 75, 76,
			68, 67, 74, 75,
			67, 66, 73, 74,
			66, 65, 72, 73,
			78, 77, 84, 85,
			77, 76, 83, 84,
			76, 75, 82, 83,
			75, 74, 81, 82,
			74, 73, 80, 81,
			73, 72, 79, 80,
			85, 84, 91, 92,
			84, 83, 90, 91,
			83, 82, 89, 90,
			82, 81, 88, 89,
			81, 80, 87, 88,
			80, 79, 86, 87,
			92, 91, 98, 99,
			91, 90, 97, 98,
			90, 89, 96, 97,
			89, 88, 95, 96,
			88, 87, 94, 95,
			87, 86, 93, 94,
			99, 98, 105, 106,
			98, 97, 104, 105,
			97, 96, 103, 104,
			96, 95, 102, 103,
			95, 94, 101, 102,
			94, 93, 100, 101,
			106, 105, 112, 113,
			105, 104, 111, 112,
			104, 103, 110, 111,
			103, 102, 109, 110,
			102, 101, 108, 109,
			101, 100, 107, 108,
			113, 112, 5, 6,
			112, 111, 4, 5,
			111, 110, 3, 4,
			110, 109, 2, 3,
			109, 108, 1, 2,
			108, 107, 0, 1
		};

		static readonly int[] tris = new int[]
		{
			21, 6, 13,
			0, 36, 7,
			21, 13, 20,
			7, 36, 14,
			21, 20, 28,
			14, 36, 22,
			21, 28, 35,
			22, 36, 29,
			21, 35, 43,
			29, 36, 37,
			21, 43, 50,
			37, 36, 44,
			21, 50, 57,
			44, 36, 51,
			21, 57, 64,
			51, 36, 58,
			21, 64, 71,
			58, 36, 65,
			21, 71, 78,
			65, 36, 72,
			21, 78, 85,
			72, 36, 79,
			21, 85, 92,
			79, 36, 86,
			21, 92, 99,
			86, 36, 93,
			21, 99, 106,
			93, 36, 100,
			21, 106, 113,
			100, 36, 107,
			21, 113, 6,
			107, 36, 0
		};

		private static Vector3 Transform( Vector3 vert, SphereCollider collider, float scale )
		{
			Vector3 xform;

			xform.x = collider.center.x + vert.x * collider.radius;
			xform.y = collider.center.y + vert.y * collider.radius;
			xform.z = collider.center.z + vert.z * collider.radius;

			// can't use TransformPoint here as the object scale affects all 3 axes of the sphere
			xform = collider.transform.rotation * xform * scale + collider.transform.position;

			return xform;
		}

		public static void Render( List<Collider> colliders, Color edgeColor, float edgeAlpha, Color faceColor, float faceAlpha, bool noDepth )
		{
			if ( colliders.Count > 0 )
			{
				if ( Display.SetColor( edgeColor, edgeAlpha, noDepth ) )
				{
					RenderLines( colliders );
				}
	
				if ( Display.SetColor( faceColor, faceAlpha, noDepth ) )
				{
					RenderQuads( colliders );
					RenderTris( colliders );
				}
			}
		}

		private static void TransformGeometry( SphereCollider collider )
		{
			float scale = CV_Common.ObjectMaxScale( collider.transform );

			for ( int i = 0; i < verts.Length; i++ )
			{
				xformVerts[i] = Transform( verts[i], collider, scale );
			}
		}

		private static void RenderLines( List<Collider> colliders )
		{
			GL.Begin( GL.LINES );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					TransformGeometry( colliders[i] as SphereCollider );

					Display.RenderWireframeQuads( xformVerts, quads );
					Display.RenderWireframeTris( xformVerts, tris );
				}
			GL.End();
		}

		private static void RenderQuads( List<Collider> colliders )
		{
			GL.Begin( GL.QUADS );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					TransformGeometry( colliders[i] as SphereCollider );
					Display.RenderSolidQuads( xformVerts, quads );
				}
			GL.End();
		}

		private static void RenderTris( List<Collider> colliders )
		{
			GL.Begin( GL.TRIANGLES );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					TransformGeometry( colliders[i] as SphereCollider );
					Display.RenderSolidTris( xformVerts, tris );
				}
			GL.End();
		}
	}
}
/*****************************************************************************/
/* ColliderView                                                              */
/* support@project-jack.com                                                  */
/*                                                                           */
/* Module Description:                                                       */
/*                                                                           */
/* Provides the primitive rendering for the 3D Capsule Collider.             */
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
/*  1/28/14  * Austin  * Initial beta release                                */
/*****************************************************************************/

using UnityEngine;
using ColliderView;
using System.Collections.Generic;

namespace ColliderView
{
	static class Capsule3D
	{
		static readonly Vector3[] cylinder = new Vector3[]
		{
			new Vector3( 0.000000f, -0.500000f, -1.000000f ),
			new Vector3( 0.000000f, 0.500000f, -1.000000f ),
			new Vector3( 0.382683f, -0.500000f, -0.923880f ),
			new Vector3( 0.382683f, 0.500000f, -0.923880f ),
			new Vector3( 0.707107f, -0.500000f, -0.707107f ),
			new Vector3( 0.707107f, 0.500000f, -0.707107f ),
			new Vector3( 0.923880f, -0.500000f, -0.382683f ),
			new Vector3( 0.923880f, 0.500000f, -0.382683f ),
			new Vector3( 1.000000f, -0.500000f, 0.000000f ),
			new Vector3( 1.000000f, 0.500000f, 0.000000f ),
			new Vector3( 0.923880f, -0.500000f, 0.382684f ),
			new Vector3( 0.923880f, 0.500000f, 0.382684f ),
			new Vector3( 0.707107f, -0.500000f, 0.707107f ),
			new Vector3( 0.707107f, 0.500000f, 0.707107f ),
			new Vector3( 0.382683f, -0.500000f, 0.923880f ),
			new Vector3( 0.382683f, 0.500000f, 0.923880f ),
			new Vector3( 0.000000f, -0.500000f, 1.000000f ),
			new Vector3( 0.000000f, 0.500000f, 1.000000f ),
			new Vector3( -0.382683f, -0.500000f, 0.923880f ),
			new Vector3( -0.382683f, 0.500000f, 0.923880f ),
			new Vector3( -0.707107f, -0.500000f, 0.707107f ),
			new Vector3( -0.707107f, 0.500000f, 0.707107f ),
			new Vector3( -0.923880f, -0.500000f, 0.382684f ),
			new Vector3( -0.923880f, 0.500000f, 0.382684f ),
			new Vector3( -1.000000f, -0.500000f, -0.000000f ),
			new Vector3( -1.000000f, 0.500000f, -0.000000f ),
			new Vector3( -0.923879f, -0.500000f, -0.382684f ),
			new Vector3( -0.923879f, 0.500000f, -0.382684f ),
			new Vector3( -0.707107f, -0.500000f, -0.707107f ),
			new Vector3( -0.707107f, 0.500000f, -0.707107f ),
			new Vector3( -0.382683f, -0.500000f, -0.923880f ),
			new Vector3( -0.382683f, 0.500000f, -0.923880f )
		};

		static Vector3[] xformCylinder = new Vector3[ cylinder.Length ];

		static readonly int[] cylinderQuads = new int[]
		{
			0, 1, 3, 2,
			2, 3, 5, 4,
			4, 5, 7, 6,
			6, 7, 9, 8,
			8, 9, 11, 10,
			10, 11, 13, 12,
			12, 13, 15, 14,
			14, 15, 17, 16,
			16, 17, 19, 18,
			18, 19, 21, 20,
			20, 21, 23, 22,
			22, 23, 25, 24,
			24, 25, 27, 26,
			26, 27, 29, 28,
			28, 29, 31, 30,
			30, 31, 1, 0
		};

		static readonly Vector3[] halfSphere = new Vector3[]
		{
			new Vector3( -0.382683f, 0.923880f, 0.000000f ),
			new Vector3( -0.707107f, 0.707107f, 0.000000f ),
			new Vector3( -0.923880f, 0.382683f, 0.000000f ),
			new Vector3( -1.000000f, -0.000000f, 0.000000f ),
			new Vector3( -0.353553f, 0.923880f, -0.146447f ),
			new Vector3( -0.653281f, 0.707107f, -0.270598f ),
			new Vector3( -0.853553f, 0.382683f, -0.353553f ),
			new Vector3( -0.923880f, -0.000000f, -0.382683f ),
			new Vector3( -0.270598f, 0.923880f, -0.270598f ),
			new Vector3( -0.500000f, 0.707107f, -0.500000f ),
			new Vector3( -0.653281f, 0.382683f, -0.653281f ),
			new Vector3( -0.707107f, -0.000000f, -0.707107f ),
			new Vector3( -0.146447f, 0.923880f, -0.353553f ),
			new Vector3( -0.270598f, 0.707107f, -0.653281f ),
			new Vector3( -0.353553f, 0.382683f, -0.853553f ),
			new Vector3( -0.382683f, -0.000000f, -0.923879f ),
			new Vector3( -0.000000f, 0.923880f, -0.382683f ),
			new Vector3( -0.000000f, 0.707107f, -0.707107f ),
			new Vector3( -0.000000f, 0.382683f, -0.923879f ),
			new Vector3( -0.000000f, -0.000000f, -1.000000f ),
			new Vector3( -0.000000f, 1.000000f, 0.000000f ),
			new Vector3( 0.146446f, 0.923880f, -0.353553f ),
			new Vector3( 0.270598f, 0.707107f, -0.653281f ),
			new Vector3( 0.353553f, 0.382683f, -0.853553f ),
			new Vector3( 0.382683f, -0.000000f, -0.923879f ),
			new Vector3( 0.270598f, 0.923880f, -0.270598f ),
			new Vector3( 0.500000f, 0.707107f, -0.500000f ),
			new Vector3( 0.653281f, 0.382683f, -0.653281f ),
			new Vector3( 0.707106f, -0.000000f, -0.707106f ),
			new Vector3( 0.353553f, 0.923880f, -0.146446f ),
			new Vector3( 0.653281f, 0.707107f, -0.270598f ),
			new Vector3( 0.853553f, 0.382683f, -0.353553f ),
			new Vector3( 0.923879f, -0.000000f, -0.382683f ),
			new Vector3( 0.382683f, 0.923880f, 0.000000f ),
			new Vector3( 0.707106f, 0.707107f, 0.000000f ),
			new Vector3( 0.923879f, 0.382683f, 0.000000f ),
			new Vector3( 0.999999f, -0.000000f, 0.000000f ),
			new Vector3( 0.353553f, 0.923880f, 0.146447f ),
			new Vector3( 0.653281f, 0.707107f, 0.270598f ),
			new Vector3( 0.853553f, 0.382683f, 0.353553f ),
			new Vector3( 0.923879f, -0.000000f, 0.382683f ),
			new Vector3( 0.270598f, 0.923880f, 0.270598f ),
			new Vector3( 0.500000f, 0.707107f, 0.500000f ),
			new Vector3( 0.653281f, 0.382683f, 0.653281f ),
			new Vector3( 0.707106f, -0.000000f, 0.707107f ),
			new Vector3( 0.146446f, 0.923880f, 0.353553f ),
			new Vector3( 0.270598f, 0.707107f, 0.653281f ),
			new Vector3( 0.353553f, 0.382683f, 0.853553f ),
			new Vector3( 0.382683f, -0.000000f, 0.923879f ),
			new Vector3( -0.000000f, 0.923880f, 0.382683f ),
			new Vector3( -0.000000f, 0.707107f, 0.707107f ),
			new Vector3( -0.000000f, 0.382683f, 0.923879f ),
			new Vector3( -0.000000f, -0.000000f, 0.999999f ),
			new Vector3( -0.146447f, 0.923880f, 0.353553f ),
			new Vector3( -0.270598f, 0.707107f, 0.653281f ),
			new Vector3( -0.353554f, 0.382683f, 0.853553f ),
			new Vector3( -0.382683f, -0.000000f, 0.923879f ),
			new Vector3( -0.270598f, 0.923880f, 0.270598f ),
			new Vector3( -0.500000f, 0.707107f, 0.500000f ),
			new Vector3( -0.653281f, 0.382683f, 0.653281f ),
			new Vector3( -0.707107f, -0.000000f, 0.707106f ),
			new Vector3( -0.353553f, 0.923880f, 0.146446f ),
			new Vector3( -0.653281f, 0.707107f, 0.270598f ),
			new Vector3( -0.853553f, 0.382683f, 0.353553f ),
			new Vector3( -0.923879f, -0.000000f, 0.382683f ),
		};

		static Vector3[] xformHalfSphere = new Vector3[ halfSphere.Length ];
		static Vector3[] xformHalfSphereFlipped = new Vector3[ halfSphere.Length ];

		static readonly int[] halfSphereQuads = new int[]
		{
			3, 2, 6, 7,
			2, 1, 5, 6,
			1, 0, 4, 5,
			7, 6, 10, 11,
			6, 5, 9, 10,
			5, 4, 8, 9,
			11, 10, 14, 15,
			10, 9, 13, 14,
			9, 8, 12, 13,
			15, 14, 18, 19,
			14, 13, 17, 18,
			13, 12, 16, 17,
			19, 18, 23, 24,
			18, 17, 22, 23,
			17, 16, 21, 22,
			24, 23, 27, 28,
			23, 22, 26, 27,
			22, 21, 25, 26,
			28, 27, 31, 32,
			27, 26, 30, 31,
			26, 25, 29, 30,
			32, 31, 35, 36,
			31, 30, 34, 35,
			30, 29, 33, 34,
			36, 35, 39, 40,
			35, 34, 38, 39,
			34, 33, 37, 38,
			40, 39, 43, 44,
			39, 38, 42, 43,
			38, 37, 41, 42,
			44, 43, 47, 48,
			43, 42, 46, 47,
			42, 41, 45, 46,
			48, 47, 51, 52,
			47, 46, 50, 51,
			46, 45, 49, 50,
			52, 51, 55, 56,
			51, 50, 54, 55,
			50, 49, 53, 54,
			56, 55, 59, 60,
			55, 54, 58, 59,
			54, 53, 57, 58,
			60, 59, 63, 64,
			59, 58, 62, 63,
			58, 57, 61, 62,
			64, 63, 2, 3,
			63, 62, 1, 2,
			62, 61, 0, 1
		};

		static readonly int[] halfSphereTris = new int[]
		{
			0, 20, 4,
			4, 20, 8,
			8, 20, 12,
			12, 20, 16,
			16, 20, 21,
			21, 20, 25,
			25, 20, 29,
			29, 20, 33,
			33, 20, 37,
			37, 20, 41,
			41, 20, 45,
			45, 20, 49,
			49, 20, 53,
			53, 20, 57,
			57, 20, 61,
			61, 20, 0
		};

		private const int DIRECTION_X_AXIS = 0;
		private const int DIRECTION_Y_AXIS = 1;
		private const int DIRECTION_Z_AXIS = 2;

		private static Vector3 CylinderScale( CapsuleCollider collider, float fOffset )
		{
			float radiusScale;
			float cylinderScale;
			CV_Common.CapsuleObjectScale( collider.transform, out radiusScale, out cylinderScale );

			float radius = collider.radius * radiusScale;
			float height = fOffset * 2.0f;

			switch( collider.direction )
			{
				case DIRECTION_X_AXIS:
					return new Vector3( height, radius, radius );
				
				case DIRECTION_Y_AXIS:
					return new Vector3( radius, height, radius );

				case DIRECTION_Z_AXIS:
					return new Vector3( radius, radius, height );
			}

			return Vector3.zero;
		}

		private static Quaternion CylinderQuat( CapsuleCollider collider )
		{
			switch( collider.direction )
			{
				case DIRECTION_X_AXIS:
					return Quaternion.AngleAxis( 90.0f, Vector3.forward );
				
				case DIRECTION_Y_AXIS:
					return Quaternion.AngleAxis( 90.0f, Vector3.up );

				case DIRECTION_Z_AXIS:
					return Quaternion.AngleAxis( 90.0f, Vector3.right );
			}

			return Quaternion.identity;
		}

		private static Vector3 TransformCylinderVert( Vector3 vert, CapsuleCollider collider, Vector3 scale, Quaternion q )
		{
			Vector3 xform = q * vert;
			xform.x = collider.center.x + xform.x * scale.x;
			xform.y = collider.center.y + xform.y * scale.y;
			xform.z = collider.center.z + xform.z * scale.z;

			xform = collider.transform.rotation * xform + collider.transform.position;
			return xform;
		}

		private static Quaternion HalfSphereQuat( CapsuleCollider collider )
		{
			switch( collider.direction )
			{
				case DIRECTION_X_AXIS:
					return Quaternion.AngleAxis( 270.0f, Vector3.forward );
				
				case DIRECTION_Y_AXIS:
					return Quaternion.AngleAxis( 0.0f, Vector3.right );

				case DIRECTION_Z_AXIS:
					return Quaternion.AngleAxis( -270.0f, Vector3.right );
			}

			return Quaternion.identity;
		}

		private static Quaternion HalfSphereQuatFlipped( CapsuleCollider collider )
		{
			switch( collider.direction )
			{
				case DIRECTION_X_AXIS:
					return Quaternion.AngleAxis( -270.0f, Vector3.forward );
				
				case DIRECTION_Y_AXIS:
					return Quaternion.AngleAxis( 180.0f, Vector3.right );

				case DIRECTION_Z_AXIS:
					return Quaternion.AngleAxis( 270.0f, Vector3.right );
			}

			return Quaternion.identity;
		}

		private static float HalfSphereOffset( CapsuleCollider collider, out Vector3 vOffset )
		{
			float radiusScale;
			float cylinderScale;
			CV_Common.CapsuleObjectScale( collider.transform, out radiusScale, out cylinderScale );

			float radius = collider.radius * radiusScale;
			float offset = ( collider.height / 2.0f ) - radius;
			offset = (cylinderScale * radius) + (cylinderScale * offset) - radius;
			offset = Mathf.Max( 0f, offset );

			vOffset = Vector3.zero;

			switch( collider.direction )
			{
				case DIRECTION_X_AXIS:
					vOffset = new Vector3( offset, 0.0f, 0.0f );
				break;
				
				case DIRECTION_Y_AXIS:
					vOffset = new Vector3( 0.0f, offset, 0.0f );
				break;

				case DIRECTION_Z_AXIS:
					vOffset = new Vector3( 0.0f, 0.0f, offset );
				break;
			}

			return offset;
		}

		private static Vector3 TransformHalfSphereVert( Vector3 vert, CapsuleCollider collider, Quaternion q, Vector3 offset, float radius )
		{
			Vector3 center = collider.center;

			Vector3 xform = q * vert;
			xform.x = center.x + offset.x + xform.x * radius;
			xform.y = center.y + offset.y + xform.y * radius;
			xform.z = center.z + offset.z + xform.z * radius;

			xform = collider.transform.rotation * xform + collider.transform.position;
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

		private static void TransformGeometry( CapsuleCollider collider )
		{
			Vector3 vOffset;
			float fOffset = HalfSphereOffset( collider, out vOffset );
			Vector3 vOffsetFlipped = vOffset * -1.0f;
			
			Vector3 scale = CylinderScale( collider, fOffset ); 
			Quaternion q = CylinderQuat( collider );

			for ( int i = 0; i < cylinder.Length; i++ )
			{
				xformCylinder[i] = TransformCylinderVert( cylinder[i], collider, scale, q );
			}

			Quaternion halfRot = HalfSphereQuat( collider );
			Quaternion halfRotFlipped = HalfSphereQuatFlipped( collider );

			float radiusScale;
			float cylinderScale;
			CV_Common.CapsuleObjectScale( collider.transform, out radiusScale, out cylinderScale );
			float radius = collider.radius * radiusScale;

			for ( int i = 0; i < halfSphere.Length; i++ )
			{
				xformHalfSphere[i] = TransformHalfSphereVert( halfSphere[i], collider, halfRot, vOffset, radius );
				xformHalfSphereFlipped[i] = TransformHalfSphereVert( halfSphere[i], collider, halfRotFlipped, vOffsetFlipped, radius );
			}
		}

		private static void RenderLines( List<Collider> colliders )
		{
			GL.Begin( GL.LINES );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					TransformGeometry( colliders[i] as CapsuleCollider );

					Display.RenderWireframeQuads( xformCylinder, cylinderQuads );
					Display.RenderWireframeQuads( xformHalfSphere, halfSphereQuads );
					Display.RenderWireframeQuads( xformHalfSphereFlipped, halfSphereQuads );
					Display.RenderWireframeTris( xformHalfSphere, halfSphereTris );
					Display.RenderWireframeTris( xformHalfSphereFlipped, halfSphereTris );
				}
			GL.End();
		}

		private static void RenderQuads( List<Collider> colliders )
		{
			GL.Begin( GL.QUADS );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					TransformGeometry( colliders[i] as CapsuleCollider );

					Display.RenderSolidQuads( xformCylinder, cylinderQuads );
					Display.RenderSolidQuads( xformHalfSphere, halfSphereQuads );
					Display.RenderSolidQuads( xformHalfSphereFlipped, halfSphereQuads );
				}
			GL.End();
		}

		private static void RenderTris( List<Collider> colliders )
		{
			GL.Begin( GL.TRIANGLES );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					TransformGeometry( colliders[i] as CapsuleCollider );

					Display.RenderSolidTris( xformHalfSphere, halfSphereTris );
					Display.RenderSolidTris( xformHalfSphereFlipped, halfSphereTris );
				}
			GL.End();
		}
	}
}
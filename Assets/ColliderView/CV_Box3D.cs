/*****************************************************************************/
/* ColliderView                                                              */
/* support@project-jack.com                                                  */
/*                                                                           */
/* Module Description:                                                       */
/*                                                                           */
/* Provides the primitive rendering for the 3D Box Collider.                 */
/*                                                                           */
/* Copyright © 2014-2015 project|JACK, LLC                                   */
/*****************************************************************************/

                           /* MODIFICATION LOG */
/*****************************************************************************/
/*  Date     * Who     * Comment                                             */
/*---------------------------------------------------------------------------*/
/*  3/30/15  * Austin  * Refactored rendering code path to reduce draw calls */
/*---------------------------------------------------------------------------*/
/* 12/30/14  * Austin  * Added function to render Bounds (AABB)              */
/*---------------------------------------------------------------------------*/
/*  1/28/14  * Austin  * Initial beta release                                */
/*****************************************************************************/

using UnityEngine;
using ColliderView;
using System.Collections.Generic;

namespace ColliderView
{
	static class Box3D
	{
		static readonly Vector3[] verts = new Vector3[]
		{
			new Vector3(  0.5f, -0.5f, -0.5f ),
			new Vector3(  0.5f, -0.5f,  0.5f ),
			new Vector3( -0.5f, -0.5f,  0.5f ),
			new Vector3( -0.5f, -0.5f, -0.5f ),
			new Vector3(  0.5f,  0.5f, -0.5f ),
			new Vector3(  0.5f,  0.5f,  0.5f ),
			new Vector3( -0.5f,  0.5f,  0.5f ),
			new Vector3( -0.5f,  0.5f, -0.5f )
		};
	
		static readonly int[] quads = new int[]
		{
			0, 1, 2, 3,
			4, 7, 6, 5,
			0, 4, 5, 1,
			1, 5, 6, 2,
			2, 6, 7, 3,
			4, 0, 3, 7
		};
	
		static Vector3[] xformVerts = new Vector3[ verts.Length ];
	
		private static Vector3 Transform( Vector3 vert, BoxCollider collider )
		{
			Vector3 xform;
	
			xform.x = collider.center.x + collider.size.x * vert.x;
			xform.y = collider.center.y + collider.size.y * vert.y;
			xform.z = collider.center.z + collider.size.z * vert.z;
			xform = collider.transform.TransformPoint( xform );
	
			return xform;
		}

		private static Vector3 Transform( Vector3 vert, Bounds bounds )
		{
			return ( bounds.center + Vector3.Scale( vert, bounds.size ) );
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
				}
			}
		}

		private static void TransformGeometry( BoxCollider collider )
		{
			xformVerts[0] = Transform( verts[0], collider );
			xformVerts[1] = Transform( verts[1], collider );
			xformVerts[2] = Transform( verts[2], collider );
			xformVerts[3] = Transform( verts[3], collider );
			xformVerts[4] = Transform( verts[4], collider );
			xformVerts[5] = Transform( verts[5], collider );
			xformVerts[6] = Transform( verts[6], collider );
			xformVerts[7] = Transform( verts[7], collider );
		}

		private static void RenderLines( List<Collider> colliders )
		{
			GL.Begin( GL.LINES );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					TransformGeometry( colliders[i] as BoxCollider );
					Display.RenderWireframeQuads( xformVerts, quads );
				}
			GL.End();
		}

		private static void RenderQuads( List<Collider> colliders )
		{
			GL.Begin( GL.QUADS );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					TransformGeometry( colliders[i] as BoxCollider );
					Display.RenderSolidQuads( xformVerts, quads );
				}
			GL.End();
		}

		public static void RenderLines( Bounds bounds )
		{
			xformVerts[0] = Transform( verts[0], bounds );
			xformVerts[1] = Transform( verts[1], bounds );
			xformVerts[2] = Transform( verts[2], bounds );
			xformVerts[3] = Transform( verts[3], bounds );
			xformVerts[4] = Transform( verts[4], bounds );
			xformVerts[5] = Transform( verts[5], bounds );
			xformVerts[6] = Transform( verts[6], bounds );
			xformVerts[7] = Transform( verts[7], bounds );

			Display.RenderWireframeQuads( xformVerts, quads );
		}

		public static void RenderQuads( Bounds bounds )
		{
			xformVerts[0] = Transform( verts[0], bounds );
			xformVerts[1] = Transform( verts[1], bounds );
			xformVerts[2] = Transform( verts[2], bounds );
			xformVerts[3] = Transform( verts[3], bounds );
			xformVerts[4] = Transform( verts[4], bounds );
			xformVerts[5] = Transform( verts[5], bounds );
			xformVerts[6] = Transform( verts[6], bounds );
			xformVerts[7] = Transform( verts[7], bounds );

			Display.RenderSolidQuads( xformVerts, quads );
		}
	}
}
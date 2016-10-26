/*****************************************************************************/
/* ColliderView                                                              */
/* support@project-jack.com                                                  */
/*                                                                           */
/* Module Description:                                                       */
/*                                                                           */
/* Provides the primitive rendering for the 2D Box Collider.                 */
/*                                                                           */
/* Copyright © 2014-2015 project|JACK, LLC                                   */
/*****************************************************************************/

                           /* MODIFICATION LOG */
/*****************************************************************************/
/*  Date     * Who     * Comment                                             */
/*---------------------------------------------------------------------------*/
/*  3/30/15  * Austin  * Refactored rendering code path to reduce draw calls */
/*---------------------------------------------------------------------------*/
/*  3/1/15   * Austin  * Compile fix for Unity 5                             */
/*---------------------------------------------------------------------------*/
/*  1/28/14  * Austin  * Initial beta release                                */
/*****************************************************************************/

using UnityEngine;
using ColliderView;
using System.Collections.Generic;

namespace ColliderView
{
	static class Box2D
	{
		static readonly Vector2[] verts = new Vector2[]
		{
			new Vector2(  0.5f, -0.5f ),
			new Vector2( -0.5f, -0.5f ),
			new Vector2( -0.5f,  0.5f ),
			new Vector2(  0.5f,  0.5f )
		};
	
		static readonly int[] quads = new int[]
		{
			0, 1, 2, 3
		};
	
		static Vector2[] xformVerts = new Vector2[ verts.Length ];
	
		private static Vector2 Transform( Vector2 vert, BoxCollider2D collider )
		{
			Vector2 xform;
			Vector2 center;
	
#if ( UNITY_5 )
			center = collider.offset;
#else
			center = collider.center;
#endif
			xform.x = center.x + collider.size.x * vert.x;
			xform.y = center.y + collider.size.y * vert.y;
			xform = collider.transform.TransformPoint( xform );
	
			return xform;
		}

		private static void TransformGeometry( BoxCollider2D collider )
		{
			xformVerts[0] = Transform( verts[0], collider );
			xformVerts[1] = Transform( verts[1], collider );
			xformVerts[2] = Transform( verts[2], collider );
			xformVerts[3] = Transform( verts[3], collider );
		}

		public static void Render( List<Collider2D> colliders, Color edgeColor, float edgeAlpha, Color faceColor, float faceAlpha, bool noDepth )
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

		private static void RenderLines( List<Collider2D> colliders )
		{
			GL.Begin( GL.LINES );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					TransformGeometry( colliders[i] as BoxCollider2D );
					Display.RenderWireframeQuads( xformVerts, quads );
				}
			GL.End();
		}

		private static void RenderQuads( List<Collider2D> colliders )
		{
			GL.Begin( GL.QUADS );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					TransformGeometry( colliders[i] as BoxCollider2D );
					Display.RenderSolidQuads( xformVerts, quads );
				}
			GL.End();
		}
	}
}
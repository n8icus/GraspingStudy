/*****************************************************************************/
/* ColliderView                                                              */
/* support@project-jack.com                                                  */
/*                                                                           */
/* Module Description:                                                       */
/*                                                                           */
/* Provides the primitive rendering for the 2D Polygon Collider.             */
/*                                                                           */
/* Copyright © 2014-2015 project|JACK, LLC                                   */
/*****************************************************************************/

                           /* MODIFICATION LOG */
/*****************************************************************************/
/*  Date     * Who     * Comment                                             */
/*---------------------------------------------------------------------------*/
/*  3/30/15  * Austin  * Refactored rendering code path to reduce draw calls */
/*---------------------------------------------------------------------------*/
/*  1/28/14  * Austin  * Initial beta release                                */
/*****************************************************************************/

using UnityEngine;
using ColliderView;
using System.Collections.Generic;

namespace ColliderView
{
	static class Mesh2D
	{
		public static void Render( List<Collider2D> colliders, Color edgeColor, float edgeAlpha, Color faceColor, float faceAlpha, bool noDepth )
		{
			if ( colliders.Count > 0 )
			{
				if ( Display.SetColor( edgeColor, edgeAlpha, noDepth ) )
				{
					RenderLines( colliders );
				}
			}
		}

		private static void RenderLines( List<Collider2D> colliders )
		{
			GL.Begin( GL.LINES );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					RenderPath( colliders[i] as PolygonCollider2D );
				}
			GL.End();
		}

		private static void RenderPath( PolygonCollider2D collider )
		{
			for ( int i = 0; i < collider.pathCount; i++ )
			{
				Vector2[] verts = TransformGeometry( collider, i );
				Display.RenderLineList( verts );
			}
		}

		private static Vector2[] TransformGeometry( PolygonCollider2D collider, int path )
		{
			Transform xform = collider.transform;
			Vector2[] verts = collider.GetPath( path );

			for ( int i = 0; i < verts.Length; i++ )
			{
				verts[i] = xform.TransformPoint( verts[i] );
			}

			return verts;
		}
	}
}
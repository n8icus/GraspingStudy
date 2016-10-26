/*****************************************************************************/
/* ColliderView                                                              */
/* support@project-jack.com                                                  */
/*                                                                           */
/* Module Description:                                                       */
/*                                                                           */
/* Provides the primitive rendering for the 2D Edge Collider.                */
/*                                                                           */
/* Copyright © 2014-2015 project|JACK, LLC                                   */
/*****************************************************************************/

                           /* MODIFICATION LOG */
/*****************************************************************************/
/*  Date     * Who     * Comment                                             */
/*---------------------------------------------------------------------------*/
/*  3/30/15  * Austin  * Refactored rendering code path to reduce draw calls */
/*---------------------------------------------------------------------------*/
/* 12/11/14  * Austin  * Initial release                                     */
/*****************************************************************************/

using UnityEngine;
using ColliderView;
using System.Collections.Generic;

namespace ColliderView
{
	static class Edge2D
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
					Vector2[] points = TransformGeometry( colliders[i] as EdgeCollider2D );

					for ( int j = 0; j < points.Length - 1; j++ )
					{
						GL.Vertex( points[j] );
						GL.Vertex( points[ (j + 1) ] );
					}
				}
			GL.End();
		}

		private static Vector2[] TransformGeometry( EdgeCollider2D collider )
		{
			Transform xform = collider.transform;
			Vector2[] verts = collider.points;

			for ( int i = 0; i < verts.Length; i++ )
			{
				verts[i] = xform.TransformPoint( verts[i] );
			}

			return verts;
		}
	}
}
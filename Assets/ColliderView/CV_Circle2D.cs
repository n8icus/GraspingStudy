/*****************************************************************************/
/* ColliderView                                                              */
/* support@project-jack.com                                                  */
/*                                                                           */
/* Module Description:                                                       */
/*                                                                           */
/* Provides the primitive rendering for the 2D Circle Collider.              */
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
/* 11/21/14  * Austin  * Support for game object scale as used in Unity's 2D */
/*           *         * Platformer example                                  */
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
	static class Circle2D
	{
		private const int NUM_SEGMENTS = 24;
		private static readonly float STEP = 360f / NUM_SEGMENTS;
		private static Vector2[] verts = new Vector2[ NUM_SEGMENTS ];

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
					RenderTris( colliders );
				}
			}
		}

		private static Vector2 TransformGeometry( CircleCollider2D collider )
		{
#if ( UNITY_5 )
			Vector2 center = collider.transform.TransformPoint( collider.offset );
#else
			Vector2 center = collider.transform.TransformPoint( collider.center );
#endif
			
			Vector3 vScale = collider.transform.lossyScale;
			float scale = Mathf.Max( vScale.x, vScale.y );
			float radius = collider.radius * scale;

			float angle = 0f;
			for ( int i = 0; i < verts.Length; i++ )
			{
				float r = angle * Mathf.Deg2Rad;
				float sinR = Mathf.Sin( r );
				float cosR = Mathf.Cos( r );

				float x = center.x + sinR * radius;
 				float y = center.y - cosR * radius;

				verts[i] = new Vector2( x, y );
				angle += STEP;
			}

			return center;
		}

		private static void RenderLines( List<Collider2D> colliders )
		{
			GL.Begin( GL.LINES );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					TransformGeometry( colliders[i] as CircleCollider2D );
					Display.RenderLineList( verts );
				}
			GL.End();
		}

		private static void RenderTris( List<Collider2D> colliders )
		{
			GL.Begin( GL.TRIANGLES );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					Vector2 center = TransformGeometry( colliders[i] as CircleCollider2D );

					for ( int j = 0; j < verts.Length; j++ )
					{
						Vector2 v0 = verts[ j ];
						Vector2 v1 = verts[ (j + 1) % verts.Length ];

						GL.Vertex( center );
						GL.Vertex( v0 );
						GL.Vertex( v1 );
					}
				}
			GL.End();
		}
	}
}
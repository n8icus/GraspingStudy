/*****************************************************************************/
/* ColliderView                                                              */
/* support@project-jack.com                                                  */
/*                                                                           */
/* Module Description:                                                       */
/*                                                                           */
/* Provides the primitive rendering for the 3D Mesh Collider.                */
/*                                                                           */
/* Copyright © 2014-2015 project|JACK, LLC                                   */
/*****************************************************************************/

                           /* MODIFICATION LOG */
/*****************************************************************************/
/*  Date     * Who     * Comment                                             */
/*---------------------------------------------------------------------------*/
/*  4/27/15  * Austin  * Render the AABB of colliders without a mesh set     */
/*---------------------------------------------------------------------------*/
/*  3/30/15  * Austin  * Refactored rendering code path to reduce draw calls.*/
/*           *         * Added static vertex buffer to avoid GC alloc (would */
/*           *         * generate ~4 mb garbage in Republique demo scene)    */
/*---------------------------------------------------------------------------*/
/* 12/30/14  * Austin  * Render the AABB of convex colliders, instead of the */
/*           *         * (misleading) actual mesh collider                   */
/*---------------------------------------------------------------------------*/
/*  1/28/14  * Austin  * Initial beta release                                */
/*****************************************************************************/

using UnityEngine;
using ColliderView;
using System.Collections.Generic;

namespace ColliderView
{
	static class Mesh3D
	{
		// 32k verts = ~400k memory
		private static int MAX_MESH_VERTS = 1024 * 32;
		private static Vector3[] xformVerts = new Vector3[ MAX_MESH_VERTS ];

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
					RenderTris( colliders );
					RenderQuads( colliders );
				}
			}
		}

		private static void TransformGeometry( MeshCollider collider )
		{
			Vector3[] verts = collider.sharedMesh.vertices;
			Transform xform = collider.transform;

			for ( int i = 0; i < verts.Length; i++ )
			{
				xformVerts[i] = xform.TransformPoint( verts[i] );
			}
		}

		private static void RenderLines( List<Collider> colliders )
		{
			GL.Begin( GL.LINES );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					MeshCollider collider = colliders[i] as MeshCollider;

					if ( RenderAABB( collider ) )
					{
						Box3D.RenderLines( collider.bounds );
					}
					else
					{
						TransformGeometry( collider );
						Display.RenderWireframeTris( xformVerts, collider.sharedMesh.triangles );
					}
				}
			GL.End();
		}

		private static void RenderTris( List<Collider> colliders )
		{
			GL.Begin( GL.TRIANGLES );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					MeshCollider collider = colliders[i] as MeshCollider;

					if ( !RenderAABB( collider ) )
					{
						TransformGeometry( collider );
						Display.RenderSolidTris( xformVerts, collider.sharedMesh.triangles );
					}
				}
			GL.End();
		}

		private static void RenderQuads( List<Collider> colliders )
		{
			GL.Begin( GL.QUADS );
				for ( int i = 0; i < colliders.Count; i++ )
				{
					MeshCollider collider = colliders[i] as MeshCollider;

					if ( RenderAABB( collider ) )
					{
						Box3D.RenderQuads( collider.bounds );
					}
				}
			GL.End();
		}

		private static bool RenderAABB( MeshCollider collider )
		{
			if ( collider.convex )
			{
				return true;
			}
			else if ( collider.sharedMesh == null )
			{
				return true;
			}

			return ( collider.sharedMesh.vertexCount >= xformVerts.Length );
		}
	}
}
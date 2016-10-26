/*****************************************************************************/
/* ColliderView                                                              */
/* support@project-jack.com                                                  */
/*                                                                           */
/* Module Description:                                                       */
/*                                                                           */
/* Routines common to ColliderView render objects.                           */
/*                                                                           */
/* Copyright © 2014-2015 project|JACK, LLC                                   */
/*****************************************************************************/

                           /* MODIFICATION LOG */
/*****************************************************************************/
/*  Date     * Who     * Comment                                             */
/*---------------------------------------------------------------------------*/
/*   3/1/15  * Austin  * Added SafeGetComponent for Unity 5                  */
/*---------------------------------------------------------------------------*/
/* 12/11/14  * Austin  * Avoid the GC alloc float array in ObjectMaxScale()  */
/*---------------------------------------------------------------------------*/
/*  1/28/14  * Austin  * Initial beta release                                */
/*****************************************************************************/

using UnityEngine;

namespace ColliderView
{
	static class CV_Common
	{
		public static float ObjectMaxScale( Transform xform )
		{
			Vector3 scale = xform.lossyScale;
			return Mathf.Max( scale.x, Mathf.Max( scale.y, scale.z ) );
		}

		public static void CapsuleObjectScale( Transform xform, out float radiusScale, out float cylinderScale )
		{
			Vector3 objectScale = xform.lossyScale;

			radiusScale = Mathf.Max( objectScale.x, objectScale.z );
			cylinderScale = objectScale.y;
		}

		public static T SafeGetComponent<T>( GameObject go ) where T : Component
		{
			T component = go.GetComponent<T>();

			if ( component == null )
			{
				string err = string.Format( "Unable to GetComponent of type \"{0}\" on GameObject \"{1}\"", typeof( T ), go.name );
				Debug.LogError( err );

#if UNITY_EDITOR
				if ( UnityEditor.SceneView.sceneViews.Count > 0 )
				{
					UnityEditor.SceneView sv = UnityEditor.SceneView.sceneViews[0] as UnityEditor.SceneView;

					if ( sv != null )
					{
						UnityEditor.EditorApplication.isPaused = true;
						UnityEditor.Selection.activeGameObject = go;
						sv.FrameSelected();
					}
				}
#endif
				throw new UnityException( err );
			}

			return component;
		}
	}
}
/*****************************************************************************/
/* ColliderView                                                              */
/* support@project-jack.com                                                  */
/*                                                                           */
/* Module Description:                                                       */
/*                                                                           */
/* Provides the main collider rendering component for Camera objects.        */
/* Displays the button GUI for selectively enabling the rendering of         */
/* collider types.                                                           */
/*                                                                           */
/* Copyright © 2014-2015 project|JACK, LLC                                   */
/*****************************************************************************/

                           /* MODIFICATION LOG */
/*****************************************************************************/
/*  Date     * Who     * Comment                                             */
/*---------------------------------------------------------------------------*/
/*  4/15/15  * Austin  * Refactored rendering code path to reduce draw calls.*/
/*           *         * Exposed customizable data for Unity Inspector.      */
/*---------------------------------------------------------------------------*/
/*  3/1/15   * Austin  * Compile fixes for Unity 5. Added ColliderView       */
/*           *         * namespace to shorten function calls.                */
/*---------------------------------------------------------------------------*/
/* 12/11/14  * Austin  * Added support for 2D Edge Colliders                 */
/*---------------------------------------------------------------------------*/
/*  1/28/14  * Austin  * Initial beta release                                */
/*****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using ColliderView;

public class CV_Camera : MonoBehaviour 
{
	private Camera compCamera;
	private Plane[] viewFrustum;

	public enum Type
	{
		Boxes = 0,
		Spheres,
		Capsules,
		Meshes,
		Characters,
		Count
	}

	private int[] guiColliderCount = new int[ (int)Type.Count ];

	private List<Collider> colliderList = new List<Collider>( 2048 );
	private List<Collider> triggerList = new List<Collider>( 2048 );

	private List<Collider2D> colliderList2D = new List<Collider2D>( 2048 );
	private List<Collider2D> triggerList2D = new List<Collider2D>( 2048 );

	public Color boxColor = Color.yellow;
	public Color sphereColor = Color.blue;
	public Color capsuleColor = Color.green;
	public Color meshColor = Color.magenta;
	public Color characterColor = Color.cyan;
	public Color triggerColor = Color.red;

	public float edgeAlpha = ColliderView.Display.ALPHA_DEFAULT_EDGE;
	public float faceAlpha = ColliderView.Display.ALPHA_DEFAULT_FACE;

	[System.Flags]
	public enum Flags
	{
		None		= 0,
		Boxes		= ( 1 << 0 ),
		Spheres		= ( 1 << 1 ),
		Capsules	= ( 1 << 2 ),
		Meshes		= ( 1 << 3 ),
		NoDepth		= ( 1 << 4 ),
		NoLines		= ( 1 << 5 ),
		NoFaces		= ( 1 << 6 ),
		NoTriggers	= ( 1 << 7 ),
		NoGUI		= ( 1 << 8 ),
	}

	public Flags renderFlags = Flags.None;
	private const Flags RENDER_COLLIDERS = ( Flags.Boxes | Flags.Spheres | Flags.Capsules | Flags.Meshes );

	public void Toggle( Flags flag )
	{
		renderFlags ^= flag;
	}

	public void Set( Flags flag )
	{
		renderFlags |= flag;
	}

	public void Clear( Flags flag )
	{
		renderFlags &= ~flag;
	}

	public bool IsSet( Flags flag )
	{
		return ( ( renderFlags & flag ) != Flags.None );
	}

	public Color ColliderColor( Type type )
	{
		switch( type )
		{
			case Type.Boxes:
				return boxColor;

			case Type.Spheres:
				return sphereColor;
	
			case Type.Capsules:
				return capsuleColor;
	
			case Type.Meshes:
				return meshColor;
	
			case Type.Characters:
				return characterColor;
		}

		throw new UnityException( "ColliderColor: unknown collider type: " + type );
	}

	public float EdgeAlpha
	{
		get { return IsSet( CV_Camera.Flags.NoLines ) ? 0f : edgeAlpha; }
	}

	public float FaceAlpha
	{
		get { return IsSet( CV_Camera.Flags.NoFaces ) ? 0f : faceAlpha; }
	}

	public void Start()
	{
		compCamera = CV_Common.SafeGetComponent< Camera >( gameObject );
	}

	public void OnPostRender()
	{
		Array.Clear( guiColliderCount, 0, guiColliderCount.Length );

		if ( !IsSet( RENDER_COLLIDERS ) )
		{
			return;
		}

		viewFrustum = GeometryUtility.CalculateFrustumPlanes( compCamera );

		if ( IsSet( Flags.Boxes ) )
		{ 
			RenderCollision<BoxCollider>( Type.Boxes );
			RenderCollision2D<BoxCollider2D>( Type.Boxes );
		}

		if ( IsSet( Flags.Spheres ) )
		{ 
			RenderCollision<SphereCollider>( Type.Spheres );
			RenderCollision2D<CircleCollider2D>( Type.Spheres );
		}

		if ( IsSet( Flags.Capsules ) )
		{
			RenderCollision<CapsuleCollider>( Type.Capsules );
			RenderCollision<CharacterController>( Type.Characters );
		}

		if ( IsSet( Flags.Meshes ) )
		{ 
			RenderCollision<MeshCollider>( Type.Meshes );
			RenderCollision2D<PolygonCollider2D>( Type.Meshes );
			RenderCollision2D<EdgeCollider2D>( Type.Meshes );
		}
	}

	private void AddToRenderList( Collider[] colliders )
	{
		colliderList.Clear();
		triggerList.Clear();

		for ( int i = 0; i < colliders.Length; i++ )
		{
			if ( !colliders[i].enabled )
			{
				continue;
			}

			if ( colliders[i].isTrigger && IsSet( Flags.NoTriggers ) )
			{
				continue;
			}

			if ( !GeometryUtility.TestPlanesAABB( viewFrustum, colliders[i].bounds ) )
			{
				continue;
			}

			if ( colliders[i].isTrigger )
			{
				triggerList.Add( colliders[i] );
			}
			else
			{
				colliderList.Add( colliders[i] );
			}
		}
	}

	private void RenderCollision<T>( Type type ) where T : Collider
	{
		T[] colliders = UnityEngine.Object.FindObjectsOfType<T>();
		Color color = ColliderColor( type );

		AddToRenderList( colliders );

		switch( type )
		{
			case Type.Boxes:
				Box3D.Render( colliderList, color, EdgeAlpha, color, FaceAlpha, IsSet( Flags.NoDepth ) );
				Box3D.Render( triggerList, triggerColor, EdgeAlpha, triggerColor, FaceAlpha, IsSet( Flags.NoDepth ) );
			break;
	
			case Type.Spheres:
				Sphere3D.Render( colliderList, color, EdgeAlpha, color, FaceAlpha, IsSet( Flags.NoDepth ) );
				Sphere3D.Render( triggerList, triggerColor, EdgeAlpha, triggerColor, FaceAlpha, IsSet( Flags.NoDepth ) );
			break;
	
			case Type.Capsules:
				Capsule3D.Render( colliderList, color, EdgeAlpha, color, FaceAlpha, IsSet( Flags.NoDepth ) );
				Capsule3D.Render( triggerList, triggerColor, EdgeAlpha, triggerColor, FaceAlpha, IsSet( Flags.NoDepth ) );
			break;
	
			case Type.Meshes:
				Mesh3D.Render( colliderList, color, EdgeAlpha, color, FaceAlpha, IsSet( Flags.NoDepth ) );
				Mesh3D.Render( triggerList, triggerColor, EdgeAlpha, triggerColor, FaceAlpha, IsSet( Flags.NoDepth ) );
			break;
	
			case Type.Characters:
				Character3D.Render( colliderList, color, EdgeAlpha, color, FaceAlpha, IsSet( Flags.NoDepth ) );
				Character3D.Render( triggerList, triggerColor, EdgeAlpha, triggerColor, FaceAlpha, IsSet( Flags.NoDepth ) );
				guiColliderCount[ (int) Type.Capsules ] += colliderList.Count + triggerList.Count;
			break;
		}

		guiColliderCount[ (int) type ] += colliderList.Count + triggerList.Count;
	}

	private void AddToRenderList2D( Collider2D[] colliders )
	{
		colliderList2D.Clear();
		triggerList2D.Clear();

		for ( int i = 0; i < colliders.Length; i++ )
		{
			if ( !colliders[i].enabled )
			{
				continue;
			}

			if ( colliders[i].isTrigger && IsSet( Flags.NoTriggers ) )
			{
				continue;
			}

#if ( !UNITY_4_3 )
			if ( !GeometryUtility.TestPlanesAABB( viewFrustum, colliders[i].bounds ) )
			{
				continue;
			}
#endif

			if ( colliders[i].isTrigger )
			{
				triggerList2D.Add( colliders[i] );
			}
			else
			{
				colliderList2D.Add( colliders[i] );
			}
		}
	}

	private void RenderCollision2D<T>( Type type ) where T : Collider2D
	{
		T[] colliders = UnityEngine.Object.FindObjectsOfType<T>();
		Color color = ColliderColor( type );

		AddToRenderList2D( colliders );

		switch ( type )
		{
			case Type.Boxes:
				Box2D.Render( colliderList2D, color, EdgeAlpha, color, FaceAlpha, IsSet( Flags.NoDepth ) );
				Box2D.Render( triggerList2D, triggerColor, EdgeAlpha, triggerColor, FaceAlpha, IsSet( Flags.NoDepth ) );
			break;

			case Type.Spheres:
				Circle2D.Render( colliderList2D, color, EdgeAlpha, color, FaceAlpha, IsSet( Flags.NoDepth ) );
				Circle2D.Render( triggerList2D, triggerColor, EdgeAlpha, triggerColor, FaceAlpha, IsSet( Flags.NoDepth ) );
			break;

			case Type.Meshes:
				if ( typeof(T) == typeof(EdgeCollider2D) )
				{
					Edge2D.Render( colliderList2D, color, EdgeAlpha, color, FaceAlpha, IsSet( Flags.NoDepth ) );
					Edge2D.Render( triggerList2D, triggerColor, EdgeAlpha, triggerColor, FaceAlpha, IsSet( Flags.NoDepth ) );
				}
				else
				{
					Mesh2D.Render( colliderList2D, color, EdgeAlpha, color, FaceAlpha, IsSet( Flags.NoDepth ) );
					Mesh2D.Render( triggerList2D, triggerColor, EdgeAlpha, triggerColor, FaceAlpha, IsSet( Flags.NoDepth ) );
				}
			break;
		}

		guiColliderCount[ (int) type ] += colliderList2D.Count + triggerList2D.Count;
	}

	// HUD FUNCTIONALITY

	private const int buttonCount = 8;
	public float guiYPosition = 1f;

	private const float frameRelativeW = 0.95f;
	private const float frameRelativeL = ( 1f - frameRelativeW ) * 0.5f;
	private const float frameRelativeH = 0.1f;
	private float frameRelativeT;

	private int framePixelW;
	private int framePixelL;
	private int framePixelH;
	private int framePixelT;

	private const int buttonPixelW = 87;
	private const float buttonRelativeH = 0.75f;
	
	private int buttonLengthTotal;
	private int buttonPixelHeight;
	private int buttonPixelPadding;
	private int buttonPixelL;

	public void OnGUI()
	{
		if ( IsSet( Flags.NoGUI ) )
		{
			return;
		}

		buttonPixelHeight = Mathf.RoundToInt( framePixelH * buttonRelativeH );
		frameRelativeT = ( guiYPosition - frameRelativeH ); 

		// use the vertical centered spacing as the horizontal button padding
		buttonPixelPadding = Mathf.RoundToInt( ( framePixelH - buttonPixelHeight ) * 0.5f );

		buttonLengthTotal = ( buttonPixelW * buttonCount ) + ( buttonPixelPadding * (buttonCount - 1) );
		buttonPixelL = ( framePixelL + framePixelW / 2 ) - ( buttonLengthTotal / 2 );

		RenderFrame();
		RenderButton( "Boxes",		Flags.Boxes,	Type.Boxes );
		RenderButton( "Capsules",	Flags.Capsules,	Type.Capsules );
		RenderButton( "Spheres",	Flags.Spheres,	Type.Spheres );
		RenderButton( "Meshes",		Flags.Meshes,	Type.Meshes );

		RenderButton( "Triggers",	Flags.NoTriggers );
		RenderButton( "Z-Testing",	Flags.NoDepth );
		RenderButton( "Lines",		Flags.NoLines );
		RenderButton( "Faces",		Flags.NoFaces );
	}

	private void RenderFrame()
	{
		framePixelW = Mathf.RoundToInt( buttonLengthTotal + buttonPixelPadding * 2 );
		framePixelL = Mathf.RoundToInt( Screen.width / 2 - framePixelW / 2 );
		framePixelH = Mathf.RoundToInt( Screen.height * frameRelativeH );
		framePixelT = Mathf.RoundToInt( Screen.height * frameRelativeT );

		Rect rect = new Rect( framePixelL, framePixelT, framePixelW, framePixelH );
		GUI.Box( rect, "" );
	}

	private void RenderButton( string setting, CV_Camera.Flags flag )
	{ 
		Rect rect = new Rect( buttonPixelL, framePixelT + buttonPixelPadding, buttonPixelW, buttonPixelHeight );

		string label = IsSet( flag ) ? "Enable\n" : "Disable\n";
		label += setting;
		
		if ( GUI.Button( rect, label ) )
		{
			Toggle( flag );
		}

		buttonPixelL += buttonPixelW + buttonPixelPadding;
	}

	private void RenderButton( string label, CV_Camera.Flags flag, CV_Camera.Type type )
	{ 
		Rect rect = new Rect( buttonPixelL, framePixelT + buttonPixelPadding, buttonPixelW, buttonPixelHeight );

		if ( IsSet( flag ) )
		{ 
			label += "\nCount: " + guiColliderCount[ (int)type ];
		}
		
		if ( GUI.Button( rect, label ) )
		{
			Toggle( flag );
		}

		buttonPixelL += buttonPixelW + buttonPixelPadding;
	}
}
/*****************************************************************************/
/* ColliderView                                                              */
/* support@project-jack.com                                                  */
/*                                                                           */
/* Module Description:                                                       */
/*                                                                           */
/* Provides Unity Inspector access to CV_Camera component.                   */
/*                                                                           */
/* Copyright © 2015 project|JACK, LLC                                        */
/*****************************************************************************/

                           /* MODIFICATION LOG */
/*****************************************************************************/
/*  Date     * Who     * Comment                                             */
/*---------------------------------------------------------------------------*/
/*  4/15/15  * Austin  * Initial release                                     */
/*****************************************************************************/

using UnityEngine;
using UnityEditor;

[ CustomEditor( typeof(CV_Camera) ) ]
public class CV_CameraEditor : Editor
{
	bool foldoutDisplay = true;
	bool foldoutDisplayOptions = true;
	bool foldoutColors = true;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		foldoutDisplay = EditorGUILayout.Foldout( foldoutDisplay, "Colliders" );
		if ( foldoutDisplay )
		{
			DisplayGUI( serializedObject );
		}

		foldoutDisplayOptions = EditorGUILayout.Foldout( foldoutDisplayOptions, "Display Options" );
		if ( foldoutDisplayOptions )
		{
			DisplayOptionsGUI( serializedObject );
		}
		
		foldoutColors = EditorGUILayout.Foldout( foldoutColors, "Colors" );
		if ( foldoutColors )
		{
			ColorsGUI( serializedObject );
		}

		serializedObject.ApplyModifiedProperties();
	}

	void DisplayGUI( SerializedObject obj )
	{
		CV_Camera cam = obj.targetObject as CV_Camera;

		DisplayFlag( cam, CV_Camera.Flags.Boxes, "Boxes" );
		DisplayFlag( cam, CV_Camera.Flags.Spheres, "Spheres" );
		DisplayFlag( cam, CV_Camera.Flags.Capsules, "Capsules" );
		DisplayFlag( cam, CV_Camera.Flags.Meshes, "Meshes" );
		DisplayFlagNegate( cam, CV_Camera.Flags.NoTriggers, "Triggers" );
	}

	void DisplayFlag( CV_Camera cam, CV_Camera.Flags flag, string label )
	{
		if ( EditorGUILayout.Toggle( label, cam.IsSet(flag) ) )
		{
			cam.Set( flag );
		}
		else
		{
			cam.Clear( flag );
		}
	}

	void DisplayFlagNegate( CV_Camera cam, CV_Camera.Flags flag, string label )
	{
		if ( EditorGUILayout.Toggle( label, !cam.IsSet(flag) ) )
		{
			cam.Clear( flag );
		}
		else
		{
			cam.Set( flag );
		}
	}

	void DisplayOptionsGUI( SerializedObject obj )
	{
		CV_Camera cam = obj.targetObject as CV_Camera;
		
		DisplayFlagNegate( cam, CV_Camera.Flags.NoLines, "Draw Lines" );
		DisplayFlagNegate( cam, CV_Camera.Flags.NoFaces, "Draw Faces" );
		DisplayFlagNegate( cam, CV_Camera.Flags.NoDepth, "Depth Test" );

		DisplayFlagNegate( cam, CV_Camera.Flags.NoGUI, "In-Game GUI" );
		SerializedProperty prop = obj.FindProperty( "guiYPosition" );
		prop.floatValue = EditorGUILayout.Slider( "GUI Y-Position", prop.floatValue, 0f, 1f );
	}

	void ColorsGUI( SerializedObject obj )
	{
		SerializedProperty prop = obj.FindProperty( "boxColor" );
		EditorGUILayout.PropertyField( prop, new GUIContent("Boxes") );

		prop = obj.FindProperty( "sphereColor" );
		EditorGUILayout.PropertyField( prop, new GUIContent("Spheres") );

		prop = obj.FindProperty( "capsuleColor" );
		EditorGUILayout.PropertyField( prop, new GUIContent("Capsules") );

		prop = obj.FindProperty( "meshColor" );
		EditorGUILayout.PropertyField( prop, new GUIContent("Meshes") );

		prop = obj.FindProperty( "characterColor" );
		EditorGUILayout.PropertyField( prop, new GUIContent("Characters") );

		prop = obj.FindProperty( "triggerColor" );
		EditorGUILayout.PropertyField( prop, new GUIContent("Triggers") );

		prop = obj.FindProperty( "edgeAlpha" );
		prop.floatValue = EditorGUILayout.Slider( "Edge Alpha", prop.floatValue, 0f, 1f );

		prop = obj.FindProperty( "faceAlpha" );
		prop.floatValue = EditorGUILayout.Slider( "Face Alpha", prop.floatValue, 0f, 1f );
	}
}
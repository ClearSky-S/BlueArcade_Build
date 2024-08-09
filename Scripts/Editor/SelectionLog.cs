//-----------------------------------------------------------------
// Selection Log - Keeps history of your selected objects in unity
//
//  To use, stick this under a folder named "Editor"
//   then select "Window->Selection Log" from toolbar
//
//  Feel free to use/adapt however you want
//
//   More info:
//     http://tools.powerhoof.com
//     dave@powerhoof.com
//     @DuzzOnDrums
//
//-----------------------------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PowerTools
{

public class SelectionLog : EditorWindow
{
	#region Definitions

	[System.Serializable]
	class SelectedObject
	{
		public bool m_locked = false;
		public Object m_object = null;
        public bool m_inScene = false;
	}

	static readonly int MAX_ITEMS = 50;
    static readonly string STRING_INSCENE = "*";

	#endregion
	#region Vars: Private

	[SerializeField] SelectedObject m_selectedObject = null;
	[SerializeField] List<SelectedObject> m_selectedObjects = new List<SelectedObject>();

	[SerializeField] Vector2 m_scrollPosition = Vector2.zero;

	[System.NonSerialized]
	GUIStyle m_lockButtonStyle;
	[System.NonSerialized]
    GUIContent m_searchButtonContent = null;

	#endregion
	#region Funcs: Private

	// Add menu item named "Super Animation Editor" to the Window menu
	[MenuItem("Window/Selection Log")]
	static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow<SelectionLog>("Selection Log");
	}

	/// Called by unity when selection changes
	void OnSelectionChange()
	{
		if ( Selection.activeObject == null )
		{
			m_selectedObject = null;
		}
		else if ( m_selectedObject == null || m_selectedObject.m_object != Selection.activeObject )
		{
			// If the object's in the list, select it, and if not locked, move it to the top of the list
			m_selectedObject = m_selectedObjects.Find(item => item.m_object == Selection.activeObject);

			if ( m_selectedObject == null )
			{
				// Insert a new selected object
				m_selectedObject = new SelectedObject()
	                {
	                    m_object = Selection.activeObject,
	                    m_inScene = AssetDatabase.Contains(Selection.activeInstanceID) == false
	                };
				InsertInList(m_selectedObject);
			}
			else if ( m_selectedObject.m_locked == false )
			{
			    // If object is already in the list, move it to the top (unless it's locked)
				m_selectedObjects.Remove( m_selectedObject );
				InsertInList(m_selectedObject);
			}

			// Cap number of items to reasonable amount
			while ( m_selectedObjects.Count > MAX_ITEMS )
				m_selectedObjects.RemoveAt(0);

			Repaint();
		}
	}

	void OnGUI ()
	{
		m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

		GUILayout.Space(5);

		bool shownClear = false;
		bool processingLocked = false;
		for ( int i = m_selectedObjects.Count-1; i >= 0; --i)
		{
			if ( m_selectedObjects[i].m_locked == false )
			{
				if ( processingLocked )
				{
					// First non-locked. so add Clear button

					shownClear = true;
					if ( LayoutClearButton() )
						break;
					processingLocked = false;
				}
			}
			else
			{
				processingLocked = true;
			}

			LayoutItem(i, m_selectedObjects[i] );
		}

		// If clear button hasn't shown above (ie: no locked items), show it below
		if ( shownClear == false )
			LayoutClearButton();

		EditorGUILayout.EndScrollView();

	}

	bool LayoutClearButton()
	{
		GUILayout.Space(5);

		bool clear = GUILayout.Button("Clear", EditorStyles.miniButton);
		if ( clear )
		{
			for ( int j = m_selectedObjects.Count-1; j >= 0; --j)
			{
				if ( m_selectedObjects[j].m_locked == false )
					m_selectedObjects.RemoveAt(j);
			}
		}

		GUILayout.Space(5);
		return clear;
	}

	void LayoutItem(int i, SelectedObject obj)
	{

		// Lazy create and cache lock button style
		if (m_lockButtonStyle == null)
		{
			GUIStyle temp = "IN LockButton";
			m_lockButtonStyle = new GUIStyle(temp);
			m_lockButtonStyle.margin.top = 3;
			m_lockButtonStyle.margin.left = 10;
			m_lockButtonStyle.margin.right = 10;
		}

		GUIStyle style = EditorStyles.miniButtonLeft;
		style.alignment = TextAnchor.MiddleLeft;

		if (  obj != null && obj.m_object != null)
		{
			GUILayout.BeginHorizontal();

			bool wasLocked = obj.m_locked;
			obj.m_locked = GUILayout.Toggle( obj.m_locked, GUIContent.none, m_lockButtonStyle);
			if ( wasLocked != obj.m_locked )
			{
				m_selectedObjects.Remove(obj);
				InsertInList(obj);
			}

			/* // Enable "up arrow" that lets you move item to top of list. Not really that useful now there's the "lock"
            if ( GUILayout.Button( "\u25B2", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(20) ) )
            {
                m_selectedObject = obj;
                Selection.activeObject = obj.m_object;
 
                // Move to top
                m_selectedObjects.Remove(obj);
                int firstNonLocked = ( m_selectedObjects.FindIndex(item => item.m_locked == true) );
                if ( obj.m_locked == false && firstNonLocked >= 0 )
                {
                    m_selectedObjects.Insert(firstNonLocked, obj );
                }
                else
                {
                    m_selectedObjects.Add( obj );
                }
			}

			style = EditorStyles.miniButtonMid;
			style.alignment = TextAnchor.MiddleLeft;
			*/

			if ( obj == m_selectedObject )
			{
				GUI.enabled = false;
			}

            string objName = obj.m_object.name;
            if ( obj.m_inScene )
                objName += STRING_INSCENE; // Append string to scene instances to easily tell them apart
            if ( GUILayout.Button( objName, style ) )
			{
			    m_selectedObject = obj;

				// If it's a scene, load the scene
				if ( obj.m_object is SceneAsset ) 
				{
					UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
					UnityEditor.SceneManagement.EditorSceneManager.OpenScene( AssetDatabase.GetAssetPath(obj.m_object) );
				}
				else 
				{   
					// Not a scene, so select the object
					Selection.activeObject = obj.m_object; 
				}
			}

			GUI.enabled = true;

			// Lazy find and cache Search button
			if ( m_searchButtonContent == null )
				m_searchButtonContent = EditorGUIUtility.IconContent("d_ViewToolZoom");

			if ( GUILayout.Button( m_searchButtonContent, EditorStyles.miniButtonRight, GUILayout.MaxWidth(25), GUILayout.MaxHeight(15) ) )
			{
				EditorGUIUtility.PingObject(obj.m_object);
			}

			GUILayout.EndHorizontal();
		}
	}

	// Adds the SelectedObject after the first non-locked object
	void InsertInList(SelectedObject obj)
	{
	    // Insert after the first non-locked object
		int firstNonLocked = ( m_selectedObjects.FindIndex(item => item.m_locked == true) );
		if ( firstNonLocked >= 0 )
			m_selectedObjects.Insert(firstNonLocked, obj );
		else
			m_selectedObjects.Add( obj );
	}

	#endregion

}

}
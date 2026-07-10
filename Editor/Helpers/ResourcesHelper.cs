using UnityEditor;
using UnityEngine;

namespace Gytis0.SOManager.Editor.Helpers
{
	public static class ResourcesHelper
	{
		public static bool IsInit = false;

		public static Font consolas;
		public static GUIStyle style_Header;
		public static GUIStyle style_Header_NewItem;
		public static GUIStyle style_Panel_NotSelected;
		public static GUIStyle style_Panel_Selected;
		public static GUIStyle style_Asset_ListLabel;
		public static GUIStyle style_Asset_GridLabel;
		public static GUIStyle style_Panel_Button;
		public static GUIStyle style_Type_NameLabel;
		public static GUIStyle style_Type_CountLabel;
		public static GUIStyle style_Inspector_Status;
		public static GUIContent icon_ActiveItems_On;
		public static GUIContent icon_ActiveItems_Off;
		public static GUIContent icon_DeletedItems_On;
		public static GUIContent icon_DeletedItems_Off;
		public static GUIContent icon_AddType;
		public static GUIContent icon_AddAsset;
		public static GUIContent icon_Delete;
		public static GUIContent icon_RestoreAsset;
		public static GUIContent icon_Save;
		public static GUIContent icon_Refresh;
		public static GUIContent icon_Build;
		public static GUIContent icon_SavePlus;
		public static GUIContent icon_Cancel;
		public static GUIContent icon_Grid;
		public static GUIContent icon_List;
		public static GUIContent icon_Help;
		public static Sprite sprite_DefaultItemIcon;

		public static void LoadIcons()
		{
			icon_ActiveItems_On = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/file.png") as Texture2D, "Hide Active Items");
			icon_ActiveItems_Off = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/file-grey.png") as Texture2D, "Show Active Items");
			icon_DeletedItems_On = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/file-x-corner.png") as Texture2D, "Hide Deleted Items");
			icon_DeletedItems_Off = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/file-x-corner-grey.png") as Texture2D, "Show Deleted Items");
			icon_AddType = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/package-plus.png") as Texture2D, "Add New Category");
			icon_AddAsset = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/AddAsset.png") as Texture2D, "Add New Item");
			icon_Delete = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/trash.png") as Texture2D, "Delete Item");
			icon_RestoreAsset = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/RestoreAsset.png") as Texture2D, "Restore Item");
			icon_Save = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/Save.png") as Texture2D, "Save");
			icon_Refresh = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/rotate-ccw.png") as Texture2D, "Refresh");
			icon_Build = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/hammer.png") as Texture2D, "Build");
			icon_SavePlus = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/save-plus.png") as Texture2D, "Save");
			icon_Cancel = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/ban.png") as Texture2D, "Cancel");
			icon_Grid = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/layout-grid.png") as Texture2D, "Grid View");
			icon_List = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/list.png") as Texture2D, "List View");
			icon_Help = new GUIContent(EditorGUIUtility.Load("Packages/com.gytis0.somanager/Editor/Icons/circle-question-mark.png") as Texture2D, "Help");
			sprite_DefaultItemIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.gytis0.somanager/Editor/Icons/box.png");
		}

		public static void Init()
		{
			IsInit = true;
			consolas = AssetDatabase.LoadAssetAtPath<Font>("Packages/com.gytis0.somanager/Fonts/Consolas-Regular.ttf");
			style_Asset_ListLabel = new GUIStyle()
			{
				alignment = TextAnchor.MiddleLeft,
				normal = { textColor = Color.white },
				font = consolas,
			};

			style_Asset_GridLabel = new GUIStyle(EditorStyles.miniLabel)
			{
				alignment = TextAnchor.MiddleCenter,
				normal = { textColor = Color.white },
				font = consolas,
			};

			style_Panel_Button = new GUIStyle(EditorStyles.miniLabel)
			{
				alignment = TextAnchor.LowerCenter,
				normal = { textColor = Color.white },
				font = consolas,
				fontStyle = FontStyle.Bold
			};

			style_Type_NameLabel = new GUIStyle()
			{
				alignment = TextAnchor.MiddleLeft,
				normal = { textColor = Color.white },
				font = consolas,
			};

			style_Inspector_Status = new GUIStyle()
			{
				alignment = TextAnchor.MiddleLeft,
				normal = { textColor = Color.white },
				font = consolas,
			};

			style_Type_CountLabel = new GUIStyle()
			{
				alignment = TextAnchor.MiddleRight,
				normal = { textColor = Color.white },
				font = consolas,
				fontStyle = FontStyle.Italic
			};

			Texture2D textureHeader = new(1, 1);
			textureHeader.SetPixel(0, 0, ColorPalette.UnityDefault_Dark);
			textureHeader.Apply();

			style_Header = new GUIStyle
			{
				normal = { background = textureHeader },
			};

			Texture2D textureNewItem = new(1, 1);
			textureNewItem.SetPixel(0, 0, ColorPalette.Orange);
			textureNewItem.Apply();

			style_Header_NewItem = new GUIStyle
			{
				normal = { background = textureNewItem },
			};

			int w = 2;
			Texture2D textureNormal = CreatePanelTexture(ColorPalette.UnityDefault_Dark_Dark, ColorPalette.UnityDefault, w);
			textureNormal.SetPixel(0, 0, ColorPalette.UnityDefault);
			textureNormal.hideFlags = HideFlags.HideAndDontSave;
			textureNormal.Apply();

			style_Panel_NotSelected = new GUIStyle
			{
				normal = { background = textureNormal },
				border = new RectOffset(w, w, w, w),
				padding = new RectOffset(w, w, w, w)
			};

			Texture2D textureSelectedPanel = CreatePanelTexture(ColorPalette.Teal, ColorPalette.UnityDefault, w);
			textureSelectedPanel.hideFlags = HideFlags.HideAndDontSave;

			style_Panel_Selected = new GUIStyle
			{
				normal = { background = textureSelectedPanel },
				border = new RectOffset(w, w, w, w),
				padding = new RectOffset(w, w, w, w)
			};
		}

		private static Texture2D CreatePanelTexture(Color borderColor, Color fillColor, int borderWidth)
		{
			Texture2D texture = new Texture2D(8, 8)
			{
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};

			borderWidth = Mathf.Clamp(borderWidth, 1, texture.width / 2);

			for (int y = 0; y < texture.height; y++)
			{
				for (int x = 0; x < texture.width; x++)
				{
					bool border = x < borderWidth || x >= texture.width - borderWidth || y < borderWidth || y >= texture.height - borderWidth;
					texture.SetPixel(x, y, border ? borderColor : fillColor);
				}
			}

			texture.Apply();

			return texture;
		}

		public static void Deinit()
		{
			IsInit = false;

			style_Header = null;
			style_Header_NewItem = null;
			style_Panel_NotSelected = null;
			style_Panel_Selected = null;

			style_Asset_ListLabel = null;
			style_Asset_GridLabel = null;
			style_Panel_Button = null;
			style_Type_NameLabel = null;
			style_Type_CountLabel = null;
		}
	}
}
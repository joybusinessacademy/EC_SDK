using SkillsVR.EnterpriseCloudSDK.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK.Editor.Editors
{
    public class ECRecordContentEditorWidget : EditorWindow
    {
        private static Texture2D openArrow, closedArrow;

        public bool isFoldoutOn = true;

        public ECRecordContent sourceRecordContent;
        public bool isOn => null == sourceRecordContent ? false : sourceRecordContent.gameScoreBool;

        public ECRecordContentEditorWidget parent;
        public List<ECRecordContentEditorWidget> children = new List<ECRecordContentEditorWidget>();

        public static List<ECRecordContentEditorWidget> GetEditorRenderingWidgetListFromRecordCollection(IEnumerable<ECRecordContent> recordCollection)
        {
            List<ECRecordContentEditorWidget> output = new List<ECRecordContentEditorWidget>();
            if (null == recordCollection)
            {
                return output;
            }
            recordCollection = ECRecordUtil.OrderContents(recordCollection);

            string info = "";
            foreach (var item in recordCollection)
            {
                var widget = new ECRecordContentEditorWidget(item);
                output.Add(widget);
            }

            foreach (var item in output)
            {
                string thisIdStr = item.sourceRecordContent.id.ToString();
                string parentIdStr = item.sourceRecordContent.parentId;
                item.parent = output.Find(x => null != x.sourceRecordContent && x.sourceRecordContent.id.ToString() == parentIdStr);
                item.children = output.FindAll(x => null != x.sourceRecordContent && x.sourceRecordContent.parentId == thisIdStr);
            }

            return output;
        }

        public ECRecordContentEditorWidget(ECRecordContent context)
        {
            sourceRecordContent = context;
            if (null == openArrow)
            {
                openArrow = (Texture2D)Resources.Load("ChildOpened", typeof(Texture2D));
            }
            if (null == closedArrow)
            {
                closedArrow = (Texture2D)Resources.Load("ChildClosed", typeof(Texture2D));
            }
        }


        public bool CheckActiveInHierarchy()
        {
            if (null == parent)
            {
                return true;
            }
            if (!parent.isFoldoutOn)
            {
                return false;
            }
            return parent.CheckActiveInHierarchy();
        }

        public void Draw()
        {
            if (!CheckActiveInHierarchy())
            {
                return;
            }
            if (null == sourceRecordContent)
            {
                return;
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20 * sourceRecordContent.depth);
            if (null != children && children.Count > 0)
            {
                if (GUILayout.Button(isFoldoutOn ? openArrow : closedArrow, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    isFoldoutOn = !isFoldoutOn;
                }
            }
            else if (sourceRecordContent.isScoreTypeBool)
            {
                sourceRecordContent.gameScoreBool = GUILayout.Toggle(sourceRecordContent.gameScoreBool, "");
                if (GUILayout.Button("Copy ID"))
                {
                    EditorGUIUtility.systemCopyBuffer = sourceRecordContent.id;
                }
            }
            else
            {
                GUILayout.Space(20);
            }

            GUIStyle wrappedLabelStyle = new GUIStyle(GUI.skin.label);
            wrappedLabelStyle.wordWrap = true;

            GUILayout.Label(sourceRecordContent.id.ToString(), GUILayout.ExpandWidth(false));
            GUILayout.Label(sourceRecordContent.name, wrappedLabelStyle);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }
}

using SkillsVR.EnterpriseCloudSDK.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SkillsVR.EnterpriseCloudSDK.UI
{
    public class ECRecordContentUI : MonoBehaviour
    {
        public const int DEPTH_WIDTH_UNIT = 20;
        public RectTransform indentObj;
        public Toggle foldoutToggle;
        public Image foldoutOffIcon;
        public Toggle gameScoreToggle;
        public Text labelText;

        private static Sprite openArrow, closedArrow;

        public bool isFoldoutOn = true;

        public ECRecordContent sourceRecordContent;
        public bool isOn => null == sourceRecordContent ? false : sourceRecordContent.gameScoreBool;

        public ECRecordContentUI parent;
        public List<ECRecordContentUI> children = new List<ECRecordContentUI>();


        public void SetSource(ECRecordContent source)
        {
            sourceRecordContent = source;
            gameObject.name = source.id.ToString();
        }

        public void SetupUI()
        {
            if (null == openArrow)
            {
                openArrow = (Sprite)Resources.Load("ChildOpened", typeof(Sprite));
            }
            if (null == closedArrow)
            {
                closedArrow = (Sprite)Resources.Load("ChildClosed", typeof(Sprite));
            }

            int depthWidth = DEPTH_WIDTH_UNIT * sourceRecordContent.depth;
            bool hasChildren = null != children && children.Count > 0;

            foldoutToggle.gameObject.SetActive(hasChildren);
            foldoutToggle.onValueChanged.AddListener((isOn) => { isFoldoutOn = isOn; foldoutOffIcon.gameObject.SetActive(!isOn); });

            gameScoreToggle.gameObject.SetActive(0 == sourceRecordContent.type);
            gameScoreToggle.onValueChanged.AddListener((isOn) =>
            {
                sourceRecordContent.gameScoreBool = isOn;
            });

            if (!gameScoreToggle.gameObject.activeInHierarchy && !foldoutToggle.gameObject.activeInHierarchy)
            {
                depthWidth += DEPTH_WIDTH_UNIT;
            }

            var size = indentObj.sizeDelta;
            size.x = depthWidth;
            indentObj.sizeDelta = size;

            labelText.text = string.Join(" ", sourceRecordContent.id.ToString(), sourceRecordContent.name);
        }

        public static List<ECRecordContentUI> CreateUIHierarchyFromRecordCollection(Func<ECRecordContentUI> createUIAction, IEnumerable<ECRecordContent> recordCollection)
        {
            List<ECRecordContentUI> output = new List<ECRecordContentUI>();
            if (null == recordCollection || null == createUIAction)
            {
                return output;
            }
            recordCollection = ECRecordUtil.OrderContents(recordCollection);

            foreach (var item in recordCollection)
            {
                var uiItem = createUIAction.Invoke();
                uiItem.SetSource(item);
                output.Add(uiItem);
            }

            foreach (var item in output)
            {
                string thisIdStr = item.sourceRecordContent.id.ToString();
                string parentIdStr = item.sourceRecordContent.parentId;
                item.parent = output.Find(x => null != x.sourceRecordContent && x.sourceRecordContent.id.ToString() == parentIdStr);
                item.children = output.FindAll(x => null != x.sourceRecordContent && x.sourceRecordContent.parentId == thisIdStr);
            }
            foreach (var uiItem in output)
            {
                uiItem.SetupUI();
            }
            return output;
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

        private void Update()
        {
            if (null != sourceRecordContent && 0 == sourceRecordContent.type && gameScoreToggle.isOn != sourceRecordContent.gameScoreBool)
            {
                gameScoreToggle.SetIsOnWithoutNotify(sourceRecordContent.gameScoreBool);
            }
        }
    }
}

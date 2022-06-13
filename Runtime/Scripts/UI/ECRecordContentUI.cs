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

        public ECRecordCollectionAsset sourceRecordAsset;
        public ECRecordContent sourceRecordContent;
        public bool isOn => null == sourceRecordContent ? false : sourceRecordContent.gameScoreBool;

        public ECRecordContentUI parent;
        public List<ECRecordContentUI> children = new List<ECRecordContentUI>();


        public void SetSource(ECRecordContent source, ECRecordCollectionAsset asset)
        {
            sourceRecordAsset = asset;
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

            gameScoreToggle.gameObject.SetActive(sourceRecordContent.isScoreTypeBool);
            gameScoreToggle.onValueChanged.AddListener((isOn) =>
            {
                sourceRecordAsset.SetGameScoreBool(sourceRecordContent, isOn);
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

        public static List<ECRecordContentUI> CreateUIHierarchyFromRecordCollection(Func<ECRecordContentUI> createUIAction, ECRecordCollectionAsset asset)
        {
            List<ECRecordContentUI> output = new List<ECRecordContentUI>();
            if (null == asset || null == asset.currentConfig.managedRecords || null == createUIAction)
            {
                return output;
            }
            asset.currentConfig.managedRecords = ECRecordUtil.OrderContents(asset.currentConfig.managedRecords);

            foreach (var item in asset.currentConfig.managedRecords)
            {
                var uiItem = createUIAction.Invoke();
                uiItem.SetSource(item, asset);
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
            if (null != sourceRecordContent && sourceRecordContent.isScoreTypeBool && gameScoreToggle.isOn != sourceRecordContent.gameScoreBool)
            {
                gameScoreToggle.SetIsOnWithoutNotify(sourceRecordContent.gameScoreBool);
            }
        }
    }
}

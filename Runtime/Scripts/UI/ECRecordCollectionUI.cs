using SkillsVR.EnterpriseCloudSDK.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillsVR.EnterpriseCloudSDK.UI
{
    public class ECRecordCollectionUI : MonoBehaviour
    {
        public ECRecordContentUI recordTemplate;

        protected List<ECRecordContentUI> managedItems = new List<ECRecordContentUI>();

        public void Rebuild()
        {
            Clear();
            var asset = ECRecordCollectionAsset.GetECRecordAsset();
            if (null != asset)
            {
                managedItems = ECRecordContentUI.CreateUIHierarchyFromRecordCollection(CreateUIItem, asset);
            }
        }

        private ECRecordContentUI CreateUIItem()
        {
            var item = GameObject.Instantiate(recordTemplate, recordTemplate.transform.parent, false);
            item.gameObject.SetActive(true);
            return item;
        }

        public void Clear()
        {
            foreach (var item in managedItems)
            {
                GameObject.Destroy(item.gameObject);
            }
            managedItems.Clear();
        }

        private void Update()
        {
            foreach (var item in managedItems)
            {
                item.gameObject.SetActive(item.CheckActiveInHierarchy());
            }
        }
    }
}

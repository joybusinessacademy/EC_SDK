using System.Collections.Generic;
using System.Linq;

namespace SkillsVR.EnterpriseCloudSDK.Data
{
	public static class ECRecordUtil
    {
        public static List<ECRecordContent> OrderContents(IEnumerable<ECRecordContent> collection)
        {
            if (null == collection)
            {
                return null;
            }

            List<ECRecordContent> orderedItem = new List<ECRecordContent>(collection.Count());
            var roots = collection.Where(x => 0 == x.depth).OrderBy(x => x.index);
            foreach (var root in roots)
            {
                AddRecordAndChildrenToOrderedList(root, orderedItem, collection);
            }

            return orderedItem;
        }

        private static List<ECRecordContent> AddRecordAndChildrenToOrderedList(ECRecordContent root, List<ECRecordContent> orderedItems, IEnumerable<ECRecordContent> sourceCollection)
        {
            if (null == orderedItems)
            {
                return orderedItems;
            }

            orderedItems.Add(root);
            var children = PickAllChildren(root, sourceCollection);
            if (null == children)
            {
                return orderedItems;
            }
            foreach (var child in children)
            {
                AddRecordAndChildrenToOrderedList(child, orderedItems, sourceCollection);
            }

            return orderedItems;
        }

        public static IEnumerable<ECRecordContent> PickAllChildren(ECRecordContent root, IEnumerable<ECRecordContent> collection)
        {
            return PickAllChildrenFromCollection(root, collection);
        }

        private static IEnumerable<ECRecordContent> PickAllChildrenFromCollection(ECRecordContent root, IEnumerable<ECRecordContent> collection)
        {
            if (null == root || null == collection)
            {
                return null;
            }
            string rootIdStr = root.id.ToString();
            return collection.Where(x => x.parentId == rootIdStr).OrderBy(x => x.index);
        }
    }
}

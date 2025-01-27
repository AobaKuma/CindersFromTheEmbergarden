using System.Xml;
using Verse;

namespace Embergarden
{
    public class PatchOperationAddSafe : PatchOperationPathed
    {
        private string testName;
        private XmlContainer value;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (xml.SelectSingleNode(xpath) == null) return false;
            foreach (object item in xml.SelectNodes(xpath))
            {
                XmlNode xmlNode = item as XmlNode;
                XmlNode xmlNode2 = xmlNode[testName];
                if (xmlNode2 == null)
                {
                    xmlNode2 = xmlNode.OwnerDocument.CreateElement(testName);
                    xmlNode.AppendChild(xmlNode2);
                }
                foreach (XmlNode childNode in value.node.ChildNodes)
                {
                    xmlNode2.AppendChild(xmlNode.OwnerDocument.ImportNode(childNode, deep: true));
                }
            }
            return true;
        }
    }
}

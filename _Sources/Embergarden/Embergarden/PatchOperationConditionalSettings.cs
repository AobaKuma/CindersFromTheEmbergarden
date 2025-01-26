using System.Xml;
using Verse;

namespace Embergarden
{
    public class PatchOperationConditionalSettings : PatchOperation
    {
        private string key;

        private PatchOperation match;

        private PatchOperation nomatch;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (GetSetting())
            {
                if (match != null)
                {
                    return match.Apply(xml);
                }
            }
            else if (nomatch != null)
            {
                return nomatch.Apply(xml);
            }
            if (match == null)
            {
                return nomatch != null;
            }
            return true;
        }

        protected bool GetSetting()
        {
            var fieldInfo = typeof(Setting).GetField(key);
            if (fieldInfo != null)
            {
                Log.Warning($"Key named {key} is not present in settings.");
            }
            return (bool)fieldInfo.GetValue(CinderMod.settings);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Embergarden
{
    public static class D
    {
        public static void Message(string s)
        {
#if DEBUG
            Log.Message(s);
#endif
        }
        public static void Message(object s)
        {
#if DEBUG
            Log.Message(s);
#endif
        }



        public static void Warning(string s)
        {

#if DEBUG
                Log.Warning(s);
#endif

        }

    }
}

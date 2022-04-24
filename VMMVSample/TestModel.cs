using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMMVSample
{
    internal class TestModel : Common.ModelBase<TestModel>
    {
        internal string Name { get; set; }
        internal string Description { get; set; }


        internal Test TestData { get; set; }

        public class Test
        {
            public string Name { get; set; }
        }
    }
}

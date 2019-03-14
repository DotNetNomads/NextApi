using System;
using System.Collections.Generic;
using Abitech.NextApi.Server.Base;
using DeepEqual.Syntax;
using Xunit;

namespace Abitech.NextApi.Server.Tests
{
    public class NextApiUtilTests
    {
        [Fact]
        public void TestPrimitives()
        {
            var original = new PrimitiveEntity
            {
                IntProp = 1,
                BoolProp = true,
                DoubleProp = 3d,
                DateTimeProp = new DateTime(2017, 5, 3),
                NullableIntProp = null,
                StringProp = "originalString"
            };
            var patch = new PrimitiveEntity
            {
                IntProp = 2,
                BoolProp = false,
                DoubleProp = 4d,
                StringProp = "changedString",
                DateTimeProp = new DateTime(2018, 2, 2),
                NullableIntProp = 2
            };
            NextApiUtils.PatchEntity(patch, original);
            original.ShouldDeepEqual(patch);
        }

        [Fact]
        public void TestNotPrimitives()
        {
            var original = new NotPrimitiveEntity
            {
                reference = new SimpleEntityReference
                {
                    StringProp = "prop1"
                },
                referenceCollection = new List<SimpleEntityReference>
                {
                    new SimpleEntityReference {StringProp = "inCollection1"},
                    new SimpleEntityReference {StringProp = "inCollection2"}
                }
            };
            var patch = new NotPrimitiveEntity
            {
                reference = new SimpleEntityReference
                {
                    StringProp = "propPatched"
                },
                referenceCollection = new List<SimpleEntityReference>()
            };
            NextApiUtils.PatchEntity(patch, original);

            Assert.False(original.reference
                .IsDeepEqual(patch.reference));
            Assert.False(original.referenceCollection
                .IsDeepEqual(patch.referenceCollection));
        }

        private class PrimitiveEntity
        {
            public string StringProp { get; set; }
            public int IntProp { get; set; }
            public int? NullableIntProp { get; set; }
            public bool BoolProp { get; set; }
            public double DoubleProp { get; set; }
            public DateTime DateTimeProp { get; set; }
        }

        private class NotPrimitiveEntity
        {
            public SimpleEntityReference reference { get; set; }
            public ICollection<SimpleEntityReference> referenceCollection { get; set; }
        }

        private class SimpleEntityReference
        {
            public string StringProp { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeepEqual.Syntax;
using Xunit;
using System.Linq;
using NextApi.Common.Filtering;
using NextApi.Server.Base;
using NextApi.Server.Entity;
using NextApi.Server.Tests.Filtering;

namespace NextApi.Server.Tests
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
                reference = new SimpleEntityReference {StringProp = "prop1"},
                referenceCollection = new List<SimpleEntityReference>
                {
                    new SimpleEntityReference {StringProp = "inCollection1"},
                    new SimpleEntityReference {StringProp = "inCollection2"}
                }
            };
            var patch = new NotPrimitiveEntity
            {
                reference = new SimpleEntityReference {StringProp = "propPatched"},
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

        [Fact]
#pragma warning disable 1998
        public async Task FilterTests()
#pragma warning restore 1998
        {
            var data = TestSource.GetData();

            // filter: entity => entity.ReferenceModel.Name.ToString().Contains("Model") &&
            //                   (entity.Number == 1 || entity.Number == 2) &&
            //                   (new [] {5,6,10}).Contains("Number")
            var filter = new FilterBuilder()
                .Not(f => f.In<int>("Number", new[] {5, 6, 10}))
                .Build();

            var filter1 = new FilterBuilder()
                .Contains("Name", "6")
                .Not(f => f.In<int>("Number", new[] {5, 6, 10}))
                .Build();

            var filter1_0 = new FilterBuilder()
                .Not(f => f
                    .Not(d => d.Equal("Number", 3))
                    .Contains("Name", "l3"))
                .Build();

            var filter2_0 = new FilterBuilder()
                .Contains("Name", "testModel")
                .Build();

            var filter2_1 = new FilterBuilder()
                .Contains("Name", "testmodel")
                .Build();

            var filter2_2 = new FilterBuilder()
                .Contains("Name", "Testmodel")
                .Build();

            var filter2_3 = new FilterBuilder()
                .Contains("Name", "TESTMODEL")
                .Build();

            var expression = filter.ToLambdaFilter<TestModel>();
            var expression1 = filter1.ToLambdaFilter<TestModel>();
            var expression1_0 = filter1_0.ToLambdaFilter<TestModel>();

            var expression2_0 = filter2_0.ToLambdaFilter<TestModel>();
            var expression2_1 = filter2_1.ToLambdaFilter<TestModel>();
            var expression2_2 = filter2_2.ToLambdaFilter<TestModel>();
            var expression2_3 = filter2_3.ToLambdaFilter<TestModel>();

            var filtered = data.Where(expression).ToList();
            var filtered1 = data.Where(expression1).ToList();
            var filtered1_0 = data.Where(expression1_0).ToList();

            var filtered2_0 = data.Where(expression2_0).ToList();
            var filtered2_1 = data.Where(expression2_1).ToList();
            var filtered2_2 = data.Where(expression2_2).ToList();
            var filtered2_3 = data.Where(expression2_3).ToList();

            Assert.True(filtered2_0.Count == 500);
            Assert.True(filtered2_1.Count == 500);
            Assert.True(filtered2_2.Count == 500);
            Assert.True(filtered2_3.Count == 500);

            Assert.True(filtered.Count == 498);
            Assert.False(filtered1.All(e => e.Number == 5 || e.Number == 6 || e.Number == 10));
            Assert.True(filtered1_0.Count == 0);

            var filterEqualToDate = new FilterBuilder()
                .EqualToDate("Date", new DateTime(2019, 1, 24, 15, 15, 15))
                .Build();
            var expressionEqualToDate = filterEqualToDate.ToLambdaFilter<TestModel>();
            var filteredEqualToDate = data.Where(expressionEqualToDate).ToList();
            Assert.Equal(100, filteredEqualToDate.Count);

            var filterEqualToDateNull = new FilterBuilder()
                .EqualToDate("Date", new DateTime())
                .Build();
            var expressionEqualToDateNull = filterEqualToDateNull.ToLambdaFilter<TestModel>();
            var filteredEqualToDateNull = data.Where(expressionEqualToDateNull).ToList();
            Assert.Single(filteredEqualToDateNull); //500
        }

        [Fact]
#pragma warning disable 1998
        public async Task FilterIntContainsTest()
#pragma warning restore 1998
        {
            // NOTE: WORKS ONLY IN EFCORE
//            var data = TestSource.GetData();
//            var filter = new FilterBuilder()
//                .Contains("Number", "423") 
//                .Build();
//
//            var expression = filter.ToLambdaFilter<TestModel>();
//
//            var filtered = data.Where(expression).ToList();
//
//            Assert.True(filtered.Count == 1);
//            Assert.Equal(423, filtered.FirstOrDefault()?.Number);
        }

        [Fact]
#pragma warning disable 1998
        public async Task FilterAnyTest()
#pragma warning restore 1998
        {
            var items = new List<TestModel>
            {
                new TestModel {Date = DateTime.Now, Id = "1", NestedModels = new List<NestedModel>(), Number = 1},
                new TestModel() {Date = DateTime.Now, Id = "2", NestedModels = null, Number = 2},
                new TestModel()
                {
                    Date = DateTime.Now,
                    Id = "3",
                    NestedModels = new List<NestedModel>
                    {
                        new NestedModel {NestedId = 1, NestedName = "Name1"},
                        new NestedModel {NestedId = 2, NestedName = "Name2"}
                    },
                    Number = 3
                },
                new TestModel()
                {
                    Date = DateTime.Now,
                    Id = "4",
                    NestedModels = new List<NestedModel> {new NestedModel {NestedId = 2, NestedName = "Name3"}},
                    Number = 4
                }
            }.AsQueryable();
            // case 1: test any without predicate
            {
                // 1.1: positive test
                var filterPositive = new FilterBuilder()
                    .NotNull("NestedModels")
                    .Any("NestedModels")
                    .Build()
                    .ToLambdaFilter<TestModel>();
                var resultPositive = items.Where(filterPositive).ToList();
                resultPositive.ShouldDeepEqual(new List<TestModel> {items.ElementAt(2), items.ElementAt(3)});

                // 1.2: negative test
                var filterNegative = new FilterBuilder(LogicalOperators.Or)
                    .Null("NestedModels")
                    .Not(b => b.Any("NestedModels"))
                    .Build()
                    .ToLambdaFilter<TestModel>();
                var resultNegative = items.Where(filterNegative).ToList();
                resultNegative.ShouldDeepEqual(new List<TestModel> {items.ElementAt(0), items.ElementAt(1)});
            }
            // case 2: test any with predicate
            {
                // 2.1: positive test
                var filterPositive = new FilterBuilder()
                    .NotNull("NestedModels")
                    .Any("NestedModels",
                        anyB => anyB.In("NestedName", new[] {"Name2", "Name3"})
                    )
                    .Build()
                    .ToLambdaFilter<TestModel>();
                var resultPositive = items.Where(filterPositive).ToList();
                resultPositive.ShouldDeepEqual(new List<TestModel> {items.ElementAt(2), items.ElementAt(3)});
                // 2.2: negative test (using all)
                var filterNegative = new FilterBuilder(
                        LogicalOperators.Or
                    )
                    .Null("NestedModels")
                    .AllNot("NestedModels",
                        allNot => allNot.In("NestedName", new[] {"Name2", "Name3"})
                    )
                    .Build()
                    .ToLambdaFilter<TestModel>();
                var resultNegative = items.Where(filterNegative).ToList();
                resultNegative.ShouldDeepEqual(new List<TestModel> {items.ElementAt(0), items.ElementAt(1)});
            }
        }
    }
}

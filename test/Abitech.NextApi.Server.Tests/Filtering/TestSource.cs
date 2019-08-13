using System.Collections.Generic;
using System.Linq;

namespace Abitech.NextApi.Server.Tests.Filtering
{
    public static class TestSource
    {
        public static IQueryable<TestModel> GetData()
        {
            var list = new List<TestModel>();

            for (int i = 0; i < 500; i++)
            {
                list.Add(new TestModel()
                {
                    Id = i.ToString(),
                    Name = $"testModel{i}",
                    Number = i,
                    ReferenceModel = new ReferenceModel
                    {
                        Id = i.ToString(),
                        Name = $"referenceModel{i}"
                    }
                });
            }
            
            list.Add(new TestModel()
            {
                Id = 500.ToString(),
                Name = null,
                Number = 500,
                ReferenceModel = new ReferenceModel()
                {
                    Id = 500.ToString(),
                    Name = null
                }
            });

            return list.AsQueryable();
        }
    }
}

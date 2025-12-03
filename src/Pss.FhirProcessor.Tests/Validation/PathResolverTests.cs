using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOH.HealthierSG.Plugins.PSS.FhirProcessor.Utilities;

namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Tests.Validation
{
    [TestClass]
    public class PathResolverTests
    {
        private class TestObject
        {
            public string Name { get; set; }
            public TestChild Child { get; set; }
            public TestChild[] Children { get; set; }
        }

        private class TestChild
        {
            public string Value { get; set; }
            public string Code { get; set; }
        }

        [TestMethod]
        public void ResolvePath_SimpleProperty_ReturnsValue()
        {
            var obj = new TestObject { Name = "Test" };
            
            var result = PathResolver.ResolvePath(obj, "Name");
            
            Assert.AreEqual("Test", result);
        }

        [TestMethod]
        public void ResolvePath_DotNotation_ReturnsValue()
        {
            var obj = new TestObject 
            { 
                Child = new TestChild { Value = "ChildValue" }
            };
            
            var result = PathResolver.ResolvePath(obj, "Child.Value");
            
            Assert.AreEqual("ChildValue", result);
        }

        [TestMethod]
        public void ResolvePath_ArrayIndex_ReturnsElement()
        {
            var obj = new TestObject 
            { 
                Children = new[] 
                { 
                    new TestChild { Value = "First" },
                    new TestChild { Value = "Second" }
                }
            };
            
            var result = PathResolver.ResolvePath(obj, "Children[1].Value");
            
            Assert.AreEqual("Second", result);
        }

        [TestMethod]
        public void ResolvePath_MissingPath_ReturnsNull()
        {
            var obj = new TestObject { Name = "Test" };
            
            var result = PathResolver.ResolvePath(obj, "NonExistent.Path");
            
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ResolvePathWithWildcard_ReturnsMultiple()
        {
            var obj = new TestObject 
            { 
                Children = new[] 
                { 
                    new TestChild { Value = "First" },
                    new TestChild { Value = "Second" },
                    new TestChild { Value = "Third" }
                }
            };
            
            var results = PathResolver.ResolvePathWithWildcard(obj, "Children[]");
            
            Assert.AreEqual(3, results.Count);
        }

        [TestMethod]
        public void ResolvePathWithWildcard_WithPropertyAccess_ReturnsValues()
        {
            var obj = new TestObject 
            { 
                Children = new[] 
                { 
                    new TestChild { Value = "First" },
                    new TestChild { Value = "Second" }
                }
            };
            
            var results = PathResolver.ResolvePathWithWildcard(obj, "Children[].Value");
            
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("First", results[0]);
            Assert.AreEqual("Second", results[1]);
        }
    }
}

namespace Primes1;

public class ObjectBenchmark : IBenchmark
{
    public class ObjectResult
    {
        public int TotalOperations { get; set; }
        public int SimpleObjectCreations { get; set; }
        public int ComplexObjectCreations { get; set; }
        public int DeepHierarchyCreations { get; set; }
        public int ArrayOfObjectsCreations { get; set; }
        public int NestedCollectionsCreations { get; set; }
        public int CloneOperations { get; set; }
        public int ObjectGraphs { get; set; }
    }

    // Simple object with primitive properties
    private class SimpleObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double Value { get; set; }
        public bool Active { get; set; }
    }

    // Complex object with nested structures
    private class ComplexObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public DateTime Created { get; set; }
        public SimpleObject? Nested { get; set; }
    }

    // Deep hierarchy
    private class BaseEntity
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
    }

    private class DerivedEntity : BaseEntity
    {
        public string Name { get; set; } = "";
        public double Score { get; set; }
    }

    private class DeepDerivedEntity : DerivedEntity
    {
        public List<string> Properties { get; set; } = new();
        public Dictionary<string, int> Counters { get; set; } = new();
    }

    // Object with heavy nesting
    private class Node
    {
        public int Value { get; set; }
        public string Data { get; set; } = "";
        public Node? Left { get; set; }
        public Node? Right { get; set; }
        public Node? Parent { get; set; }
    }

    public object Execute(int scale)
    {
        var result = new ObjectResult();

        // 1. Simple object creation and destruction
        for (var i = 0; i < scale; i++)
        {
            var obj = new SimpleObject
            {
                Id = i,
                Name = $"Object_{i}",
                Value = i * 1.5,
                Active = i % 2 == 0
            };
            result.SimpleObjectCreations++;
            // Object goes out of scope and becomes eligible for GC
        }

        // 2. Complex object creation with nested structures
        for (var i = 0; i < scale / 10; i++)
        {
            var obj = new ComplexObject
            {
                Id = i,
                Name = $"Complex_{i}",
                Tags = new() { $"tag_{i}", $"category_{i % 10}" },
                Metadata = new()
                {
                    ["created"] = DateTime.Now,
                    ["priority"] = i % 5,
                    ["score"] = i * 2.5
                },
                Created = DateTime.Now.AddDays(-i),
                Nested = new()
                {
                    Id = i * 2,
                    Name = $"Nested_{i}",
                    Value = i * 3.0,
                    Active = true
                }
            };
            result.ComplexObjectCreations++;
        }

        // 3. Deep inheritance hierarchy
        for (var i = 0; i < scale / 10; i++)
        {
            var obj = new DeepDerivedEntity
            {
                Id = i,
                Type = "DeepDerived",
                Name = $"Entity_{i}",
                Score = i * 1.5,
                Properties = new() { $"prop_{i}", $"attr_{i}" },
                Counters = new()
                {
                    ["visits"] = i,
                    ["clicks"] = i * 2
                }
            };
            result.DeepHierarchyCreations++;
        }

        // 4. Arrays of objects
        for (var i = 0; i < scale / 100; i++)
        {
            var array = new SimpleObject[100];
            for (var j = 0; j < 100; j++)
            {
                array[j] = new()
                {
                    Id = i * 100 + j,
                    Name = $"Item_{j}",
                    Value = j * 0.5,
                    Active = j % 2 == 0
                };
                result.ArrayOfObjectsCreations++;
            }
        }

        // 5. Nested collections of objects
        for (var i = 0; i < scale / 50; i++)
        {
            var list = new List<List<ComplexObject>>();
            for (var j = 0; j < 10; j++)
            {
                var innerList = new List<ComplexObject>();
                for (var k = 0; k < 10; k++)
                {
                    innerList.Add(new()
                    {
                        Id = i * 100 + j * 10 + k,
                        Name = $"Nested_{i}_{j}_{k}",
                        Tags = new() { $"tag_{k}" },
                        Metadata = new() { ["index"] = k },
                        Created = DateTime.Now
                    });
                    result.NestedCollectionsCreations++;
                }
                list.Add(innerList);
            }
        }

        // 6. Object cloning/copying
        var original = new ComplexObject
        {
            Id = 999,
            Name = "Original",
            Tags = new() { "tag1", "tag2" },
            Metadata = new() { ["key"] = "value" },
            Created = DateTime.Now
        };

        for (var i = 0; i < scale / 10; i++)
        {
            var clone = new ComplexObject
            {
                Id = original.Id,
                Name = original.Name,
                Tags = new(original.Tags),
                Metadata = new(original.Metadata),
                Created = original.Created
            };
            result.CloneOperations++;
        }

        // 7. Object graph creation (tree structure)
        for (var i = 0; i < scale / 100; i++)
        {
            var root = CreateTree(5, i * 100); // Depth of 5
            result.ObjectGraphs++;
        }

        // 8. Object with circular references (testing GC)
        for (var i = 0; i < scale / 20; i++)
        {
            var node1 = new Node { Value = i, Data = $"Node1_{i}" };
            var node2 = new Node { Value = i + 1, Data = $"Node2_{i}" };
            var node3 = new Node { Value = i + 2, Data = $"Node3_{i}" };
            
            node1.Left = node2;
            node1.Right = node3;
            node2.Parent = node1;
            node3.Parent = node1;
            
            result.TotalOperations += 3;
        }

        result.TotalOperations += result.SimpleObjectCreations + 
                                 result.ComplexObjectCreations +
                                 result.DeepHierarchyCreations + 
                                 result.ArrayOfObjectsCreations +
                                 result.NestedCollectionsCreations + 
                                 result.CloneOperations +
                                 result.ObjectGraphs;

        return result;
    }

    private Node CreateTree(int depth, int baseValue)
    {
        if (depth == 0)
            return new() { Value = baseValue, Data = $"Leaf_{baseValue}" };

        var node = new Node 
        { 
            Value = baseValue, 
            Data = $"Node_{baseValue}_{depth}" 
        };
        
        node.Left = CreateTree(depth - 1, baseValue * 2);
        node.Right = CreateTree(depth - 1, baseValue * 2 + 1);
        node.Left.Parent = node;
        node.Right.Parent = node;

        return node;
    }

    public int GetMetric(object result)
    {
        return ((ObjectResult)result).TotalOperations;
    }

    public string GetSample(object result)
    {
        var r = (ObjectResult)result;
        return $"Total operations: {r.TotalOperations:N0}\n" +
               $"Simple object creations: {r.SimpleObjectCreations:N0}\n" +
               $"Complex object creations: {r.ComplexObjectCreations:N0}\n" +
               $"Deep hierarchy creations: {r.DeepHierarchyCreations:N0}\n" +
               $"Array of objects: {r.ArrayOfObjectsCreations:N0}\n" +
               $"Nested collections: {r.NestedCollectionsCreations:N0}\n" +
               $"Clone operations: {r.CloneOperations:N0}\n" +
               $"Object graphs: {r.ObjectGraphs:N0}";
    }

    public string GetName() => "Object Creation/Destruction";

    public string GetScaleUnit() => "objects";
}


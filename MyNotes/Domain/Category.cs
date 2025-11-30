using System;

namespace Domain
{
    public class Category
    {
        public Guid Id { get; }
        public string Name { get; }

        public Category(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        public override string ToString() => Name;
    }
}

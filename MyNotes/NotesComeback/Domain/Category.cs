using System;

namespace Domain
{
    public class Category
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        // EF Core требует конструктор без параметров
        private Category() { }

        public Category(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        public override string ToString() => Name;
    }
}

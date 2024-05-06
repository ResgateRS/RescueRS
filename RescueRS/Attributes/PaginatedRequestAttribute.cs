namespace ResgateRS.Attributes;

public class PaginatedRequestAttribute: Attribute {

    public PaginationType paginationType { get; set; }
    public string Description { get; set; } = "";
    public Type Type { get; set; } = typeof(string);

    public PaginatedRequestAttribute(PaginationType pagType) =>
        (paginationType) = (pagType);

    public PaginatedRequestAttribute(string description, PaginationType pagType) =>
        (Description, paginationType) = (description, pagType);

    public PaginatedRequestAttribute(string description, PaginationType pagType, Type type) =>
        (Description, Type, paginationType) = (description, type, pagType);

}

public enum PaginationType {
    Offset,
    Cursor,
    SinglePage,
}
namespace Lavadero.Application.Common;

/// <summary>
/// Representa una página de resultados junto con la información necesaria
/// para construir una paginación en la interfaz.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalItems { get; init; }

    public int TotalPages => PageSize <= 0
        ? 0
        : (int)Math.Ceiling((double)TotalItems / PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}

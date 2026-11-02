using Lavadero.Application.Common;
using Xunit;

namespace Lavadero.Tests;

public sealed class PagedResultTests
{
    [Fact]
    public void CalculaPaginasYLimitesCorrectamente()
    {
        var resultado = new PagedResult<int>
        {
            Items = [11, 12, 13],
            Page = 2,
            PageSize = 3,
            TotalItems = 8
        };

        Assert.Equal(3, resultado.TotalPages);
        Assert.True(resultado.HasPreviousPage);
        Assert.True(resultado.HasNextPage);
    }
}

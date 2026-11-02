using System.ComponentModel.DataAnnotations;

namespace Lavadero.Domain.Entities;

public enum EstadoPedido { Pendiente, EnPreparacion, Entregado, Cancelado }

public class Pedido
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Nombre { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Apellido { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string Celular { get; set; } = string.Empty;
    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
    public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;
    public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
}

public class DetallePedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    [Required, MaxLength(50)] public string Codigo { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Nombre { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
}

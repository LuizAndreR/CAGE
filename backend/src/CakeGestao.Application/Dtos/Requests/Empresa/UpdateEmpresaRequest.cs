namespace CakeGestao.Application.Dtos.Requests.Empresa;

public class UpdateEmpresaRequest
{
    public required string Nome { get; set; }
    public required string Endereco { get; set; }
    public required string Status { get; set; }
}

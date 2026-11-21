using System.ComponentModel.DataAnnotations;

namespace Hermes.Api.Rooms;

// Corpo de POST /api/sessions. O nome é opcional: o contrato não o exige, mas ajuda o ecrã "As minhas salas".
public class CreateRoomRequest
{
    [StringLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
    public string? Name { get; set; }
}

// Corpo de POST /api/sessions/{code}/join.
// bool? + [Required]: se "anonymous" não vier no pedido, a API devolve 400 sozinha (RF06: sem valor por defeito).
public class JoinRequest
{
    [Required(ErrorMessage = "Escolhe se queres ficar anónimo ou visível para os colegas.")]
    public bool? Anonymous { get; set; }
}

// Estado da sala como o contrato da API o mostra: "created", "active" ou "ended".
public record RoomDto(string Code, string Name, string Status);

public record JoinResponse(bool Anonymous);

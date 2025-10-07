using Modest.Core.Common.Models;

namespace Modest.Core.Features.References.Product;

public record ProductLookupDto(Guid Id, string Name, int PieceCountInUnit) : LookupDto(Id, Name);

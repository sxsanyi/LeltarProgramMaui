using LeltarKezelo.Models;

namespace LeltarKezelo.Messages;

public enum ProductChangeKind
{
    Added,
    Updated,
    Deleted
}

/// <summary>
/// Lazán csatolt értesítés (WeakReferenceMessenger) arról, hogy egy termék megváltozott.
/// A lista ebből frissíti a saját állapotát, újratöltés nélkül (szerkesztésnél a görgetési pozíció megmarad).
/// </summary>
public sealed record ProductChangedMessage(Product Product, ProductChangeKind Kind);

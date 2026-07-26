using System.ComponentModel.DataAnnotations;
using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

public class TicketType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? EarlyBirdPrice { get; set; } 
    public DateTime? EarlyBirdExpiryDate { get; set; }
    
    public int Capacity { get; private set; } 
    public int SoldCount { get; private set; }
    public int ReservedCount { get; private set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
    
    public TicketCategory Category { get; set; }
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public TicketType() { }

    public TicketType(string name, int capacity, decimal price)
    {
        Name = name;
        Capacity = capacity;
        Price = price;
    }

    public void UpdateCapacity(int newCapacity)
    {
        if (newCapacity < (SoldCount + ReservedCount))
            throw new InvalidOperationException("Novi kapacitet ne može biti manji od broja već prodatih/rezervisanih karata.");
        Capacity = newCapacity;
    }

    public void CancelSoldTicket(int quantity)
    {
        if (SoldCount < quantity)
            throw new InvalidOperationException("Nije moguće otkazati više prodatih karata.");
        SoldCount -= quantity;
    }
    
    public decimal GetCurrentPrice() => (EarlyBirdPrice.HasValue && EarlyBirdExpiryDate.HasValue && DateTime.UtcNow <= EarlyBirdExpiryDate.Value) ? EarlyBirdPrice.Value : Price;

    public bool HasAvailableCapacity(int requestedQuantity) => (SoldCount + ReservedCount + requestedQuantity) <= Capacity;

    public void Reserve(int quantity)
    {
        if (!HasAvailableCapacity(quantity)) throw new InvalidOperationException("Nema dovoljno slobodnih karata.");
        ReservedCount += quantity;
    }

    public void ConfirmPurchase(int quantity)
    {
        if (ReservedCount < quantity) throw new InvalidOperationException("Ne postoji dovoljno rezervacija za potvrdu.");
        ReservedCount -= quantity;
        SoldCount += quantity;
    }

    public void ReleaseReservation(int quantity)
    {
        if (ReservedCount < quantity) throw new InvalidOperationException("Nema dovoljno rezervacija za oslobađanje.");
        ReservedCount -= quantity;
    }
}
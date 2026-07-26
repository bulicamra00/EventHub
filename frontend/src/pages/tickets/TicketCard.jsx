const TicketCard = ({ ticket, onCancel }) => {
  const formatPurchaseDate = (dateString) => {
    if (!dateString) return 'Nepoznat datum';
    return new Date(dateString).toLocaleDateString('sr-RS', { 
      day: '2-digit', month: '2-digit', year: 'numeric' 
    });
  };

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-5 hover:shadow-md transition-shadow">
      <h3 className="text-lg font-bold text-gray-800 mb-2 truncate">{ticket.eventName}</h3>
      <div className="text-sm text-gray-600 mb-4 space-y-1">
        <p>📅 Kupljeno: {formatPurchaseDate(ticket.purchaseDate)}</p>
        <p className="font-medium text-gray-700">
          💰 Cena: {ticket.purchasePrice?.toLocaleString('sr-RS')} RSD
        </p>
        <p className="text-xs text-gray-400 font-mono truncate pt-1">Kod: {ticket.ticketCode}</p>
      </div>

      {ticket.qrCodeBase64 && (
        <div className="mb-4">
          <img src={`data:image/png;base64,${ticket.qrCodeBase64}`} alt="QR" className="w-24 h-24 mx-auto border p-1 rounded" />
        </div>
      )}
      
      <div className="flex justify-between items-center border-t pt-4">
        <span className={`px-3 py-1 rounded-full text-xs font-semibold 
          ${ticket.status === 'Active' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
          {ticket.status === 'Active' ? 'Aktivna' : ticket.status}
        </span>
        
        {ticket.status === 'Active' && (
          <button 
            className="text-red-500 hover:text-red-700 text-sm font-semibold transition-colors"
            onClick={() => onCancel(ticket.id)}
          >
            Otkaži
          </button>
        )}
      </div>
    </div>
  );
};

export default TicketCard;
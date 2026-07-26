import { useNavigate } from 'react-router-dom';
import api from '../../api';
import toast, { Toaster } from 'react-hot-toast';

const BookingCard = ({ booking }) => {
  const navigate = useNavigate();

  const handleCheckout = (bookingId) => {
    navigate(`/checkout/${bookingId}`);
  };

  const handleCancel = (bookingId) => {
    toast((t) => (
      <div className="flex flex-col gap-2">
        <p className="text-sm">Da li ste sigurni da želite da otkažete ovu rezervaciju?</p>
        <div className="flex gap-2">
          <button
            onClick={async () => {
              toast.dismiss(t.id); 
              try {
                const response = await api.post(`/bookings/${bookingId}/cancel`);
                if (response.status === 200) {
                  toast.success("Rezervacija je uspešno otkazana.");
                  setTimeout(() => window.location.reload(), 1000);
                }
              } catch (error) {
                console.error("Greška pri otkazivanju:", error);
                toast.error(error.response?.data?.message || "Došlo je do greške.");
              }
            }}
            className="bg-red-600 text-white px-3 py-1 rounded text-xs font-bold"
          >
            Da, otkaži
          </button>
          <button
            onClick={() => toast.dismiss(t.id)}
            className="bg-gray-200 text-gray-800 px-3 py-1 rounded text-xs font-bold"
          >
            Odustani
          </button>
        </div>
      </div>
    ), { duration: 5000 }); 
  };

  const formatDate = (dateString) => {
    if (!dateString || dateString === '0001-01-01T00:00:00') return 'N/A';
    return new Date(dateString).toLocaleDateString('sr-RS');
  };

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-5 hover:shadow-md transition-shadow">
      <Toaster position="top-right" />

      <h3 className="text-lg font-bold text-gray-800 mb-3 truncate">
        {booking.eventTitle || "Nepoznat događaj"}
      </h3>
      
      <div className="text-sm text-gray-600 mb-4 space-y-1">
        <p>📅 Rezervisano: {formatDate(booking.createdAt)}</p>
        <p>📦 Količina: {booking.quantity} ulaznica</p>
        <p className="font-semibold text-gray-800">
          💰 Ukupno: {booking.totalPrice.toLocaleString('sr-RS')} RSD
        </p>
        <p className="pt-2">
          Status: <span className={`font-semibold ${
            booking.status === 'Pending' ? 'text-yellow-600' : 
            booking.status === 'Cancelled' ? 'text-red-500' : 
            booking.status === 'Expired' ? 'text-gray-500' : 'text-green-600'
          }`}>
            {booking.status}
          </span>
        </p>
      </div>
      
      {booking.status === 'Pending' && (
        <div className="flex gap-2">
          <button 
            onClick={() => handleCheckout(booking.id)}
            className="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 rounded-md transition-colors text-sm"
          >
            Plaćanje
          </button>
          
          <button 
            onClick={() => handleCancel(booking.id)}
            className="flex-1 bg-gray-100 hover:bg-red-50 text-gray-700 hover:text-red-600 font-semibold py-2 rounded-md transition-colors text-sm border border-gray-200"
          >
            Otkaži
          </button>
        </div>
      )}
    </div>
  );
};

export default BookingCard;
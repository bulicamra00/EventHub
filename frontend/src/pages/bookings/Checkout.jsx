import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import Navbar from '../../components/Navbar';
import api from '../../api';
import toast from 'react-hot-toast';

const Checkout = () => {
  const { bookingId } = useParams();
  const navigate = useNavigate();
  const [booking, setBooking] = useState(null);
  const [isProcessing, setIsProcessing] = useState(false);

  useEffect(() => {
    api.get(`/Bookings/${bookingId}`)
      .then(res => setBooking(res.data))
      .catch(() => {
        toast.error("Greška pri učitavanju rezervacije.");
        navigate('/moje-ulaznice');
      });
  }, [bookingId, navigate]);

  const handlePayment = async () => {
    setIsProcessing(true);
    try {
      await api.post(`/Tickets/purchase`, {
        ticketTypeId: booking.ticketTypeId,
        quantity: booking.quantity,
        attendeeName: "Korisnik", 
        attendeeEmail: "korisnik@example.com"
      }, {
        headers: { 'Idempotency-Key': bookingId } 
      });
      
      toast.success("Kupovina uspešno inicirana! Karta je na putu do vas.");
      navigate('/moje-ulaznice');
    } catch (err) {
      toast.error(err.response?.data?.message || "Došlo je do greške pri kupovini.");
    } finally {
      setIsProcessing(false);
    }
  };

  if (!booking) return <div className="p-10 text-center">Učitavanje...</div>;

  return (
    <>
      <Navbar />
      <div className="max-w-xl mx-auto mt-10 p-6 bg-white rounded shadow border border-gray-100">
        <h2 className="text-xl font-bold mb-4">Potvrda rezervacije: {booking.eventTitle}</h2>
        
        <div className="space-y-2 text-gray-700 mb-6">
          <p>Broj ulaznica: <span className="font-semibold">{booking.quantity}</span></p>
          <p className="text-lg font-bold">Ukupno: {booking.totalPrice?.toLocaleString('sr-RS')} RSD</p>
        </div>
        
        <div className="flex gap-4 mt-6">
          <button 
            onClick={() => navigate(-1)} 
            disabled={isProcessing}
            className="flex-1 bg-gray-200 hover:bg-gray-300 py-3 rounded transition"
          >
            Nazad
          </button>
          <button 
            onClick={handlePayment} 
            disabled={isProcessing}
            className={`flex-1 py-3 rounded font-bold transition ${isProcessing ? 'bg-gray-400 cursor-not-allowed' : 'bg-green-600 hover:bg-green-700'} text-white`}
          >
            {isProcessing ? "Obrada..." : "Plati sada"}
          </button>
        </div>
      </div>
    </>
  );
};

export default Checkout;
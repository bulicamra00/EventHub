import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import api from '../../api';
import Navbar from '../../components/Navbar';
import TicketCard from './TicketCard';
import BookingCard from '../bookings/BookingCard';
import EventCard from '../events/EventCard';

const MyTickets = () => {
  const [tickets, setTickets] = useState([]);
  const [bookings, setBookings] = useState([]);
  const [invitations, setInvitations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('tickets');

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [ticketsRes, bookingsRes, invitationsRes] = await Promise.all([
        api.get('/Tickets/my-tickets'),
        api.get('/Bookings/my-bookings'),
        api.get('/Events/my-invitations') 
      ]);
      setTickets(ticketsRes.data);
      setBookings(bookingsRes.data);
      setInvitations(invitationsRes.data); 
    } catch (error) {
      toast.error('Nismo uspeli da učitamo vaše podatke.');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = (ticketId) => {
    toast((t) => (
      <span className="flex items-center gap-3">
        <span className="text-sm">Da li ste sigurni da želite da otkažete kartu?</span>
        <div className="flex gap-2">
          <button
            onClick={async () => {
              toast.dismiss(t.id);
              try {
                await api.delete(`/Tickets/${ticketId}/cancel`);
                toast.success("Karta je uspešno otkazana.");
                fetchData();
              } catch (error) {
                toast.error(error.response?.data?.message || "Otkazivanje nije uspelo.");
              }
            }}
            className="bg-red-600 text-white px-3 py-1 rounded-md text-xs font-medium hover:bg-red-700 transition"
          >
            Potvrdi
          </button>
          <button
            onClick={() => toast.dismiss(t.id)}
            className="bg-gray-200 text-gray-800 px-3 py-1 rounded-md text-xs font-medium hover:bg-gray-300 transition"
          >
            Odustani
          </button>
        </div>
      </span>
    ), { duration: 8000, position: 'top-center' });
  };

  const handlePay = (id) => {
    toast.success("Pokretanje procesa plaćanja...");
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="container mx-auto px-4 py-8">
        <h2 className="text-3xl font-bold text-gray-900 mb-8">Moje aktivnosti</h2>
        
        <div className="flex space-x-6 border-b border-gray-200 mb-8">
          <button 
            className={`pb-3 font-semibold transition-colors ${activeTab === 'tickets' ? 'text-blue-600 border-b-2 border-blue-600' : 'text-gray-500 hover:text-gray-700'}`}
            onClick={() => setActiveTab('tickets')}
          >
            Moje ulaznice ({tickets.length})
          </button>
          <button 
            className={`pb-3 font-semibold transition-colors ${activeTab === 'bookings' ? 'text-blue-600 border-b-2 border-blue-600' : 'text-gray-500 hover:text-gray-700'}`}
            onClick={() => setActiveTab('bookings')}
          >
            Moje rezervacije ({bookings.length})
          </button>
          <button 
            className={`pb-3 font-semibold transition-colors ${activeTab === 'invitations' ? 'text-blue-600 border-b-2 border-blue-600' : 'text-gray-500 hover:text-gray-700'}`}
            onClick={() => setActiveTab('invitations')}
          >
            Moje pozivnice ({invitations.length})
          </button>
        </div>

        {loading ? (
          <div className="text-center py-20 text-gray-600">Učitavanje podataka...</div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {activeTab === 'tickets' ? (
              tickets.length === 0 ? (
                <div className="col-span-full text-center py-10 text-gray-500">Nemate kupljenih ulaznica.</div>
              ) : (
                tickets.map((ticket) => (
                  <TicketCard key={ticket.id} ticket={ticket} onCancel={handleCancel} />
                ))
              )
            ) : activeTab === 'bookings' ? (
              bookings.length === 0 ? (
                <div className="col-span-full text-center py-10 text-gray-500">Trenutno nemate aktivnih rezervacija.</div>
              ) : (
                bookings.map((booking) => (
                  <BookingCard key={booking.id} booking={booking} onAction={handlePay} />
                ))
              )
            ) : (
              invitations.length === 0 ? (
                <div className="col-span-full text-center py-10 text-gray-500">Nemate novih pozivnica.</div>
              ) : (
                invitations.map((event) => (
                  <EventCard key={event.id} event={event} /> 
                ))
              )
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default MyTickets;
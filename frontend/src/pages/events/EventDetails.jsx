import { useEffect, useState } from 'react';
import { useParams, useSearchParams, useNavigate } from 'react-router-dom';
import Navbar from '../../components/Navbar';
import api from '../../api';
import toast, { Toaster } from 'react-hot-toast';
import ReviewList from '../../components/ReviewList';
import Review from '../../components/Review';

const EventDetails = () => {
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  
  const [event, setEvent] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedTickets, setSelectedTickets] = useState({});
  const [reviews, setReviews] = useState([]);

  const fetchEvent = async () => {
    const token = searchParams.get('token');
    try {
      const url = token ? `/Events/${id}?token=${token}` : `/Events/${id}`;
      const response = await api.get(url);
      setEvent(response.data);
    } catch (err) {
      if (err.response?.status === 401) {
        toast.error("Potrebna je prijava za pristup ovom događaju.");
        navigate(`/login?redirect=/events/${id}${token ? `?token=${token}` : ''}`);
      } else {
        setError('Nisam uspeo da dohvatim detalje.');
      }
    }
  };

  const fetchReviews = async () => {
    try {
      const response = await api.get(`/Reviews/event/${id}`);
      setReviews(response.data);
    } catch (err) {
      console.error("Greška pri dohvatanju recenzija:", err);
    }
  };

  useEffect(() => {
    const init = async () => {
      setLoading(true);
      await Promise.all([fetchEvent(), fetchReviews()]);
      setLoading(false);
    };
    init();
  }, [id, searchParams]);

  const handleBooking = async () => {
    const loadingToast = toast.loading("Rezervišem...");
    try {
      for (const [ticketId, quantity] of Object.entries(selectedTickets)) {
        if (quantity > 0) {
          await api.post('/Bookings/create', {
            eventId: id,
            ticketTypeId: ticketId,
            quantity: quantity
          }, {
            headers: { 'Idempotency-Key': crypto.randomUUID() }
          });
        }
      }
      toast.dismiss(loadingToast);
      toast.success("Rezervacija uspešna!");
      setSelectedTickets({});
      await fetchEvent();
    } catch (err) {
      toast.dismiss(loadingToast);
      toast.error("Greška: " + (err.response?.data?.message || err.message));
    }
  };

  const handleAcceptInvitation = async () => {
    const token = searchParams.get('token');
    if (!token) {
      toast.error("Nevažeći link pozivnice.");
      return;
    }
    const loadingToast = toast.loading("Prihvatam pozivnicu...");
    try {
      await api.post(`/Invitations/accept/${token}`);
      toast.dismiss(loadingToast);
      toast.success("Uspešno ste potvrdili dolazak!");
      await fetchEvent();
    } catch (err) {
      toast.dismiss(loadingToast);
      toast.error("Greška: " + (err.response?.data?.message || err.message));
    }
  };

  if (loading) return <div className="text-center mt-10">Učitavam...</div>;
  if (error) return <div className="text-center mt-10 text-red-500">{error}</div>;
  if (!event) return null;

  const isCompleted = event.status === 4;
  const isCancelled = event.status === 3;
  const isSoldOut = event.status === 5;

  const getDisabledMessage = () => {
    if (!isCompleted) {
      return "Događaj još uvek nije prošao, ne možete ga oceniti.";
    }
    if (!event.userHasUsedTicket) {
      return "Niste prisustvovali događaju i ne možete ga oceniti.";
    }
    if (event.userAlreadyReviewed) {
      return "Već ste ocenili ovaj događaj.";
    }
    return "";
  };

  const canReview = isCompleted && event.userHasUsedTicket && !event.userAlreadyReviewed;
  const disabledMessage = getDisabledMessage();

  return (
    <div className="min-h-screen bg-gray-50">
      <Toaster position="top-right" />
      <Navbar />
      <div className="p-8 max-w-4xl mx-auto bg-white shadow-sm rounded-xl mt-6">
        
        {isCancelled && (
          <div className="bg-red-100 border-l-4 border-red-500 text-red-700 p-4 mb-6" role="alert">
            <p className="font-bold">Ovaj događaj je otkazan!</p>
            <p>Razlog: {event.cancelReason || "Nije naveden razlog."}</p>
          </div>
        )}

        <img src={event.coverImageUrl} alt={event.title} className="w-full h-80 object-cover rounded-xl" />
        
        <h1 className="text-4xl font-bold mt-6">{event.title}</h1>
        <p className="text-sm text-gray-500 mt-2">{event.categoryName}</p>
        
        {event.tagNames && event.tagNames.length > 0 && (
          <div className="flex flex-wrap gap-2 mt-3">
            {event.tagNames.map((tag, index) => (
              <span key={index} className="inline-block bg-blue-100 text-blue-800 text-xs font-semibold px-3 py-1 rounded-full">
                #{tag}
              </span>
            ))}
          </div>
        )}

        <p className="text-gray-700 mt-4">{event.description}</p>

        <div className="grid grid-cols-2 gap-4 mt-6 bg-gray-50 p-4 rounded-lg">
          <div>
            <p className="text-gray-500 text-sm">📍 Lokacija:</p>
            <p className="font-medium">{event.location}</p>
          </div>
          <div>
            <p className="text-gray-500 text-sm">📅 Početak:</p>
            <p className="font-medium">{new Date(event.startDate).toLocaleString()}</p>
          </div>
        </div>
        
        {!isCancelled && (
          <div className="mt-8">
            {isCompleted ? (
              <div className="p-4 bg-gray-100 rounded-lg text-center text-gray-600 font-medium">
                Događaj je završen.
              </div>
            ) : isSoldOut ? (
              <div className="p-4 bg-yellow-100 rounded-lg text-center text-yellow-800 font-bold">
                Događaj je rasprodat!
              </div>
            ) : event.isPrivate ? (
              <div className="p-6 bg-blue-50 border border-blue-200 rounded-lg text-center">
                <h3 className="font-bold text-lg mb-2">Privatan događaj</h3>
                {event.userHasTicket ? (
                  <p className="text-green-700 font-semibold">✅ Već ste potvrdili dolazak.</p>
                ) : (
                  <button onClick={handleAcceptInvitation} className="w-full bg-blue-600 text-white py-3 rounded-lg font-bold hover:bg-blue-700">
                    Prihvati pozivnicu
                  </button>
                )}
              </div>
            ) : event.isBookable ? (
              <div>
                <h3 className="font-bold text-xl mb-4">Izaberite ulaznice</h3>
                {event.ticketTypes.map((ticket) => (
                  <div key={ticket.id} className="flex justify-between items-center p-4 border rounded-lg mb-2">
                    <div>
                      <p className="font-semibold">{ticket.name}</p>
                      <p className="text-sm text-gray-500">{ticket.price} RSD</p>
                    </div>
                    <input
                      type="number"
                      min="0"
                      max={ticket.availableQuantity}
                      className="w-20 p-2 border rounded"
                      placeholder="0"
                      onChange={(e) => {
                        const val = parseInt(e.target.value) || 0;
                        setSelectedTickets(prev => ({ ...prev, [ticket.id]: val }));
                      }}
                    />
                  </div>
                ))}
                <button 
                  onClick={handleBooking} 
                  className="w-full bg-green-600 text-white py-3 rounded-lg font-bold hover:bg-green-700 mt-4"
                >
                  Rezerviši ulaznice
                </button>
              </div>
            ) : (
              <div className="p-4 bg-gray-100 rounded-lg text-center text-gray-600">
                Rezervacije nisu dostupne.
              </div>
            )}
          </div>
        )}

        <div className="mt-12 border-t pt-8">
          <h3 className="text-2xl font-bold mb-6">Recenzije učesnika</h3>
          <ReviewList reviews={reviews} />
          <Review 
            eventId={id} 
            onReviewSubmitted={fetchReviews}
            canReview={canReview}
            disabledMessage={disabledMessage}
          />
        </div>
      </div>
    </div>
  );
};

export default EventDetails;
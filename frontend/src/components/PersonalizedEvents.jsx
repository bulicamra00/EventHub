import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import api from '../api';

const PersonalizedEvents = () => {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchPersonalized = async () => {
    const token = localStorage.getItem('token');
    if (!token) {
      setEvents([]);
      setLoading(false);
      return;
    }

    setLoading(true);
    try {
      const res = await api.get('/Events/personalized');
      setEvents(res.data);
    } catch (error) {
      console.error("Greška pri učitavanju personalizovanih događaja:", error);
      setEvents([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPersonalized();

    const handleAuthChange = () => {
      fetchPersonalized();
    };

    window.addEventListener('authChanged', handleAuthChange);

    return () => window.removeEventListener('authChanged', handleAuthChange);
  }, []);

  if (loading || events.length === 0) return null;

  return (
    <section className="mb-16">
      <h2 className="text-2xl font-bold mb-8 text-blue-600">Događaji organizatora koje pratiš</h2>
      <div className="grid md:grid-cols-3 gap-8">
        {events.map((event) => (
          <div key={event.id} className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden hover:shadow-lg transition">
            <img 
              src={event.coverImageUrl || '/placeholder.jpg'} 
              alt={event.title} 
              className="h-40 w-full object-cover" 
            />
            <div className="p-4">
              <h3 className="font-bold text-lg mb-1">{event.title}</h3>
              
              <p className="text-sm text-blue-600 font-semibold mb-2">
                Organizator: {event.organizerName || 'Nepoznat organizator'}
              </p>

              <p className="text-gray-500 text-sm mb-2">
                {new Date(event.startDate).toLocaleDateString()} • {event.location}
              </p>

              <div className="flex flex-wrap gap-2 mb-4">
                {event.tagNames?.map(tag => (
                  <span key={tag} className="text-xs bg-gray-100 px-2 py-1 rounded text-gray-600">{tag}</span>
                ))}
              </div>
              
              <Link to={`/events/${event.id}`} className="block text-blue-600 font-bold hover:underline">
                Detalji &rarr;
              </Link>
            </div>
          </div>
        ))}
      </div>
    </section>
  );
};

export default PersonalizedEvents;
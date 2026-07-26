import { useEffect, useState, useContext } from 'react';
import { useSearchParams } from 'react-router-dom';
import Navbar from '../components/Navbar';
import Footer from '../components/Footer';
import EventCard from './events/EventCard';
import api from '../api';
import { SearchContext } from '../context/SearchContext';

const Events = () => {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  const [activeStatus, setActiveStatus] = useState(null); 

  const [searchParams] = useSearchParams();
  const selectedCategoryId = searchParams.get('categoryId');

  const { searchTerm, location, sortBy, setSortBy, descending, setDescending } = useContext(SearchContext);

  const tabs = [
    { name: 'Svi', value: null },
    { name: 'Aktivni', value: 2 },
    { name: 'Ponavljajući', value: 'recurring' },
    { name: 'Rasprodato', value: 5 },
    { name: 'Završeni', value: 4 },
    { name: 'Otkazani', value: 3 },
  ];

  const fetchEvents = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await api.get('/Events', {
        params: { 
          searchTerm: searchTerm,
          userLatitude: location.lat,
          userLongitude: location.lon,
          radiusKm: location.lat ? location.radius : null,
          
          categoryId: selectedCategoryId || null,
          
          status: activeStatus !== 'recurring' ? activeStatus : null,
          onlyRecurring: activeStatus === 'recurring',
          
          sortBy: sortBy,
          descending: descending,
          
          pageNumber: 1, 
          pageSize: 20 
        }
      });
      
      const fetchedEvents = response.data?.data; 
      setEvents(Array.isArray(fetchedEvents) ? fetchedEvents : []);
    } catch (err) {
      console.error("Greška pri učitavanju:", err);
      setError("Došlo je do greške pri učitavanju događaja.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const delayDebounceFn = setTimeout(() => {
      fetchEvents();
    }, 500);

    return () => clearTimeout(delayDebounceFn);
  }, [searchTerm, location, activeStatus, selectedCategoryId, sortBy, descending]);

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      
      <main className="max-w-6xl mx-auto py-8 px-4">
        <div className="flex flex-col md:flex-row md:items-center md:justify-between mb-6">
          <h1 className="text-3xl font-bold mb-4 md:mb-0">Svi događaji</h1>
          
          <div className="flex items-center gap-2">
            <label htmlFor="sortSelect" className="text-sm font-medium text-gray-700">Sortiraj po:</label>
            <select
              id="sortSelect"
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value)}
              className="border border-gray-300 rounded-lg px-3 py-1.5 bg-white text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="date">Datumu (Najskoriji)</option>
              <option value="popularity">Popularnosti (Po broju prijava)</option>
            </select>

            <button
              onClick={() => setDescending(!descending)}
              className="border border-gray-300 rounded-lg px-3 py-1.5 bg-white text-sm text-gray-700 hover:bg-gray-100 transition"
              title="Promeni smer sortiranja"
            >
              {descending ? '⬇️ Silazno' : '⬆️ Uzlazno'}
            </button>
          </div>
        </div>
        
        <div className="flex gap-2 mb-8 overflow-x-auto pb-2">
          {tabs.map((tab) => (
            <button
              key={tab.name}
              onClick={() => setActiveStatus(tab.value)}
              className={`px-4 py-2 rounded-full text-sm font-medium transition-colors ${
                activeStatus === tab.value 
                  ? 'bg-blue-600 text-white' 
                  : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-100'
              }`}
            >
              {tab.name}
            </button>
          ))}
        </div>
        
        {loading ? (
          <p className="text-center text-gray-600">Učitavam događaje...</p>
        ) : error ? (
          <p className="text-center text-red-500">{error}</p>
        ) : (
          <div className="grid md:grid-cols-3 gap-6">
            {events.length > 0 ? (
              events.map(event => (
                <EventCard key={event.id} event={event} />
              ))
            ) : (
              <p className="text-gray-600 col-span-full">
                Nema rezultata za odabrani kriterijum.
              </p>
            )}
          </div>
        )}
      </main>

      <Footer />
    </div>
  );
};

export default Events;
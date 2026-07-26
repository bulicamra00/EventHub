import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Navbar from '../components/Navbar';
import Footer from '../components/Footer';
import PersonalizedEvents from '../components/PersonalizedEvents';
import EventCard from './events/EventCard';
import api from '../api';

const Home = () => {
  const isLoggedIn = !!localStorage.getItem('token');
  const [requestStatus, setRequestStatus] = useState('');
  const [loading, setLoading] = useState(false);
  
  const [categories, setCategories] = useState([]);
  const [loadingCategories, setLoadingCategories] = useState(true);

  const [upcomingEvents, setUpcomingEvents] = useState([]);
  const [loadingEvents, setLoadingEvents] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoadingCategories(true);
        setLoadingEvents(true);

        const danas = new Date().toISOString().split('T')[0];

        const [categoriesRes, eventsRes] = await Promise.all([
          api.get('/categories'),
          api.get('/Events', { 
            params: { 
              pageSize: 3, 
              pageNumber: 1, 
              startDate: danas 
            } 
          })
        ]);

        setCategories(categoriesRes.data);
        
        const eventsData = eventsRes.data?.data || eventsRes.data;
        setUpcomingEvents(Array.isArray(eventsData) ? eventsData : []);
      } catch (error) {
        console.error("Greška pri učitavanju podataka:", error);
      } finally {
        setLoadingCategories(false);
        setLoadingEvents(false);
      }
    };

    fetchData();
  }, []);

  const handleRequestOrganizer = async () => {
    try {
      setLoading(true);
      setRequestStatus('');
      
      await api.post('/users/request-organizer');

      setRequestStatus('Uspešno si poslala zahtev za organizatora! Admin će ga pregledati.');
    } catch (error) {
      setRequestStatus('Došlo je do greške prilikom slanja zahteva.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      
      <header className="bg-blue-600 text-white py-20 px-4 text-center">
        <h1 className="text-4xl md:text-5xl font-bold mb-4">Otkrij događaje koji te pokreću</h1>
        <p className="text-lg md:text-xl mb-8 opacity-90">Pronađi inspiraciju, edukaciju ili zabavu u tvom gradu.</p>
        <div className="flex justify-center gap-4">
          <Link to="/events" className="bg-white text-blue-600 px-8 py-3 rounded-full font-bold hover:bg-gray-100 transition shadow-lg">
            Istraži događaje
          </Link>
        </div>
      </header>

      <main className="max-w-6xl mx-auto py-12 px-4">
        
        {isLoggedIn && <PersonalizedEvents />}

        <section className="mb-16">
          <h2 className="text-2xl font-bold mb-8 text-gray-800">Pretraži po kategoriji</h2>
          {loadingCategories ? (
            <p className="text-gray-500">Učitavanje kategorija...</p>
          ) : (
            <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
              {categories.length > 0 ? (
                categories.map((cat) => (
                  <Link 
                    key={cat.id} 
                    to={`/events?categoryId=${cat.id}`} 
                    className="bg-white p-6 rounded-xl shadow-sm hover:shadow-md border border-gray-200 text-center transition"
                  >
                    <span className="text-lg font-semibold text-gray-700">{cat.name}</span>
                  </Link>
                ))
              ) : (
                <p className="text-gray-500 col-span-full">Trenutno nema dostupnih kategorija.</p>
              )}
            </div>
          )}
        </section>

        <section className="mb-16">
          <div className="flex justify-between items-center mb-8">
            <h2 className="text-2xl font-bold text-gray-800">Predstojeći događaji</h2>
            <Link to="/events" className="text-blue-600 font-bold hover:underline">Vidi sve &rarr;</Link>
          </div>
          
          {loadingEvents ? (
            <p className="text-gray-500">Učitavanje događaja...</p>
          ) : (
            <div className="grid md:grid-cols-3 gap-6">
              {upcomingEvents.length > 0 ? (
                upcomingEvents.map((event) => (
                  <EventCard key={event.id} event={event} />
                ))
              ) : (
                <p className="text-gray-500 col-span-full">Trenutno nema predstojećih događaja.</p>
              )}
            </div>
          )}
        </section>

        <section className="bg-white p-8 rounded-2xl shadow-sm border border-gray-100 text-center">
          <h2 className="text-2xl font-bold text-gray-800">Želiš da organizuješ događaj?</h2>
          <p className="mt-2 text-gray-600 mb-6">Pridruži se našoj zajednici i lako upravljaj svojim projektima.</p>
          
          {isLoggedIn ? (
            <div>
              <button 
                onClick={handleRequestOrganizer}
                disabled={loading}
                className="bg-blue-600 text-white px-6 py-2 rounded-full font-bold hover:bg-blue-700 transition shadow"
              >
                {loading ? 'Slanje...' : 'Pošalji zahtev za organizatora →'}
              </button>
              {requestStatus && <p className="mt-4 text-sm text-gray-700 font-medium">{requestStatus}</p>}
            </div>
          ) : (
            <Link to="/register" className="text-blue-600 font-bold hover:underline">
              Postani organizator &rarr;
            </Link>
          )}
        </section>
      </main>

      <Footer />
    </div>
  );
};

export default Home;
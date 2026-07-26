import { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom'; 
import api from '../api';
import Navbar from '../components/Navbar';
import { toast } from 'react-hot-toast';

const Organizers = () => {
  const [organizers, setOrganizers] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchOrganizers = async () => {
      try {
        const response = await api.get('/Follows/organizers');
        setOrganizers(response.data);
      } catch (error) {
        console.error("Greška pri dohvatanju organizatora:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchOrganizers();
  }, []);

  const handleFollow = async (organizerId) => {
    const token = localStorage.getItem('token');
    
    if (!token) {
      toast.error("Morate biti ulogovani da biste pratili organizatora.");
      navigate('/login?redirect=/organizers');
      return;
    }

    try {
      await api.post(`/Follows/follow/${organizerId}`);
      toast.success("Uspešno ste zapratili organizatora!");
      
      setOrganizers(prev => prev.map(o => 
        o.id === organizerId ? { ...o, isFollowed: true } : o
      ));
    } catch (error) {
      console.error("Greška pri praćenju:", error);
      toast.error("Došlo je do greške. Pokušajte ponovo.");
    }
  };

  if (loading) return <div className="p-10 text-center">Učitavanje...</div>;

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      
      <div className="max-w-6xl mx-auto p-6">
        <h1 className="text-3xl font-bold text-gray-800 mb-8">Lista organizatora</h1>
        
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {organizers.map((org) => (
            <div key={org.id} className="bg-white p-6 rounded-xl shadow-sm border border-gray-200 hover:shadow-md transition-shadow">
              <Link to={`/organizers/${org.id}`}>
                <h3 className="text-xl font-semibold text-gray-900 mb-2 hover:text-blue-600 transition-colors">
                  {org.fullName}
                </h3>
              </Link>
              
              <p className="text-gray-600 mb-4">
                Objavljenih događaja: <span className="font-bold text-blue-600">{org.publishedEventsCount}</span>
              </p>
              
              <button 
                onClick={() => handleFollow(org.id)}
                className={`w-full py-2 px-4 rounded-lg font-medium transition ${
                  org.isFollowed 
                    ? 'bg-gray-100 text-gray-500 cursor-not-allowed' 
                    : 'bg-blue-600 text-white hover:bg-blue-700'
                }`}
                disabled={org.isFollowed}
              >
                {org.isFollowed ? 'Već pratite' : 'Zaprati'}
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default Organizers;
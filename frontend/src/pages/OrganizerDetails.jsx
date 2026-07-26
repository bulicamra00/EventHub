import { useEffect, useState } from 'react';
import { useParams, useNavigate, useLocation, Link } from 'react-router-dom';
import api from '../api';
import Navbar from '../components/Navbar';
import { toast } from 'react-hot-toast';

const OrganizerDetails = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const [organizer, setOrganizer] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchOrganizerDetails = async () => {
      try {
        const response = await api.get(`/Follows/organizers/${id}`);
        setOrganizer(response.data);
      } catch (error) {
        console.error("Greška pri dohvatanju detalja:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchOrganizerDetails();
  }, [id]);

  const handleFollowToggle = async () => {
    const token = localStorage.getItem('token');
    
    if (!token) {
      toast.error("Morate biti ulogovani da biste pratili organizatora.");
      navigate(`/login?redirect=${encodeURIComponent(location.pathname)}`);
      return;
    }

    try {
      if (organizer.isFollowed) {
        await api.delete(`/Follows/unfollow/${id}`);
        toast.success("Otpratili ste organizatora.");
      } else {
        await api.post(`/Follows/follow/${id}`);
        toast.success("Zapratili ste organizatora!");
      }
      setOrganizer(prev => ({ ...prev, isFollowed: !prev.isFollowed }));
    } catch (error) {
      console.error("Greška:", error);
      toast.error("Došlo je do greške pri promeni statusa praćenja.");
    }
  };

  if (loading) return <div className="p-10 text-center">Učitavanje detalja...</div>;
  if (!organizer) return <div className="p-10 text-center">Organizator nije pronađen.</div>;

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="max-w-4xl mx-auto p-6">
        <div className="bg-white p-6 rounded-lg shadow-sm mb-8 flex justify-between items-start">
          <div>
            <h1 className="text-3xl font-bold text-gray-800">{organizer.fullName}</h1>
            <p className="text-gray-600">{organizer.email}</p>
            <div className="mt-4">
              <span className="bg-blue-100 text-blue-800 px-3 py-1 rounded-full text-sm font-medium">
                Događaja: {organizer.publishedEventsCount}
              </span>
            </div>
          </div>
          
          <button 
            onClick={handleFollowToggle}
            className={`px-6 py-2 rounded-lg font-medium transition ${
              organizer.isFollowed 
                ? 'bg-gray-100 text-gray-700 hover:bg-gray-200 border border-gray-300' 
                : 'bg-blue-600 text-white hover:bg-blue-700'
            }`}
          >
            {organizer.isFollowed ? 'Otprati' : 'Zaprati'}
          </button>
        </div>

        <h2 className="text-2xl font-bold mb-4">Objavljeni događaji</h2>
        <div className="grid gap-4">
          {organizer.events.length > 0 ? (
            organizer.events.map((event) => (
              <Link 
                to={`/events/${event.id}`} 
                key={event.id} 
                className="block bg-white p-4 rounded shadow-sm hover:shadow-md transition-all border border-gray-100 hover:border-blue-200"
              >
                <div className="flex items-center gap-4">
                  <img 
                    src={event.coverImageUrl} 
                    alt={event.title} 
                    className="w-20 h-20 object-cover rounded" 
                  />
                  <div>
                    <h3 className="font-bold text-lg text-gray-900 hover:text-blue-600 transition-colors">
                      {event.title}
                    </h3>
                    <p className="text-sm text-gray-500">
                      Datum: {new Date(event.startDate).toLocaleDateString()}
                    </p>
                    <p className="text-sm text-gray-500">Lokacija: {event.location}</p>
                  </div>
                </div>
              </Link>
            ))
          ) : (
            <p className="text-gray-500">Organizator trenutno nema objavljenih događaja.</p>
          )}
        </div>
      </div>
    </div>
  );
};

export default OrganizerDetails;
import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import api from '../../api';
import Navbar from '../../components/Navbar';
import SendInvitationModal from '../../components/modals/SendInvitationModal';

const MyEvents = () => {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState(1); 

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedEventId, setSelectedEventId] = useState(null);

  const tabs = [
    { id: 1, label: 'Draft' },
    { id: 2, label: 'Objavljeni' },
    { id: 3, label: 'Otkazani' },
    { id: 4, label: 'Završeni' },
    { id: 5, label: 'Rasprodato' }
  ];

  useEffect(() => {
    fetchMyEvents(activeTab);
  }, [activeTab]);

  const fetchMyEvents = async (status) => {
    try {
      setLoading(true);
      const response = await api.get('/events/my-events', {
        params: { status: status }
      });
      setEvents(response.data.data);
    } catch (error) {
      console.error("Greška pri učitavanju:", error);
      toast.error("Nismo uspeli da učitamo tvoje događaje.");
    } finally {
      setLoading(false);
    }
  };

  const handlePublish = async (eventId) => {
    try {
      await api.patch(`/events/${eventId}/publish`);
      toast.success("Događaj je uspešno objavljen!");
      fetchMyEvents(activeTab);
    } catch (error) {
      toast.error("Nismo uspeli da objavimo događaj.");
    }
  };

  const handleCancel = async (eventId) => {
    const reason = prompt("Unesite razlog otkazivanja:");
    if (!reason) return;

    try {
      await api.post(`/events/${eventId}/cancel`, { reason });
      toast.success("Događaj je otkazan.");
      fetchMyEvents(activeTab);
    } catch (error) {
      toast.error("Greška pri otkazivanju događaja.");
    }
  };

  const getStatusLabel = (id) => tabs.find(t => t.id === id)?.label || 'Nepoznato';

  const isEventPrivate = (event) => {
    return event.isPrivate || event.IsPrivate || event.private || event.Private;
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />

      <div className="container mx-auto p-6">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold text-gray-800">Moji događaji</h1>
          <Link to="/kreiraj-dogadjaj" className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 transition">
            + Kreiraj novi događaj
          </Link>
        </div>

        <div className="flex flex-wrap gap-2 mb-6 border-b border-gray-200">
          {tabs.map((tab) => (
            <button 
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`pb-2 px-4 transition-colors ${activeTab === tab.id ? 'border-b-2 border-blue-600 font-bold text-blue-600' : 'text-gray-500 hover:text-blue-500'}`}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {loading ? (
          <div className="flex justify-center py-20">Učitavanje...</div>
        ) : events.length === 0 ? (
          <div className="text-center py-20 text-gray-600">Nema događaja u ovoj kategoriji.</div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {events.map((event) => (
              <div key={event.id} className="border rounded-lg shadow-sm bg-white overflow-hidden flex flex-col justify-between">
                <div>
                  <div className="relative">
                    <img src={event.coverImageUrl} alt={event.title} className="w-full h-40 object-cover" />
                    
                    {isEventPrivate(event) && (
                      <span className="absolute top-2 right-2 bg-amber-600 text-white text-xs font-bold px-2.5 py-1 rounded-full shadow">
                        🔒 Privatno
                      </span>
                    )}
                  </div>

                  <div className="p-4">
                    <h2 className="text-lg font-semibold mb-2 line-clamp-1">{event.title}</h2>
                    <p className="text-sm text-gray-500 mb-4">{new Date(event.startDate).toLocaleDateString()}</p>
                    
                    <div className="flex justify-between items-center mb-3">
                      <span className="text-xs font-bold bg-gray-100 px-2 py-1 rounded">
                        Status: {getStatusLabel(event.status)}
                      </span>
                    </div>
                  </div>
                </div>

                <div className="p-4 bg-gray-50 border-t flex flex-wrap justify-between items-center gap-2">
                  <div className="flex flex-wrap gap-2 items-center">
                    {event.status === 1 && (
                      <button 
                        onClick={() => handlePublish(event.id)}
                        className="text-green-600 text-sm font-bold hover:underline"
                      >
                        Objavi
                      </button>
                    )}
                    
                    <Link 
                      to={`/izmeni-dogadjaj/${event.id}`} 
                      className="text-blue-600 text-sm hover:underline"
                    >
                      Izmeni
                    </Link>
                    
                    {isEventPrivate(event) && (
                      <button 
                        onClick={() => {
                          setSelectedEventId(event.id);
                          setIsModalOpen(true);
                        }}
                        className="text-amber-600 text-sm font-bold hover:underline"
                      >
                        Pošalji pozivnicu
                      </button>
                    )}
                    
                    {event.status === 2 && (
                      <button 
                        onClick={() => handleCancel(event.id)}
                        className="text-red-600 text-sm hover:underline"
                      >
                        Otkaži
                      </button>
                    )}
                  </div>

                  <Link 
                    to={`/upravljanje-dogadjajem/${event.id}`} 
                    className="bg-purple-600 text-white text-xs px-3 py-1.5 rounded hover:bg-purple-700 transition font-medium"
                  >
                    Upravljaj ⚙️
                  </Link>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      <SendInvitationModal 
        eventId={selectedEventId}
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
      />
    </div>
  );
};

export default MyEvents;
import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import Navbar from '../../components/Navbar';
import EventStatistics from '../../components/organizer/EventStatistics'; 
import EventAttendees from '../../components/organizer/EventAttendees'; 
import EventScanner from '../../components/organizer/EventScanner'; 
import EventAnnouncements from '../../components/organizer/EventAnnouncements';

const EventManagement = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('stats');

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="container mx-auto p-6">
        <button 
          onClick={() => navigate(-1)} 
          className="mb-4 text-blue-600 hover:underline text-sm font-medium"
        >
          ← Nazad na listu događaja
        </button>

        <h1 className="text-3xl font-bold text-gray-800 mb-6">Upravljanje događajem</h1>

        <div className="flex gap-6 border-b border-gray-200 mb-6 overflow-x-auto">
          <button 
            onClick={() => setActiveTab('stats')}
            className={`pb-3 px-1 transition-colors border-b-2 whitespace-nowrap ${activeTab === 'stats' ? 'border-blue-600 font-bold text-blue-600' : 'border-transparent text-gray-500 hover:text-blue-500'}`}
          >
            Statistika
          </button>
          <button 
            onClick={() => setActiveTab('attendees')}
            className={`pb-3 px-1 transition-colors border-b-2 whitespace-nowrap ${activeTab === 'attendees' ? 'border-blue-600 font-bold text-blue-600' : 'border-transparent text-gray-500 hover:text-blue-500'}`}
          >
            Učesnici
          </button>
          <button 
            onClick={() => setActiveTab('scanner')}
            className={`pb-3 px-1 transition-colors border-b-2 whitespace-nowrap ${activeTab === 'scanner' ? 'border-blue-600 font-bold text-blue-600' : 'border-transparent text-gray-500 hover:text-blue-500'}`}
          >
            Skeniranje karata
          </button>
          <button 
            onClick={() => setActiveTab('announcements')}
            className={`pb-3 px-1 transition-colors border-b-2 whitespace-nowrap ${activeTab === 'announcements' ? 'border-blue-600 font-bold text-blue-600' : 'border-transparent text-gray-500 hover:text-blue-500'}`}
          >
            Obaveštenja
          </button>
        </div>

        <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-100">
          {activeTab === 'stats' && <EventStatistics eventId={id} />}
          {activeTab === 'attendees' && <EventAttendees eventId={id} />}
          {activeTab === 'scanner' && <EventScanner eventId={id} />}
          {activeTab === 'announcements' && <EventAnnouncements eventId={id} />}
        </div>
      </div>
    </div>
  );
};

export default EventManagement;
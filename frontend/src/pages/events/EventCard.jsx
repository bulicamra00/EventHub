import { Link } from 'react-router-dom';

const EventCard = ({ event }) => {
  if (!event) return null;

  const formattedDate = event.startDate 
    ? new Date(event.startDate).toLocaleDateString('sr-RS', { day: 'numeric', month: 'short' })
    : 'Datum nije definisan';

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition duration-300">
      <div className="h-40 bg-gray-200 flex items-center justify-center text-gray-400 overflow-hidden">
        {event.coverImageUrl ? (
          <img src={event.coverImageUrl} alt={event.title} className="w-full h-full object-cover" />
        ) : (
          <span>Bez slike</span>
        )}
      </div>
      
      <div className="p-4">
        <h3 className="text-xl font-bold text-gray-800 mb-1">{event.title}</h3>
        <p className="text-sm text-gray-500 mb-2 font-medium">
          {formattedDate} • {event.location || 'Lokacija nije navedena'}
        </p>
        <p className="text-gray-600 text-sm line-clamp-2 h-10">{event.description}</p>
        
        <Link 
          to={`/events/${event.id}`} 
          className="mt-4 block w-full text-center bg-blue-50 text-blue-600 font-bold py-2 rounded-lg hover:bg-blue-100 transition"
        >
          Detalji
        </Link>
      </div>
    </div>
  );
};

export default EventCard;
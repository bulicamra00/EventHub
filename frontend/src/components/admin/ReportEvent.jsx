import { useState, useEffect } from 'react';
import toast from 'react-hot-toast';
import api from '../../api';

const ReportEvent = () => {
  const [events, setEvents] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [blockModalOpen, setBlockModalOpen] = useState(false);
  const [selectedEventId, setSelectedEventId] = useState(null);
  const [blockReason, setBlockReason] = useState('');

  const fetchEvents = async () => {
    try {
      const response = await api.get('/Events', {
        params: { pageNumber: 1, pageSize: 50 }
      });
      
      const fetchedEvents = response.data?.data || response.data;
      setEvents(Array.isArray(fetchedEvents) ? fetchedEvents : []);
    } catch (error) {
      console.error("Greška pri učitavanju događaja:", error);
      toast.error("Nismo uspeli da učitamo događaje.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchEvents();
  }, []);

  const handleBlockEvent = async (e) => {
    e.preventDefault();
    if (!blockReason.trim()) {
      toast.error("Unesi razlog blokiranja.");
      return;
    }

    try {
      await api.post(`/admin/events/${selectedEventId}/block`, JSON.stringify(blockReason), {
        headers: {
          'Content-Type': 'application/json'
        }
      });
      
      toast.success("Događaj je uspešno blokiran.");
      setBlockModalOpen(false);
      setBlockReason('');
      setSelectedEventId(null);
      fetchEvents();
    } catch (error) {
      console.error("Greška pri blokiranju:", error);
      const errorMessage = error.response?.data?.message || "Došlo je do greške prilikom blokiranja događaja.";
      toast.error(errorMessage);
    }
  };

  const handleUnblockEvent = async (eventId) => {
    try {
      await api.post(`/admin/events/${eventId}/unblock`);
      toast.success("Događaj je uspešno odblokiran.");
      fetchEvents();
    } catch (error) {
      console.error("Greška pri odblokiranju:", error);
      const errorMessage = error.response?.data?.message || "Došlo je do greške prilikom odblokiranja događaja.";
      toast.error(errorMessage);
    }
  };

  if (isLoading) return <div className="text-center mt-6 text-gray-500">Učitavanje događaja...</div>;

  return (
    <div>
      <h2 className="text-xl font-semibold text-gray-700 mb-4">Prijave i moderacija događaja</h2>

      <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-100 border-b border-gray-200">
              <th className="p-4 font-semibold text-gray-600">Naziv</th>
              <th className="p-4 font-semibold text-gray-600">Lokacija</th>
              <th className="p-4 font-semibold text-gray-600">Status</th>
              <th className="p-4 font-semibold text-gray-600 text-right">Akcija</th>
            </tr>
          </thead>
          <tbody>
            {events.length > 0 ? (
              events.map((ev) => {
                const isBlocked = ev.isBlocked || ev.IsBlocked;
                return (
                  <tr key={ev.id} className="border-b border-gray-100 hover:bg-gray-50">
                    <td className="p-4 font-medium text-gray-800">{ev.title}</td>
                    <td className="p-4 text-gray-600">{ev.location}</td>
                    <td className="p-4">
                      {isBlocked ? (
                        <span className="bg-red-100 text-red-700 px-2 py-1 rounded text-xs font-semibold" title={ev.blockReason || ev.BlockReason}>
                          Blokiran
                        </span>
                      ) : (
                        <span className="bg-green-100 text-green-700 px-2 py-1 rounded text-xs font-semibold">Aktivan</span>
                      )}
                    </td>
                    <td className="p-4 text-right">
                      {isBlocked ? (
                        <button
                          onClick={() => handleUnblockEvent(ev.id)}
                          className="bg-green-600 text-white px-3 py-1 rounded text-sm hover:bg-green-700 transition"
                        >
                          Odblokiraj
                        </button>
                      ) : (
                        <button
                          onClick={() => {
                            setSelectedEventId(ev.id);
                            setBlockModalOpen(true);
                          }}
                          className="bg-red-500 text-white px-3 py-1 rounded text-sm hover:bg-red-600 transition"
                        >
                          Blokiraj
                        </button>
                      )}
                    </td>
                  </tr>
                );
              })
            ) : (
              <tr>
                <td colSpan="4" className="p-6 text-center text-gray-500">Nema pronađenih događaja.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {blockModalOpen && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="bg-white p-6 rounded-lg max-w-md w-full shadow-lg">
            <h3 className="text-lg font-bold text-gray-800 mb-3">Razlog blokiranja događaja</h3>
            <form onSubmit={handleBlockEvent} className="flex flex-col gap-4">
              <textarea
                value={blockReason}
                onChange={(e) => setBlockReason(e.target.value)}
                placeholder="Unesi razlog (npr. Neprikladan sadržaj)..."
                required
                className="w-full border border-gray-300 rounded p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                rows="3"
              />
              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  onClick={() => {
                    setBlockModalOpen(false);
                    setBlockReason('');
                  }}
                  className="bg-gray-300 text-gray-700 px-4 py-2 rounded hover:bg-gray-400 transition"
                >
                  Otkaži
                </button>
                <button
                  type="submit"
                  className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700 transition"
                >
                  Potvrdi blokiranje
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default ReportEvent;
import { useState, useEffect } from 'react';
import toast from 'react-hot-toast';
import api from '../../api';

const EventAttendees = ({ eventId }) => {
  const [attendees, setAttendees] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (eventId) {
      fetchAttendees();
    }
  }, [eventId]);

  const fetchAttendees = async () => {
    try {
      setLoading(true);
      const response = await api.get(`/tickets/${eventId}/attendees`);
      setAttendees(response.data);
    } catch (error) {
      console.error("Greška pri učitavanju učesnika:", error);
      toast.error("Nismo uspeli da učitamo listu učesnika.");
    } finally {
      setLoading(false);
    }
  };

  const handleExportCsv = async () => {
    try {
      const response = await api.get(`/tickets/${eventId}/export-csv`, {
        responseType: 'blob',
      });

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Ucesnici_Dogadjaj_${eventId}.csv`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      
      toast.success("CSV fajl je uspešno preuzet!");
    } catch (error) {
      console.error("Greška pri izvozu CSV-a:", error);
      toast.error("Došlo je do greške pri preuzimanju CSV fajla.");
    }
  };

  if (loading) {
    return <div className="text-center py-10 text-gray-500">Učitavanje liste učesnika...</div>;
  }

  return (
    <div className="bg-white border rounded-lg shadow-sm overflow-hidden">
      <div className="px-6 py-4 border-b border-gray-100 flex justify-between items-center">
        <h3 className="text-lg font-semibold text-gray-800">Lista učesnika ({attendees.length})</h3>
        <button
          onClick={handleExportCsv}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 transition flex items-center gap-2 shadow-sm"
        >
          📥 Izvezi u CSV
        </button>
      </div>

      {attendees.length === 0 ? (
        <p className="p-6 text-gray-500 text-center">Trenutno nema prijavljenih učesnika za ovaj događaj.</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Ime</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tip karte</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {attendees.map((attendee) => (
                <tr key={attendee.ticketId} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    {attendee.attendeeName}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                    {attendee.attendeeEmail}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                    {attendee.ticketTypeName}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm">
                    {attendee.isScanned ? (
                      <span className="px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800">
                        Skenirano
                      </span>
                    ) : (
                      <span className="px-2 py-1 text-xs font-semibold rounded-full bg-yellow-100 text-yellow-800">
                        Nije skenirano
                      </span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default EventAttendees;
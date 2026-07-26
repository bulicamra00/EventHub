import { useState, useEffect } from 'react';
import toast from 'react-hot-toast';
import api from '../../api';

const EventStatistics = ({ eventId }) => {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (eventId) {
      fetchEventStats();
    }
  }, [eventId]);

  const fetchEventStats = async () => {
    try {
      setLoading(true);
      const response = await api.get(`/tickets/stats/${eventId}`);
      setStats(response.data);
    } catch (error) {
      console.error("Greška pri učitavanju statistike događaja:", error);
      toast.error("Nismo uspeli da učitamo statistiku za ovaj događaj.");
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <div className="text-center py-10 text-gray-500">Učitavanje statistike...</div>;
  }

  if (!stats) {
    return <div className="text-center py-10 text-gray-500">Nema dostupnih statističkih podataka.</div>;
  }

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="p-4 border rounded-lg shadow-sm bg-white">
          <p className="text-sm text-gray-500">Ukupno prodato karata</p>
          <h3 className="text-2xl font-bold text-gray-800">{stats.totalTicketsSold}</h3>
        </div>

        <div className="p-4 border rounded-lg shadow-sm bg-white">
          <p className="text-sm text-gray-500">Ukupan prihod</p>
          <h3 className="text-2xl font-bold text-green-600">{stats.totalRevenue} RSD</h3>
        </div>

        <div className="p-4 border rounded-lg shadow-sm bg-white">
          <p className="text-sm text-gray-500">Popunjenost kapaciteta</p>
          <h3 className="text-2xl font-bold text-blue-600">
            {stats.capacityUtilizationPercentage ? stats.capacityUtilizationPercentage.toFixed(1) : 0}%
          </h3>
        </div>

        <div className="p-4 border rounded-lg shadow-sm bg-white">
          <p className="text-sm text-gray-500">Otkazane karte</p>
          <h3 className="text-2xl font-bold text-red-600">{stats.totalCancelledTickets}</h3>
        </div>
      </div>

      <div className="bg-white border rounded-lg shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-100">
          <h3 className="text-lg font-semibold text-gray-800">Statistika po tipovima karata</h3>
        </div>
        
        {(!stats.ticketTypeStats || stats.ticketTypeStats.length === 0) ? (
          <p className="p-6 text-gray-500 text-sm">Nema definisanih tipova karata ili podataka za ovaj događaj.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Naziv tipa karte</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Prodato komada</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Ukupan kapacitet</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {stats.ticketTypeStats.map((item, index) => (
                  <tr key={index} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {item.ticketTypeName}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {item.soldCount}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {item.totalCapacity}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default EventStatistics;
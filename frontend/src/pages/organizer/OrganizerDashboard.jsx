import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import api from '../../api';
import Navbar from '../../components/Navbar';

const OrganizerDashboard = () => {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchStats();
  }, []);

  const fetchStats = async () => {
    try {
      setLoading(true);
      const response = await api.get('/tickets/stats/global');
      setStats(response.data);
    } catch (error) {
      console.error("Greška pri učitavanju statistike:", error);
      toast.error("Nismo uspeli da učitamo globalnu statistiku.");
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="min-h-screen p-10 text-center">Učitavam dashboard...</div>;
  if (!stats) return <div className="min-h-screen p-10 text-center">Nema dostupnih podataka.</div>;

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="container mx-auto p-6">
        <h1 className="text-3xl font-bold mb-6">Globalna statistika</h1>

        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <div className="bg-white p-6 rounded-lg shadow-sm border">
            <p className="text-gray-500 text-sm">Ukupno prodato</p>
            <h2 className="text-2xl font-bold">{stats.totalTicketsSold}</h2>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-sm border">
            <p className="text-gray-500 text-sm">Ukupan prihod</p>
            <h2 className="text-2xl font-bold">{stats.totalRevenue.toLocaleString()} RSD</h2>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-sm border">
            <p className="text-gray-500 text-sm">Popunjenost</p>
            <h2 className="text-2xl font-bold">{stats.capacityUtilizationPercentage.toFixed(1)}%</h2>
          </div>
          <div className="bg-white p-6 rounded-lg shadow-sm border">
            <p className="text-gray-500 text-sm">Otkazane karte</p>
            <h2 className="text-2xl font-bold text-red-600">{stats.totalCancelledTickets}</h2>
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg shadow-sm border">
          <h3 className="text-xl font-bold mb-4">Zbirni pregled po tipovima karata</h3>
          <table className="w-full text-left">
            <thead>
              <tr className="border-b">
                <th className="pb-3">Naziv karte</th>
                <th className="pb-3">Prodato</th>
                <th className="pb-3">Ukupan kapacitet</th>
              </tr>
            </thead>
            <tbody>
              {stats.ticketTypeStats.map((tt, index) => (
                <tr key={index} className="border-b last:border-0">
                  <td className="py-3 font-medium">{tt.ticketTypeName}</td>
                  <td className="py-3">{tt.soldCount}</td>
                  <td className="py-3">{tt.totalCapacity}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="mt-8 flex gap-4">
            <Link to="/moji-dogadjaji" className="text-gray-600 px-6 py-2 border rounded hover:bg-gray-100">Nazad na listu događaja</Link>
        </div>
      </div>
    </div>
  );
};

export default OrganizerDashboard;
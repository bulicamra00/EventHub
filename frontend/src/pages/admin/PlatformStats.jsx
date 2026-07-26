import { useState, useEffect } from 'react';
import toast from 'react-hot-toast';
import api from '../../api';
import Navbar from '../../components/Navbar';

const PlatformStats = () => {
  const [stats, setStats] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  const fetchStats = async () => {
    try {
      setIsLoading(true);
      const response = await api.get('/admin/stats');
      setStats(response.data);
    } catch (error) {
      console.error("Greška pri učitavanju statistike:", error);
      toast.error("Nismo uspeli da učitamo statistiku platforme.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchStats();
  }, []);

  if (isLoading) return <div className="text-center mt-10">Učitavanje...</div>;

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="container mx-auto p-6 max-w-5xl">
        
        <h1 className="text-3xl font-bold text-gray-800 mb-6">Statistika platforme</h1>

        {stats && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
              <h3 className="text-sm font-medium text-gray-500 uppercase tracking-wider">Ukupno korisnika</h3>
              <p className="text-3xl font-bold text-gray-800 mt-2">{stats.totalUsers}</p>
              <div className="mt-2 text-sm text-gray-600 flex justify-between">
                <span>Organizatori: {stats.totalOrganizers}</span>
                <span>Posetioci: {stats.totalAttendees}</span>
              </div>
            </div>

            <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
              <h3 className="text-sm font-medium text-gray-500 uppercase tracking-wider">Ukupno događaja</h3>
              <p className="text-3xl font-bold text-gray-800 mt-2">{stats.totalEvents}</p>
              <div className="mt-2 text-sm text-gray-600">
                <span>Objavljeni: {stats.publishedEvents}</span>
              </div>
            </div>

            <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
              <h3 className="text-sm font-medium text-gray-500 uppercase tracking-wider">Prodatih karata</h3>
              <p className="text-3xl font-bold text-gray-800 mt-2">{stats.totalTicketsSold}</p>
            </div>

            <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
              <h3 className="text-sm font-medium text-gray-500 uppercase tracking-wider">Ukupan prihod</h3>
              <p className="text-3xl font-bold text-green-600 mt-2">{stats.totalRevenue} RSD</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default PlatformStats;
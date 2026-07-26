import { useState, useEffect } from 'react';
import toast from 'react-hot-toast';
import api from '../../api';
import Navbar from '../../components/Navbar';

const OrganizerRequests = () => {
  const [requests, setRequests] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [actionLoadingId, setActionLoadingId] = useState(null);

  const fetchRequests = async () => {
    try {
      setIsLoading(true);
      const response = await api.get('/admin/organizer-requests');
      setRequests(response.data);
    } catch (error) {
      console.error("Greška pri učitavanju zahteva:", error);
      toast.error("Nismo uspeli da učitamo zahteve.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchRequests();
  }, []);

  const handleApprove = async (userId) => {
    try {
      setActionLoadingId(userId);
      await api.post(`/admin/users/${userId}/approve-organizer`);
      toast.success("Zahtev je uspešno odobren!");
      fetchRequests();
    } catch (error) {
      console.error("Greška pri odobravanju zahteva:", error);
      const errorMessage = error.response?.data || "Nismo uspeli da odobrimo zahtev.";
      toast.error(typeof errorMessage === 'string' ? errorMessage : "Došlo je do greške.");
    } finally {
      setActionLoadingId(null);
    }
  };

  if (isLoading) return <div className="text-center mt-10">Učitavanje...</div>;

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="container mx-auto p-6 max-w-4xl">
        
        <h1 className="text-3xl font-bold text-gray-800 mb-6">Zahtevi za organizatore</h1>

        <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-gray-100 border-b border-gray-200">
                <th className="p-4 font-semibold text-gray-600">Ime i prezime</th>
                <th className="p-4 font-semibold text-gray-600">Email</th>
                <th className="p-4 font-semibold text-gray-600">Grad</th>
                <th className="p-4 font-semibold text-gray-600 text-right">Akcija</th>
              </tr>
            </thead>
            <tbody>
              {requests.length > 0 ? (
                requests.map((user) => (
                  <tr key={user.id} className="border-b border-gray-100 hover:bg-gray-50">
                    <td className="p-4 font-medium text-gray-800">{user.fullName}</td>
                    <td className="p-4 text-gray-600">{user.email}</td>
                    <td className="p-4 text-gray-600">{user.city || 'Nema unetog grada'}</td>
                    <td className="p-4 text-right">
                      <button
                        onClick={() => handleApprove(user.id)}
                        disabled={actionLoadingId === user.id}
                        className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 transition font-medium text-sm disabled:bg-green-300 shadow-sm"
                      >
                        {actionLoadingId === user.id ? 'Odobravanje...' : 'Odobri'}
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan="4" className="p-6 text-center text-gray-500">Trenutno nema novih zahteva na čekanju.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default OrganizerRequests;
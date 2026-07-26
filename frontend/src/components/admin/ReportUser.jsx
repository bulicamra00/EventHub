import { useState, useEffect } from 'react';
import toast from 'react-hot-toast';
import api from '../../api';

const ReportUser = () => {
  const [users, setUsers] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [banModalOpen, setBanModalOpen] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState(null);
  const [banReason, setBanReason] = useState('');

  const fetchUsers = async () => {
    try {
      const response = await api.get('/admin/users');
      setUsers(response.data);
    } catch (error) {
      console.error("Greška pri učitavanju korisnika:", error);
      toast.error("Nismo uspeli da učitamo korisnike.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  const handleBlockUser = async (e) => {
    e.preventDefault();
    if (!banReason.trim()) {
      toast.error("Unesi razlog blokiranja.");
      return;
    }

    try {
      await api.post(`/admin/users/${selectedUserId}/block`, JSON.stringify(banReason), {
        headers: {
          'Content-Type': 'application/json'
        }
      });
      
      toast.success("Korisnik je uspešno blokiran.");
      setBanModalOpen(false);
      setBanReason('');
      setSelectedUserId(null);
      fetchUsers();
    } catch (error) {
      console.error("Greška pri blokiranju:", error);
      const errorMessage = error.response?.data?.message || "Došlo je do greške prilikom blokiranja korisnika.";
      toast.error(errorMessage);
    }
  };

  const handleUnblockUser = async (userId) => {
    try {
      await api.post(`/admin/users/${userId}/unblock`);
      toast.success("Korisnik je uspešno odblokiran.");
      fetchUsers();
    } catch (error) {
      console.error("Greška pri odblokiranju:", error);
      const errorMessage = error.response?.data?.message || "Došlo je do greške prilikom odblokiranja korisnika.";
      toast.error(errorMessage);
    }
  };

  if (isLoading) return <div className="text-center mt-6 text-gray-500">Učitavanje korisnika...</div>;

  return (
    <div>
      <h2 className="text-xl font-semibold text-gray-700 mb-4">Upravljanje korisnicima i blokiranje</h2>

      <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-gray-100 border-b border-gray-200">
              <th className="p-4 font-semibold text-gray-600">Ime i prezime</th>
              <th className="p-4 font-semibold text-gray-600">Email</th>
              <th className="p-4 font-semibold text-gray-600">Uloga</th>
              <th className="p-4 font-semibold text-gray-600">Status</th>
              <th className="p-4 font-semibold text-gray-600 text-right">Akcija</th>
            </tr>
          </thead>
          <tbody>
            {users.length > 0 ? (
              users.map((u) => (
                <tr key={u.id} className="border-b border-gray-100 hover:bg-gray-50">
                  <td className="p-4 font-medium text-gray-800">{u.fullName}</td>
                  <td className="p-4 text-gray-600">{u.email}</td>
                  <td className="p-4">
                    {u.role === 2 ? (
                      <span className="bg-purple-100 text-purple-700 px-2 py-1 rounded text-xs font-semibold">Organizator</span>
                    ) : (
                      <span className="bg-blue-100 text-blue-700 px-2 py-1 rounded text-xs font-semibold">Posetilac</span>
                    )}
                  </td>
                  <td className="p-4">
                    {u.isBlocked ? (
                      <span className="bg-red-100 text-red-700 px-2 py-1 rounded text-xs font-semibold">Blokiran</span>
                    ) : (
                      <span className="bg-green-100 text-green-700 px-2 py-1 rounded text-xs font-semibold">Aktivan</span>
                    )}
                  </td>
                  <td className="p-4 text-right">
                    {u.isBlocked ? (
                      <button
                        onClick={() => handleUnblockUser(u.id)}
                        className="bg-green-600 text-white px-3 py-1 rounded text-sm hover:bg-green-700 transition"
                      >
                        Odblokiraj
                      </button>
                    ) : (
                      <button
                        onClick={() => {
                          setSelectedUserId(u.id);
                          setBanModalOpen(true);
                        }}
                        className="bg-red-500 text-white px-3 py-1 rounded text-sm hover:bg-red-600 transition"
                      >
                        Blokiraj
                      </button>
                    )}
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan="5" className="p-6 text-center text-gray-500">Nema pronađenih korisnika.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {banModalOpen && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <div className="bg-white p-6 rounded-lg max-w-md w-full shadow-lg">
            <h3 className="text-lg font-bold text-gray-800 mb-3">Razlog blokiranja korisnika</h3>
            <form onSubmit={handleBlockUser} className="flex flex-col gap-4">
              <textarea
                value={banReason}
                onChange={(e) => setBanReason(e.target.value)}
                placeholder="Unesi razlog (npr. Kršenje pravila platforme)..."
                required
                className="w-full border border-gray-300 rounded p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                rows="3"
              />
              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  onClick={() => {
                    setBanModalOpen(false);
                    setBanReason('');
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

export default ReportUser;
import { useEffect, useState } from 'react';
import api from "../../api";
import toast from 'react-hot-toast';
import Navbar from '../../components/Navbar';

const UserProfile = () => {
  const [profile, setProfile] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isEditing, setIsEditing] = useState(false);
  
  const [formData, setFormData] = useState({
    fullName: '',
    city: '',
    interests: '' 
  });

  const fetchProfile = async () => {
    try {
      const res = await api.get('/Users/profile');
      setProfile(res.data);
      setFormData({
        fullName: res.data.fullName,
        city: res.data.city || '',
        interests: res.data.interests ? res.data.interests.join(', ') : ''
      });
    } catch (error) {
      toast.error('Greška pri učitavanju profila.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchProfile();
  }, []);

  const handleUpdate = async () => {
    try {
      await api.put('/Users/profile', formData);
      toast.success('Profil uspešno ažuriran!');
      setIsEditing(false);
      fetchProfile();
    } catch (error) {
      toast.error('Greška pri ažuriranju profila.');
    }
  };

  if (isLoading) return <div className="text-center mt-10">Učitavanje...</div>;
  if (!profile) return <div className="text-center mt-10">Profil nije pronađen.</div>;

  return (
    <div className="min-h-screen bg-gray-100">
      <Navbar />
      <div className="container mx-auto p-4 md:p-8">
        <div className="max-w-2xl mx-auto bg-white p-8 rounded-lg shadow-md">
          <div className="flex justify-between items-center mb-6">
            <h2 className="text-2xl font-bold text-blue-600">Moj profil</h2>
            <button 
              onClick={() => isEditing ? handleUpdate() : setIsEditing(true)}
              className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
            >
              {isEditing ? 'Sačuvaj izmene' : 'Izmeni profil'}
            </button>
          </div>

          {isEditing ? (
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium">Ime:</label>
                <input className="w-full border p-2 rounded" value={formData.fullName} onChange={(e) => setFormData({...formData, fullName: e.target.value})} />
              </div>
              <div>
                <label className="block text-sm font-medium">Grad:</label>
                <input className="w-full border p-2 rounded" value={formData.city} onChange={(e) => setFormData({...formData, city: e.target.value})} />
              </div>
              <div>
                <label className="block text-sm font-medium">Interesovanja (razdvojena zarezom):</label>
                <input className="w-full border p-2 rounded" value={formData.interests} onChange={(e) => setFormData({...formData, interests: e.target.value})} />
              </div>
            </div>
          ) : (
            <div className="space-y-4">
              <p><span className="font-bold">Ime:</span> {profile.fullName}</p>
              <p><span className="font-bold">Email:</span> {profile.email}</p>
              <p><span className="font-bold">Grad:</span> {profile.city || 'Nije uneto'}</p>
              <p><span className="font-bold">Datum pridruživanja:</span> {new Date(profile.joinedAt).toLocaleDateString()}</p>
              <p><span className="font-bold">Broj rezervacija:</span> {profile.totalBookingsCount}</p>
              
              <div className="mt-6">
                <h3 className="font-semibold mb-2">Interesovanja:</h3>
                <div className="flex flex-wrap gap-2">
                  {profile.interests?.length > 0 ? profile.interests.map((cat, idx) => (
                    <span key={idx} className="bg-blue-100 text-blue-800 px-3 py-1 rounded-full text-sm font-medium">{cat}</span>
                  )) : <span className="text-gray-500 italic text-sm">Nema odabranih interesovanja.</span>}
                </div>
              </div>

              <div className="mt-6">
                <h3 className="font-semibold mb-2">Posetio sam ove događaje:</h3>
                {profile.attendedEvents?.length > 0 ? (
                  <ul className="list-disc list-inside space-y-1">
                    {profile.attendedEvents.map((evt) => (
                      <li key={evt.eventId} className="text-gray-700">
                        <span className="font-medium">{evt.title}</span> 
                        <span className="text-sm text-gray-500 ml-2">({new Date(evt.date).toLocaleDateString()})</span>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="text-gray-500 italic text-sm">Još uvek nisi posetio nijedan događaj.</p>
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default UserProfile;
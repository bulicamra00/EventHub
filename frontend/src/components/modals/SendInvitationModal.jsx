import { useState } from 'react';
import toast from 'react-hot-toast';
import api from '../../api';

const SendInvitationModal = ({ eventId, isOpen, onClose }) => {
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);

  if (!isOpen) return null;

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!email.trim()) return;

    try {
      setLoading(true);
      await api.post('/invitations/create', {
        eventId: eventId,
        email: email
      });
      toast.success("Pozivnica je uspešno poslata!");
      setEmail('');
      onClose();
    } catch (error) {
      console.error("Greška pri slanju pozivnice:", error);
      toast.error("Nismo uspeli da pošaljemo pozivnicu.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-white/15 backdrop-blur-[2px] flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg max-w-md w-full p-6 shadow-2xl border border-gray-200">
        <h3 className="text-xl font-bold text-gray-800 mb-2">Pošalji pozivnicu</h3>
        <p className="text-sm text-gray-500 mb-4">
          Unesi email adresu osobe koju želiš da pozoveš na ovaj privatni događaj.
        </p>

        <form onSubmit={handleSubmit}>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="pera@example.com"
            required
            className="w-full border border-gray-300 rounded p-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500 mb-4"
          />

          <div className="flex justify-end gap-2">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm text-gray-600 hover:bg-gray-100 rounded transition"
            >
              Otkaži
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-4 py-2 text-sm bg-amber-600 text-white font-bold rounded hover:bg-amber-700 transition disabled:opacity-50"
            >
              {loading ? 'Slanje...' : 'Pošalji'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default SendInvitationModal;